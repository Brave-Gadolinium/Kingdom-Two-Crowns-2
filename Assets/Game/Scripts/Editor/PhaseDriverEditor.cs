#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PhaseDriver))]
public sealed class PhaseDriverEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PhaseDriver driver = (PhaseDriver)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Development", EditorStyles.boldLabel);
        if (GUILayout.Button("Next Phase"))
            driver.DebugNextPhase();

        if (GUILayout.Button("Set Remaining Time"))
            driver.DebugSetRemainingTime();
    }
}
#endif
