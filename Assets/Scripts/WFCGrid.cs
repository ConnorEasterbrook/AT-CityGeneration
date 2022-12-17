using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
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
        public Vector2 gridOffset = new Vector2(0, 0); // The offset of the grid.

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

        /// <summary>
        /// Core function of the algorithm.
        /// </summary>
        public void Generate(GameObject generator, Vector2 gridPos)
        {
            gridOffset = gridPos;
            Initialize(generator); // Initialize the wave function collapse algorithm.

            // For each iteration, if generation failed, clear the grid and re-initialize the wave function collapse algorithm.
            for (int iteration = 0; iteration < _MAX_ITERATIONS; iteration++)
            {
                int index = NextSlot(); // Get the next slot.

                // If next slot is found, propagate the wave function collapse algorithm.
                if (index >= 0)
                {
                    FindPossibleModules(index);
                }
                else
                {
                    // If the next slot is not found, generation is successful.
                    return;
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
        }

        /// <summary>
        /// Initialize the wave function collapse algorithm.
        /// </summary>
        void Initialize(GameObject generator)
        {
            // Organise in inspector.
            gridRoot = new GameObject(name).transform; // Create a new game object with the name of the grid.
            gridRoot.parent = generator.transform;  // Set the parent of the game object to the generator.
            // gridRoot.position = new Vector3(generator.transform.position.x, generator.transform.position.y + ((slotSize * gridHeight) / 2) - (slotSize / 2), generator.transform.position.z); // Set the root x, z positions to the generator and keep the bottom layer at the same height as the generator yPos.
            gridRoot.position = new Vector3(gridOffset.x * (slotSize), 0, gridOffset.y * (slotSize));
            gridRoot.rotation = generator.transform.rotation; // Set the rotation of the game object to the generator.

            // Initialize variables.
            slotOffset = new Vector3(gridWidth * slotSize / 2f, gridHeight * slotSize / 2f, gridLength * slotSize / 2f); // Initialize the block offset.
            generation = new bool[gridWidth * gridLength * gridHeight, 6, generationModules.Count, generationModules.Count]; // Initialize the wave function collapse algorithm.
            possibilities = new bool[gridWidth * gridLength * gridHeight, generationModules.Count]; // Initialize the possibilities boolean.
            entropy = new int[gridWidth * gridLength * gridHeight]; // Initialize the entropy integer.

            // For each slot in the grid
            for (int currentSlot = 0; currentSlot < generation.GetLength(0); currentSlot++)
            {
                // For each module
                for (int slotModule = 0; slotModule < generationModules.Count; slotModule++)
                {
                    bool possible = true; // Assume the module is possible

                    // For each face of the cube
                    for (int neighbourSlot = 0; neighbourSlot < 6; neighbourSlot++)
                    {
                        // If the module doesn't connect to the other module
                        if (!GetAdjacent(currentSlot, neighbourSlot, out _))
                        {
                            continue;
                        }

                        bool connected = false; // Assume the module isn't connected

                        // For each face of the block, run through the modules.
                        for (int neighbourSlotModule = 0; neighbourSlotModule < generationModules.Count; neighbourSlotModule++)
                        {
                            // If the module connects to the other module
                            if (generationModules[slotModule].ConnectsTo(generationModules[neighbourSlotModule], neighbourSlot))
                            {
                                // If the module has enabled banning of specific modules, check this module is not banned.
                                if (generationModules[slotModule].CheckBannedNeighbour(generationModules[neighbourSlotModule], neighbourSlot))
                                {
                                    // If them modules differ then continue, else if the module has banned duplicates of itself, check this module is not a duplicate.
                                    if (generationModules[slotModule] != generationModules[neighbourSlotModule])
                                    {
                                        generation[currentSlot, neighbourSlot, slotModule, neighbourSlotModule] = true; // Set the wave booleans to true
                                        connected = true; // Set connected to true
                                    }
                                    else
                                    {
                                        if (generationModules[slotModule].CheckDuplicateRestriction(generationModules[neighbourSlotModule], neighbourSlot))
                                        {
                                            generation[currentSlot, neighbourSlot, slotModule, neighbourSlotModule] = true; // Set the wave booleans to true
                                            connected = true; // Set connected to true
                                        }
                                    }
                                }
                            }
                        }

                        // If the module isn't connected
                        if (!connected)
                        {
                            possible = false; // Set possible to false
                        }
                    }

                    possibilities[currentSlot, slotModule] = possible; // Set the possibilities to the possible boolean. True if connected, false if not.

                    // If the module is possible
                    if (possible)
                    {
                        entropy[currentSlot]++; // Increase the entropy
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
        /// Begin the process of collapsing the wave function algorithm by finding the possible modules.
        /// </summary>
        void FindPossibleModules(int slotNumber)
        {
            List<int> candidates = new List<int>(); // Instantiate a list to contain the possible module generations.

            int result = -1; // Reset variable.
            int randomModule = 0; // Set randomModule to the first module by default in case there are no possible modules.

            // For each module.
            for (int module = 0; module < generationModules.Count; module++)
            {
                // If the module is possible.
                if (possibilities[slotNumber, module])
                {
                    int tempRNG = Mathf.RoundToInt(Random.Range(0, generationModules[module].probability * 100));

                    // If the random number is less than the minimum
                    if (tempRNG > randomModule)
                    {
                        randomModule = tempRNG; // Set the minimum to the random number.
                        result = module; // Set the result to the module index.
                    }

                    candidates.Add(module); // Add the module index to the list of possible modules.
                }
            }

            // For each module in the list of possible modules.
            foreach (int candidate in candidates)
            {
                // If the module does not match the result.
                if (candidate != result)
                {
                    Propagate(slotNumber, candidate); // Propagate the module
                }
            }
        }

        /// <summary>
        /// Propagate the Wave Function Collapse algorithm.
        /// </summary>
        void Propagate(int slotNumber, int module)
        {
            // If the module is not possible.
            if (entropy[slotNumber] <= 1)
            {
                failed = true;
                return;
            }

            possibilities[slotNumber, module] = false; // Mark the possible module as false.
            entropy[slotNumber]--; // Decrease the entropy of the block.

            // For each possible face of a slot.
            for (int slotFaces = 0; slotFaces < 6; slotFaces++)
            {
                // For each module.
                for (int slotModule = 0; slotModule < generationModules.Count; slotModule++)
                {
                    generation[slotNumber, slotFaces, module, slotModule] = false; // Mark the slot face of the module as false.

                    // If the slot face is adjacent to another block.
                    if (GetAdjacent(slotNumber, slotFaces, out int currentSlot))
                    {
                        int neighbourSlot = _OPPOSITE[slotFaces]; // Get the opposite face of the slot face.

                        // If a module is possible after taking note of the current module.
                        if (possibilities[currentSlot, slotModule])
                        {
                            generation[currentSlot, neighbourSlot, slotModule, module] = false; // Mark the opposite face of the adjacent block as false.
                            bool possible = false; // Reset variable.

                            // For each possible module.
                            for (int neighbourSlotModule = 0; neighbourSlotModule < generationModules.Count; neighbourSlotModule++)
                            {
                                // If the opposite face of the adjacent block is possible.
                                if (generation[currentSlot, neighbourSlot, slotModule, neighbourSlotModule])
                                {
                                    possible = true; // Mark the opposite face of the adjacent block as possible.
                                    break; // Break the loop.
                                }
                            }

                            // If the opposite face of the adjacent block is not a possible match.
                            if (!possible)
                            {
                                Propagate(currentSlot, slotModule); // Re-run this function to propagate the algorithm with the next possible module.
                            }
                        }
                    }
                }
            }

            // If the entropy of the block is 1.
            if (entropy[slotNumber] == 1)
            {
                Collapse(slotNumber); // Collapse the block. Fill the slot.
            }
        }

        /// <summary>
        /// Collapse the wave function algorithm.
        /// </summary>
        void Collapse(int slotNumber)
        {
            int module = -1; // Reset variable.

            // For each module.
            for (int i = 0; i < generationModules.Count; i++)
            {
                // If the module is possible. Possibilities have been reduced to only one at this point.
                if (possibilities[slotNumber, i])
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

            InstantiatePrefabs(slotNumber, module);
        }

        /// <summary>
        /// Instantiate the prefabs. Implementing randomness.
        /// </summary>
        private void InstantiatePrefabs(int index, int module)
        {
            // If a module was set.
            if (generationModules[module].prefab != null)
            {
                int randomPrefab = Random.Range(0, generationModules[module].prefab.Length); // Get a random number between 0 and the length of the module prefab array.

                GameObject item; // Instantiate a new game object.
                // float tempRotation = 0; // Reset variable.
                Quaternion rotation = Quaternion.identity; // Reset variable.

                if (generationModules[module].prefab[randomPrefab] != null)
                {
                    // Instantiate the prefab.
                    item = GameObject.Instantiate(generationModules[module].prefab[randomPrefab], gridRoot); // Instantiate the module prefab at the root transform.
                    item.name = generationModules[module].prefab[randomPrefab].name + " | " + index; // Set the name of the prefab to the name of the prefab + the index of the slot.
                    // tempRotation = generationModules[module].prefab[randomPrefab].transform.localRotation.x;
                }
                else
                {
                    item = new GameObject();
                    item.name = "Empty | " + index;
                    item.transform.parent = gridRoot.transform; // Set the parent of the prefab to the root transform.
                }

                Vector3 slotCoordinates = Reshape(index); // Get the x, y, and z coordinates of the slot.
                int x = Mathf.RoundToInt(slotCoordinates.x); // Get the x coordinate of the slot.
                int y = Mathf.RoundToInt(slotCoordinates.y); // Get the y coordinate of the slot.
                int z = Mathf.RoundToInt(slotCoordinates.z); // Get the z coordinate of the slot.

                if (generationModules[module].randomPlacement)
                {
                    float randX = Random.Range(-5, 5);
                    float randZ = Random.Range(-5, 5);
                    Vector3 position = GetPosition(x, y, z); // Set the position of the module prefab to the open slot.
                    position.x += randX;
                    position.z += randZ;
                    item.transform.localPosition = position; // Set the position of the module prefab to the open slot.
                }
                else
                {
                    item.transform.localPosition = GetPosition(x, y, z); // Set the position of the module prefab to the open slot.
                }

                // item.transform.localRotation = Quaternion.Euler(tempRotation, 90 * generationModules[module].ReturnConnectionDirection(), item.transform.localRotation.z); // Set the rotation of the module prefab to the open slot.


                if (generationModules[module].rotate180)
                {
                    rotation.y = 180;
                }

                if (generationModules[module].randomRotation)
                {
                    rotation.y = Random.Range(0, 360); // Rotate the prefab randomly.
                }

                item.transform.localRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z); // Set the rotation of the module prefab to the open slot.
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

            // Gizmos.color = Color.black; // Set the Wave Function Collapse gizmo color to white.
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
            style.fontSize = 20;
            style.fontStyle = FontStyle.Bold;

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

                        Handles.Label(position, entropy.ToString(), style); // Draw the entropy of the slot.
                    }
                }
            }
        }
    }
}