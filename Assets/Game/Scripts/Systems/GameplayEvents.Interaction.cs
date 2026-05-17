using System;
using UnityEngine;

namespace RPGProject.Systems
{
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

    public static partial class GameplayEvents
    {
        public static event Action<InteractionFeedbackEvent> InteractionFeedbackResolved;

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
