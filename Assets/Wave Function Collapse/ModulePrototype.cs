using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace WaveFunctionCollapse
{
    public class ModulePrototype : MonoBehaviour
    {
        [Serializable]
        public abstract class FaceDetails
        {
            public int Connector;

            [HideInInspector]
            public Fingerprint Fingerprint;
        }

        [Serializable]
        public class HorizontalFaceDetails : FaceDetails
        {
            public bool Symmetric;
            public bool Flipped;

            public override string ToString()
            {
                return Connector.ToString() + (Symmetric ? "s" : (Flipped ? "F" : ""));
            }
        }

        [Serializable]
        public class VerticalFaceDetails : FaceDetails
        {
            public bool Invariant;
            public int Rotation;

            public override string ToString()
            {
                return Connector.ToString() + (Invariant ? "i" : (Rotation != 0 ? "_bcd".ElementAt(Rotation).ToString() : ""));
            }
        }

        public HorizontalFaceDetails Forward;
        public HorizontalFaceDetails Right;
        public HorizontalFaceDetails Back;
        public HorizontalFaceDetails Left;
        public VerticalFaceDetails Up;
        public VerticalFaceDetails Down;

        public bool CreateRotatedVariants;
        public bool useVariants = false;
        public GameObject[] variations;
        public bool xRotate90 = false;

        public float Probability = 1.0f;

        public FaceDetails[] Faces
        {
            get
            {
                return new FaceDetails[]
                {
                    Left,
                    Down,
                    Back,
                    Right,
                    Up,
                    Forward
                };
            }
        }

        public List<Module> CreateModules()
        {
            var modules = new List<Module>();

            ModulePrototype[] prototypes = ModulePrototype.GetAll().ToArray();


            foreach (var prototype in prototypes)
            {
                for (int rotation = 0; rotation < (prototype.CreateRotatedVariants ? 4 : 1); rotation++)
                {
                    modules.Add(new Module(prototype, rotation));
                }
            }

            foreach (var module in modules)
            {
                module.PossibleNeighbours = Enumerable.Range(0, 6).Select(direction => Enumerable.Range(0, modules.Count).Where(i => module.Fits(direction, modules[i])).ToArray()).ToArray();
            }

            return modules;
        }

        public static IEnumerable<ModulePrototype> GetAll()
        {
            foreach (Transform transform in GameObject.FindObjectOfType<ModulePrototype>().transform.parent)
            {
                var item = transform.GetComponent<ModulePrototype>();
                if (item != null && item.enabled)
                {
                    yield return item;
                }
            }
        }

        public GameObject getModel()
        {
            GameObject model = variations[UnityEngine.Random.Range(0, variations.Length)];
            return model;
        }

        private void OnDrawGizmos()
        {
            ModulePrototype modulePrototype = this; // The module prototype to draw.
            Vector3 position = modulePrototype.transform.position; // The position of the module prototype.

            GUIStyle style = new GUIStyle(); // The style of the text.
            style.alignment = TextAnchor.MiddleCenter; // The alignment of the text.
            style.normal.textColor = Color.black; // The color of the text.
            style.fontSize = 20; // The size of the text.

            // For each side of the module prototype draw labels to represent the assigned connectors.
            for (int i = 0; i < 6; i++)
            {
                var face = modulePrototype.Faces[i];
                Handles.Label(position + Orientations.All[i] * Vector3.forward * Generator._SlotSize / 2f, face.ToString(), style);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ModulePrototype))]
    public class ModulePrototypeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ModulePrototype modulePrototype = (ModulePrototype)target;
            if (GUILayout.Button("Distribute"))
            {
                int i = 0;
                foreach (Transform transform in modulePrototype.transform.parent)
                {
                    transform.localPosition = Vector3.forward * i * Generator._SlotSize * 2f;
                    i++;
                }
            }
        }
    }
#endif
}
