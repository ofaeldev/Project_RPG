using System;
using RPGProject.Gameplay;
using UnityEngine;

namespace RPGProject.Systems
{
    public readonly struct CombatAttackEvent
    {
        public CombatAttackEvent(CombatActor attacker, HealthComponent target, DamageResult damage)
        {
            Attacker = attacker;
            Target = target;
            Damage = damage;
        }

        public CombatActor Attacker { get; }
        public HealthComponent Target { get; }
        public DamageResult Damage { get; }
    }

    public readonly struct EnemyStateChangedEvent
    {
        public EnemyStateChangedEvent(EnemyCombatController enemy, EnemyCombatState previousState, EnemyCombatState currentState)
        {
            Enemy = enemy;
            PreviousState = previousState;
            CurrentState = currentState;
        }

        public EnemyCombatController Enemy { get; }
        public EnemyCombatState PreviousState { get; }
        public EnemyCombatState CurrentState { get; }
    }

    public readonly struct InventoryItemActionEvent
    {
        public InventoryItemActionEvent(InventoryItemStack stack, int slotIndex, string actionName, bool succeeded)
        {
            Stack = stack;
            SlotIndex = slotIndex;
            ActionName = actionName;
            Succeeded = succeeded;
        }

        public InventoryItemStack Stack { get; }
        public int SlotIndex { get; }
        public string ActionName { get; }
        public bool Succeeded { get; }
    }

    public enum InventoryDropFailureReason
    {
        None,
        MissingDependencies,
        InvalidItem,
        BlockedPosition,
        RemoveFailed
    }

    public readonly struct InventoryDropEvent
    {
        public InventoryDropEvent(
            InventoryItemStack stack,
            string itemName,
            GameObject droppedObject,
            bool succeeded,
            InventoryDropFailureReason failureReason,
            UnityEngine.Object feedbackSource)
        {
            Stack = stack;
            ItemName = itemName;
            DroppedObject = droppedObject;
            Succeeded = succeeded;
            FailureReason = failureReason;
            FeedbackSource = feedbackSource;
        }

        public InventoryItemStack Stack { get; }
        public string ItemName { get; }
        public GameObject DroppedObject { get; }
        public bool Succeeded { get; }
        public InventoryDropFailureReason FailureReason { get; }
        public UnityEngine.Object FeedbackSource { get; }
    }

    public readonly struct LootTakenEvent
    {
        public LootTakenEvent(ILootSource lootSource, bool succeeded)
            : this(lootSource, succeeded ? 1 : 0, succeeded ? 1 : 0, lootSource as UnityEngine.Object)
        {
        }

        public LootTakenEvent(ILootSource lootSource, int availableStacks, int claimedStacks, UnityEngine.Object feedbackSource)
        {
            LootSource = lootSource;
            AvailableStacks = availableStacks;
            ClaimedStacks = claimedStacks;
            FeedbackSource = feedbackSource;
        }

        public ILootSource LootSource { get; }
        public int AvailableStacks { get; }
        public int ClaimedStacks { get; }
        public UnityEngine.Object FeedbackSource { get; }
        public bool Succeeded => ClaimedStacks > 0;
    }

    public readonly struct AutoAttackOutOfRangeEvent
    {
        public AutoAttackOutOfRangeEvent(AutoAttackController controller, HealthComponent target)
        {
            Controller = controller;
            Target = target;
        }

        public AutoAttackController Controller { get; }
        public HealthComponent Target { get; }
    }

    public enum InteractionFeedbackType
    {
        EnemyAttackStarted,
        EnemyNoLoot,
        EnemyDefeated,
        DoorAlreadyOpen,
        DoorLocked,
        DoorOpened,
        ContainerLocked,
        ItemPickupInventoryUnavailable,
        ItemPickedUp,
        ItemPickupNoSpace,
        ItemPickupInvalid
    }

    public readonly struct InteractionFeedbackEvent
    {
        public InteractionFeedbackEvent(
            InteractionFeedbackType feedbackType,
            string displayName,
            string detail,
            UnityEngine.Object feedbackSource)
        {
            FeedbackType = feedbackType;
            DisplayName = displayName;
            Detail = detail;
            FeedbackSource = feedbackSource;
        }

        public InteractionFeedbackType FeedbackType { get; }
        public string DisplayName { get; }
        public string Detail { get; }
        public UnityEngine.Object FeedbackSource { get; }
    }

    public static class GameplayEvents
    {
        public static event Action<CombatAttackEvent> CombatAttackResolved;
        public static event Action<EnemyStateChangedEvent> EnemyStateChanged;
        public static event Action<InventoryItemActionEvent> InventoryItemActionResolved;
        public static event Action<InventoryDropEvent> InventoryDropResolved;
        public static event Action<LootTakenEvent> LootTaken;
        public static event Action<AutoAttackOutOfRangeEvent> AutoAttackOutOfRange;
        public static event Action<InteractionFeedbackEvent> InteractionFeedbackResolved;

        public static void PublishCombatAttackResolved(CombatActor attacker, HealthComponent target, DamageResult damage)
        {
            CombatAttackResolved?.Invoke(new CombatAttackEvent(attacker, target, damage));
        }

        public static void PublishEnemyStateChanged(EnemyCombatController enemy, EnemyCombatState previousState, EnemyCombatState currentState)
        {
            EnemyStateChanged?.Invoke(new EnemyStateChangedEvent(enemy, previousState, currentState));
        }

        public static void PublishInventoryItemActionResolved(InventoryItemStack stack, int slotIndex, string actionName, bool succeeded)
        {
            InventoryItemActionResolved?.Invoke(new InventoryItemActionEvent(stack, slotIndex, actionName, succeeded));
        }

        public static void PublishInventoryDropResolved(
            InventoryItemStack stack,
            string itemName,
            GameObject droppedObject,
            bool succeeded,
            InventoryDropFailureReason failureReason,
            UnityEngine.Object feedbackSource)
        {
            InventoryDropResolved?.Invoke(new InventoryDropEvent(
                stack,
                itemName,
                droppedObject,
                succeeded,
                failureReason,
                feedbackSource));
        }

        public static void PublishLootTaken(ILootSource lootSource, bool succeeded)
        {
            LootTaken?.Invoke(new LootTakenEvent(lootSource, succeeded));
        }

        public static void PublishLootTaken(ILootSource lootSource, int availableStacks, int claimedStacks, UnityEngine.Object feedbackSource)
        {
            LootTaken?.Invoke(new LootTakenEvent(lootSource, availableStacks, claimedStacks, feedbackSource));
        }

        public static void PublishAutoAttackOutOfRange(AutoAttackController controller, HealthComponent target)
        {
            AutoAttackOutOfRange?.Invoke(new AutoAttackOutOfRangeEvent(controller, target));
        }

        public static void PublishInteractionFeedback(
            InteractionFeedbackType feedbackType,
            string displayName,
            string detail,
            UnityEngine.Object feedbackSource)
        {
            InteractionFeedbackResolved?.Invoke(new InteractionFeedbackEvent(
                feedbackType,
                displayName,
                detail,
                feedbackSource));
        }
    }
}
