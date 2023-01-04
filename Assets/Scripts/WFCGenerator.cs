using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;

namespace WFCGenerator
{
    public class WFCGenerator : MonoBehaviour
    {
        public WFCChunk chunkGrid; // The script that calculates each chunk generation, forming the map grid

        [Header("Visual Options")]
        public bool drawGizmos = true; // Draw the generation markers in the scene view
        public bool drawGenerationMarkers = true; // Draw the generation markers in the scene view
        public static int delay { get; set; }
        public int chunkAmount = 1; // The amount of chunks to generate in each direction from the center chunk
        public float slotSize = 10; // The size of each slot in the grid
        public GameObject mapParent; // The parent object of the map
        public GameObject player; // The player object

        private Dictionary<Vector2, GameObject> chunks = new Dictionary<Vector2, GameObject>(); // A dictionary of all the chunks generated
        private List<GameObject> chunkList = new List<GameObject>(); // A list of all the chunks generated
        private bool initialGenerationComplete = false; // Has the initial generation completed?

        private void Start()
        {
            CallGenerate(); // Generate the map
        }

        private void Update()
        {
            //! This is a very inefficient way of doing this, but it works for now. A possible solution would be to use a quadtree to store the chunks and only check the chunks in the player's view.
            // Check if the chunks are within the player's view distance, and disable them if they are not.
            foreach (GameObject chunk in chunkList)
            {
                if (Vector3.Distance(player.transform.position, chunk.transform.position) > 200)
                {
                    chunk.SetActive(false);
                }
                else
                {
                    chunk.SetActive(true);
                }
            }

            float chunkSize = chunkGrid.gridWidth * slotSize; // Calculate the size of each chunk

            // Calculate the current chunk position
            int currentChunkPosX = Mathf.RoundToInt(player.transform.position.x / chunkSize);
            int currentChunkPosZ = Mathf.RoundToInt(player.transform.position.z / chunkSize);

            int chunksInDistance = Mathf.RoundToInt(100 / chunkSize); // Calculate the distance of chunk generation from the player

            if (initialGenerationComplete)
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
                            chunkGrid.Generate(gameObject, currentChunkPos / 10, mapParent, slotSize); // Generate the chunk

                            chunks.Add(currentChunkPos, chunkGrid.ReturnChunk()); // Add the chunk to the dictionary
                            chunkList.Add(chunkGrid.ReturnChunk()); // Add the chunk to the list
                        }
                    }
                }
            }
        }

        void OnDrawGizmos()
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
        public void CallGenerate()
        {
            // If the map parent is null, set it to the game object this script is attached to.
            if (mapParent == null)
            {
                mapParent = gameObject;
            }

            ClearVariables(); // Clear the variables

            // // Generate the chunks. Start from negative chunkAmount and go to chunkAmount + 1, so that the center chunk is generated. 
            // for (int x = -chunkAmount; x < chunkAmount + 1; x++)
            // {
            //     for (int y = -chunkAmount; y < chunkAmount + 1; y++)
            //     {
            StartCoroutine(GenerateChunks());

            // Check if coroutine is finished


            // Vector2 mapSize = new Vector2();

            // mapSize.x = x * chunkGrid.gridWidth;
            // mapSize.y = y * chunkGrid.gridLength;
            // chunkGrid.Generate(gameObject, mapSize, mapParent, slotSize); // Pass the map size to the chunk grid script

            // Vector2 chunkPos = new Vector2(mapSize.x * slotSize, mapSize.y * slotSize); // Calculate the chunk position
            // chunks.Add(chunkPos, chunkGrid.ReturnChunk()); // Add the chunk to the dictionary
            // chunkList.Add(chunkGrid.ReturnChunk()); // Add the chunk to the list
            // }
            // }
        }

        private IEnumerator GenerateChunks()
        {
            for (int x = -chunkAmount; x < chunkAmount + 1; x++)
            {
                for (int y = -chunkAmount; y < chunkAmount + 1; y++)
                {
                    yield return new WaitForSeconds(delay);

                    Vector2 mapSize = new Vector2();

                    mapSize.x = x * chunkGrid.gridWidth;
                    mapSize.y = y * chunkGrid.gridLength;
                    chunkGrid.Generate(gameObject, mapSize, mapParent, slotSize); // Pass the map size to the chunk grid script

                    Vector2 chunkPos = new Vector2(mapSize.x * slotSize, mapSize.y * slotSize); // Calculate the chunk position
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
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(WFCGenerator))]
    public class WFCGeneratorEditor : Editor
    {
        private GUIStyle headerStyle = new GUIStyle();
        private int generationDelay = 0;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            WFCGenerator generator = (WFCGenerator)target;

            // Create a generation delay slider in the inspector.
            generationDelay = EditorGUILayout.IntSlider("Delay", generationDelay, 1, 100);
            WFCGenerator.delay = generationDelay;

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