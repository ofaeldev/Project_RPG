using System;
using RPGProject.Gameplay;

namespace RPGProject.Systems
{
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

    public static partial class GameplayEvents
    {
        public static event Action<AutoAttackOutOfRangeEvent> AutoAttackOutOfRange;

        public static void PublishAutoAttackOutOfRange(AutoAttackController controller, HealthComponent target)
        {
            AutoAttackOutOfRange?.Invoke(new AutoAttackOutOfRangeEvent(controller, target));
        }
    }
}
