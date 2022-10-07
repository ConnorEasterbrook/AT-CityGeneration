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

# if UNITY_EDITOR
[CustomEditor(typeof(LSystemGenRule))]
public class LSystemGenRuleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

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
