namespace RPGProject.Systems
{
    public readonly struct GameplayFeedbackMessage
    {
        public string Text { get; }
        public FeedbackMessageType MessageType { get; }
        public float VisibleSeconds { get; }
        public int SourceId { get; }

        public GameplayFeedbackMessage(string text, FeedbackMessageType messageType, float visibleSeconds, int sourceId)
        {
            Text = text;
            MessageType = messageType;
            VisibleSeconds = visibleSeconds;
            SourceId = sourceId;
        }
    }
}
