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

    public readonly struct LootTakenEvent
    {
        public LootTakenEvent(ILootSource lootSource, bool succeeded)
        {
            LootSource = lootSource;
            Succeeded = succeeded;
        }

        public ILootSource LootSource { get; }
        public bool Succeeded { get; }
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

    public static class GameplayEvents
    {
        public static event Action<CombatAttackEvent> CombatAttackResolved;
        public static event Action<EnemyStateChangedEvent> EnemyStateChanged;
        public static event Action<InventoryItemActionEvent> InventoryItemActionResolved;
        public static event Action<LootTakenEvent> LootTaken;
        public static event Action<AutoAttackOutOfRangeEvent> AutoAttackOutOfRange;

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

        public static void PublishLootTaken(ILootSource lootSource, bool succeeded)
        {
            LootTaken?.Invoke(new LootTakenEvent(lootSource, succeeded));
        }

        public static void PublishAutoAttackOutOfRange(AutoAttackController controller, HealthComponent target)
        {
            AutoAttackOutOfRange?.Invoke(new AutoAttackOutOfRangeEvent(controller, target));
        }
    }
}
