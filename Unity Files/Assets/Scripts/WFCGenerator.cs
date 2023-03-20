using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WFCGenerator
{
    public class WFCGenerator : MonoBehaviour
    {
        public WFCChunk chunkGrid; // The script that calculates each chunk generation, forming the map grid

        [Header("Visual Options")]
        public bool drawGizmos = true; // Draw the generation markers in the scene view
        public bool drawGenerationMarkers = true; // Draw the generation markers in the scene view
        [Range(0, 4)] public int chunkAmount = 1; // The amount of chunks to generate in each direction from the center chunk
        [Range(0, 2)] public float delay = 1; // The delay between each chunk generation
        public Vector3 gridSize = new Vector3(16, 16, 1); // The size of the grid
        public float slotSize = 10; // The size of each slot in the grid
        public GameObject mapParent; // The parent object of the map
        public GameObject player; // The player object

        [Header("Modules")]
        public List<WFCModule> generationModules; // List of modules.

        public static Dictionary<Vector2, GameObject> chunks = new Dictionary<Vector2, GameObject>(); // A dictionary of all the chunks generated
        private List<GameObject> chunkList = new List<GameObject>(); // A list of all the chunks generated
        private bool initialGenerationComplete = false; // Has the initial generation completed?

        public static WFCGenerator instance; // The instance of the WFCGenerator class

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(instance);
            }
        }

        private void Start()
        {
            Generate(); // Generate the map
        }

        private void Update()
        {
            //! This is a very inefficient way of doing this, but it works for now. A possible solution would be to use a quadtree to store the chunks and only check the chunks in the player's view.
            // Check if the chunks are within the player's view distance, and disable them if they are not.
            foreach (GameObject chunk in chunkList)
            {
                if (Vector3.Distance(player.transform.position, chunk.transform.position) > 300)
                {
                    chunk.SetActive(false);
                }
                else
                {
                    chunk.SetActive(true);
                }
            }

            float chunkSize = gridSize.x * slotSize; // Calculate the size of each chunk

            // Calculate the current chunk position
            int currentChunkPosX = Mathf.RoundToInt(player.transform.position.x / chunkSize);
            int currentChunkPosZ = Mathf.RoundToInt(player.transform.position.z / chunkSize);

            int chunksInDistance = Mathf.RoundToInt(100 / chunkSize); // Calculate the distance of chunk generation from the player

            if (initialGenerationComplete || chunkAmount == 0)
            {
                // Generate chunks in a square around the player
                for (int zOffset = -chunksInDistance; zOffset <= chunksInDistance; zOffset++)
                {
                    for (int xOffset = -chunksInDistance; xOffset <= chunksInDistance; xOffset++)
                    {
                        Vector2 currentChunkPos = new Vector2((currentChunkPosX + xOffset) * chunkSize, (currentChunkPosZ + zOffset) * chunkSize); // Calculate the current chunk position

                        // Generate a new chunk if it doesn't exist
                        if (!chunks.ContainsKey(currentChunkPos))
                        {
                            chunkGrid = new WFCChunk(gameObject, mapParent, slotSize, generationModules, currentChunkPos); // Create a new chunk grid

                            chunkGrid.Generate(currentChunkPos / 10, gridSize); // Generate the chunk

                            chunks.Add(currentChunkPos, chunkGrid.ReturnChunk()); // Add the chunk to the dictionary
                            chunkList.Add(chunkGrid.ReturnChunk()); // Add the chunk to the list
                        }
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
            {
                return;
            }

            if (drawGenerationMarkers)
            {
                chunkGrid.DrawGenMarkers(gameObject);
            }
        }

        /// <summary>
        /// Generate the map.
        /// </summary>
        private void Generate()
        {
            // If the map parent is null, set it to the game object this script is attached to.
            if (mapParent == null)
            {
                mapParent = gameObject;
            }

            ClearVariables(); // Clear the variables
            /*StartCoroutine(GenerateChunks());*/
            CallGenerate();
        }

        private IEnumerator GenerateChunks()
        {
            for (int x = -chunkAmount; x < chunkAmount + 1; x++)
            {
                Debug.Log("Generating chunk row " + x);
                for (int y = -chunkAmount; y < chunkAmount + 1; y++)
                {
                    Debug.Log("Generating chunk " + x + ", " + y);
                    yield return new WaitForSeconds(delay);

                    Vector2 mapSize = new Vector2();
                    Vector2 chunkPos = new Vector2(mapSize.x * slotSize, mapSize.y * slotSize); // Calculate the chunk position

                    chunkGrid = new WFCChunk(gameObject, mapParent, slotSize, generationModules, chunkPos); // Create a new chunk grid
                    mapSize.x = x * gridSize.x;
                    mapSize.y = y * gridSize.z;
                    chunkGrid.Generate(mapSize, gridSize); // Pass the map size to the chunk grid script

                    chunks.Add(chunkPos, chunkGrid.ReturnChunk()); // Add the chunk to the dictionary
                    chunkList.Add(chunkGrid.ReturnChunk()); // Add the chunk to the list

                    if (x == chunkAmount && y == chunkAmount)
                    {
                        initialGenerationComplete = true;
                    }
                }
            }
        }

        /// <summary>
        /// Generate the map when called from the editor.
        /// </summary>
        public void CallGenerate()
        {
            // If the map parent is null, set it to the game object this script is attached to.
            if (mapParent == null)
            {
                mapParent = gameObject;
            }

            ClearVariables(); // Clear the variables

            for (int x = -chunkAmount; x < chunkAmount + 1; x++)
            {
                for (int y = -chunkAmount; y < chunkAmount + 1; y++)
                {
                    Vector2 mapSize = new Vector2();
                    mapSize.x = x * gridSize.x;
                    mapSize.y = y * gridSize.z;
                    Vector2 chunkPos = new Vector2(mapSize.x * slotSize, mapSize.y * slotSize); // Calculate the chunk position

                    chunkGrid = new WFCChunk(gameObject, mapParent, slotSize, generationModules, chunkPos); // Create a new chunk grid
                    chunkGrid.Generate(mapSize, gridSize); // Pass the map size to the chunk grid script

                    chunks.Add(chunkPos, chunkGrid.ReturnChunk()); // Add the chunk to the dictionary
                    chunkList.Add(chunkGrid.ReturnChunk()); // Add the chunk to the list

                    if (x == chunkAmount && y == chunkAmount)
                    {
                        initialGenerationComplete = true;
                    }
                }
            }
        }

        private void ClearVariables()
        {
            chunkGrid.Clear(mapParent);
            chunks.Clear();
            chunkList.Clear();
        }

        public bool GetDrawGizmos()
        {
            return drawGizmos;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WFCGenerator))]
    public class WFCGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            WFCGenerator generator = (WFCGenerator)target;

            // Create Generate and Clear buttons in the inspector, next to each other.
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate"))
            {
                // generator.grid.Generate(generator.gameObject);
                generator.CallGenerate();
            }

            if (GUILayout.Button("Clear"))
            {
                generator.chunkGrid.Clear(generator.mapParent);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}