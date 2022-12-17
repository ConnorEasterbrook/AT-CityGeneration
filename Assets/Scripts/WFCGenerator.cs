using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace WFCGenerator
{
    public class WFCGenerator : MonoBehaviour
    {
        public WFCGrid grid;

        [Header("Visual Options")]
        public bool drawGizmos = true;
        public bool drawGenerationMarkers = true;
        public static int delay { get; set; }
        public int chunkAmount = 1;

        private void Start()
        {
            // grid.Generate(gameObject);
            CallGenerate();
        }

        void OnDrawGizmos()
        {
            if (!drawGizmos)
            {
                return;
            }

            if (drawGenerationMarkers)
            {
                grid.DrawGenMarkers(gameObject);
            }
        }

        public void CallGenerate()
        {
            grid.Clear(gameObject);
            Vector2 mapSize = new Vector2();

            for (int x = 0; x < chunkAmount; x++)
            {
                for (int y = 0; y < chunkAmount; y++)
                {
                    mapSize.x = x * grid.gridWidth;
                    mapSize.y = y * grid.gridLength;
                    grid.Generate(gameObject, mapSize);
                }
            }

            // for (int i = 0; i < chunkAmount; i++)
            // {
            //     Debug.Log("Generating chunk " + (i + 1) + " of " + chunkAmount);
            //     grid.Generate(gameObject);
            // }
            // grid.Generate(gameObject);
        }

        private void OnEnable()
        {
            grid.Clear(gameObject);
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
                generator.grid.Clear(generator.gameObject);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}