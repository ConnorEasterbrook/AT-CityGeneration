using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// We make this a scriptable object to make it easier to create assets that contain specific rules as it will always contain the specific data we need.
// This would be an appropriate decision if we wanted different rules for different tiles.
[CreateAssetMenu(menuName = "CityGeneration/GenRule")] // Ensure that the assets are able to be created quickly in the editor.
public class GenRule : ScriptableObject
{
    public string identifier; // The identifier for triggering the rule.
    public string[] result = null; // The result of the rule.

    /// <summary> 
    /// This is a simple function that will return the result of the rule. 
    /// Necessary to get multiple results that the single identifier can output.
    /// </summary>
    public string getResult()
    {
        return result[0];
    }
}
