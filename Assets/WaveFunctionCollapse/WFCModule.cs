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
        [HideInInspector] public WFCConnection forward;
        [HideInInspector] public WFCConnection right;
        [HideInInspector] public WFCConnection back;
        [HideInInspector] public WFCConnection left;
        [HideInInspector] public WFCConnection above;
        [HideInInspector] public WFCConnection below;
        public bool rotate180;

        public bool ConnectsTo(WFCModule other, int direction)
        {
            switch (direction)
            {
                case 0:
                    return back.ConnectsTo(other.forward);
                case 1:
                    return left.ConnectsTo(other.right);
                case 2:
                    return forward.ConnectsTo(other.back);
                case 3:
                    return right.ConnectsTo(other.left);
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
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            WFCModule module = (WFCModule)target;

            serializedObject.FindProperty("forward.name").stringValue = EditorGUILayout.TextField("Forward", module.forward.name);
            serializedObject.FindProperty("right.name").stringValue = EditorGUILayout.TextField("Right", module.right.name);
            serializedObject.FindProperty("back.name").stringValue = EditorGUILayout.TextField("Back", module.back.name);
            serializedObject.FindProperty("left.name").stringValue = EditorGUILayout.TextField("Left", module.left.name);
            serializedObject.FindProperty("above.name").stringValue = EditorGUILayout.TextField("Above", module.above.name);
            serializedObject.FindProperty("below.name").stringValue = EditorGUILayout.TextField("Below", module.below.name);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
