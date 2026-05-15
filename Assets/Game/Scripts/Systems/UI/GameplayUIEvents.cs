using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RPGProject.Systems
{
    public static class GameplayUIEvents
    {
        public static event Action<string, FeedbackMessageType, float, Object> FeedbackRequested;

        public static void Show(
            string message,
            FeedbackMessageType messageType = FeedbackMessageType.Info,
            float visibleSeconds = -1f,
            Object source = null)
        {
            FeedbackRequested?.Invoke(message, messageType, visibleSeconds, source);
        }

        public static void ShowInfo(string message, float visibleSeconds = -1f, Object source = null)
        {
            Show(message, FeedbackMessageType.Info, visibleSeconds, source);
        }

        public static void ShowSuccess(string message, float visibleSeconds = -1f, Object source = null)
        {
            Show(message, FeedbackMessageType.Success, visibleSeconds, source);
        }

        public static void ShowWarning(string message, float visibleSeconds = -1f, Object source = null)
        {
            Show(message, FeedbackMessageType.Warning, visibleSeconds, source);
        }

        public static void ShowQuest(string message, float visibleSeconds = -1f, Object source = null)
        {
            Show(message, FeedbackMessageType.Quest, visibleSeconds, source);
        }

        public static void ShowLoot(string message, float visibleSeconds = -1f, Object source = null)
        {
            Show(message, FeedbackMessageType.Loot, visibleSeconds, source);
        }
    }
}
