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
    public LSystemGenRoad lSystemGenRoad;


    [Header("GameObjects")]
    public LSystemGenRule[] rules; // Input space for L-System rules.
    List<Vector3> positions = new List<Vector3>(); // We need to save the positions of the blocks as we generate them.
    public GameObject prefab; // The prefab to use for the blocks.
    public Material lineMaterial; // The material to use for the lines.

    [Header("Settings")]
    public string axiom; // The starting string for the L-System. 
    [Range(0, 8)] public int iterations = 1; // The number of iterations to run the L-System for.
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

    [Header("Variation")]
    public bool ignoreRules = false; // Whether or not to ignore the rules and just use the axiom.
    [Range(0, 1)] public float ignoreRuleChance = 0.5f; // The amount of randomness to use in the rules.
    [Range(0, 4)] public int ignoreAfterIterationIndex = 1; // The iteration to start ignoring the rules.

    private void Start()
    {
        string sequence = GenerateResult(axiom); // Generate the sequence of characters.   
        Visualize(sequence); // Visualize the sequence. 
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
                // If we are ignoring the rules, we need to check whether or not there is randomness. Only after a set iterationIndex to avoid the randomness at the start.
                if (ignoreRules && iterationIndex >= ignoreAfterIterationIndex)
                {
                    if (Random.value < ignoreRuleChance)
                    {
                        return;
                    }
                }
                newResult.Append(Grow(rule.GetResult(), iterationIndex + 1)); // Call the grow function to check if more characters need to be added.
            }
        }
    }

    /// <summary>
    /// This function will visualize the generated L-System .
    /// </summary>
    public void Visualize(string sequence = null)
    {
        if (sequence == null)
        {
            sequence = GenerateResult(axiom);
        }

        Stack<LSystemAssistantScript> blockStack = new Stack<LSystemAssistantScript>(); // We need a stack to save the current position and rotation of the block being generated.
        Vector3 currentPos = Vector3.zero; // We need to save the current position of the block being generated. This could help additional generation items, such as trees, to be generated in the correct position.

        Vector3 direction = Vector3.forward; // Starting direction of the generated block.
        Vector3 newPos = Vector3.zero; // The new position of the block being generated.

        positions.Add(currentPos); // Add the current position to the list of positions.

        foreach (char letter in sequence)
        {
            switch (letter)
            {
                case 'F': // Move forward and draw a line.
                    newPos = currentPos + (direction * Length); // Calculate the new position.
                    DrawLine(currentPos, newPos); // Draw a line between the current position and the new position.


                    lSystemGenRoad.PlaceStreet(currentPos, direction, length);


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

                case 'r': // Turn right 90.
                    direction = Quaternion.Euler(0, 90, 0) * direction; // Update the direction.
                    break;

                case 'L': // Turn left.
                    direction = Quaternion.Euler(0, -angle, 0) * direction; // Update the direction.
                    break;

                case 'l': // Turn left 90.
                    direction = Quaternion.Euler(0, 90, 0) * direction; // Update the direction.
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
        foreach (Vector3 position in positions)
        {
            GameObject instantiation = Instantiate(prefab, position, Quaternion.identity); // Instantiate the prefab.
            instantiation.transform.parent = gameObject.transform; // Set the parent of the instantiated prefab to this game object.
            instantiation.transform.localScale = new Vector3(0.5f, 2, 0.5f); // Ensure the prefab is appropriately sized.

            Collider[] overlapColliders = Physics.OverlapSphere(instantiation.transform.position, 5f, LayerMask.GetMask("UI"));

            foreach (Collider instantiationcollider in overlapColliders)
            {
                DrawLine(instantiationcollider.transform.position, position);

                Vector3 dir = (instantiationcollider.transform.position - position).normalized;
                float distance = Vector3.Distance(instantiationcollider.transform.position, position);

                lSystemGenRoad.PlaceConnectingStreet(position, dir, Mathf.RoundToInt(distance) - 1);
            }
        }
    }

    /// <summary>
    /// This function will draw a line between the two given points. The previously generated block and the newly generated block.
    /// </summary>
    private void DrawLine(Vector3 start, Vector3 end)
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
        // Delete the children of the current game object.
        while (gameObject.transform.childCount != 0)
        {
            Destroy(gameObject.transform.GetChild(0).gameObject);
        };

        positions.Clear(); // Clear the positions list.

        while (lSystemGenRoad.transform.childCount != 0)
        {
            Destroy(lSystemGenRoad.transform.GetChild(0).gameObject);
        };
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
            generator.CallVisualizeFromEditor();
        }
        if (GUILayout.Button("Clear"))
        {
            generator.ClearInspectorView();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("GenerateText"))
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
        GUILayout.Label("R = Right turn");
        GUILayout.Label("r = Right turn 90");
        GUILayout.Label("L = Left turn");
        GUILayout.Label("l = Left turn 90");
        GUILayout.Label("+ = Length++");
        GUILayout.Label("- = Length--");
    }
}
#endif
