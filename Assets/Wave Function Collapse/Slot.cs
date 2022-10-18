using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WaveFunctionCollapse
{
    //* The term "this" is used on certain lines because each slot requires it's own calculations. If "this" was not added then slots that follow the first one would build from the set variables.
    [System.Serializable]
    public class Slot
    {
        private Generator generator; // The generator that this slot belongs to.
        private Vector3 slotPosition; // The position of this slot in the grid.
        private List<int> modules; // The list of modules that can occupy this slot.
        private Module moduleScript; // The module that is currently occupying this slot.
        private Slot[] neighbouringSlots; // The neighbouring slots.
        public int[][] PossibleNeighbours; // The possible neighbours for each module.
        public bool isCollapsed => moduleScript != null; // Whether or not this slot is collapsed.
        public int getEntropy => modules.Count; // The entropy of this slot.

        /// <summary>
        /// Constructor for the slot class.
        /// </summary>
        public Slot(int x, int y, int z, Generator generator)
        {
            slotPosition.x = x;
            slotPosition.y = y;
            slotPosition.z = z;
            this.generator = generator;
            modules = new List<int>(Enumerable.Range(0, generator.modules.Length));
        }

        /// <summary>
        /// Get the adjacent slots.
        /// </summary>
        public void GetAdjacent()
        {
            // Get all adjacent slots
            this.neighbouringSlots = new Slot[6]; // 6 directions
            // Left
            if (this.slotPosition.x > 0)
            {
                this.neighbouringSlots[0] = generator.grid[(int)this.slotPosition.x - 1, (int)this.slotPosition.y, (int)this.slotPosition.z];
            }
            // Down
            if (this.slotPosition.y > 0)
            {
                this.neighbouringSlots[1] = generator.grid[(int)this.slotPosition.x, (int)this.slotPosition.y - 1, (int)this.slotPosition.z];
            }
            // Back
            if (this.slotPosition.z > 0)
            {
                this.neighbouringSlots[2] = generator.grid[(int)this.slotPosition.x, (int)this.slotPosition.y, (int)this.slotPosition.z - 1];
            }
            // Right
            if (this.slotPosition.x < generator.gridSize.x - 1)
            {
                this.neighbouringSlots[3] = generator.grid[(int)this.slotPosition.x + 1, (int)this.slotPosition.y, (int)this.slotPosition.z];
            }
            // Up
            if (this.slotPosition.y < generator.gridSize.y - 1)
            {
                this.neighbouringSlots[4] = generator.grid[(int)this.slotPosition.x, (int)this.slotPosition.y + 1, (int)this.slotPosition.z];
            }
            // Forward
            if (this.slotPosition.z < generator.gridSize.z - 1)
            {
                this.neighbouringSlots[5] = generator.grid[(int)this.slotPosition.x, (int)this.slotPosition.y, (int)this.slotPosition.z + 1];
            }
        }

        /// <summary>
        /// In the case of having no modules to collapse into, choose a random one.
        /// </summary>
        public void RandomCollapse()
        {
            List<int> collapseCandidates = modules.ToList(); // The list of modules that can be collapsed.

            // Get the max possible entropy of a cell.
            float maxEntropy = new float();
            for (int i = 0; i < generator.modules.Length; i++)
            {
                maxEntropy += generator.modules[i].Prototype.Probability;
            }

            float rng = Random.Range(0f, maxEntropy); // Generate a random entropy number
            float probability = 0; // Initialize the current probability of the module being collapsed.

            // For each collapse candidate, add the probability of the module to the current probability.
            foreach (var candidate in collapseCandidates)
            {
                probability += generator.modules[candidate].Prototype.Probability;

                // If the current probability is greater than the random entropy number, collapse the slot.
                if (probability >= rng)
                {
                    Collapse(candidate);
                    return;
                }
            }

            // If no module was collapsed, collapse the first module in the list.
            Collapse(collapseCandidates.First());
        }

        /// <summary>
        /// Collapse this slot.
        /// </summary>
        public void Collapse(int index)
        {
            this.moduleScript = generator.modules[index];
            InstantiatePrefab();

            var toRemove = this.modules.ToList();
            toRemove.Remove(index);
            RemoveModules(toRemove);
        }

        /// <summary>
        /// Instantiate the module prefab in this slot.
        /// </summary>
        public void InstantiatePrefab()
        {
            // Spawn the module. If there are multiple variants, use a random one.
            GameObject modulePrefab = moduleScript.Prototype.useVariants ? GameObject.Instantiate(moduleScript.Prototype.getModel()) : GameObject.Instantiate(moduleScript.Prototype.gameObject);

            ModulePrototype prototype = modulePrefab.GetComponent<ModulePrototype>(); // Get the module prototype.
            GameObject.DestroyImmediate(prototype); // Destroy the module prototype.
            modulePrefab.transform.parent = generator.transform; // Set the parent of the module to the generator.
            modulePrefab.transform.position = generator.GetSlotPosition(slotPosition.x, slotPosition.y, slotPosition.z); // Set the position of the module.

            Quaternion rotation = Quaternion.Euler(Vector3.up * 90f * moduleScript.Rotation); // Get the rotation of the module.

            if (moduleScript.Prototype.xRotate90)
            {
                modulePrefab.transform.rotation = Quaternion.Euler(-90, rotation.y, rotation.z); // Set the rotation of the module.
            }
            else
            {
                modulePrefab.transform.rotation = rotation;
            }
        }

        /// <summary>
        /// Remove the modules that are not possible to collapse into.
        /// </summary>
        public void RemoveModules(List<int> modules)
        {
            // Create a list that consists of each neighbour.
            List<int>[] validNeighbourModules = new List<int>[6]; 

            // For each neighbour, create a list of modules that can be collapsed into.
            for (int i = 0; i < 6; i++)
            {
                validNeighbourModules[i] = new List<int>();
            }

            // For each module in the list of modules
            foreach (int module in modules)
            {
                // If the slot module does not contain the current module, continue.
                if (!this.modules.Contains(module))
                {
                    continue;
                }

                // For each neighbour
                for (int slotSide = 0; slotSide < 6; slotSide++)
                {
                    // For each module in the neighbour's list of modules
                    foreach (int possibleNeighbour in generator.modules[module].PossibleNeighbours[slotSide])
                    {
                        // If the neighbour's list of modules contains a valid neighbour, add it to the list of affected neighbours.
                        if (this.PossibleNeighbours[slotSide][possibleNeighbour] == 1)
                        {
                            validNeighbourModules[slotSide].Add(possibleNeighbour);
                        }

                        this.PossibleNeighbours[slotSide][possibleNeighbour]--; // Remove the module from the neighbour's list of modules.
                    }
                }

                this.modules.Remove(module); // Remove the module from the slot's list of modules.
            }

            for (int slotSide = 0; slotSide < 6; slotSide++)
            {
                if (validNeighbourModules[slotSide].Any() && this.neighbouringSlots[slotSide] != null)
                {
                    this.neighbouringSlots[slotSide].RemoveModules(validNeighbourModules[slotSide]);
                }
            }
        }
    }
}
