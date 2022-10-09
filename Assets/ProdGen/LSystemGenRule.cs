using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// A scriptable object that contains the rules for a specific L-System.
/// </summary>
// We make this a scriptable object to make it easier to create assets that contain specific rules as it will always contain the specific data we need.
// This would be an appropriate decision if we wanted different rules for different tiles.
[CreateAssetMenu(menuName = "CityGeneration/LSystemGenRule")] // Ensure that the assets are able to be created quickly in the editor.
public class LSystemGenRule : ScriptableObject
{
    public string identifier; // The string to look for in order to trigger the rule.
    public string[] result = null; // The result of the rule.

    /// <summary> 
    /// This is a simple function that will return the result of the rule. 
    /// Necessary to get multiple results that the single identifier can output.
    /// </summary>
    public string GetResult()
    {
        return result[0];
    }
}

// Simple editor script to add labels in the editor inspector.
# if UNITY_EDITOR
[CustomEditor(typeof(LSystemGenRule))]
public class LSystemGenRuleEditor : Editor
{
    private GUIStyle headerStyle = new GUIStyle();
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        // Style the header GUIStyle.
        headerStyle.fontSize = 15;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.gray;

        // ALGORITHM AXIOM RULES
        GUILayout.Space(20);
        GUILayout.Label("ALGORITHM AXIOM RULES", headerStyle);
        GUILayout.Label("RULE LETTERS");
        GUILayout.Label("F = Draw");
        GUILayout.Label("[ = Save");    
        GUILayout.Label("] = Restore");
        GUILayout.Label("L = Left turn");
        GUILayout.Label("R = Right turn");
        GUILayout.Label("+ = Length++");
        GUILayout.Label("- = Length--");
    }
}
#endif

/// <summary>
/// A small class that contains the data for the currently generated block within the L-System.
/// </summary>
public class LSystemAssistantScript
{
    // We need variables to save the current position and rotation of the block being generated.
    public Vector3 position; 
    public Vector3 direction;
    public int length;
}
