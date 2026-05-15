using System;
using System.IO;
using RPGProject.Character;
using RPGProject.Gameplay;
using RPGProject.Inputs;
using RPGProject.Systems;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class TutorialQuestSceneBuilder
{
    private const string ScenePath = "Assets/Game/Scenes/TutorialScene.unity";
    private const string AssetFolder = "Assets/Game/ScriptableObjects/Tutorial";
    private const string ArtFolder = "Assets/Game/Art/Tutorial";

    [MenuItem("RPG Project/Build Tutorial Quest Scene")]
    public static void Build()
    {
        EnsureProjectFolders();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "TutorialScene";

        Sprite squareSprite = CreateSpriteAsset($"{ArtFolder}/tutorial_square.png", Color.white);
        ItemDefinition tutorialKey = CreateItem(
            "OldChapelKey",
            "Chave da Capela Antiga",
            "Uma chave de ferro entregue pela guardia Mira. Abre a capela fechada no limite da aldeia.",
            ItemCategory.Key,
            false,
            1);
        ItemDefinition ratPelt = CreateItem(
            "MistRatPelt",
            "Pele de Rato da Nevoa",
            "Uma pequena pele manchada pela nevoa. Serve como loot de teste para validar corpos saqueaveis.",
            ItemCategory.Generic,
            true,
            99);
        CharacterMovementSettings movementSettings = CreateMovementSettings("PlayerTutorialMovement", 4.5f);
        CharacterMovementSettings ratMovementSettings = CreateMovementSettings("MistRatMovement", 2.2f);
        CombatStatsDefinition playerStats = CreateCombatStats("PlayerTutorialStats", 4, 1);
        CombatStatsDefinition ratStats = CreateCombatStats("MistRatStats", 2, 1);
        DamageResolver damageResolver = CreateDamageResolver("PlayerBasicDamageResolver");
        CombatAttackSettings attackSettings = CreateAttackSettings("PlayerTutorialAttack", 1.5f, 3, 1f, damageResolver);
        CombatAttackSettings ratAttackSettings = CreateAttackSettings("MistRatAttack", 1.05f, 1, 0.65f, damageResolver);
        EnemyCombatBehaviorSettings ratBehaviorSettings = CreateEnemyBehavior("MistRatBehavior", EnemyAttackMode.Melee, EnemyMovementPolicy.ChaseTarget, 4.5f);
        LootTableDefinition ratLootTable = CreateMistRatLootTable(ratPelt);

        QuestDefinition talkQuest = CreateQuest(
            "tutorial_find_mira",
            "O primeiro sinal",
            "Lio pediu para voce procurar Mira, a guardia da aldeia. Ela esta perto do campo onde a nevoa apareceu.",
            "Mira vai explicar o que esta acontecendo.",
            new[] { CreateObjective("talk_mira", QuestObjectiveType.Talk, "mira_guard", "Fale com Mira", 1) },
            Array.Empty<ItemStackDefinition>());

        QuestDefinition ratQuest = CreateQuest(
            "tutorial_clear_mist_rats",
            "Ratos na nevoa",
            "Mira quer confirmar se a nevoa esta enlouquecendo os animais. Derrote 10 ratos antes que cheguem aos celeiros.",
            "Chave da Capela Antiga",
            new[] { CreateObjective("kill_mist_rats", QuestObjectiveType.Kill, "mist_rat", "Derrote ratos da nevoa", 10) },
            new[] { CreateStack(tutorialKey, 1) });

        DialogueDefinition introDialogue = CreateDialogue(
            "dialogue_lio_intro",
            "Lio",
            true,
            new[]
            {
                CreateLine("Lio", "Voce acordou bem na hora errada. A nevoa desceu do bosque antes do amanhecer."),
                CreateLine("Lio", "Mira, nossa guardia, esta perto dos campos. Fale com ela antes que os animais avancem."),
            },
            Array.Empty<DialogueCondition>(),
            talkQuest,
            true);

        DialogueDefinition guideOfferDialogue = CreateDialogue(
            "dialogue_mira_offer_rat_hunt",
            "Mira",
            true,
            new[] { CreateLine("Mira", "Entao Lio te mandou. Bom. A nevoa deixou os ratos agressivos. Derrote 10 deles e volte para mim.") },
            new[] { CreateCondition(talkQuest, QuestState.Completed) },
            ratQuest,
            true);

        DialogueDefinition guideActiveDialogue = CreateDialogue(
            "dialogue_mira_active_rat_hunt",
            "Mira",
            true,
            new[] { CreateLine("Mira", "Ainda consigo ouvir arranhoes no campo. Volte quando tiver derrotado os 10 ratos.") },
            Array.Empty<DialogueCondition>(),
            null,
            false);

        DialogueDefinition guideCompletedDialogue = CreateDialogue(
            "dialogue_mira_completed_rat_hunt",
            "Mira",
            true,
            new[] { CreateLine("Mira", "Voce se saiu melhor do que eu esperava. Pegue esta chave. A capela antiga talvez explique a origem da nevoa.") },
            Array.Empty<DialogueCondition>(),
            null,
            false);

        DialogueDefinition guideRewardClaimedDialogue = CreateDialogue(
            "dialogue_mira_reward_claimed",
            "Mira",
            true,
            new[] { CreateLine("Mira", "A chave esta com voce. Abra o inventario com I e confira antes de seguir para a capela.") },
            Array.Empty<DialogueCondition>(),
            null,
            false);

        CreateCamera();
        GameObject managers = CreateManagers();
        BuildWorld(squareSprite, movementSettings, ratMovementSettings, playerStats, ratStats, attackSettings, ratAttackSettings, ratBehaviorSettings, ratLootTable, introDialogue, talkQuest, guideOfferDialogue, ratQuest, guideActiveDialogue, guideCompletedDialogue, guideRewardClaimedDialogue);
        BuildUi(managers);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
    }

    private static void EnsureProjectFolders()
    {
        EnsureFolder("Assets/Game");
        EnsureFolder("Assets/Game/Scenes");
        EnsureFolder("Assets/Game/ScriptableObjects");
        EnsureFolder(AssetFolder);
        EnsureFolder("Assets/Game/Art");
        EnsureFolder(ArtFolder);
    }

    private static void BuildWorld(
        Sprite squareSprite,
        CharacterMovementSettings movementSettings,
        CharacterMovementSettings ratMovementSettings,
        CombatStatsDefinition playerStats,
        CombatStatsDefinition ratStats,
        CombatAttackSettings attackSettings,
        CombatAttackSettings ratAttackSettings,
        EnemyCombatBehaviorSettings ratBehaviorSettings,
        LootTableDefinition ratLootTable,
        DialogueDefinition introDialogue,
        QuestDefinition talkQuest,
        DialogueDefinition guideOfferDialogue,
        QuestDefinition ratQuest,
        DialogueDefinition guideActiveDialogue,
        DialogueDefinition guideCompletedDialogue,
        DialogueDefinition guideRewardClaimedDialogue)
    {
        CreateGroundMarker("Caminho", new Vector2(0f, -2.1f), new Vector2(9.5f, 3.7f), new Color(0.18f, 0.23f, 0.20f, 1f), squareSprite);
        CreateGroundMarker("Area_NPCs", new Vector2(0f, 1.25f), new Vector2(7.8f, 1.7f), new Color(0.17f, 0.20f, 0.25f, 1f), squareSprite);

        InputActionAsset actionsAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        GameObject player = CreateActor("Player", new Vector2(-5f, -2f), squareSprite, new Color(0.24f, 0.63f, 1f, 1f), new Vector2(0.65f, 0.85f));
        player.tag = "Player";
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        PlayerInputReader inputReader = player.AddComponent<PlayerInputReader>();
        player.AddComponent<CharacterMotor2D>();
        HealthComponent playerHealth = player.AddComponent<HealthComponent>();
        CombatActor playerCombatActor = player.AddComponent<CombatActor>();
        player.AddComponent<HitFlashPresenter>();
        player.AddComponent<PlayerMovementController>();
        AutoAttackController autoAttack = player.AddComponent<AutoAttackController>();
        player.AddComponent<PlayerActionController>();
        SetObject(inputReader, "actionsAsset", actionsAsset);
        SetObject(player.GetComponent<CharacterMotor2D>(), "movementSettings", movementSettings);
        SetObject(playerCombatActor, "baseStats", playerStats);
        SetObject(playerCombatActor, "attackSettings", attackSettings);
        SetObject(autoAttack, "combatActor", playerCombatActor);
        SetInt(playerHealth, "maximumHealth", 100);
        SetInt(playerHealth, "currentHealth", 100);
        AddWorldLabel(player.transform, "Player", Color.white, new Vector3(0f, 0.75f, 0f));

        GameObject firstNpc = CreateActor("NPC_Lio", new Vector2(-2.8f, 1.3f), squareSprite, new Color(0.95f, 0.72f, 0.32f, 1f), new Vector2(0.75f, 0.95f));
        NPCInteractionTarget firstTarget = firstNpc.AddComponent<NPCInteractionTarget>();
        SetString(firstTarget, "displayName", "Lio");
        SetObject(firstTarget, "dialogueDefinition", introDialogue);
        SetObject(firstTarget, "questToOffer", talkQuest);
        SetBool(firstTarget, "startQuestAfterDialogue", true);
        firstNpc.AddComponent<NPCQuestIndicator>();
        AddWorldLabel(firstNpc.transform, "Lio\n1) Fale aqui", Color.white, new Vector3(0f, 0.85f, 0f));

        GameObject guideNpc = CreateActor("NPC_Mira", new Vector2(2.8f, 1.3f), squareSprite, new Color(0.36f, 0.88f, 0.58f, 1f), new Vector2(0.75f, 0.95f));
        NPCInteractionTarget guideTarget = guideNpc.AddComponent<NPCInteractionTarget>();
        SetString(guideTarget, "displayName", "Mira");
        SetString(guideTarget, "talkObjectiveTargetId", "mira_guard");
        SetObject(guideTarget, "dialogueDefinition", guideOfferDialogue);
        SetObject(guideTarget, "questToOffer", ratQuest);
        SetBool(guideTarget, "startQuestAfterDialogue", true);
        SetObject(guideTarget, "activeQuestDialogueDefinition", guideActiveDialogue);
        SetObject(guideTarget, "completedQuestDialogueDefinition", guideCompletedDialogue);
        SetObject(guideTarget, "rewardClaimedQuestDialogueDefinition", guideRewardClaimedDialogue);
        guideNpc.AddComponent<NPCQuestIndicator>();
        AddWorldLabel(guideNpc.transform, "Mira\n2) Depois fale aqui", Color.white, new Vector3(0f, 0.85f, 0f));

        for (int i = 0; i < 10; i++)
        {
            float angle = i / 10f * Mathf.PI * 2f;
            Vector2 position = new(Mathf.Cos(angle) * 3.4f, Mathf.Sin(angle) * 2.0f - 1.9f);
            GameObject rat = CreateActor($"Rat_{i + 1:00}", position, squareSprite, new Color(0.60f, 0.50f, 0.43f, 1f), new Vector2(0.45f, 0.32f));
            Rigidbody2D ratBody = rat.AddComponent<Rigidbody2D>();
            ratBody.gravityScale = 0f;
            ratBody.freezeRotation = true;
            ratBody.interpolation = RigidbodyInterpolation2D.Interpolate;
            CharacterMotor2D ratMotor = rat.AddComponent<CharacterMotor2D>();
            HealthComponent ratHealth = rat.AddComponent<HealthComponent>();
            CombatActor ratCombatActor = rat.AddComponent<CombatActor>();
            rat.AddComponent<HitFlashPresenter>();
            rat.AddComponent<CorpseDecayController>();
            EnemyCombatController ratCombat = rat.AddComponent<EnemyCombatController>();
            CorpseLootSource corpseLoot = rat.AddComponent<CorpseLootSource>();
            SetObject(ratMotor, "movementSettings", ratMovementSettings);
            SetObject(ratCombatActor, "baseStats", ratStats);
            SetObject(ratCombatActor, "attackSettings", ratAttackSettings);
            SetBool(ratCombatActor, "selectTargets", false);
            SetObject(ratCombat, "behaviorSettings", ratBehaviorSettings);
            SetObject(ratCombat, "combatActor", ratCombatActor);
            SetInt(ratHealth, "maximumHealth", 20);
            SetInt(ratHealth, "currentHealth", 20);
            SetString(corpseLoot, "displayName", $"Corpo de Rato da Nevoa {i + 1}");
            SetObject(corpseLoot, "lootTable", ratLootTable);
            QuestKillTarget killTarget = rat.AddComponent<QuestKillTarget>();
            SetString(killTarget, "targetId", "mist_rat");
            EnemyInteractionTarget enemy = rat.AddComponent<EnemyInteractionTarget>();
            SetString(enemy, "displayName", $"Rato da Nevoa {i + 1}");
            SetObject(enemy, "questKillTarget", killTarget);
            SetBool(enemy, "deactivateOnDefeat", false);
            AddWorldLabel(rat.transform, $"Rato {i + 1}", Color.white, new Vector3(0f, 0.42f, 0f), 2.4f);
        }
    }

    private static GameObject CreateManagers()
    {
        GameObject managers = new("Managers");
        GameObject dialogueSystem = CreateSystemObject(managers.transform, "Dialogue System");
        GameObject inventorySystem = CreateSystemObject(managers.transform, "Inventory System");
        GameObject lootSystem = CreateSystemObject(managers.transform, "Loot System");
        GameObject gameplaySystem = CreateSystemObject(managers.transform, "Gameplay System");
        GameObject saveSystem = CreateSystemObject(managers.transform, "Save System");

        dialogueSystem.AddComponent<DialogueManager>();
        dialogueSystem.AddComponent<QuestManager>();
        inventorySystem.AddComponent<InventoryManager>();
        inventorySystem.AddComponent<InventoryWorldDropper>();
        lootSystem.AddComponent<LootService>();
        gameplaySystem.AddComponent<GameplayInputBlocker>();
        GameProgressSaveManager saveManager = saveSystem.AddComponent<GameProgressSaveManager>();
        SetString(saveManager, "playerPrefsKey", "RPGProject.TutorialQuestScene.Progress");
        SetBool(saveManager, "loadOnStart", false);
        SetBool(saveManager, "saveOnQuit", false);
        return managers;
    }

    private static GameObject CreateSystemObject(Transform parent, string name)
    {
        GameObject systemObject = new(name);
        systemObject.transform.SetParent(parent, false);
        return systemObject;
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6.5f;
        camera.backgroundColor = new Color(0.10f, 0.13f, 0.14f, 1f);
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static void BuildUi(GameObject managers)
    {
        GameObject uiManager = new("UIManager");
        GameObject gameplayUiSystem = CreateSystemObject(uiManager.transform, "Gameplay UI System");
        GameObject dialogueUiSystem = CreateSystemObject(uiManager.transform, "Dialogue UI System");
        GameObject questLogUiSystem = CreateSystemObject(uiManager.transform, "Quest Log UI System");
        GameObject inventoryUiSystem = CreateSystemObject(uiManager.transform, "Inventory UI System");
        GameObject lootUiSystem = CreateSystemObject(uiManager.transform, "Loot UI System");
        GameObject combatUiSystem = CreateSystemObject(uiManager.transform, "Combat UI System");

        GameplayUIManager gameplayUi = gameplayUiSystem.AddComponent<GameplayUIManager>();
        GlobalFeedbackUIController feedbackUi = gameplayUiSystem.AddComponent<GlobalFeedbackUIController>();
        DialogueUIController dialogueUi = dialogueUiSystem.AddComponent<DialogueUIController>();
        QuestLogUIController questLogUi = questLogUiSystem.AddComponent<QuestLogUIController>();
        InventoryUIController inventoryUi = inventoryUiSystem.AddComponent<InventoryUIController>();
        LootUIController lootUi = lootUiSystem.AddComponent<LootUIController>();
        CombatFollowToggleUIController followToggleUi = combatUiSystem.AddComponent<CombatFollowToggleUIController>();
        combatUiSystem.AddComponent<CombatWorldUIController>();
        SetObject(gameplayUi, "feedbackController", feedbackUi);

        GameObject canvasObject = new("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() == null)
        {
            _ = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        GameObject hint = CreateUIPanel(canvasObject.transform, "TutorialHint", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -56f), new Vector2(-420f, 92f), new Color(0.04f, 0.05f, 0.06f, 0.82f));
        TMP_Text hintText = CreateUIText(hint.transform, "HintText", "Vale de Lumen: botao direito interage/ataca. Espaco ou Enter avanca dialogo. J abre quests. I abre inventario.", 18, TextAlignmentOptions.Center, Color.white);
        Stretch(hintText.rectTransform);

        GameObject dialoguePanel = CreateUIPanel(canvasObject.transform, "DialoguePanel", new Vector2(0.08f, 0f), new Vector2(0.92f, 0f), new Vector2(0f, 112f), new Vector2(0f, 190f), new Color(0.05f, 0.06f, 0.08f, 0.95f));
        TMP_Text speakerText = CreateUIText(dialoguePanel.transform, "SpeakerNameText", "Speaker", 24, TextAlignmentOptions.Left, new Color(0.98f, 0.85f, 0.52f, 1f));
        SetRect(speakerText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(110f, -34f), new Vector2(-110f, 34f));
        TMP_Text dialogueText = CreateUIText(dialoguePanel.transform, "DialogueText", "Dialogue", 22, TextAlignmentOptions.TopLeft, Color.white);
        SetRect(dialogueText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(110f, 54f), new Vector2(-110f, -54f));
        TMP_Text progressText = CreateUIText(dialoguePanel.transform, "ProgressText", "1/1", 16, TextAlignmentOptions.Right, new Color(0.75f, 0.80f, 0.88f, 1f));
        SetRect(progressText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-170f, -34f), new Vector2(78f, 30f));
        Image leftPortrait = CreateUIImage(dialoguePanel.transform, "LeftPortrait", new Color(0.36f, 0.58f, 0.94f, 1f));
        SetRect(leftPortrait.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(52f, 0f), new Vector2(72f, 96f));
        Image rightPortrait = CreateUIImage(dialoguePanel.transform, "RightPortrait", new Color(0.94f, 0.68f, 0.35f, 1f));
        SetRect(rightPortrait.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-52f, 0f), new Vector2(72f, 96f));
        Button nextButton = CreateUIButton(dialoguePanel.transform, "NextButton", "Proximo", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-178f, 32f), new Vector2(136f, 42f));
        Button closeButton = CreateUIButton(dialoguePanel.transform, "CloseButton", "Fechar", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-34f, 32f), new Vector2(112f, 42f));
        SetObject(dialogueUi, "dialoguePanel", dialoguePanel);
        SetObject(dialogueUi, "speakerNameText", speakerText);
        SetObject(dialogueUi, "dialogueText", dialogueText);
        SetObject(dialogueUi, "progressText", progressText);
        SetObject(dialogueUi, "nextButton", nextButton);
        SetObject(dialogueUi, "closeButton", closeButton);
        SetObject(dialogueUi, "leftPortraitImage", leftPortrait);
        SetObject(dialogueUi, "rightPortraitImage", rightPortrait);
        SetBool(dialogueUi, "useTypewriter", false);
        dialoguePanel.SetActive(false);

        GameObject feedbackRoot = CreateUIPanel(canvasObject.transform, "GlobalFeedback", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -118f), new Vector2(520f, 54f), new Color(0.24f, 0.21f, 0.5f, 0.94f));
        CanvasGroup feedbackGroup = feedbackRoot.AddComponent<CanvasGroup>();
        TMP_Text feedbackText = CreateUIText(feedbackRoot.transform, "MessageText", "Feedback", 18, TextAlignmentOptions.Center, Color.white);
        Stretch(feedbackText.rectTransform);
        SetObject(feedbackUi, "feedbackRoot", feedbackRoot);
        SetObject(feedbackUi, "canvasGroup", feedbackGroup);
        SetObject(feedbackUi, "backgroundImage", feedbackRoot.GetComponent<Image>());
        SetObject(feedbackUi, "messageText", feedbackText);
        feedbackRoot.SetActive(false);

        Toggle followToggle = CreateUIToggle(canvasObject.transform, "CombatFollowToggle", "Follow: ON", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-98f, -36f), new Vector2(168f, 42f));
        SetObject(followToggleUi, "autoAttackController", GameObject.FindGameObjectWithTag("Player").GetComponent<AutoAttackController>());
        SetObject(followToggleUi, "followToggle", followToggle);
        SetObject(followToggleUi, "labelText", followToggle.GetComponentInChildren<TMP_Text>());

        Button questToggle = CreateUIButton(canvasObject.transform, "QuestLogToggleButton", "Quests", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(76f, -36f), new Vector2(132f, 42f));
        GameObject questPanel = CreateUIPanel(canvasObject.transform, "QuestLogPanel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(202f, -178f), new Vector2(360f, 250f), new Color(0.05f, 0.06f, 0.08f, 0.94f));
        TMP_Text questTitle = CreateUIText(questPanel.transform, "TitleText", "Quest Log", 22, TextAlignmentOptions.TopLeft, new Color(0.98f, 0.85f, 0.52f, 1f));
        SetRect(questTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -32f), new Vector2(-18f, 30f));
        TMP_Text questContent = CreateUIText(questPanel.transform, "ContentText", "Nenhuma quest ativa.", 17, TextAlignmentOptions.TopLeft, Color.white);
        SetRect(questContent.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(18f, 16f), new Vector2(-18f, -64f));
        SetObject(questLogUi, "questLogPanel", questPanel);
        SetObject(questLogUi, "toggleButton", questToggle);
        SetObject(questLogUi, "toggleButtonText", questToggle.GetComponentInChildren<TMP_Text>());
        SetObject(questLogUi, "titleText", questTitle);
        SetObject(questLogUi, "contentText", questContent);
        questPanel.SetActive(false);

        Button inventoryToggle = CreateUIButton(canvasObject.transform, "InventoryToggleButton", "Inventario", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(218f, -36f), new Vector2(150f, 42f));
        GameObject inventoryPanel = CreateUIPanel(canvasObject.transform, "InventoryPanel", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-260f, -190f), new Vector2(440f, 280f), new Color(0.05f, 0.06f, 0.08f, 0.94f));
        TMP_Text inventoryTitle = CreateUIText(inventoryPanel.transform, "TitleText", "Inventario", 22, TextAlignmentOptions.TopLeft, new Color(0.98f, 0.85f, 0.52f, 1f));
        SetRect(inventoryTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -32f), new Vector2(-18f, 30f));
        TMP_Text inventoryEmpty = CreateUIText(inventoryPanel.transform, "ContentText", "Inventario vazio.", 18, TextAlignmentOptions.TopLeft, Color.white);
        SetRect(inventoryEmpty.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(20f, 18f), new Vector2(-20f, -72f));
        TMP_Text inventoryDetails = CreateUIText(inventoryPanel.transform, "DetailsText", "Selecione um item para ver detalhes.", 15, TextAlignmentOptions.BottomLeft, new Color(0.82f, 0.88f, 0.92f, 1f));
        SetRect(inventoryDetails.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(18f, 16f), new Vector2(-18f, 72f));
        Button useButton = CreateUIButton(inventoryPanel.transform, "UseButton", "Usar", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-70f, 36f), new Vector2(112f, 38f));
        SetObject(inventoryUi, "inventoryPanel", inventoryPanel);
        SetObject(inventoryUi, "toggleButton", inventoryToggle);
        SetObject(inventoryUi, "toggleButtonText", inventoryToggle.GetComponentInChildren<TMP_Text>());
        SetObject(inventoryUi, "titleText", inventoryTitle);
        SetObject(inventoryUi, "contentText", inventoryEmpty);
        SetObject(inventoryUi, "detailsText", inventoryDetails);
        SetObject(inventoryUi, "useButton", useButton);
        SetObject(inventoryUi, "useButtonText", useButton.GetComponentInChildren<TMP_Text>());
        inventoryPanel.SetActive(false);

        GameObject lootPanel = CreateUIPanel(canvasObject.transform, "LootPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -16f), new Vector2(360f, 250f), new Color(0.05f, 0.06f, 0.08f, 0.96f));
        TMP_Text lootTitle = CreateUIText(lootPanel.transform, "TitleText", "Loot", 22, TextAlignmentOptions.TopLeft, new Color(0.98f, 0.85f, 0.52f, 1f));
        SetRect(lootTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(18f, -32f), new Vector2(-18f, 30f));
        TMP_Text lootContent = CreateUIText(lootPanel.transform, "ContentText", "Nada para pegar.", 18, TextAlignmentOptions.TopLeft, Color.white);
        SetRect(lootContent.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(20f, 64f), new Vector2(-20f, -70f));
        Button takeAllButton = CreateUIButton(lootPanel.transform, "TakeAllButton", "Pegar Tudo", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-100f, 34f), new Vector2(136f, 38f));
        Button lootCloseButton = CreateUIButton(lootPanel.transform, "CloseButton", "Fechar", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(82f, 34f), new Vector2(112f, 38f));
        SetObject(lootUi, "lootPanel", lootPanel);
        SetObject(lootUi, "titleText", lootTitle);
        SetObject(lootUi, "contentText", lootContent);
        SetObject(lootUi, "takeAllButton", takeAllButton);
        SetObject(lootUi, "closeButton", lootCloseButton);
        lootPanel.SetActive(false);
    }

    private static Sprite CreateSpriteAsset(string path, Color color)
    {
        Texture2D texture = new(16, 16, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[16 * 16];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        texture.SetPixels(pixels);
        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 16f;
        importer.filterMode = FilterMode.Point;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static ItemDefinition CreateItem(string itemId, string displayName, string description, ItemCategory category, bool stackable, int maxStack)
    {
        ItemDefinition item = GetOrCreateAsset<ItemDefinition>($"{AssetFolder}/{itemId}.asset");
        SetString(item, "itemId", itemId);
        SetString(item, "displayName", displayName);
        SetString(item, "description", description);
        SetEnum(item, "category", category);
        SetBool(item, "isStackable", stackable);
        SetInt(item, "maxStackSize", maxStack);
        EditorUtility.SetDirty(item);
        return item;
    }

    private static CharacterMovementSettings CreateMovementSettings(string assetName, float speed)
    {
        CharacterMovementSettings settings = GetOrCreateAsset<CharacterMovementSettings>($"{AssetFolder}/{assetName}.asset");
        SetFloat(settings, "moveSpeed", speed);
        EditorUtility.SetDirty(settings);
        return settings;
    }

    private static DamageResolver CreateDamageResolver(string assetName)
    {
        DamageResolver resolver = GetOrCreateAsset<BasicDamageResolver>($"{AssetFolder}/{assetName}.asset");
        EditorUtility.SetDirty(resolver);
        return resolver;
    }

    private static CombatStatsDefinition CreateCombatStats(string assetName, int attack, int defense)
    {
        CombatStatsDefinition stats = GetOrCreateAsset<CombatStatsDefinition>($"{AssetFolder}/{assetName}.asset");
        SetInt(stats, "attack", attack);
        SetInt(stats, "defense", defense);
        EditorUtility.SetDirty(stats);
        return stats;
    }

    private static CombatAttackSettings CreateAttackSettings(string assetName, float attackRange, int damage, float attacksPerSecond, DamageResolver damageResolver)
    {
        CombatAttackSettings settings = GetOrCreateAsset<CombatAttackSettings>($"{AssetFolder}/{assetName}.asset");
        SetFloat(settings, "attackRange", attackRange);
        SetInt(settings, "damage", damage);
        SetFloat(settings, "attacksPerSecond", attacksPerSecond);
        SetObject(settings, "damageResolver", damageResolver);
        EditorUtility.SetDirty(settings);
        return settings;
    }

    private static EnemyCombatBehaviorSettings CreateEnemyBehavior(string assetName, EnemyAttackMode attackMode, EnemyMovementPolicy movementPolicy, float detectionRange)
    {
        EnemyCombatBehaviorSettings settings = GetOrCreateAsset<EnemyCombatBehaviorSettings>($"{AssetFolder}/{assetName}.asset");
        SetEnum(settings, "attackMode", attackMode);
        SetEnum(settings, "movementPolicy", movementPolicy);
        SetFloat(settings, "detectionRange", detectionRange);
        EditorUtility.SetDirty(settings);
        return settings;
    }

    private static LootTableDefinition CreateMistRatLootTable(ItemDefinition ratPelt)
    {
        LootTableDefinition table = GetOrCreateAsset<LootTableDefinition>($"{AssetFolder}/MistRatLootTable.asset");
        SetSerialized(table, "entries", property =>
        {
            property.arraySize = 1;
            SerializedProperty entry = property.GetArrayElementAtIndex(0);
            entry.FindPropertyRelative("item").objectReferenceValue = ratPelt;
            entry.FindPropertyRelative("rarity").enumValueIndex = Convert.ToInt32(LootRarity.Common);
            entry.FindPropertyRelative("dropChance").floatValue = 0.65f;
            entry.FindPropertyRelative("minAmount").intValue = 1;
            entry.FindPropertyRelative("maxAmount").intValue = 2;
        });
        EditorUtility.SetDirty(table);
        return table;
    }

    private static QuestDefinition CreateQuest(string questId, string title, string description, string rewardDescription, QuestObjectiveDefinition[] objectives, ItemStackDefinition[] rewardItems)
    {
        QuestDefinition quest = GetOrCreateAsset<QuestDefinition>($"{AssetFolder}/{questId}.asset");
        SetString(quest, "questId", questId);
        SetString(quest, "title", title);
        SetString(quest, "description", description);
        SetString(quest, "rewardDescription", rewardDescription);
        SetArray(quest, "objectives", objectives);
        SetArray(quest, "rewardItems", rewardItems);
        EditorUtility.SetDirty(quest);
        return quest;
    }

    private static DialogueDefinition CreateDialogue(string id, string displayName, bool repeatable, DialogueLine[] lines, DialogueCondition[] conditions, QuestDefinition questToStart, bool startQuestAfterDialogue)
    {
        DialogueDefinition dialogue = GetOrCreateAsset<DialogueDefinition>($"{AssetFolder}/{id}.asset");
        SetString(dialogue, "dialogueId", id);
        SetString(dialogue, "displayName", displayName);
        SetBool(dialogue, "isRepeatable", repeatable);
        SetArray(dialogue, "lines", lines);
        SetArray(dialogue, "conditions", conditions);
        SetArray(dialogue, "choices", Array.Empty<DialogueChoice>());
        SetObject(dialogue, "questToStart", questToStart);
        SetBool(dialogue, "startQuestAfterDialogue", startQuestAfterDialogue);
        EditorUtility.SetDirty(dialogue);
        return dialogue;
    }

    private static DialogueLine CreateLine(string speaker, string text)
    {
        DialogueLine line = new();
        SetField(line, "speakerName", speaker);
        SetField(line, "text", text);
        SetField(line, "speakerSide", DialogueSpeakerSide.Left);
        return line;
    }

    private static DialogueCondition CreateCondition(QuestDefinition quest, QuestState state)
    {
        DialogueCondition condition = new();
        SetField(condition, "quest", quest);
        SetField(condition, "conditionType", DialogueConditionType.QuestState);
        SetField(condition, "requiredState", state);
        return condition;
    }

    private static QuestObjectiveDefinition CreateObjective(string objectiveId, QuestObjectiveType type, string targetId, string description, int requiredAmount)
    {
        QuestObjectiveDefinition objective = new();
        SetField(objective, "objectiveId", objectiveId);
        SetField(objective, "objectiveType", type);
        SetField(objective, "targetId", targetId);
        SetField(objective, "description", description);
        SetField(objective, "requiredAmount", requiredAmount);
        return objective;
    }

    private static ItemStackDefinition CreateStack(ItemDefinition item, int amount)
    {
        ItemStackDefinition stack = new();
        SetField(stack, "item", item);
        SetField(stack, "amount", amount);
        return stack;
    }

    private static GameObject CreateActor(string name, Vector2 position, Sprite sprite, Color color, Vector2 colliderSize)
    {
        GameObject actor = new(name, typeof(SpriteRenderer), typeof(BoxCollider2D));
        actor.transform.position = position;
        SpriteRenderer renderer = actor.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = 10;

        BoxCollider2D collider = actor.GetComponent<BoxCollider2D>();
        collider.size = colliderSize;
        collider.isTrigger = true;
        return actor;
    }

    private static void CreateGroundMarker(string name, Vector2 position, Vector2 scale, Color color, Sprite sprite)
    {
        GameObject marker = new(name, typeof(SpriteRenderer));
        marker.transform.position = position;
        marker.transform.localScale = scale;
        SpriteRenderer renderer = marker.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = -10;
    }

    private static void AddWorldLabel(Transform parent, string text, Color color, Vector3 offset, float fontSize = 3.2f)
    {
        GameObject label = new("Label", typeof(TextMeshPro));
        label.transform.SetParent(parent, false);
        label.transform.localPosition = offset;
        TextMeshPro tmp = label.GetComponent<TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color;
        tmp.sortingOrder = 30;
    }

    private static GameObject CreateUIPanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject panel = new(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private static TMP_Text CreateUIText(Transform parent, string name, string text, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObject = new(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TMP_Text tmp = textObject.GetComponent<TMP_Text>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        return tmp;
    }

    private static Image CreateUIImage(Transform parent, string name, Color color)
    {
        GameObject imageObject = new(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static Button CreateUIButton(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject buttonObject = new(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        buttonObject.GetComponent<Image>().color = new Color(0.18f, 0.22f, 0.27f, 1f);

        TMP_Text label = CreateUIText(buttonObject.transform, "Text", text, 16, TextAlignmentOptions.Center, Color.white);
        Stretch(label.rectTransform);
        return buttonObject.GetComponent<Button>();
    }

    private static Toggle CreateUIToggle(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject toggleObject = new(name, typeof(RectTransform), typeof(Image), typeof(Toggle));
        toggleObject.transform.SetParent(parent, false);
        RectTransform rect = toggleObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image background = toggleObject.GetComponent<Image>();
        background.color = new Color(0.18f, 0.22f, 0.27f, 1f);

        Image checkmark = CreateUIImage(toggleObject.transform, "Checkmark", new Color(0.32f, 0.76f, 0.38f, 1f));
        SetRect(checkmark.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(22f, 0f), new Vector2(18f, 18f));

        TMP_Text label = CreateUIText(toggleObject.transform, "Text", text, 15, TextAlignmentOptions.MidlineLeft, Color.white);
        SetRect(label.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(28f, 0f), new Vector2(-38f, 0f));

        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.targetGraphic = background;
        toggle.graphic = checkmark;
        toggle.isOn = true;
        return toggle;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static T GetOrCreateAsset<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null)
        {
            return asset;
        }

        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void EnsureFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folder = Path.GetFileName(path);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folder);
    }

    private static void SetSerialized(UnityEngine.Object target, string propertyName, Action<SerializedProperty> setter)
    {
        SerializedObject serialized = new(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
        {
            throw new InvalidOperationException($"Property {propertyName} not found on {target}.");
        }

        setter(property);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetString(UnityEngine.Object target, string propertyName, string value)
    {
        SetSerialized(target, propertyName, property => property.stringValue = value);
    }

    private static void SetBool(UnityEngine.Object target, string propertyName, bool value)
    {
        SetSerialized(target, propertyName, property => property.boolValue = value);
    }

    private static void SetInt(UnityEngine.Object target, string propertyName, int value)
    {
        SetSerialized(target, propertyName, property => property.intValue = value);
    }

    private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
    {
        SetSerialized(target, propertyName, property => property.floatValue = value);
    }

    private static void SetEnum<T>(UnityEngine.Object target, string propertyName, T value) where T : Enum
    {
        SetSerialized(target, propertyName, property => property.enumValueIndex = Convert.ToInt32(value));
    }

    private static void SetObject(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
    {
        SetSerialized(target, propertyName, property => property.objectReferenceValue = value);
    }

    private static void SetArray<T>(UnityEngine.Object target, string propertyName, T[] values)
    {
        SetSerialized(target, propertyName, property =>
        {
            property.arraySize = values?.Length ?? 0;
            for (int i = 0; i < property.arraySize; i++)
            {
                property.GetArrayElementAtIndex(i).boxedValue = values[i];
            }
        });
    }

    private static void SetField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (field == null)
        {
            throw new InvalidOperationException($"Field {fieldName} not found on {target.GetType().Name}.");
        }

        field.SetValue(target, value);
    }
}
