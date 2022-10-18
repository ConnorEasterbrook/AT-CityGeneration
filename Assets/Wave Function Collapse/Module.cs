using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WaveFunctionCollapse
{
    [Serializable]
    public class Module
    {
        public ModulePrototype Prototype; // The prototype of this module from the PrototypeParent in the editor.
        public int Rotation; // The rotation of the module.
        public int[][] PossibleNeighbours; // The possible neighbours for each module.

        /// <summary>
        /// Constructor for the module class.
        /// </summary>
        public Module(ModulePrototype prototype, int rotation)
        {
            Prototype = prototype;
            Rotation = rotation;
        }

        private static readonly int[] horizontalFaces = { 0, 2, 3, 5 };

        public bool Fits(int direction, Module module)
        {
            int otherDirection = (direction + 3) % 6;

            if (horizontalFaces.Contains(direction))
            {
                ModulePrototype.HorizontalFaceDetails prototypeHorizontalFaces = (ModulePrototype.HorizontalFaceDetails)Prototype.Faces
                [
                    horizontalFaces
                    [
                        (Array.IndexOf(horizontalFaces, otherDirection) + module.Rotation) % 4
                    ]
                ];

                ModulePrototype.HorizontalFaceDetails neighbourModuleHorizontalFaces = (ModulePrototype.HorizontalFaceDetails)module.Prototype.Faces
                [
                    horizontalFaces
                    [
                        (Array.IndexOf(horizontalFaces, otherDirection) + module.Rotation) % 4
                    ]
                ];

                if (prototypeHorizontalFaces.Connector == neighbourModuleHorizontalFaces.Connector)
                {
                    if (prototypeHorizontalFaces.Symmetric || prototypeHorizontalFaces.Flipped != neighbourModuleHorizontalFaces.Flipped)
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
                    return false;
                }

            }
            else
            {
                ModulePrototype.VerticalFaceDetails prototypeVerticalFaces = (ModulePrototype.VerticalFaceDetails)Prototype.Faces[direction];
                ModulePrototype.VerticalFaceDetails neighbourModuleVerticalFaces = (ModulePrototype.VerticalFaceDetails)module.Prototype.Faces[otherDirection];

                if (prototypeVerticalFaces.Connector == neighbourModuleVerticalFaces.Connector)
                {
                    if (prototypeVerticalFaces.Invariant || (prototypeVerticalFaces.Rotation + Rotation) % 4 == (neighbourModuleVerticalFaces.Rotation + module.Rotation) % 4)
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
                    return false;
                }
            }
        }
    }
}