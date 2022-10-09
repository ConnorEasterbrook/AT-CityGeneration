using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// The core of the L-System based procedural generation.
/// </summary>
public class LSystemGenerator : MonoBehaviour
{
    public string axiom; // The starting string for the L-System. 
    public LSystemGenRule[] rules; // Input space for L-System rules.
    [Range(0, 5)] public int iterations = 1; // The number of iterations to run the L-System for.

    private void Start()
    {
        Debug.Log(GenerateResult());
    }

    /// <summary>
    /// This function will generate a string based on the L-System rules.
    /// </summary>
    public string GenerateResult(string result = null)
    {
        if (result == null)
        {
            result = axiom;
        }

        return Grow(result);
    }

    /// <summary>
    /// This function will recursively grow the string based on the rules.
    /// </summary>
    private string Grow(string result, int iterationIndex = 0)
    {
        // If we have reached the end of the iterations, return the input.
        if (iterationIndex >= iterations)
        {
            return result;
        }

        // Else, we need to grow the string.
        StringBuilder newResult = new StringBuilder(); // We use a string builder to avoid the overhead of creating a new string every time we add a character.

        // Iterate through every letter in the current iteration result string.
        foreach (char character in result)
        {
            newResult.Append(character); // Add the current character to the new result.

            ProcessRules(newResult, character, iterationIndex); // Recursively call this function to grow the string further, taking the rules into account.
        }

        // Return the result of growing the string.
        return newResult.ToString();
    }

    /// <summary>
    /// This function will process the rules for the current character and grow the string appropriately. Calling Grow() to check if more characters are needed
    /// </summary>
    private void ProcessRules(StringBuilder newResult, char character, int iterationIndex)
    {
        // Iterate through every rule.
        foreach (LSystemGenRule rule in rules)
        {
            // If the current character matches the identifier for the rule, add the result of the rule to the new result.
            if (rule.identifier == character.ToString())
            {
                newResult.Append(Grow(rule.GetResult(), iterationIndex + 1)); // Call the grow function to check if more characters need to be added.
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LSystemGenerator))]
public class LSystemGeneratorEditor : Editor
{
    private GUIStyle headerStyle = new GUIStyle();

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LSystemGenerator generator = (LSystemGenerator)target;

        if (GUILayout.Button("Generate"))
        {
            Debug.Log(generator.GenerateResult());
        }
        
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
