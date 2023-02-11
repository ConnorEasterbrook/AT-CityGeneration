using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace WFCGenerator
{
    [CreateAssetMenu(menuName = "WFC/Module")]
    [System.Serializable]
    public class WFCModule : ScriptableObject
    {
        public GameObject[] prefab;
        [HideInInspector] public WFCConnection down;
        [HideInInspector] public WFCConnection left;
        [HideInInspector] public WFCConnection up;
        [HideInInspector] public WFCConnection right;
        [HideInInspector] public WFCConnection above;
        [HideInInspector] public WFCConnection below;
        public bool rotate180;
        public bool randomRotation;
        public bool banForward = false;
        public bool banLeft = false;
        public bool banModules = false;
        public WFCModule[] bannedModules;
        [Range(0, 1)] public float probability = 1;
        public bool randomPlacement = false;
        [HideInInspector] public int connection;

        public bool ConnectsTo(WFCModule other, int direction)
        {
            // Check direction in order down, left, up, right, above, below
            if (direction == 0)
            {
                return down.ConnectsTo(other.up);
            }
            if (direction == 1)
            {
                return left.ConnectsTo(other.right);
            }
            if (direction == 2)
            {
                return up.ConnectsTo(other.down);
            }
            if (direction == 3)
            {
                return right.ConnectsTo(other.left);
            }
            if (direction == 4)
            {
                return above.ConnectsTo(other.below);
            }
            if (direction == 5)
            {
                return below.ConnectsTo(other.above);
            }
            else
            {
                throw new System.ArgumentException("Invalid direction");
            }
        }

        // public bool ConnectsTo(int moduleNumer, int direction, List<WFCModule> modules)
        // {
        //     if (direction == 0)
        //     {
        //         return left.ConnectsTo(moduleNumer, modules);
        //     }
        //     else if (direction == 1)
        //     {
        //         return right.ConnectsTo(moduleNumer, modules);
        //     }
        //     else if (direction == 2)
        //     {
        //         return up.ConnectsTo(moduleNumer, modules);
        //     }
        //     else if (direction == 3)
        //     {
        //         return down.ConnectsTo(moduleNumer, modules);
        //     }
        //     else if (direction == 4)
        //     {
        //         return above.ConnectsTo(moduleNumer, modules);
        //     }
        //     else if (direction == 5)
        //     {
        //         return below.ConnectsTo(moduleNumer, modules);
        //     }
        //     else
        //     {
        //         throw new System.ArgumentException("Invalid direction");
        //     }
        // }

        // public int ReturnConnectionDirection()
        // {
        //     return connection;
        // }

        public bool CheckBannedNeighbour(WFCModule other, int direction)
        {
            if (direction == 0)
            {
                if (banModules)
                {
                    bool banned = false;
                    for (int bannedNeighbourModule = 0; bannedNeighbourModule < bannedModules.Length; bannedNeighbourModule++)
                    {
                        if (down.ConnectsTo(other.up) && bannedModules[bannedNeighbourModule] == other)
                        {
                            banned = true;
                        }
                    }

                    if (down.ConnectsTo(other.up) && !banned)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else if (direction == 1)
            {

                if (banModules)
                {
                    bool banned = false;
                    for (int bannedNeighbourModule = 0; bannedNeighbourModule < bannedModules.Length; bannedNeighbourModule++)
                    {
                        if (left.ConnectsTo(other.right) && bannedModules[bannedNeighbourModule] == other)
                        {
                            banned = true;
                        }
                    }

                    if (left.ConnectsTo(other.right) && !banned)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else if (direction == 2)
            {
                if (banModules)
                {
                    bool banned = false;
                    for (int bannedNeighbourModule = 0; bannedNeighbourModule < bannedModules.Length; bannedNeighbourModule++)
                    {
                        if (up.ConnectsTo(other.down) && bannedModules[bannedNeighbourModule] == other)
                        {
                            banned = true;
                        }
                    }

                    if (up.ConnectsTo(other.down) && !banned)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else if (direction == 3)
            {

                if (banModules)
                {
                    bool banned = false;
                    for (int bannedNeighbourModule = 0; bannedNeighbourModule < bannedModules.Length; bannedNeighbourModule++)
                    {
                        if (right.ConnectsTo(other.left) && bannedModules[bannedNeighbourModule] == other)
                        {
                            banned = true;
                        }
                    }

                    if (right.ConnectsTo(other.left) && !banned)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                throw new System.ArgumentException("Invalid direction");
            }
        }

        public bool CheckDuplicateRestriction(WFCModule other, int direction)
        {
            if (direction == 0)
            {
                if (down.ConnectsTo(other.up) && !banForward)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (direction == 1)
            {
                if (left.ConnectsTo(other.right) && !banLeft)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (direction == 2)
            {
                if (up.ConnectsTo(other.down) && !banForward)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (direction == 3)
            {
                if (right.ConnectsTo(other.left) && !banLeft)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
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

            serializedObject.FindProperty("left.name").stringValue = EditorGUILayout.TextField("Left", module.left.name);
            serializedObject.FindProperty("right.name").stringValue = EditorGUILayout.TextField("Right", module.right.name);
            serializedObject.FindProperty("up.name").stringValue = EditorGUILayout.TextField("Up", module.up.name);
            serializedObject.FindProperty("down.name").stringValue = EditorGUILayout.TextField("Down", module.down.name);
            serializedObject.FindProperty("above.name").stringValue = EditorGUILayout.TextField("Above", module.above.name);
            serializedObject.FindProperty("below.name").stringValue = EditorGUILayout.TextField("Below", module.below.name);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
