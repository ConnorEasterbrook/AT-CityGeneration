using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace WFCGenerator
{
    [CreateAssetMenu(menuName = "WFC/Module")]
    [System.Serializable]
    public class WFCModule : ScriptableObject
    {
        public GameObject[] prefab;
        [HideInInspector] public WFCConnection north;
        [HideInInspector] public WFCConnection east;
        [HideInInspector] public WFCConnection south;
        [HideInInspector] public WFCConnection west;
        [HideInInspector] public WFCConnection above;
        [HideInInspector] public WFCConnection below;

        public bool ConnectsTo(WFCModule other, int direction)
        {
            switch (direction)
            {
                case 0:
                    return south.ConnectsTo(other.north);
                case 1:
                    return west.ConnectsTo(other.east);
                case 2:
                    return north.ConnectsTo(other.south);
                case 3:
                    return east.ConnectsTo(other.west);
                case 4:
                    return above.ConnectsTo(other.below);
                case 5:
                    return below.ConnectsTo(other.above);
                default:
                    throw new System.ArgumentException("Invalid direction");
            }
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(WFCModule))]
    public class WFCModuleEditor : UnityEditor.Editor
    {
        private SerializedObject prefab;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            WFCModule module = (WFCModule)target;

            serializedObject.FindProperty("north.name").stringValue = EditorGUILayout.TextField("North", module.north.name);
            serializedObject.FindProperty("east.name").stringValue = EditorGUILayout.TextField("East", module.east.name);
            serializedObject.FindProperty("south.name").stringValue = EditorGUILayout.TextField("South", module.south.name);
            serializedObject.FindProperty("west.name").stringValue = EditorGUILayout.TextField("West", module.west.name);
            serializedObject.FindProperty("above.name").stringValue = EditorGUILayout.TextField("Above", module.above.name);
            serializedObject.FindProperty("below.name").stringValue = EditorGUILayout.TextField("Below", module.below.name);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
