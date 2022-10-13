using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace PersonalGenerator
{
    public class Generate : MonoBehaviour
    {
        public int mapSize = 20; // Size of the grid
        public int seed = 0; // Seed for a random number generator
        public int roadLength = 5;
        public bool drawNode;
        private Vector2 startPos;
        private List<Vector3> nodes = new List<Vector3>();
        private List<Segments> segments = new List<Segments>();

        // Start is called before the first frame update
        void Start()
        {
            GetRandomVariables();
            // Thread thread = new Thread(GenerateMap);
            // thread.Start();
            GenerateMap();
        }

        /// <summary>
        /// Calculates random variables as Random can only be used in the main thread.
        /// </summary>
        private void GetRandomVariables()
        {
            // Generation Seed
            if (seed == 0)
            {
                seed = Random.Range(0, 100000);
            }

            // Calculate randomised start coordinates.
            startPos.x = (Random.Range(0, (mapSize * 100)) / 100f) - (mapSize / 2f); // Random starting x coordinate.
            startPos.y = (Random.Range(0, (mapSize * 100)) / 100f) - (mapSize / 2f); // Random starting y coordinate.
        }

        private void GenerateMap()
        {
            // Generate the starting points.
            Vector3 pointA = new Vector3(this.startPos.x, 0, this.startPos.y); ;
            Vector3 pointB;

            for (int i = 0; i < mapSize; i++)
            {
                Vector2 randomDirection = new Vector2(Random.Range(0, 30), Random.Range(0, 30));
                pointB = new Vector3(pointA.x + randomDirection.normalized.x * roadLength, 0, pointA.z + randomDirection.normalized.y * roadLength);
                nodes.Add(pointB);

                Segments segment = new Segments(pointA, pointB, Quaternion.LookRotation(pointB - pointA));
                segments.Add(segment);

                pointA = pointB;
            }
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (drawNode)
                {
                    Gizmos.DrawSphere(nodes[i], 0.2f);
                }

                if (i < nodes.Count - 1)
                {
                    Gizmos.DrawLine(nodes[i], nodes[i + 1]);
                }
            }
        }
    }
}
