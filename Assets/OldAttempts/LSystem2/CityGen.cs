using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEditor;

namespace CityGenerator
{
    public class CityGen : MonoBehaviour
    {
        [Header("Map Generation Settings")]
        public int maxDegree = 2;
        public int maxPrimaryRoad = 1000; // Maximum number of major roads
        public int maxSecondaryRoad = 10000; // Maximum number of minor roads
        public int mapSize = 20; // Size of the grid
        public int seed = 0; // Seed for a random number generator

        [Header("Visual Aid")]
        public Material edgeMaterial; // The material to use for the lines.
        public Material nodeMaterial; // The material to use for the lines.
        public bool drawNodes = false; // Draw the nodes
        public bool drawRoads = true; // Draw the roads

        // Private variables
        private MapGenGraph mapGenGraph; //Graph which will be built, and then drawn
        private System.Random rand; // Random number generator

        // Road Generation Variables
        private List<RoadSegment> globalGoalsPrimaryRoads;
        private List<RoadSegment> globalGoalsSecondaryRoads;
        private List<RoadSegment> primaryRoadSegmentQueue;
        private List<RoadSegment> secondaryRoadSegmentQueue;
        private List<RoadSegment> primaryRoadSegments;
        private List<RoadSegment> secondaryRoadSegments;
        // Generate a number for the beginning primary road segment
        private int sampleX; // Random x coordinate
        private int sampleY; // Random y coordinate
        private const int RoadLength = 10;

        void Start()
        {
            mapGenGraph = new MapGenGraph();

            GetRandomVariables();
            // GenRoadAndNodes();

            Thread t = new Thread(GenRoadAndNodes);
            t.Start();
        }

        /// <summary>
        /// Generates a random seed and random variable.
        /// </summary>
        private void GetRandomVariables()
        {
            if (seed == 0)
            {
                seed = Random.Range(0, 100000);
            }

            rand = new System.Random(seed);

            // Generate a number for the beginning primary road segment
            sampleX = Random.Range(0, (mapSize * 100)); // Random x coordinate
            sampleY = Random.Range(0, (mapSize * 100)); // Random y coordinate
        }

        /// <summary>
        /// Generate the road nodes and segments.
        /// </summary>
        private void GenRoadAndNodes()
        {
            // Initialize the lists
            globalGoalsPrimaryRoads = new List<RoadSegment>();
            globalGoalsSecondaryRoads = new List<RoadSegment>();
            primaryRoadSegmentQueue = new List<RoadSegment>();
            secondaryRoadSegmentQueue = new List<RoadSegment>();
            primaryRoadSegments = new List<RoadSegment>();
            secondaryRoadSegments = new List<RoadSegment>();

            // Generate roads.
            GenerateRoadsAndNodes(primaryRoadSegmentQueue, primaryRoadSegments, maxPrimaryRoad, 0); // Generate primary roads.
            GenerateRoadsAndNodes(secondaryRoadSegmentQueue, secondaryRoadSegments, maxSecondaryRoad, 1); // Generate secondary roads.
        }

        /// <summary>
        /// Generate the road nodes and segments.
        /// </summary>
        private void GenerateRoadsAndNodes(List<RoadSegment> roadSegmentQueue, List<RoadSegment> roadSegments, int maxRoad, int id)
        {
            GenerateStartSegments(roadSegmentQueue, id); // Generate the start segments.

            // Generate the rest of the segments.
            while (roadSegmentQueue.Count() != 0 && roadSegments.Count() <= maxRoad)
            {
                RoadSegment currentRoadSegment = roadSegmentQueue[0]; // Get the first segment in the queue.
                roadSegmentQueue.RemoveAt(0); // Remove the first segment in the queue. This stops an endless loop.

                // If the segment is not valid, skip it.
                if (!CheckConstraints(currentRoadSegment, roadSegments, id))
                {
                    continue;
                }

                roadSegments.Add(currentRoadSegment); // Add the segment to the list of segments.
                AddNodeToMapGraph(currentRoadSegment, id); // Add the node to the map graph.

                if (id == 0)
                {
                    GlobalGoalsPrimary(currentRoadSegment);
                }
                else
                {
                    GlobalGoalsSecondary(currentRoadSegment);
                }
            }

            // Delete additional nodes and edges.
            if (id != 0)
            {
                DeleteNodesAndEdges();
            }
        }

