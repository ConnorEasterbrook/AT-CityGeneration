using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CityGenerator
{
    public class GenNode
    {
        public float X { get; set; }
        public float Y { get; set; }
        public List<Edge> Edges { get; set; } // Edges connected to this node.

        public GenNode(float x, float y)
        {
            X = x;
            Y = y;

            Edges = new List<Edge>();
        }

        public void AddEdge(Edge edge)
        {
            Edges.Add(edge);
        }
    }

    /// <summary>
    /// The Edge class is used to connect two nodes together. It also contains the direction of the edge
    /// </summary>
    public class Edge
    {
        public GenNode NodeA { get; set; }
        public GenNode NodeB { get; set; }

        public Edge(GenNode first, GenNode second)
        {
            NodeA = first;
            NodeB = second;

            first.AddEdge(this);
            second.AddEdge(this);
        }
    }

    /// <summary>
    /// The Graph class is the main class of the road generation. It contains the nodes and edges of the graph.
    /// </summary>
    public class MapGenGraph
    {
        public List<GenNode> primaryNodes { get; set; }
        public List<Edge> primaryEdges { get; set; }
        public List<GenNode> secondaryNodes { get; set; }
        public List<Edge> secondaryEdges { get; set; }

        public MapGenGraph()
        {
            primaryNodes = new List<GenNode>();
            secondaryNodes = new List<GenNode>();

            primaryEdges = new List<Edge>();
            secondaryEdges = new List<Edge>();
        }
    }

    public class RoadSegment
    {
        public GenNode startNode { get; set; }
        public GenNode endNode { get; set; }

        public int LeanIteration { get; set; }
        public bool LeanRight { get; set; }
        public bool LeanLeft { get; set; }

        public bool EndRoad { get; set; }

        /// <summary>
        /// Creates a new Road Part.
        /// </summary>
        public RoadSegment(GenNode start, GenNode end, int leanNumb)
        {
            startNode = start;
            endNode = end;
            LeanIteration = leanNumb;

            LeanLeft = false;
            LeanRight = false;
            EndRoad = false;
        }

        public bool IsCrossing(RoadSegment road) //Check if the given road intersects with this road
        {
            //if (NodeFrom == road.NodeFrom || NodeFrom == road.NodeTo && NodeTo == road.NodeFrom || NodeTo == road.NodeTo) return true; //Two roadsegment is overlapping each other
            if (startNode == road.startNode || startNode == road.endNode || endNode == road.startNode || endNode == road.endNode) return false; //One of the Nodes is the same (it doesnt count as an intersection now)

            int o1 = Orientation(startNode, endNode, road.endNode);
            int o2 = Orientation(startNode, endNode, road.startNode);
            int o3 = Orientation(road.startNode, road.endNode, startNode);
            int o4 = Orientation(road.startNode, road.endNode, endNode);

            //General case
            if (o1 != o2 && o3 != o4)
                return true;

            //Special case (rare, but need to handle) - happens if two segments are collinear - we need to check if they are overlap or not
            if (o1 == 0 && o2 == 0 && o3 == 0 && o4 == 0)
            {
                if (o1 == 0 && OnSegment(startNode, endNode, road.endNode)) return true;
                if (o2 == 0 && OnSegment(startNode, endNode, road.startNode)) return true;
                if (o3 == 0 && OnSegment(road.startNode, road.endNode, startNode)) return true;
                if (o4 == 0 && OnSegment(road.startNode, road.endNode, endNode)) return true;
            }

            return false;
        }

        private int Orientation(GenNode nodeA, GenNode nodeB, GenNode TestNode) //Oriantation can be calculated with the cross product of two Vectors made from the 3 Nodes
        {
            float val = (nodeA.Y - TestNode.Y) * (nodeB.X - TestNode.X) - (nodeA.X - TestNode.X) * (nodeB.Y - TestNode.Y); //cross product calculation

            if (val > 0.00001f) return 1; //clockwise
            if (val < -0.00001f) return -1; //anticlockwise
            else return 0; //collinear
        }

        private bool OnSegment(GenNode nodeA, GenNode nodeB, GenNode TestNode) //Check if TestNode is between BaseNodes (we only call this if the 3 points are collinear)
        {
            //If X and Y coordinates are between the BaseNodes X and Y coordinates, then TestNode overlappes
            if (TestNode.X <= Mathf.Max(nodeA.X, nodeB.X) && TestNode.X >= Mathf.Min(nodeA.X, nodeB.X) &&
                TestNode.Y <= Mathf.Max(nodeA.Y, nodeB.Y) && TestNode.Y >= Mathf.Min(nodeA.Y, nodeB.Y))
            {
                return true;
            }

            return false;
        }
    }
}
