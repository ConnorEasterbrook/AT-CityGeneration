using System;
using UnityEngine;
using UnityEditor;

namespace WFCGenerator
{
    public class WFCGenerator : MonoBehaviour
    {
        public WFCWaveGrid grid;

        [Header("Visual Options")]
        public bool drawGizmos = true;
        public bool drawGenerationMarkers = true;
        public static int delay { get; set; }

        private void Start()
        {
            grid.Generate(gameObject);
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
        private float generationDelay = 0;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            WFCGenerator generator = (WFCGenerator)target;

            // Create a generation delay slider in the inspector.
            generationDelay = EditorGUILayout.Slider("Delay", generationDelay, 0f, 1f);
            WFCGenerator.delay = generationDelay > 0 ? Mathf.CeilToInt(10f / generationDelay) : 0; // If delay is 0, set delay to 0, otherwise implement a generation delay.

            // Create Generate and Clear buttons in the inspector, next to each other.
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate"))
            {
                generator.grid.Generate(generator.gameObject);
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