using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LSystemGenRoad : MonoBehaviour
{
    public GameObject roadStraight; // The road prefabs.
    private Dictionary<Vector3, GameObject> roadMap = new Dictionary<Vector3, GameObject>(); // A map of all the roads that have been generated.
    private HashSet<Vector3> roadFix = new HashSet<Vector3>(); // Fixes roads upon next block generation, if needed.
    
    /// <summary>
    /// Generates a road at the given position in the correct rotation.
    /// </summary>
    public void PlaceStreet(Vector3 startPos, Vector3 direction, int length)
    {
        for (int i = 0; i < length + 1; i++)
        {
            Vector3 position = startPos + (direction * i);

            if (roadMap.ContainsKey(position))
            {
                continue;
            }
            Quaternion rotation = Quaternion.LookRotation(position - startPos); // Rotate the road to face the start position.

            // Check if there is already a road at this position.
            Collider[] roadColliders = Physics.OverlapBox(position, new Vector3(1, 1, 1), rotation);
            if (roadColliders.Length <= 1)
            {
                GameObject road = Instantiate(roadStraight, position, rotation, transform); // Generate the road.
                road.transform.parent = gameObject.transform; // Set the parent of the road to the road generator.

                if (!roadMap.ContainsKey(position))
                {
                    roadMap.Add(position, road); // Add the road to the road map.
                }
            }

            // Check if the road needs to be fixed.
            if (i == 0)
            {
                roadFix.Add(position);
            }
        }
    }

    public void PlaceConnectingStreet(Vector3 startPos, Vector3 direction, int length)
    {
        for (int i = 0; i < length + 1; i++)
        {
            Vector3 position = startPos + (direction * i);

            if (roadMap.ContainsKey(position))
            {
                continue;
            }
            Quaternion rotation = Quaternion.LookRotation(position - startPos); // Rotate the road to face the start position.

            // Check if there is already a road at this position.
            Collider[] roadColliders = Physics.OverlapBox(position, new Vector3(1, 1, 1), rotation);
            if (roadColliders.Length <= 1)
            {
                position = new Vector3(position.x, position.y - 0.005f, position.z); // Move the road down a bit to avoid texture clipping.
                GameObject road = Instantiate(roadStraight, position, rotation, transform); // Generate the road.
                road.transform.parent = gameObject.transform; // Set the parent of the road to the road generator.

                if (!roadMap.ContainsKey(position))
                {
                    roadMap.Add(position, road); // Add the road to the road map.
                }
            }

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