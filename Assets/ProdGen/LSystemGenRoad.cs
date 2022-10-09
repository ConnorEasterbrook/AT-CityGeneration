using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LSystemGenRoad : MonoBehaviour
{
    public GameObject roadStraight; // The road prefabs.
    private Dictionary<Vector3, GameObject> roadMap = new Dictionary<Vector3, GameObject>(); // A map of all the roads that have been generated.
    private HashSet<Vector3> roadFix = new HashSet<Vector3>(); // Fixes roads upon next block generation, if needed.

    // Start is called before the first frame update
    void Start()
    {

    }

    /// <summary>
    /// Generates a road at the given position in the correct rotation.
    /// </summary>
    public void PlaceStreet(Vector3 startPos, Vector3 direction, int length)
    {
        for (int i = 0; i < length; i++)
        {
            Vector3 position = startPos + (direction * i);
            Quaternion rotation = Quaternion.LookRotation(position - startPos);

            if (roadMap.ContainsKey(position))
            {
                continue;
            }

            GameObject road = Instantiate(roadStraight, position, rotation, transform);
            road.transform.parent = gameObject.transform; // Set the parent of the road to the road generator.
            roadMap.Add(position, road); // Add the road to the road map.

            // Check if the road needs to be fixed.
            if (i == 0)
            {
                roadFix.Add(position);
            }
        }
    }

    public void Clear()
    {
        roadMap.Clear();
        roadFix.Clear();
    }
}

# if UNITY_EDITOR
[CustomEditor(typeof(LSystemGenRoad))]
public class LSystemGenRoadEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LSystemGenRoad generator = (LSystemGenRoad)target;

        if (GUILayout.Button("Clear"))
        {
            generator.Clear();
        }
    }
}
#endif