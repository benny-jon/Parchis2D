using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PieceScenarioLoader))]
public class PieceScenarioLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var loader = (PieceScenarioLoader) target;

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Load Scenario " + loader.GetScenarioTitle()))
        {
            loader.LoadScenario();
            EditorUtility.SetDirty(loader);
        }
    }
}