        /// <summary>
        /// Generate the start segments for the road generation.
        /// </summary>
        private void GenerateStartSegments(List<RoadSegment> roadSegmentQueue, int id)
        {
            if (id == 0)
            {
                // Then get the starting node.
                float startXPos = ((float)sampleX / 100.0f) - (float)mapSize / 2;
                float startYPos = ((float)sampleY / 100.0f) - (float)mapSize / 2;
                GenNode startNode = new GenNode(startXPos, startYPos);

                //Secondly Generate a vector which determines the two starting directions
                int randomDirX = rand.Next(-100, 100);
                int randomDirY = rand.Next(-100, 100);
                Vector2 startDir = new Vector2(randomDirX, randomDirY);
                GenNode starterNodeTo1 = new GenNode(startNode.X + startDir.normalized.x, startYPos + startDir.normalized.y);
                GenNode starterNodeTo2 = new GenNode(startNode.X - startDir.normalized.x, startYPos - startDir.normalized.y);

                //Thirdly We make two starting RoadSegment from these
                RoadSegment starterSegment1 = new RoadSegment(startNode, starterNodeTo1, 0);
                RoadSegment starterSegment2 = new RoadSegment(startNode, starterNodeTo2, 0);
                roadSegmentQueue.Add(starterSegment1);
                roadSegmentQueue.Add(starterSegment2);
            }
            else
            {
                foreach (RoadSegment roadSegment in primaryRoadSegments)
                {
                    if (roadSegment.EndRoad)
                    {
                        continue;
                    }

                    Vector2 dirVector = new Vector2(roadSegment.endNode.X - roadSegment.startNode.X, roadSegment.endNode.Y - roadSegment.startNode.Y);
                    Vector2 normalVector1 = new Vector2(dirVector.y, -dirVector.x);
                    Vector2 normalVector2 = new Vector2(-dirVector.y, dirVector.x);

                    RoadSegment branchedSegment2 = new RoadSegment(roadSegment.endNode, new GenNode(roadSegment.endNode.X + normalVector1.normalized.x, roadSegment.endNode.Y + normalVector1.normalized.y), 0);
                    RoadSegment branchedSegment3 = new RoadSegment(roadSegment.endNode, new GenNode(roadSegment.endNode.X + normalVector2.normalized.x, roadSegment.endNode.Y + normalVector2.normalized.y), 0);
                    roadSegmentQueue.Add(branchedSegment2);
                    roadSegmentQueue.Add(branchedSegment3);
                }
            }
        }

