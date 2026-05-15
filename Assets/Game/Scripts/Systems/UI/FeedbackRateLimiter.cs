using System.Collections.Generic;

namespace RPGProject.Systems
{
    public sealed class FeedbackRateLimiter
    {
        private readonly Dictionary<int, float> sourceCooldownUntil = new();

        public bool ShouldSuppress(
            GameplayFeedbackMessage message,
            bool hasCurrentMessage,
            GameplayFeedbackMessage currentMessage,
            float now)
        {
            if (message.SourceId == 0)
            {
                return false;
            }

            bool isSameCurrentSource = hasCurrentMessage
                && currentMessage.SourceId == message.SourceId
                && currentMessage.MessageType == message.MessageType;

            return isSameCurrentSource
                && sourceCooldownUntil.TryGetValue(message.SourceId, out float cooldownUntil)
                && now < cooldownUntil;
        }

        public void Register(GameplayFeedbackMessage message, float now, float cooldownSeconds)
        {
            if (message.SourceId != 0 && cooldownSeconds > 0f)
            {
                sourceCooldownUntil[message.SourceId] = now + cooldownSeconds;
            }
        }
    }
}
