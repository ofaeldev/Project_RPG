using System.Collections;
using System.Linq;
using NUnit.Framework;
using RPGProject.Gameplay;
using RPGProject.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RPGProject.Tests
{
    public sealed class TutorialScenePlayModeTests
    {
        private const string TutorialScenePath = "Assets/Game/Scenes/TutorialScene.unity";
        private const string RatQuestId = "tutorial_clear_mist_rats";

        [UnityTest]
        public IEnumerator TutorialScene_CombatQuestAndCorpseLootFlow_WorksEndToEnd()
        {
            yield return SceneManager.LoadSceneAsync(TutorialScenePath, LoadSceneMode.Single);
            yield return null;

            QuestManager questManager = Object.FindFirstObjectByType<QuestManager>();
            InventoryManager inventory = Object.FindFirstObjectByType<InventoryManager>();
            LootService lootService = Object.FindFirstObjectByType<LootService>();
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject mira = GameObject.Find("NPC_Mira");

            Assert.NotNull(questManager, "Tutorial scene should contain a QuestManager.");
            Assert.NotNull(inventory, "Tutorial scene should contain an InventoryManager.");
            Assert.NotNull(lootService, "Tutorial scene should contain a LootService.");
            Assert.NotNull(player, "Tutorial scene should contain a Player tagged object.");
            Assert.NotNull(mira, "Tutorial scene should contain NPC_Mira.");

            QuestDefinition ratQuest = mira.GetComponent<NPCInteractionTarget>().QuestToOffer;
            Assert.NotNull(ratQuest, "NPC_Mira should offer the mist rat quest.");
            Assert.AreEqual(RatQuestId, ratQuest.QuestId);
            Assert.IsTrue(questManager.StartQuest(ratQuest));
            Assert.AreEqual(QuestState.Active, questManager.GetQuestState(RatQuestId));

            CombatActor playerActor = player.GetComponent<CombatActor>();
            Assert.NotNull(playerActor, "Player should have a CombatActor.");

            GameObject[] rats = Enumerable.Range(1, 10)
                .Select(index => GameObject.Find($"Rat_{index:00}"))
                .ToArray();

            Assert.AreEqual(10, rats.Length);
            Assert.IsTrue(rats.All(rat => rat != null), "Tutorial scene should contain Rat_01 through Rat_10.");

            CorpseLootSource firstCorpseLoot = rats[0].GetComponent<CorpseLootSource>();
            CreatureIdentity firstRatIdentity = rats[0].GetComponent<CreatureIdentity>();
            EnemyInteractionTarget firstEnemyInteraction = rats[0].GetComponent<EnemyInteractionTarget>();
            CombatActor firstRatActor = rats[0].GetComponent<CombatActor>();

            Assert.NotNull(firstRatIdentity, "Rat_01 should derive its display identity from CreatureIdentity.");
            Assert.AreEqual("Rato Presa-da-Bruma 1", firstRatIdentity.DisplayName);
            Assert.NotNull(firstEnemyInteraction, "Rat_01 should expose enemy right-click interaction.");
            Assert.NotNull(firstRatActor, "Rat_01 should have CombatActor for combat range and attack execution.");
            Assert.AreEqual(
                firstRatActor.AttackRange,
                firstEnemyInteraction.GetActionRange(RightClickActionType.Attack),
                0.001f,
                "Enemy interaction attack range should mirror CombatActor instead of owning duplicate range data.");
            Assert.AreEqual(
                $"Corpo de {firstRatIdentity.DisplayName}",
                firstCorpseLoot.DisplayName,
                "Corpse loot display name should be derived from the creature identity.");

            ItemDefinition deterministicLootItem = ResolveFirstConfiguredLootItem(firstCorpseLoot);
            ConfigureDeterministicLoot(firstCorpseLoot, deterministicLootItem);

            foreach (GameObject rat in rats)
            {
                HealthComponent ratHealth = rat.GetComponent<HealthComponent>();
                CombatActor ratActor = rat.GetComponent<CombatActor>();
                QuestKillTarget killTarget = rat.GetComponent<QuestKillTarget>();

                Assert.NotNull(ratHealth, $"{rat.name} should have HealthComponent.");
                Assert.NotNull(ratActor, $"{rat.name} should have CombatActor.");
                Assert.NotNull(killTarget, $"{rat.name} should have QuestKillTarget.");
                Assert.AreEqual("mist_rat", killTarget.TargetId);

                rat.transform.position = player.transform.position + Vector3.right * 0.5f;
                ratHealth.SetMaximumHealth(1, fillToMaximum: true, source: playerActor);
                playerActor.SetTarget(ratActor);

                Assert.IsTrue(playerActor.TryAttackCurrentTarget(), $"Player should be able to attack {rat.name}.");
                yield return null;

                Assert.IsTrue(ratHealth.IsDead, $"{rat.name} should be dead after the test attack.");
                Assert.IsTrue(killTarget.HasReported, $"{rat.name} should report quest kill progress once.");
            }

            QuestLogEntry ratQuestEntry = questManager.GetQuestLogEntries()
                .FirstOrDefault(entry => entry.QuestId == RatQuestId);

            Assert.NotNull(ratQuestEntry, "Quest log should expose the rat quest after it is accepted.");
            Assert.AreEqual(QuestState.Completed, ratQuestEntry.State);
            Assert.IsTrue(ratQuestEntry.Objectives.Count > 0);
            Assert.IsTrue(ratQuestEntry.Objectives.All(objective => objective.IsComplete));

            Assert.NotNull(firstCorpseLoot, "Rat_01 should expose corpse loot.");
            Assert.IsFalse(firstCorpseLoot.WasLooted);
            Assert.IsTrue(firstCorpseLoot.Loot.Any(stack => stack != null && stack.IsValid));

            CorpseLootIndicatorPresenter lootIndicator = rats[0].GetComponent<CorpseLootIndicatorPresenter>();
            Assert.NotNull(lootIndicator, "Rat_01 should expose a corpse loot visual indicator.");
            Assert.IsTrue(lootIndicator.IsMarkerVisible, "Corpse loot marker should be visible while loot is available.");

            int amountBeforeLoot = inventory.GetAmount(deterministicLootItem);
            int claimedStacks = lootService.ClaimAll(firstCorpseLoot, firstCorpseLoot);
            yield return null;

            int amountAfterLoot = inventory.GetAmount(deterministicLootItem);

            Assert.Greater(claimedStacks, 0, "Claiming deterministic corpse loot should transfer at least one stack.");
            Assert.IsTrue(firstCorpseLoot.WasLooted);
            Assert.IsFalse(lootIndicator.IsMarkerVisible, "Corpse loot marker should hide after the corpse is looted.");
            Assert.Greater(amountAfterLoot, amountBeforeLoot);
        }

        [UnityTest]
        public IEnumerator TutorialScene_MistRatAggro_ChasesPlayerInDetectionRange()
        {
            yield return SceneManager.LoadSceneAsync(TutorialScenePath, LoadSceneMode.Single);
            yield return null;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject rat = GameObject.Find("Rat_01");

            Assert.NotNull(player, "Tutorial scene should contain a Player tagged object.");
            Assert.NotNull(rat, "Tutorial scene should contain Rat_01.");

            EnemyCombatController controller = rat.GetComponent<EnemyCombatController>();
            HealthComponent ratHealth = rat.GetComponent<HealthComponent>();
            EnemyCombatVisualPresenter visualPresenter = rat.GetComponent<EnemyCombatVisualPresenter>();

            Assert.NotNull(controller, "Rat_01 should have EnemyCombatController.");
            Assert.NotNull(ratHealth, "Rat_01 should have HealthComponent.");
            Assert.NotNull(visualPresenter, "Rat_01 should have EnemyCombatVisualPresenter.");

            player.transform.position = Vector3.zero;
            DisableOtherRats(rat);
            SetWorldPosition(rat, Vector2.right * 3.6f);
            controller.SetHomePosition(rat.transform.position);
            controller.ClearTarget();

            yield return null;
            yield return null;

            Assert.IsNull(controller.CurrentTarget);
            Assert.AreEqual(EnemyCombatState.Idle, controller.CurrentState);

            SetWorldPosition(rat, Vector2.right * 2f);

            yield return WaitUntilOrTimeout(() => controller.CurrentTarget != null, 1f);

            Assert.IsFalse(ratHealth.IsDead);
            Assert.AreSame(player.GetComponent<HealthComponent>(), controller.CurrentTarget);
            Assert.That(
                controller.CurrentState,
                Is.EqualTo(EnemyCombatState.Chasing).Or.EqualTo(EnemyCombatState.Attacking),
                "Mist rats should actively engage the player when inside detection range.");
            Assert.IsTrue(visualPresenter.IsAggroIndicatorVisible, "Aggro indicator should appear when the mist rat first engages.");
        }

        [UnityTest]
        public IEnumerator TutorialScene_MistRatLeash_ReturnsHomeByWalkingWhenTargetLeavesRange()
        {
            yield return SceneManager.LoadSceneAsync(TutorialScenePath, LoadSceneMode.Single);
            yield return null;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject rat = GameObject.Find("Rat_01");

            Assert.NotNull(player, "Tutorial scene should contain a Player tagged object.");
            Assert.NotNull(rat, "Tutorial scene should contain Rat_01.");

            DisableOtherRats(rat);

            EnemyCombatController controller = rat.GetComponent<EnemyCombatController>();
            HealthComponent playerHealth = player.GetComponent<HealthComponent>();

            Assert.NotNull(controller, "Rat_01 should have EnemyCombatController.");
            Assert.NotNull(playerHealth, "Player should have HealthComponent.");

            Vector2 homePosition = Vector2.zero;
            controller.SetHomePosition(homePosition);
            controller.ClearTarget();
            SetWorldPosition(rat, new Vector2(2.5f, 0f));
            SetWorldPosition(player, new Vector2(2.8f, 0f));

            yield return WaitUntilOrTimeout(() => controller.CurrentTarget == playerHealth, 1f);

            Assert.AreSame(playerHealth, controller.CurrentTarget, "Mist rat should acquire the nearby player before leash is tested.");

            SetWorldPosition(player, new Vector2(12f, 0f));

            yield return WaitUntilOrTimeout(() => controller.CurrentState == EnemyCombatState.Returning, 1.5f);

            Vector2 positionBeforeReturnStep = rat.transform.position;
            float distanceBeforeReturnStep = Vector2.Distance(positionBeforeReturnStep, homePosition);

            yield return new WaitForSeconds(0.35f);

            Vector2 positionAfterReturnStep = rat.transform.position;
            float distanceAfterReturnStep = Vector2.Distance(positionAfterReturnStep, homePosition);
            float movementStep = Vector2.Distance(positionBeforeReturnStep, positionAfterReturnStep);

            Assert.AreEqual(EnemyCombatState.Returning, controller.CurrentState);
            Assert.Less(distanceAfterReturnStep, distanceBeforeReturnStep, "Mist rat should move closer to its home position while returning.");
            Assert.Less(movementStep, 1.5f, "Mist rat should not teleport back to home in a single return step.");
        }

        [UnityTest]
        public IEnumerator TutorialScene_AttackVisualFeedback_DoesNotMoveEnemyRootToSpawn()
        {
            yield return SceneManager.LoadSceneAsync(TutorialScenePath, LoadSceneMode.Single);
            yield return null;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject rat = GameObject.Find("Rat_01");

            Assert.NotNull(player, "Tutorial scene should contain a Player tagged object.");
            Assert.NotNull(rat, "Tutorial scene should contain Rat_01.");

            DisableOtherRats(rat);

            EnemyCombatController controller = rat.GetComponent<EnemyCombatController>();
            CombatActor ratActor = rat.GetComponent<CombatActor>();
            HealthComponent playerHealth = player.GetComponent<HealthComponent>();

            Assert.NotNull(controller, "Rat_01 should have EnemyCombatController.");
            Assert.NotNull(ratActor, "Rat_01 should have CombatActor.");
            Assert.NotNull(playerHealth, "Player should have HealthComponent.");

            controller.enabled = false;
            Vector2 ratPosition = new(2.5f, 0f);
            SetWorldPosition(rat, ratPosition);
            SetWorldPosition(player, new Vector2(3.2f, 0f));

            ratActor.SetTarget(playerHealth);

            Assert.IsTrue(ratActor.TryAttackCurrentTarget(), "Rat should be able to attack the nearby player during visual regression test.");

            yield return null;
            yield return null;

            Assert.Less(
                Vector2.Distance(rat.transform.position, ratPosition),
                0.05f,
                "Attack visual feedback must not move the enemy root transform.");
        }

        [UnityTest]
        public IEnumerator TutorialScene_PlayerCombatPacing_KillsMistRatInFourStarterHits()
        {
            yield return SceneManager.LoadSceneAsync(TutorialScenePath, LoadSceneMode.Single);
            yield return null;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject rat = GameObject.Find("Rat_01");

            Assert.NotNull(player, "Tutorial scene should contain a Player tagged object.");
            Assert.NotNull(rat, "Tutorial scene should contain Rat_01.");

            DisableOtherRats(rat);

            AutoAttackController autoAttack = player.GetComponent<AutoAttackController>();
            CombatActor ratActor = rat.GetComponent<CombatActor>();
            HealthComponent ratHealth = rat.GetComponent<HealthComponent>();
            EnemyCombatController ratController = rat.GetComponent<EnemyCombatController>();

            Assert.NotNull(autoAttack, "Player should have AutoAttackController.");
            Assert.NotNull(ratActor, "Rat_01 should have CombatActor.");
            Assert.NotNull(ratHealth, "Rat_01 should have HealthComponent.");

            ratController.ClearTarget();
            ratHealth.SetMaximumHealth(20, fillToMaximum: true, source: autoAttack);
            player.transform.position = Vector3.zero;
            rat.transform.position = Vector3.right;

            int damageEvents = 0;
            ratHealth.HealthChanged += change =>
            {
                if (change.ChangeType == HealthChangeType.Damage)
                {
                    damageEvents++;
                }
            };

            autoAttack.StartAttacking(ratActor);

            float timeout = Time.time + 6f;
            while (!ratHealth.IsDead && Time.time < timeout)
            {
                yield return null;
            }

            Assert.IsTrue(ratHealth.IsDead);
            Assert.AreEqual(4, damageEvents, "Starter player damage should defeat a 20 HP mist rat in four landed hits.");
        }

        [UnityTest]
        public IEnumerator TutorialScene_MistRatPressure_DamagesButDoesNotOverwhelmPlayer()
        {
            yield return SceneManager.LoadSceneAsync(TutorialScenePath, LoadSceneMode.Single);
            yield return null;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject rat = GameObject.Find("Rat_01");

            Assert.NotNull(player, "Tutorial scene should contain a Player tagged object.");
            Assert.NotNull(rat, "Tutorial scene should contain Rat_01.");

            DisableOtherRats(rat);

            HealthComponent playerHealth = player.GetComponent<HealthComponent>();
            EnemyCombatController ratController = rat.GetComponent<EnemyCombatController>();
            CombatActor ratActor = rat.GetComponent<CombatActor>();
            Rigidbody2D ratBody = rat.GetComponent<Rigidbody2D>();
            RPGProject.Character.CharacterMotor2D ratMotor = rat.GetComponent<RPGProject.Character.CharacterMotor2D>();

            Assert.NotNull(playerHealth, "Player should have HealthComponent.");
            Assert.NotNull(ratController, "Rat_01 should have EnemyCombatController.");
            Assert.NotNull(ratActor, "Rat_01 should have CombatActor.");

            ratController.enabled = false;
            if (ratMotor != null)
            {
                ratMotor.Stop();
                ratMotor.enabled = false;
            }

            playerHealth.SetMaximumHealth(100, fillToMaximum: true, source: ratController);
            player.transform.position = Vector3.zero;
            rat.transform.position = Vector3.right;
            if (ratBody != null)
            {
                ratBody.linearVelocity = Vector2.zero;
                ratBody.position = Vector2.right;
            }

            ratActor.SetTarget(playerHealth);
            int startingHealth = playerHealth.CurrentHealth;

            float timeout = Time.time + 5f;
            while (Time.time < timeout)
            {
                ratActor.TryAttackCurrentTarget();
                yield return null;
            }

            int damageTaken = startingHealth - playerHealth.CurrentHealth;
            Assert.IsFalse(playerHealth.IsDead);
            Assert.That(damageTaken, Is.InRange(2, 10), "A single mist rat should deal readable but non-lethal starter damage in the first few seconds.");
        }

        private static ItemDefinition ResolveFirstConfiguredLootItem(CorpseLootSource corpseLoot)
        {
            Assert.NotNull(corpseLoot, "CorpseLootSource is required to resolve deterministic test loot.");
            LootTableDefinition lootTable = GetPrivateField<LootTableDefinition>(corpseLoot, "lootTable");
            if (lootTable != null)
            {
                foreach (LootDropEntry entry in lootTable.Entries)
                {
                    if (entry != null && entry.IsValid)
                    {
                        return entry.Item;
                    }
                }
            }

            foreach (ItemStackDefinition stack in corpseLoot.Loot)
            {
                if (stack != null && stack.IsValid)
                {
                    return stack.Item;
                }
            }

            Assert.Fail("Expected the tutorial corpse loot source to roll at least one valid item for test setup.");
            return null;
        }

        private static void ConfigureDeterministicLoot(CorpseLootSource corpseLoot, ItemDefinition item)
        {
            Assert.NotNull(corpseLoot);
            Assert.NotNull(item);

            corpseLoot.ResetLootRoll();
            SetPrivateField(corpseLoot, "lootTable", null);
            SetPrivateField(corpseLoot, "loot", new[] { new ItemStackDefinition(item, 1) });
            corpseLoot.ResetLootRoll();
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            System.Reflection.FieldInfo field = target.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            Assert.NotNull(field, $"Expected private field '{fieldName}' on {target.GetType().Name}.");
            field.SetValue(target, value);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            System.Reflection.FieldInfo field = target.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            Assert.NotNull(field, $"Expected private field '{fieldName}' on {target.GetType().Name}.");
            return (T)field.GetValue(target);
        }

        private static void DisableOtherRats(GameObject activeRat)
        {
            foreach (GameObject rat in Enumerable.Range(1, 10).Select(index => GameObject.Find($"Rat_{index:00}")))
            {
                if (rat != null && rat != activeRat)
                {
                    rat.SetActive(false);
                }
            }
        }

        private static void SetWorldPosition(GameObject target, Vector2 position)
        {
            target.transform.position = position;
            if (target.TryGetComponent(out Rigidbody2D body))
            {
                body.linearVelocity = Vector2.zero;
                body.position = position;
            }
        }

        private static IEnumerator WaitUntilOrTimeout(System.Func<bool> predicate, float timeoutSeconds)
        {
            float timeout = Time.time + timeoutSeconds;
            while (!predicate() && Time.time < timeout)
            {
                yield return null;
            }
        }
    }
}
