using System.Collections.Generic;
using UnityEngine;

public static class AStarPathfinding
{

    public static List<Node> FindPath(Node start, Node end, List<Node> allNodes, List<Node> dangerousNodes, List<Transform> ghostsPosition)
    {
        List<AStarNode> openList = new List<AStarNode>();
        List<AStarNode> closedList = new List<AStarNode>();

        AStarNode current = new AStarNode(start, 0, 0, 0, null);
        openList.Add(current);

        while (openList.Count > 0)
        {
            current = GetLowestFCost(openList);
            closedList.Add(current);
            openList.Remove(current);

            if (current.node == end)
            {
                return GetPath(current);
            }

            foreach (Node neighbour in current.node.GetNeighbors())
            {
                if (neighbour == null)
                {
                    continue;
                }

                if (closedList.Find(x => x.node == neighbour) != null)
                {
                    continue;
                }

                float gCost = current.gCost + 1;
                float hCost = GetDistance(neighbour, end, ghostsPosition);
                if (dangerousNodes.Contains(neighbour))
                {
                    hCost += 10000;
                }
                float fCost = gCost + hCost;


                AStarNode neighbourNode = openList.Find(x => x.node == neighbour);

                if (neighbourNode == null)
                {
                    neighbourNode = new AStarNode(neighbour, gCost, hCost, fCost, current);
                    openList.Add(neighbourNode);
                }
                else if (gCost < neighbourNode.gCost)
                {
                    neighbourNode.gCost = gCost;
                    neighbourNode.hCost = hCost;
                    neighbourNode.fCost = fCost;
                    neighbourNode.parent = current;
                }
            }
        }

        return null;

    }

    private static List<Node> GetPath(AStarNode current)
    {
        List<Node> path = new List<Node>();

        while (current != null)
        {
            path.Add(current.node);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    private static float GetDistance(Node nodeA, Node nodeB, List<Transform> ghostsPosition)
    {
        //get nearest ghost
        float minDistance = 100000;
        for (int i = 0; i < ghostsPosition.Count; i++)
        {
            float distance = Vector2.Distance(nodeA.transform.position, ghostsPosition[i].position);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }


        return Vector3.Distance(nodeA.transform.position, nodeB.transform.position) + (1/minDistance * 10000);
    }

    private static AStarNode GetLowestFCost(List<AStarNode> list)
    {
        AStarNode lowest = list[0];

        for (int i = 1; i < list.Count; i++)
        {
            if (list[i].fCost < lowest.fCost)
            {
                lowest = list[i];
            }
        }

        return lowest;
    }

    public class AStarNode
    {
        public Node node;
        public float gCost;
        public float hCost;
        public float fCost;
        public AStarNode parent;

        public AStarNode(Node node, float gCost, float hCost, float fCost, AStarNode parent)
        {
            this.node = node;
            this.gCost = gCost;
            this.hCost = hCost;
            this.fCost = fCost;
            this.parent = parent;
        }
    }
}