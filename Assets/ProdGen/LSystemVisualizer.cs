using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LSystemVisualizer : MonoBehaviour
{
    [Header("GameObjects")]
    public LSystemGenerator lSystemGenerator;
    List<Vector3> positions = new List<Vector3>(); // We need to save the positions of the blocks as we generate them.
    public GameObject prefab; // The prefab to use for the blocks.
    public Material lineMaterial; // The material to use for the lines.

    [Header("Settings")]
    public string axiom; // The starting string for the L-System.
    public int length = 10; // The length of the generations.
    public float angle = 90; // The angle that the algorithm uses to turn left or right after every iteration.

    public int Length
    {
        get
        {
            // We need to make sure that the length is always greater than 0 in order to keep sections visible.
            if (length > 0)
            {
                return length;
            }
            else
            {
                return 1;
            }
        }
        set
        {
            length = value;
        }
    }

    private void Start()
    {
        string sequence = lSystemGenerator.GenerateResult(axiom); // Generate the sequence of characters.   
        Visualize(sequence); // Visualize the sequence. 
    }

    /// <summary>
    /// This function will visualize the generated L-System .
    /// </summary>
    public void Visualize(string sequence = null)
    {
        if (sequence == null)
        {
            sequence = lSystemGenerator.GenerateResult(axiom);
        }

        Stack<LSystemAssistantScript> blockStack = new Stack<LSystemAssistantScript>(); // We need a stack to save the current position and rotation of the block being generated.
        Vector3 currentPos = Vector3.zero; // We need to save the current position of the block being generated. This could help additional generation items, such as trees, to be generated in the correct position.

        Vector3 direction = Vector3.forward; // Starting direction of the generated block.
        Vector3 newPos = Vector3.zero;

        positions.Add(currentPos); // Add the current position to the list of positions.

        foreach (char letter in sequence)
        {
            switch (letter)
            {
                case 'F': // Move forward and draw a line.
                    newPos = currentPos + direction * Length; // Calculate the new position.
                    DrawLine(currentPos, newPos, Color.red); // Draw a line between the current position and the new position.
                    positions.Add(newPos); // Add the new position to the list of positions.
                    currentPos = newPos; // Update the current position.
                    // Length -= 1; // Decrease the length of the generation.
                    break;

                // case 'f': // Move forward without drawing a line.
                //     currentPos += direction * Length; // Update the current position.
                //     break;

                // The following cases are for the additional generation items, such as trees.
                case '[': // Save the current position and rotation.
                    blockStack.Push(new LSystemAssistantScript
                    {
                        position = currentPos,
                        direction = direction,
                        length = Length
                    }); // Push the current position and rotation to the stack.
                    break;

                case ']': // Restore the current position and rotation.
                    if (blockStack.Count > 0)
                    {
                        LSystemAssistantScript assistant = blockStack.Pop(); // Pop the current position and rotation from the stack.
                        currentPos = assistant.position; // Update the current position.
                        direction = assistant.direction; // Update the direction.
                        Length = assistant.length; // Update the length.
                    }
                    else
                    {
                        Debug.LogError("Dont have saved point in our stack");
                    }
                    break;

                // The following cases are for turning the block.
                case 'R': // Turn right.
                    direction = Quaternion.Euler(0, angle, 0) * direction; // Update the direction.
                    break;

                case 'L': // Turn left.
                    direction = Quaternion.Euler(0, -angle, 0) * direction; // Update the direction.
                    break;

                // The following cases are used to change the length of the generated blocks.
                case '+': // Increase the length.
                    Length += 1;
                    break;

                case '-': // Decrease the length.
                    Length -= 1;
                    break;

                default:
                    break;
            }
        }

        prefab.transform.localScale = new Vector3(1, 1, 1); // Ensure the prefab is appropriately sized.

        // We need to instantiate the prefab for each position in the list of positions.
        foreach (var position in positions)
        {
            GameObject instantiation = Instantiate(prefab, position, Quaternion.identity);
            instantiation.transform.parent = gameObject.transform;
        }
    }

    /// <summary>
    /// This function will draw a line between the two given points. The previously generated block and the newly generated block.
    /// </summary>
    private void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        GameObject line = new GameObject("line"); // Create a new line gameobject.
        var lineRenderer = line.AddComponent<LineRenderer>(); // Add a line renderer to the line gameobject.
        lineRenderer.material = lineMaterial; // Set the material of the line renderer.

        // Set the positions of the line renderer.
        lineRenderer.SetPosition(1, start);
        lineRenderer.SetPosition(0, end);

        // We need to set the width of the line renderer otherwise it looks odd.
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        line.transform.parent = gameObject.transform; // Set the parent of the line to the current game object.
    }

    public void CallVisualizeFromEditor()
    {
        ClearInspectorView();
        Visualize();
    }

    public void ClearInspectorView()
    {
        gameObject.transform.DeleteChildren(); // Delete the children of the current game object.
        positions.Clear(); // Clear the positions list.
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LSystemVisualizer))]
public class LSystemVisualizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LSystemVisualizer visualizer = (LSystemVisualizer)target;

        if (GUILayout.Button("Generate"))
        {
            visualizer.CallVisualizeFromEditor();
        }
        if (GUILayout.Button("Clear"))
        {
            visualizer.ClearInspectorView();
        }
    }
}
#endif
