using UnityEngine;
using UnityEditor;

namespace WFCGenerator
{
    public class WFCGenerator : MonoBehaviour
    {
        public WFCGrid[] grid;

        [Header("Visual Options")]
        public bool drawGizmos = true;
        public bool drawGenerationMarkers = true;
        public static int delay { get; set; }
        public int chunkAmount = 1;

        private void Start()
        {
            foreach (WFCGrid g in grid)
            {
                g.Generate(gameObject);
            }

            // grid.Generate(gameObject);
        }

        void OnDrawGizmos()
        {
            if (!drawGizmos)
            {
                return;
            }

            if (drawGenerationMarkers)
            {
                foreach (WFCGrid g in grid)
                {
                    g.DrawGenMarkers(gameObject);
                }
                // grid.DrawGenMarkers(gameObject);
            }
        }

        private void OnEnable()
        {
            foreach (WFCGrid g in grid)
            {
                g.Clear(gameObject);
            }
            // grid.Clear(gameObject);
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

            // WFCGenerator.delay = generationDelay > 0 ? Mathf.CeilToInt(10f / generationDelay) : 0; // If delay is 0, set delay to 0, otherwise implement a generation delay.

            // Create Generate and Clear buttons in the inspector, next to each other.
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate"))
            {
                foreach (WFCGrid g in generator.grid)
                {
                    g.Generate(generator.gameObject);
                }
                // generator.grid[WFCGenerator.index].Generate(generator.gameObject);
            }

            if (GUILayout.Button("Clear"))
            {
                foreach (WFCGrid g in generator.grid)
                {
                    g.Clear(generator.gameObject);
                }
                // generator.grid.Clear(generator.gameObject);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}