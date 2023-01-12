/**
 * Copyright 2022 Connor Easterbrook
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
*/

using UnityEngine;
using UnityEditor;

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
            if (direction == 0)
            {
                // if (back.ConnectsTo(other.forward))
                // {
                //     connection = 0;
                //     return true;
                // }
                // else if (right.ConnectsTo(other.forward))
                // {
                //     connection = 1;
                //     return true;
                // }
                // else if (forward.ConnectsTo(other.forward))
                // {
                //     connection = 2;
                //     return true;
                // }
                // else if (left.ConnectsTo(other.forward))
                // {
                //     connection = 3;
                //     return true;
                // }
                // else
                // {
                //     return false;
                // }

                return back.ConnectsTo(other.forward);
            }
            else if (direction == 1)
            {
                // if (left.ConnectsTo(other.right))
                // {
                //     connection = 0;
                //     return true;
                // }
                // else if (back.ConnectsTo(other.right))
                // {
                //     connection = 1;
                //     return true;
                // }
                // else if (right.ConnectsTo(other.right))
                // {
                //     connection = 2;
                //     return true;
                // }
                // else if (forward.ConnectsTo(other.right))
                // {
                //     connection = 3;
                //     return true;
                // }
                // else
                // {
                //     return false;
                // }

                return left.ConnectsTo(other.right);
            }
            else if (direction == 2)
            {
                // if (forward.ConnectsTo(other.back))
                // {
                //     connection = 0;
                //     return true;
                // }
                // else if (left.ConnectsTo(other.back))
                // {
                //     connection = 1;
                //     return true;
                // }
                // else if (back.ConnectsTo(other.back))
                // {
                //     connection = 2;
                //     return true;
                // }
                // else if (right.ConnectsTo(other.back))
                // {
                //     connection = 3;
                //     return true;
                // }
                // else
                // {
                //     return false;
                // }

                return forward.ConnectsTo(other.back);
            }
            else if (direction == 3)
            {
                // if (right.ConnectsTo(other.left))
                // {
                //     connection = 0;
                //     return true;
                // }
                // else if (back.ConnectsTo(other.left))
                // {
                //     connection = 1;
                //     return true;
                // }
                // else if (left.ConnectsTo(other.left))
                // {
                //     connection = 2;
                //     return true;
                // }
                // else if (forward.ConnectsTo(other.left))
                // {
                //     connection = 3;
                //     return true;
                // }
                // else
                // {
                //     return false;
                // }

                return right.ConnectsTo(other.left);
            }
            else if (direction == 4)
            {
                return above.ConnectsTo(other.below);
            }
            else if (direction == 5)
            {
                return below.ConnectsTo(other.above);
            }
            else
            {
                throw new System.ArgumentException("Invalid direction");
            }
        }

        public int ReturnConnectionDirection()
        {
            return connection;
        }

        public bool CheckBannedNeighbour(WFCModule other, int direction)
        {
            if (direction == 0)
            {
                if (banModules)
                {
                    bool banned = false;
                    for (int bannedNeighbourModule = 0; bannedNeighbourModule < bannedModules.Length; bannedNeighbourModule++)
                    {
                        if (back.ConnectsTo(other.forward) && bannedModules[bannedNeighbourModule] == other)
                        {
                            banned = true;
                        }
                    }

                    if (back.ConnectsTo(other.forward) && !banned)
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
                        if (forward.ConnectsTo(other.back) && bannedModules[bannedNeighbourModule] == other)
                        {
                            banned = true;
                        }
                    }

                    if (forward.ConnectsTo(other.back) && !banned)
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
                if (back.ConnectsTo(other.forward) && !banForward)
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
                if (forward.ConnectsTo(other.back) && !banForward)
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