        /// <summary>
        /// Check if the road segment meets the constraints.
        /// </summary>
        private bool CheckConstraints(RoadSegment currentRoadSegment, List<RoadSegment> roadSegments, int id)
        {
            if (id == 0)
            {
                foreach (RoadSegment road in roadSegments)
                {
                    //If the new segment end is close to another segments Node, Fix it's end to it
                    if (CheckNodeProximity(currentRoadSegment.endNode, road.endNode, 0.8f))
                    {
                        currentRoadSegment.endNode = road.endNode;
                        currentRoadSegment.EndRoad = true;
                    }
                }

                //NodeTo already connected to NodeFrom
                foreach (Edge edge in currentRoadSegment.endNode.Edges)
                {
                    if (edge.NodeA == currentRoadSegment.startNode || edge.NodeB == currentRoadSegment.startNode)
                    {
                        return false;
                    }
                }
            }
            else
            {
                bool streched = false;

                foreach (RoadSegment road in primaryRoadSegments) //first check majorNodes
                {
                    if (CheckNodeProximity(currentRoadSegment.endNode, road.endNode, 0.7f) && !streched)
                    {
                        currentRoadSegment.endNode = road.endNode;
                        currentRoadSegment.EndRoad = true;
                        streched = true;
                        break;
                    }

                    if (currentRoadSegment.IsCrossing(road))
                    {
                        return false;
                    }

                }

                if (!streched)
                {
                    foreach (RoadSegment road in secondaryRoadSegments) //then check minorNodes
                    {
                        if (CheckNodeProximity(currentRoadSegment.endNode, road.endNode, 0.7f) && !streched)
                        {
                            currentRoadSegment.endNode = road.endNode;
                            currentRoadSegment.EndRoad = true;
                            streched = true;
                            break;
                        }

                        if (currentRoadSegment.IsCrossing(road))
                        {
                            return false;
                        }

                    }
                }
            }


            //Check if segment is out of border
            if (currentRoadSegment.startNode.X > mapSize || currentRoadSegment.startNode.X < -mapSize || currentRoadSegment.startNode.Y > mapSize || currentRoadSegment.startNode.Y < -mapSize)
            {
                return false;
            }

            //Check if segment would come into itself
            if (currentRoadSegment.startNode.X == currentRoadSegment.endNode.X && currentRoadSegment.startNode.Y == currentRoadSegment.endNode.Y)
            {
                return false;
            }

            // If StartNode and EndNode have more than 4 edges
            if (currentRoadSegment.startNode.Edges.Count >= 4 || currentRoadSegment.endNode.Edges.Count >= 4)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the two nodes are close to each other.
        /// </summary>
        private bool CheckNodeProximity(GenNode A, GenNode B, float n)
        {
            if (((float)Mathf.Pow(A.X - B.X, 2) + (float)Mathf.Pow(A.Y - B.Y, 2)) < (n * n)) return true; //The two points are closer than 0.8f
            return false;
        }

        /// <summary>
        /// Add node locations to the map graph.
        /// </summary>
        private void AddNodeToMapGraph(RoadSegment currentRoadSegment, int id)
        {
            // Add the nodes to the map graph
            if (id == 0)
            {
                if (!mapGenGraph.primaryNodes.Contains(currentRoadSegment.startNode))
                {
                    mapGenGraph.primaryNodes.Add(currentRoadSegment.startNode);
                }
                if (!mapGenGraph.primaryNodes.Contains(currentRoadSegment.endNode))
                {
                    mapGenGraph.primaryNodes.Add(currentRoadSegment.endNode);
                }
                mapGenGraph.primaryEdges.Add(new Edge(currentRoadSegment.startNode, currentRoadSegment.endNode));
            }
            else
            {
                if (!mapGenGraph.primaryNodes.Contains(currentRoadSegment.startNode) && !mapGenGraph.secondaryNodes.Contains(currentRoadSegment.startNode))
                {
                    mapGenGraph.secondaryNodes.Add(currentRoadSegment.startNode);
                }
                if (!mapGenGraph.primaryNodes.Contains(currentRoadSegment.endNode) && !mapGenGraph.secondaryNodes.Contains(currentRoadSegment.endNode))
                {
                    mapGenGraph.secondaryNodes.Add(currentRoadSegment.endNode);
                }
                mapGenGraph.secondaryEdges.Add(new Edge(currentRoadSegment.startNode, currentRoadSegment.endNode));
            }
        }

        /// <summary>
        ///
        /// </summary>
        private void GlobalGoalsPrimary(RoadSegment currentRoadSegment)
        {
            if (currentRoadSegment.EndRoad)
            {
                return;
            }

            globalGoalsPrimaryRoads.Clear(); // Clear the list of global goals

            //First calculate the direction Vector
            Vector2 dirVector = new Vector2(currentRoadSegment.endNode.X - currentRoadSegment.startNode.X, currentRoadSegment.endNode.Y - currentRoadSegment.startNode.Y);

            // Get random branch chance
            int randomBranch = rand.Next(0, 30);

            // Check if randomBranch lands on a branching number
            if (randomBranch == 1) // Branch left
            {
                Vector2 normalVector = new Vector2(dirVector.y, -dirVector.x);
                RoadSegment branchedSegment = CalcNewRoadSegment(currentRoadSegment.endNode, normalVector, 0);
                globalGoalsPrimaryRoads.Add(branchedSegment);
            }
            else if (randomBranch == 2) // Branch right
            {
                Vector2 normalVector = new Vector2(-dirVector.y, dirVector.x);
                RoadSegment branchedSegment = CalcNewRoadSegment(currentRoadSegment.endNode, normalVector, 0);
                globalGoalsPrimaryRoads.Add(branchedSegment);
            }
            else if (randomBranch == 3) // Branch both
            {
                Vector2 normalVector1 = new Vector2(dirVector.y, -dirVector.x);
                Vector2 normalVector2 = new Vector2(-dirVector.y, dirVector.x);
                RoadSegment branchedSegment1 = CalcNewRoadSegment(currentRoadSegment.endNode, normalVector1, 0);
                RoadSegment branchedSegment2 = CalcNewRoadSegment(currentRoadSegment.endNode, normalVector2, 0);
                globalGoalsPrimaryRoads.Add(branchedSegment1);
                globalGoalsPrimaryRoads.Add(branchedSegment2);
            }

            if (maxDegree < 1)
            {
                CalcNewRoadSegment(currentRoadSegment.endNode, dirVector, 0);
                globalGoalsPrimaryRoads.Add(currentRoadSegment);
            }

            //Then check if we need to determine a new lean. If yes, calculate the next RoadSegment like that
            if (currentRoadSegment.LeanIteration == 3)
            {
                int randomNumber = rand.Next(0, 3);

                if (randomNumber == 1)
                {
                    dirVector = RotateVector(dirVector, GetRandomAngle(-2, -maxDegree * 2));
                    RoadSegment newSegment = CalcNewRoadSegment(currentRoadSegment.endNode, dirVector, 0);
                    newSegment.LeanLeft = true;
                    globalGoalsPrimaryRoads.Add(newSegment);
                }
                else if (randomNumber == 2)
                {
                    dirVector = RotateVector(dirVector, GetRandomAngle(2, maxDegree * 2));
                    RoadSegment newSegment = CalcNewRoadSegment(currentRoadSegment.endNode, dirVector, 0);
                    newSegment.LeanLeft = true;
                    globalGoalsPrimaryRoads.Add(newSegment);
                }

            }
            else //if not, grow the new segment following the lean
            {
                if (currentRoadSegment.LeanLeft == true)
                {
                    dirVector = RotateVector(dirVector, GetRandomAngle(2, maxDegree));
                    RoadSegment newSegment = CalcNewRoadSegment(currentRoadSegment.endNode, dirVector, maxDegree);
                    newSegment.LeanLeft = true;
                    globalGoalsPrimaryRoads.Add(newSegment);
                }
                else if (currentRoadSegment.LeanRight == true)
                {
                    dirVector = RotateVector(dirVector, GetRandomAngle(-2, -maxDegree));
                    RoadSegment newSegment = CalcNewRoadSegment(currentRoadSegment.endNode, dirVector, maxDegree);
                    newSegment.LeanRight = true;
                    globalGoalsPrimaryRoads.Add(newSegment);
                }
                else
                {
                    RoadSegment newSegment = CalcNewRoadSegment(currentRoadSegment.endNode, dirVector, 0);
                    globalGoalsPrimaryRoads.Add(newSegment);
                }
            }

            foreach (RoadSegment newSegment in globalGoalsPrimaryRoads)
            {
                primaryRoadSegmentQueue.Add(newSegment);
            }
        }

        private Vector2 RotateVector(Vector2 dirVector, float rotationAngle)
        {
            //This works like a rotation matrix
            dirVector.x = ((float)Mathf.Cos(rotationAngle) * dirVector.x) - ((float)Mathf.Sin(rotationAngle) * dirVector.y);
            dirVector.y = ((float)Mathf.Sin(rotationAngle) * dirVector.x) + ((float)Mathf.Cos(rotationAngle) * dirVector.y);

            return dirVector;
        }

        private float GetRandomAngle(int a, int b) //Calculates an Angle between a and b, returns it in radian
        {
            //First we make 'a' smaller, and generate a random number in the range
            if (b < a)
            {
                int temp = b;
                b = a;
                a = temp;
            }
            int range = Mathf.Abs(b - a);
            int rotation = rand.Next(0, range) + a;

            //then we make it to radian, and return it
            float rotationAngle = (float)(Mathf.PI / 180) * rotation;
            return rotationAngle;
        }

        private void GlobalGoalsSecondary(RoadSegment currentRoadSegment)
        {
            if (currentRoadSegment.EndRoad) return;

            globalGoalsSecondaryRoads.Clear();

            //Generate in the 3 other possible direction
            Vector2 dirVector = new Vector2(currentRoadSegment.endNode.X - currentRoadSegment.startNode.X, currentRoadSegment.endNode.Y - currentRoadSegment.startNode.Y);
            Vector2 normalVector1 = new Vector2(dirVector.y, -dirVector.x);
            Vector2 normalVector2 = new Vector2(-dirVector.y, dirVector.x);

            RoadSegment branchedSegment1 = new RoadSegment(currentRoadSegment.endNode, new GenNode(currentRoadSegment.endNode.X + dirVector.normalized.x, currentRoadSegment.endNode.Y + dirVector.normalized.y), 0);
            RoadSegment branchedSegment2 = new RoadSegment(currentRoadSegment.endNode, new GenNode(currentRoadSegment.endNode.X + normalVector1.normalized.x, currentRoadSegment.endNode.Y + normalVector1.normalized.y), 0);
            RoadSegment branchedSegment3 = new RoadSegment(currentRoadSegment.endNode, new GenNode(currentRoadSegment.endNode.X + normalVector2.normalized.x, currentRoadSegment.endNode.Y + normalVector2.normalized.y), 0);
            globalGoalsSecondaryRoads.Add(branchedSegment1);
            globalGoalsSecondaryRoads.Add(branchedSegment2);
            globalGoalsSecondaryRoads.Add(branchedSegment3);

            foreach (RoadSegment newSegment in globalGoalsSecondaryRoads)
            {
                secondaryRoadSegmentQueue.Add(newSegment);
            }
        }

        private RoadSegment CalcNewRoadSegment(GenNode startNode, Vector2 dirVector, int angle)
        {
            var newNodeTo = new GenNode(startNode.X + dirVector.normalized.x * RoadLength, startNode.Y + dirVector.normalized.y * RoadLength);
            return new RoadSegment(startNode, newNodeTo, angle);
        }

        private void DeleteNodesAndEdges()
        {
            List<Edge> removableEdges = new List<Edge>();
            List<GenNode> removableNodes = new List<GenNode>();

            foreach (Edge edge in mapGenGraph.secondaryEdges)
            {
                if (edge.NodeA.Edges.Count == 1 && edge.NodeB.Edges.Count == 1)
                {
                    removableEdges.Add(edge);
                }
            }

            foreach (Edge edge in removableEdges)
            {
                mapGenGraph.secondaryEdges.Remove(edge);
                edge.NodeA.Edges.Remove(edge);
                edge.NodeB.Edges.Remove(edge);

                foreach (GenNode node in mapGenGraph.secondaryNodes)
                {
                    if (node.Edges.Contains(edge)) node.Edges.Remove(edge);
                }

                foreach (GenNode node in mapGenGraph.primaryNodes)
                {
                    if (node.Edges.Contains(edge)) node.Edges.Remove(edge);
                }

                mapGenGraph.secondaryEdges.Remove(edge);
            }

            foreach (GenNode node in mapGenGraph.secondaryNodes) //Randomly choose nodes to delete, store these nodes and its edges
            {
                if (node.X > (mapSize - 2) || node.X < (-mapSize + 2) || node.Y > (mapSize - 2) || node.Y < (-mapSize + 2)) continue;
                if (rand.Next(0, 10) == 1)
                {
                    foreach (Edge edge in node.Edges)
                    {
                        if (!removableEdges.Contains(edge)) removableEdges.Add(edge);
                    }

                    removableNodes.Add(node);
                }
            }

            foreach (Edge edge in removableEdges) //Remove the edges from other nodes and from the minoredges list
            {
                foreach (GenNode node in mapGenGraph.primaryNodes)
                {
                    if (node.Edges.Contains(edge))
                    {
                        node.Edges.Remove(edge);
                    }
                }
                foreach (GenNode node in mapGenGraph.secondaryNodes)
                {
                    if (node.Edges.Contains(edge))
                    {
                        node.Edges.Remove(edge);
                    }
                }
                mapGenGraph.secondaryEdges.Remove(edge);
            }

            foreach (GenNode node in mapGenGraph.secondaryNodes)
            {
                if (node.Edges.Count <= 0)
                {
                    removableNodes.Add(node);
                }
                if (node.Edges.Count <= 1 && !NodeIsCloseToMapEdge(node))
                {
                    removableNodes.Add(node);
                }
            }

            foreach (GenNode node in removableNodes)
            {
                mapGenGraph.secondaryNodes.Remove(node);
            }
        }

        private bool NodeIsCloseToMapEdge(GenNode node)
        {
            return node.X >= (mapSize - 2) || node.X <= (-mapSize + 2) || node.Y >= (mapSize - 2) || node.Y <= (-mapSize + 2);
        }

        // Display the map in editor
        private void OnDrawGizmos()
        {
            if (mapGenGraph != null)
            {
                if (drawRoads)
                {
                    if (mapGenGraph.primaryEdges == null || mapGenGraph.primaryEdges.Count == 0) return;

                    for (int x = mapGenGraph.primaryEdges.Count - 1; x > -1; x--) //for loop start from backwards, because the list is getting new elements while being read
                    {
                        Vector3 start = new Vector3(mapGenGraph.primaryEdges[x].NodeA.X, mapGenGraph.primaryEdges[x].NodeA.Y, 0f);
                        Vector3 end = new Vector3(mapGenGraph.primaryEdges[x].NodeB.X, mapGenGraph.primaryEdges[x].NodeB.Y, 0f);

                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(start, end);
                    }

                    if (mapGenGraph.secondaryEdges == null || mapGenGraph.secondaryEdges.Count == 0) return;

                    for (int x = mapGenGraph.secondaryEdges.Count - 1; x > -1; x--) //for loop start from backwards, because the list is getting new elements while being read
                    {
                        Vector3 start = new Vector3(mapGenGraph.secondaryEdges[x].NodeA.X, mapGenGraph.secondaryEdges[x].NodeA.Y, 0f);
                        Vector3 end = new Vector3(mapGenGraph.secondaryEdges[x].NodeB.X, mapGenGraph.secondaryEdges[x].NodeB.Y, 0f);

                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(start, end);
                    }
                }

                if (drawNodes)
                {
                    for (int x = mapGenGraph.primaryNodes.Count - 1; x > -1; x--) //for loop start from backwards, because the list is getting new elements while being read
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(new Vector3(mapGenGraph.primaryNodes[x].X, mapGenGraph.primaryNodes[x].Y, 0f), 0.5f);
                    }
                    for (int x = mapGenGraph.secondaryNodes.Count - 1; x > -1; x--) //for loop start from backwards, because the list is getting new elements while being read
                    {
                        Gizmos.color = Color.gray;
                        Gizmos.DrawSphere(new Vector3(mapGenGraph.secondaryNodes[x].X, mapGenGraph.secondaryNodes[x].Y, 0f), 0.3f);
                    }
                }
            }
        }
    }
}
