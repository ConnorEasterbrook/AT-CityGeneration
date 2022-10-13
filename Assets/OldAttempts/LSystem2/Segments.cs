using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PersonalGenerator
{
    public class Segments
    {
        public Vector3 nodeA { get; set; }
        public Vector3 nodeB { get; set; }
        public Quaternion segmentRotation { get; set; }

        public int LeanIteration { get; set; }
        public bool LeanRight { get; set; }
        public bool LeanLeft { get; set; }

        public bool EndRoad { get; set; }

        /// <summary>
        /// Creates a new Road Part.
        /// </summary>
        public Segments(Vector3 start, Vector3 end, Quaternion rotation)
        {
            nodeA = start;
            nodeB = end;
            segmentRotation = rotation;
        }

    }
}
