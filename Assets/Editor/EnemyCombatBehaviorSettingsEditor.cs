using RPGProject.Gameplay;
using UnityEditor;

[CustomEditor(typeof(EnemyCombatBehaviorSettings))]
public sealed class EnemyCombatBehaviorSettingsEditor : Editor
{
    private SerializedProperty attackMode;
    private SerializedProperty movementPolicy;
    private SerializedProperty engagementPolicy;
    private SerializedProperty detectionRange;
    private SerializedProperty preferredDistance;
    private SerializedProperty lowHealthThreshold;
    private SerializedProperty fleeDistance;
    private SerializedProperty fleeSpeedMultiplier;

    private void OnEnable()
    {
        attackMode = serializedObject.FindProperty("attackMode");
        movementPolicy = serializedObject.FindProperty("movementPolicy");
        engagementPolicy = serializedObject.FindProperty("engagementPolicy");
        detectionRange = serializedObject.FindProperty("detectionRange");
        preferredDistance = serializedObject.FindProperty("preferredDistance");
        lowHealthThreshold = serializedObject.FindProperty("lowHealthThreshold");
        fleeDistance = serializedObject.FindProperty("fleeDistance");
        fleeSpeedMultiplier = serializedObject.FindProperty("fleeSpeedMultiplier");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(attackMode);
        EditorGUILayout.PropertyField(engagementPolicy);
        EditorGUILayout.PropertyField(movementPolicy);
        EditorGUILayout.PropertyField(detectionRange);

        EditorGUILayout.Space(6f);
        EditorGUILayout.HelpBox(BuildSummary(), MessageType.Info);

        if ((EnemyMovementPolicy)movementPolicy.enumValueIndex == EnemyMovementPolicy.FleeAtLowHealth)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.PropertyField(lowHealthThreshold);
            EditorGUILayout.PropertyField(fleeDistance);
            EditorGUILayout.PropertyField(fleeSpeedMultiplier);
        }
        else if ((EnemyMovementPolicy)movementPolicy.enumValueIndex == EnemyMovementPolicy.FleeWhenDamaged)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.PropertyField(fleeDistance);
            EditorGUILayout.PropertyField(fleeSpeedMultiplier);
        }
        else if ((EnemyMovementPolicy)movementPolicy.enumValueIndex == EnemyMovementPolicy.KeepDistance)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.PropertyField(preferredDistance);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private string BuildSummary()
    {
        EnemyEngagementPolicy engagement = (EnemyEngagementPolicy)engagementPolicy.enumValueIndex;
        EnemyMovementPolicy movement = (EnemyMovementPolicy)movementPolicy.enumValueIndex;

        string engagementText = engagement switch
        {
            EnemyEngagementPolicy.AggressiveOnSight => "engages when the player enters detection range",
            EnemyEngagementPolicy.RetaliateWhenTargeted => "engages after the player targets it",
            EnemyEngagementPolicy.RetaliateWhenDamaged => "engages only after taking damage",
            EnemyEngagementPolicy.Passive => "does not start combat on its own",
            _ => "uses custom engagement"
        };

        string movementText = movement switch
        {
            EnemyMovementPolicy.ChaseTarget => "chases into attack range",
            EnemyMovementPolicy.HoldPosition => "holds position after engaging",
            EnemyMovementPolicy.FleeWhenDamaged => "flees after being damaged",
            EnemyMovementPolicy.FleeAtLowHealth => "flees when low on health",
            EnemyMovementPolicy.KeepDistance => "keeps its preferred distance",
            _ => "uses custom movement"
        };

        return $"Combat intent: {engagementText}, then {movementText}.";
    }
}
