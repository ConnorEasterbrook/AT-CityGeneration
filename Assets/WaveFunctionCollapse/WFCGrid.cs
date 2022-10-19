using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace WFCGenerator
{
    [System.Serializable]
    public class WFCGrid
    {

        [Header("Identity")]
        public string name = "WFCGrid"; // Name of the grid.

        [Header("Variables")]
        public List<WFCModule> generationModules; // List of modules.

        [Header("Grid")]
        public float slotSize = 10f; // Size of each block.
        public int gridWidth = 10; // Width of the grid.
        public int gridLength = 15; // Length of the grid.
        public int gridHeight = 1; // Height of the grid.
        public float heightOffset = 2.5f; // The height offset for each slot in the grid.

        // Private generation variables.
        static private readonly int _MAX_ATTEMPTS = 5; // Maximum number of attempts to generate a grid.
        static private readonly int _MAX_ITERATIONS = 1000; // Maximum number of iterations to generate a grid.
        static private readonly int[] _OPPOSITE = { 2, 3, 0, 1, 5, 4 }; // Opposite directions. 0-2, 1-3, 2-0, 3-1, 4-5, 5-4. North, East, South, West, Above, Below.
        private Vector3 slotOffset;
        private Transform gridRoot;
        private bool[,,,] generation;
        private bool[,] possibilities;
        private int[] entropy;
        private bool failed;

        // Private vanity variables
        private int ticks;

        /// <summary>
        /// Core function of the algorithm.
        /// </summary>
        public async void Generate(GameObject generator)
        {
            // For each grid, if generation failed, clear the grid and re-initialize the wave function collapse algorithm.
            for (int attempt = 0; attempt < _MAX_ATTEMPTS; attempt++)
            {
                Clear(generator); // Clear the grid.
                Initialize(generator); // Initialize the wave function collapse algorithm.

                // For each iteration, if generation failed, clear the grid and re-initialize the wave function collapse algorithm.
                for (int iteration = 0; iteration < _MAX_ITERATIONS; iteration++)
                {
                    int index = NextSlot(); // Get the next slot.

                    // If next slot is found, propagate the wave function collapse algorithm.
                    if (index >= 0)
                    {
                        await Observe(index);
                    }
                    else
                    {
                        // If the next slot is not found, generation is successful.
                        Debug.Log("Generation successful!");
                        return;
                    }
                }
                if (!failed)
                {
                    break; // If generation is successful, break the loop.
                }
            }
        }

        /// <summary>
        /// Clear all variables and gameobjects.
        /// </summary>
        public void Clear(GameObject generator)
        {
            // Delete all children objects.
            while (generator.transform.childCount != 0)
            {
                GameObject.DestroyImmediate(generator.transform.GetChild(0).gameObject);
            }

            // Reset variables.
            failed = false;
            generation = null;
            ticks = 0;
        }

        /// <summary>
        /// Initialize the wave function collapse algorithm.
        /// </summary>
        void Initialize(GameObject generator)
        {
            // Organise in inspector.
            gridRoot = new GameObject(name).transform; // Create a new game object with the name of the grid.
            gridRoot.parent = generator.transform;  // Set the parent of the game object to the generator.
            gridRoot.position = new Vector3(generator.transform.position.x, generator.transform.position.y + ((slotSize * gridHeight) / 2) - (slotSize / 2), generator.transform.position.z); // Set the root x, z positions to the generator and keep the bottom layer at the same height as the generator yPos.
            gridRoot.rotation = generator.transform.rotation; // Set the rotation of the game object to the generator.

            // Instantiate variables.
            slotOffset = new Vector3(gridWidth * slotSize / 2f, gridHeight * slotSize / 2f, gridLength * slotSize / 2f); // Instantiate the block offset.
            generation = new bool[gridWidth * gridLength * gridHeight, 6, generationModules.Count, generationModules.Count]; // Instantiate the wave function collapse algorithm.
            possibilities = new bool[gridWidth * gridLength * gridHeight, generationModules.Count]; // Instantiate the possibilities boolean.
            entropy = new int[gridWidth * gridLength * gridHeight]; // Instantiate the entropy integer.

            // For each slot in the grid
            for (int slotIndex = 0; slotIndex < generation.GetLength(0); slotIndex++)
            {
                // For each module
                for (int module = 0; module < generationModules.Count; module++)
                {
                    bool possible = true; // Assume the module is possible

                    // For each face of the cube
                    for (int face = 0; face < 6; face++)
                    {
                        // If the module doesn't connect to the other module
                        if (!GetAdjacent(slotIndex, face, out _))
                        {
                            continue;
                        }

                        bool connected = false; // Assume the module isn't connected

                        // For each face of the block, run through the modules.
                        for (int connectingModule = 0; connectingModule < generationModules.Count; connectingModule++)
                        {
                            // If the module connects to the other module
                            if (generationModules[module].ConnectsTo(generationModules[connectingModule], face))
                            {
                                generation[slotIndex, face, module, connectingModule] = true; // Set the wave booleans to true
                                connected = true; // Set connected to true
                            }
                        }

                        // If the module isn't connected
                        if (!connected)
                        {
                            possible = false; // Set possible to false
                        }
                    }

                    possibilities[slotIndex, module] = possible; // Set the possibilities to the possible boolean. True if connected, false if not.

                    // If the module is possible
                    if (possible)
                    {
                        entropy[slotIndex]++; // Increase the entropy
                    }
                }
            }
        }

        /// <summary>
        /// Check whether the module is connected to the other module.
        /// </summary>
        bool GetAdjacent(int index, int direction, out int adjacent)
        {
            adjacent = -1; // Reset adjacent.

            Vector3 blockCoordinates = Reshape(index); // Get the x, y, and z coordinates of the block.
            int x = Mathf.RoundToInt(blockCoordinates.x); // Get the x coordinate of the block.
            int y = Mathf.RoundToInt(blockCoordinates.y); // Get the y coordinate of the block.
            int z = Mathf.RoundToInt(blockCoordinates.z); // Get the z coordinate of the block.

            // Run through all possible directions and mark the empty slot.
            switch (direction)
            {
                // If the direction is up (north)
                case 0:
                    if (z < gridLength - 1)
                    {
                        adjacent = index + gridWidth;
                        return true;
                    }
                    return false;

                // If the direction is right (east)
                case 1:
                    if (x < gridWidth - 1)
                    {
                        adjacent = index + 1;
                        return true;
                    }
                    return false;

                // If the direction is down (south)
                case 2:
                    if (z > 0)
                    {
                        adjacent = index - gridWidth;
                        return true;
                    }
                    return false;

                // If the direction is left (west)
                case 3:
                    if (x > 0)
                    {
                        adjacent = index - 1;
                        return true;
                    }
                    return false;

                // If the direction is above
                case 4:
                    if (y < gridHeight - 1)
                    {
                        adjacent = index + gridLength * gridWidth;
                        return true;
                    }
                    return false;

                // If the direction is below
                case 5:
                    if (y > 0)
                    {
                        adjacent = index - gridLength * gridWidth;
                        return true;
                    }
                    return false;

                // If the direction is invalid (shouldn't happen)
                default:
                    throw new System.ArgumentException("Invalid direction");
            }
        }

        /// <summary>
        /// Get the x, y, and z coordinates of the block. 
        /// </summary>
        public Vector3 Reshape(int index)
        {
            int y = index / (gridLength * gridWidth); // Get the y coordinate.
            int z = index / gridWidth; // Get the z coordinate.
            int x = index % gridWidth; // Get the x coordinate.

            return new Vector3(x, y, z);// Return the x, y, and z coordinates.
        }

        /// <summary>
        /// Calculate what will fill the next slot.
        /// </summary>
        int NextSlot()
        {
            // If finding the next slot has failed, mark the generation as complete.
            if (failed)
            {
                return -1;
            }

            int result = -1; // Reset variable.
            float min = float.MaxValue; // Reset variable.

            // For each block in the grid
            for (int i = 0; i < generation.GetLength(0); i++)
            {
                int entropy = this.entropy[i]; // Get the entropy of the block.

                // If the entropy is 0, the block is already filled.
                if (entropy > 1 && entropy <= min)
                {
                    float rng = Random.Range(0f, 1f); // Get a random number between 0 and 1.

                    // If the random number + the entropy amount is less than the minimum
                    if (entropy + rng < min)
                    {
                        min = entropy + rng; // Set the minimum to the entropy + random number.
                        result = i; // Set the result to the block index.
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Begin the process of collapsing the wave function algorithm.
        /// </summary>
        async Task Observe(int index)
        {
            List<int> candidates = new List<int>(); // Instantiate a list to contain the possible module generations.

            float result = -1; // Reset variable.
            float min = float.MaxValue; // Reset variable.

            // For each module.
            for (int i = 0; i < generationModules.Count; i++)
            {
                // If the module is possible.
                if (possibilities[index, i])
                {
                    float rng = Random.Range(0f, 1f); // Get a random number between 0 and 1.

                    // If the random number is less than the minimum
                    if (rng < min)
                    {
                        min = rng; // Set the minimum to the random number.
                        result = i; // Set the result to the module index.
                    }

                    candidates.Add(i); // Add the module index to the list of possible modules.
                }
            }

            // For each module in the list of possible modules.
            foreach (int candidate in candidates)
            {
                // If the module does not match the result.
                if (candidate != result)
                {
                    await Propagate(index, candidate); // Propagate the module
                }
            }
        }

        /// <summary>
        /// Propagate the Wave Function Collapse algorithm.
        /// </summary>
        async Task Propagate(int index, int module)
        {
            // Slow generation to be shown, if delay has been enabled.
            if (WFCGenerator.delay > 0 && ++ticks >= WFCGenerator.delay)
            {
                ticks = 0;
                await Task.Delay(10);
            }

            // If the module is not possible.
            if (entropy[index] <= 1)
            {
                failed = true;
                return;
            }

            possibilities[index, module] = false; // Mark the possible module as false.
            entropy[index]--; // Decrease the entropy of the block.

            // For each possible face of a slot.
            for (int slotFaces = 0; slotFaces < 6; slotFaces++)
            {
                // For each module.
                for (int j = 0; j < generationModules.Count; j++)
                {
                    generation[index, slotFaces, module, j] = false; // Mark the slot face of the module as false.

                    // If the slot face is adjacent to another block.
                    if (GetAdjacent(index, slotFaces, out int adjacent))
                    {
                        int opposite = _OPPOSITE[slotFaces]; // Get the opposite face of the slot face.

                        // If a module is possible after taking note of the current module.
                        if (possibilities[adjacent, j])
                        {
                            generation[adjacent, opposite, j, module] = false; // Mark the opposite face of the adjacent block as false.
                            bool possible = false; // Reset variable.

                            // For each possible module.
                            for (int k = 0; k < generationModules.Count; k++)
                            {
                                // If the opposite face of the adjacent block is possible.
                                if (generation[adjacent, opposite, j, k])
                                {
                                    possible = true; // Mark the opposite face of the adjacent block as possible.
                                    break; // Break the loop.
                                }
                            }

                            // If the opposite face of the adjacent block is not a possible match.
                            if (!possible)
                            {
                                await Propagate(adjacent, j); // Re-run this function to propagate the algorithm with the next possible module.
                            }
                        }
                    }
                }
            }

            // If the entropy of the block is 1.
            if (entropy[index] == 1)
            {
                Collapse(index); // Collapse the block. Fill the slot.
            }
        }

        /// <summary>
        /// Collapse the wave function algorithm.
        /// </summary>
        void Collapse(int index)
        {
            int module = -1; // Reset variable.

            // For each module.
            for (int i = 0; i < generationModules.Count; i++)
            {
                // If the module is possible.
                if (possibilities[index, i])
                {
                    module = i; // Set the module to the module index.
                    break; // Break the loop.
                }
            }

            // If a possible module was not found.
            if (module == -1)
            {
                // Mark the generation as failed.
                Debug.LogError("Failed to collapse");
                return;
            }

            InstantiatePrefabs(index, module);
        }

        /// <summary>
        /// Instantiate the prefabs. Implementing randomness.
        /// </summary>
        private void InstantiatePrefabs(int index, int module)
        {
            // If a module was set.
            if (generationModules[module].prefab != null)
            {
                int rng = Random.Range(0, generationModules[module].prefab.Length); // Get a random number between 0 and the length of the module prefab array.

                GameObject item; // Instantiate a new game object.

                if (generationModules[module].prefab[rng] != null)
                {
                    // Instantiate the prefab.
                    item = GameObject.Instantiate(generationModules[module].prefab[rng], gridRoot); // Instantiate the module prefab at the root transform.
                    item.name = generationModules[module].prefab[rng].name;
                }
                else
                {
                    item = new GameObject();
                    item.name = "Empty";
                    item.transform.parent = gridRoot.transform; // Set the parent of the prefab to the root transform.
                }

                Vector3 blockCoordinates = Reshape(index); // Get the x, y, and z coordinates of the block.
                int x = Mathf.RoundToInt(blockCoordinates.x); // Get the x coordinate of the block.
                int y = Mathf.RoundToInt(blockCoordinates.y); // Get the y coordinate of the block.
                int z = Mathf.RoundToInt(blockCoordinates.z); // Get the z coordinate of the block.

                item.transform.localPosition = GetPosition(x, y, z); // Set the position of the module prefab to the open slot.

                if (generationModules[module].rotate180)
                {
                    item.transform.rotation = Quaternion.Euler(-90, 180, 180);
                }
            }
        }

        /// <summary>
        /// Get the position of the slot.
        /// </summary>
        Vector3 GetPosition(int x, int y, int z)
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

        /// <summary>
        /// Draw objects that represent the wave function in empty slots.
        /// </summary>
        public void DrawGenMarkers(GameObject generator)
        {
            // If the generation has not been started or has been completed.
            if (generation == null)
            {
                return;
            }

            Gizmos.color = Color.red; // Set the Wave Function Collapse gizmo color to white.

            // For each slot on the grid.
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    for (int z = 0; z < gridLength; z++)
                    {
                        int index = gridLength * gridWidth * y + gridWidth * z + x; // Get the index of the slot.
                        int entropy = this.entropy[index]; // Get the entropy of the slot.

                        // If the slot is empty.
                        if (entropy == 1)
                        {
                            continue;
                        }

                        Vector3 position = generator.transform.localPosition + GetPosition(x, y, z); // Get the position of the slot.

                        Gizmos.DrawCube(position, Vector3.one * 2f * entropy / generationModules.Count); // Draw a cube at the slot position.
                    }
                }
            }
        }
    }
}