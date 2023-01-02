using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace WFCGenerator
{
    public class WFCGenerator : MonoBehaviour
    {
        public WFCChunk chunkGrid;

        [Header("Visual Options")]
        public bool drawGizmos = true;
        public bool drawGenerationMarkers = true;
        public static int delay { get; set; }
        public int chunkAmount = 1;
        public float slotSize = 10;
        public GameObject mapParent;
        public GameObject player;

        private Dictionary<Vector2, GameObject> chunks = new Dictionary<Vector2, GameObject>();
        private List<GameObject> chunkList = new List<GameObject>();

        private void Start()
        {
            CallGenerate();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CallGenerate();
            }

            //! This is a very inefficient way of doing this, but it works for now. A possible solution would be to use a quadtree to store the chunks and only check the chunks in the player's view.
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

        public void CallGenerate()
        {
            if (mapParent == null)
            {
                mapParent = gameObject;
            }

            ClearVariables();

            Vector2 mapSize = new Vector2();

            for (int x = -chunkAmount; x < chunkAmount + 1; x++)
            {
                for (int y = -chunkAmount; y < chunkAmount + 1; y++)
                {
                    mapSize.x = x * chunkGrid.gridWidth;
                    mapSize.y = y * chunkGrid.gridLength;
                    chunkGrid.Generate(gameObject, mapSize, mapParent, slotSize);

                    Vector2 chunkPos = new Vector2(mapSize.x * slotSize, mapSize.y * slotSize);

                    chunks.Add(chunkPos, chunkGrid.ReturnChunk());
                    chunkList.Add(chunkGrid.ReturnChunk());
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