using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace WFCGenerator
{
    [System.Serializable]
    public class WFCWaveGrid
    {
        static readonly int MAX_ATTEMPTS = 10; // Maximum number of attempts to find a solution.
        static readonly int MAX_ITERATIONS = 10000; // Maximum number of iterations to try to find a solution.
        static readonly int[] OPPOSITE = { 2, 3, 0, 1, 5, 4 };

        [Header("Identity")]
        public string name = "WFCGrid"; // Name of the grid.

        [Header("Variables")]
        public List<WFCModule> modules; // List of modules.

        [Header("Grid")]
        public int width = 5; // Width of the grid.
        public int length = 10; // Length of the grid.
        public int height = 1; // Height of the grid.
        public float blockSize = 5f; // Size of each block.

        // Private variables.
        private Vector3 blockOffset;
        private Transform root;
        private bool[,,,] wave;
        private bool[,] possibilities;
        private int[] entropy;
        private int ticks;
        private bool failed;

        /// <summary>
        /// Core function of the algorithm.
        /// </summary>
        public async void Generate(GameObject generator)
        {
            // For each grid, if generation failed, clear the grid and re-initialize the wave function collapse algorithm.
            for (int forAttempts = 0; forAttempts < MAX_ATTEMPTS; forAttempts++)
            {
                Clear(generator); // Clear the grid.
                Initialize(generator); // Initialize the wave function collapse algorithm.

                // For each iteration, if generation failed, clear the grid and re-initialize the wave function collapse algorithm.
                for (int forIterations = 0; forIterations < MAX_ITERATIONS; forIterations++)
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
            wave = null;
            ticks = 0;
        }

        /// <summary>
        /// Initialize the wave function collapse algorithm.
        /// </summary>
        void Initialize(GameObject generator)
        {
            // Organise in inspector.
            root = new GameObject(name).transform; // Create a new game object with the name of the grid.
            root.parent = generator.transform;  // Set the parent of the game object to the generator.

            // Instantiate variables.
            blockOffset = new Vector3(width * blockSize / 2f, height * blockSize / 2f, length * blockSize / 2f); // Instantiate the block offset.
            wave = new bool[width * length * height, 6, modules.Count, modules.Count]; // Instantiate the wave function collapse algorithm.
            possibilities = new bool[width * length * height, modules.Count]; // Instantiate the possibilities boolean.
            entropy = new int[width * length * height]; // Instantiate the entropy integer.

            // For each block in the grid
            for (int blockIndex = 0; blockIndex < wave.GetLength(0); blockIndex++)
            {
                // For each module
                for (int forModules = 0; forModules < modules.Count; forModules++)
                {
                    bool possible = true; // Assume the module is possible

                    // For each face of the cube
                    for (int forFace = 0; forFace < 6; forFace++)
                    {
                        // If the module doesn't connect to the other module
                        if (!GetAdjacent(blockIndex, forFace, out _))
                        {
                            continue;
                        }

                        bool connected = false; // Assume the module isn't connected

                        // For each face of the block, run through the modules.
                        for (int forFaceModules = 0; forFaceModules < modules.Count; forFaceModules++)
                        {
                            // If the module connects to the other module
                            if (modules[forModules].ConnectsTo(modules[forFaceModules], forFace))
                            {
                                wave[blockIndex, forFace, forModules, forFaceModules] = true; // Set the wave booleans to true
                                connected = true; // Set connected to true
                            }
                        }

                        // If the module isn't connected
                        if (!connected)
                        {
                            possible = false; // Set possible to false
                        }
                    }

                    possibilities[blockIndex, forModules] = possible; // Set the possibilities to the possible boolean. True if connected, false if not.

                    // If the module is possible
                    if (possible)
                    {
                        entropy[blockIndex]++; // Increase the entropy
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
                    if (z < length - 1)
                    {
                        adjacent = index + width;
                        return true;
                    }
                    return false;

                // If the direction is right (east)
                case 1:
                    if (x < width - 1)
                    {
                        adjacent = index + 1;
                        return true;
                    }
                    return false;

                // If the direction is down (south)
                case 2:
                    if (z > 0)
                    {
                        adjacent = index - width;
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
                    if (y < height - 1)
                    {
                        adjacent = index + length * width;
                        return true;
                    }
                    return false;

                // If the direction is below
                case 5:
                    if (y > 0)
                    {
                        adjacent = index - length * width;
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
            int y = index / (length * width); // Get the y coordinate.
            index -= y * length * width; // Remove the y coordinate from the index.
            int z = index / width; // Get the z coordinate.
            int x = index % width; // Get the x coordinate.

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
            for (int i = 0; i < wave.GetLength(0); i++)
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
            for (int i = 0; i < modules.Count; i++)
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
                for (int j = 0; j < modules.Count; j++)
                {
                    wave[index, slotFaces, module, j] = false; // Mark the slot face of the module as false.

                    // If the slot face is adjacent to another block.
                    if (GetAdjacent(index, slotFaces, out int adjacent))
                    {
                        int opposite = OPPOSITE[slotFaces]; // Get the opposite face of the slot face.

                        // If a module is possible after taking note of the current module.
                        if (possibilities[adjacent, j])
                        {
                            wave[adjacent, opposite, j, module] = false; // Mark the opposite face of the adjacent block as false.
                            bool possible = false; // Reset variable.

                            // For each possible module.
                            for (int k = 0; k < modules.Count; k++)
                            {
                                // If the opposite face of the adjacent block is possible.
                                if (wave[adjacent, opposite, j, k])
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
            for (int i = 0; i < modules.Count; i++)
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

            // If a module was set.
            if (modules[module].prefab != null)
            {
                int rng = Random.Range(0, modules[module].prefab.Length); // Get a random number between 0 and the length of the module prefab array.
                GameObject item = GameObject.Instantiate(modules[module].prefab[rng], root); // Instantiate the module prefab at the root transform.

                Vector3 blockCoordinates = Reshape(index); // Get the x, y, and z coordinates of the block.
                int x = Mathf.RoundToInt(blockCoordinates.x); // Get the x coordinate of the block.
                int y = Mathf.RoundToInt(blockCoordinates.y); // Get the y coordinate of the block.
                int z = Mathf.RoundToInt(blockCoordinates.z); // Get the z coordinate of the block.

                item.transform.localPosition = GetPosition(x, y, z); // Set the position of the module prefab to the open slot.
            }
        }

        /// <summary>
        /// Get the position of the slot.
        /// </summary>
        Vector3 GetPosition(int x, int y, int z)
        {
            Vector3 position = new Vector3(
                x * blockSize - blockOffset.x + blockSize,
                y * blockSize - blockOffset.y + blockSize / 2f,
                z * blockSize - blockOffset.z + blockSize / 2f
            );
            return position;
        }

        public void DrawGizmos()
        {
            // If the generation has not been started or has been completed.
            if (wave == null)
            {
                return;
            }

            Gizmos.color = Color.white; // Set the Wave Function Collapse gizmo color to white.

            // For each slot on the grid.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < length; z++)
                    {
                        int index = length * width * y + width * z + x; // Get the index of the slot.
                        int entropy = this.entropy[index]; // Get the entropy of the slot.

                        // If the slot is empty.
                        if (entropy == 1)
                        {
                            continue;
                        }

                        Gizmos.DrawCube(GetPosition(x, y, z), Vector3.one * 2f * entropy / modules.Count); // Draw a cube at the slot position.
                    }
                }
            }
        }
    }
}