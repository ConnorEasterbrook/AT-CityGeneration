using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSystemVisualizer : MonoBehaviour
{
    public LSystemGenerator lSystemGenerator;
    List<Vector3> positions = new List<Vector3>(); // We need to save the positions of the blocks as we generate them.
    public GameObject prefab; // The prefab to use for the blocks.
    public Material lineMaterial; // The material to use for the lines.
    private int length = 8; // The length of the generations.
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
        string sequence = lSystemGenerator.GenerateResult(); // Generate the sequence of characters.   
        Visualize(sequence); // Visualize the sequence. 
    }

    /// <summary>
    /// This function will visualize the generated L-System .
    /// </summary>
    public void Visualize(string sequence)
    {
        Stack<LSystemAssistantScript> blockStack = new Stack<LSystemAssistantScript>(); // We need a stack to save the current position and rotation of the block being generated.
        Vector3 currentPos = Vector3.zero; // We need to save the current position of the block being generated. This could help additional generation items, such as trees, to be generated in the correct position.

        Vector3 direction = Vector3.forward; // Starting direction of the generated block.
        Vector3 tempPos = Vector3.zero;

        positions.Add(currentPos); // Add the current position to the list of positions.

        foreach (char letter in sequence)
        {
            switch (letter)
            {
                case 'F': // Move forward.
                    tempPos = currentPos + direction * Length; // Calculate the new position.
                    DrawLine(tempPos, currentPos, Color.red); // Draw a line between the current position and the new position.
                    positions.Add(tempPos); // Add the new position to the list of positions.
                    currentPos = tempPos; // Update the current position.
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
            Instantiate(prefab, position, Quaternion.identity);
        }
    }

    /// <summary>
    /// This function will draw a line between the two given points.
    /// </summary>
    private void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        GameObject line = new GameObject("line");
        line.transform.position = start;
        var lineRenderer = line.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.SetPosition(0, end);
        lineRenderer.SetPosition(1, start);
    }
}
