using RPGProject.Gameplay;

namespace RPGProject.Systems
{
    public enum QuestIndicatorVisualState
    {
        Hidden,
        Available,
        Active,
        Completed
    }

    public static class QuestIndicatorStateResolver
    {
        public static QuestIndicatorVisualState Resolve(QuestDefinition questToOffer, QuestState questState)
        {
            if (questToOffer == null)
            {
                return QuestIndicatorVisualState.Hidden;
            }

            return questState switch
            {
                QuestState.Locked => QuestIndicatorVisualState.Available,
                QuestState.Available => QuestIndicatorVisualState.Available,
                QuestState.Active => QuestIndicatorVisualState.Active,
                QuestState.Completed => QuestIndicatorVisualState.Completed,
                _ => QuestIndicatorVisualState.Hidden
            };
        }
    }
}
