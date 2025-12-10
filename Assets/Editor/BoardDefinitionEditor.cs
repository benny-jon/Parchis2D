using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoardDefinition))]
public class BoardDefinitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BoardDefinition board = (BoardDefinition) target;

        GUILayout.Space(10);

        if (GUILayout.Button($"Generate Full Layout ({BoardDefinition.TOTAL_TILES} tiles)"))
        {
            BoardDefinitionGenerator.GenerateFullBoard(board);
            EditorUtility.SetDirty(board);
        }
    }
}
