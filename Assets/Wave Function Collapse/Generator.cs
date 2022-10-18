using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace WaveFunctionCollapse
{
    public class Generator : MonoBehaviour
    {
        //* NOTE: The terms "Slot" and "Module" can be used interchangeably when discussing the cubic area that contains a single module. Individually, a module is a single prefab that contains placement settings within a script and is placed in a slot.
        [Header("Core Variables")]
        public Transform moduleParent; // The container for the blueprints. This will contain the module prefabs, each with their own attached positioning script.

        [Header("Grid Variables")]
        public Vector3 gridSize; // The size of the grid. This is the number of modules in each direction.
        public int slotSize = 10; // The size of each slot in the grid.
        public float heightOffset = 2.5f; // The height offset for each slot in the grid.

        //? Hidden variables
        [HideInInspector] public Module[] modules; // The array of modules that will be used to generate the grid.
        [HideInInspector] public static int _SlotSize = 10; // The size of each slot in the grid.
        [HideInInspector] public int SlotsFilled = 0; // The number of slots that have been filled.
        [HideInInspector] public Slot[,,] grid; // The grid of slots that will be used to generate the grid.
        private Vector3 slotOffset; // The offset of the grid from the origin.

        /// <summary>
        /// Clears the grid of any existing modules.
        /// </summary>
        public void Clear()
        {
            // Delete all children objects.
            while (transform.childCount != 0)
            {
                GameObject.DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        /// <summary>
        /// Initializes the grid and variables.
        /// </summary>
        private void Initialize()
        {
            _SlotSize = slotSize; // Set the static slot size to the current slot size.
            slotOffset = new Vector3(gridSize.x * slotSize / 2f, gridSize.y * slotSize / 2f, gridSize.z * slotSize / 2f); // Instantiate the block offset.
            SlotsFilled = 0; // Keep track of the number of slots that have been filled.

            ModulePrototype modulePrototype = new ModulePrototype(); // Create a new module prototype.
            modules = modulePrototype.CreateModules().ToArray(); // Create modules

            grid = new Slot[(int)gridSize.x, (int)gridSize.y, (int)gridSize.z];

            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    for (int z = 0; z < gridSize.z; z++)
                    {
                        grid[x, y, z] = new Slot(x, y, z, this); // Create a new slot at the current position.
                    }
                }
            }
        }

        /// <summary>
        /// Generates the grid of slots that contain the generation.
        /// </summary>
        public IEnumerator Generate()
        {
            Clear(); // Clear the grid of any existing modules.
            Initialize(); // Initialize the grid and variables.

            int[][] slotNeighbourState = new int[6][]; // The initial state of the slot neighbours.

            // For each possible neighbour, add the index of the module to the slot neighbours initial state.
            for (int directions = 0; directions < 6; directions++)
            {
                slotNeighbourState[directions] = new int[modules.Length];

                // For each module.
                foreach (Module module in modules)
                {
                    // For each possible neighbour.
                    foreach (int possibleNeighbour in module.PossibleNeighbours[directions])
                    {
                        slotNeighbourState[directions][possibleNeighbour]++; // Increment the slot neighbours initial state and add the possible neighbour.
                    }
                }
            }

            // For each slot in the grid.
            foreach (Slot slot in grid)
            {
                slot.GetAdjacent(); // Initialize the slot neighbours.
                slot.PossibleNeighbours = slotNeighbourState.Select(neighbourState => neighbourState.ToArray()).ToArray(); // Select the slot neighbour's state and add it to the possible neighbours. Done through LINQ to avoid reference issues and keep code clean.
            }

            int totalSlots = (int)gridSize.x * (int)gridSize.y * (int)gridSize.z;  // Get the total number of slots in the grid.
            grid[0, 0, 0].RandomCollapse(); // Collapse a random module into the first slot.

            // While there are still slots that need to be filled.
            while (SlotsFilled < totalSlots)
            {
                Collapse(); // Collapse the grid.
                SlotsFilled++; // Increment the number of slots that have been filled.
                yield return new WaitForSeconds(0.5f);
            }
        }

        /// <summary>
        /// Collapse the grid.
        /// </summary>
        public void Collapse()
        {
            int lowestEntropy = 10; // Random high number to work down from.
            List<Slot> lowestEntropySlots = new List<Slot>(); // The list of slots with the lowest entropy.

            // For each slot in the grid.
            foreach (Slot slot in grid)
            {
                // If the slot has not yet been collapsed
                if (!slot.isCollapsed)
                {
                    // If the slot has a lower entropy than the current lowest entropy then set the lowest entropy to match it
                    if (slot.getEntropy <= lowestEntropy)
                    {
                        lowestEntropy = slot.getEntropy;
                    }

                    // If the slot has the same entropy as the lowest entropy then add it to the list of slots with the lowest entropy.
                    if (slot.getEntropy == lowestEntropy)
                    {
                        lowestEntropySlots.Add(slot);
                    }
                }
            }

            lowestEntropySlots[Random.Range(0, lowestEntropySlots.Count)].RandomCollapse(); // Collapse a random module into the lowest entropy slot.
        }

        /// <summary>
        /// Get the position of the slot in the grid. Called by each slot.
        /// </summary>
        public Vector3 GetSlotPosition(float x, float y, float z)
        {
            // Calculate the position of the slot.
            Vector3 position = new Vector3
            (
                x * slotSize - slotOffset.x + slotSize / 2f,
                (y * slotSize) + heightOffset,
                z * slotSize - slotOffset.z + slotSize / 2f
            );

            return position;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(Generator))]
    public class GeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            Generator generator = (Generator)target;

            // Create Generate and Clear buttons in the inspector, next to each other.
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate"))
            {
                if (Application.isPlaying)
                {
                    generator.StartCoroutine("Generate");
                }
                else
                {
                    IEnumerator innerRoutine = generator.Generate();
                    while (innerRoutine.MoveNext()) ;
                }
            }

            if (GUILayout.Button("Clear"))
            {
                generator.Clear();
            }
            EditorGUILayout.EndHorizontal();
        }
#endif
    }
}
