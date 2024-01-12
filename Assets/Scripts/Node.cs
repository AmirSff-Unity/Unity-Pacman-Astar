using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public LayerMask obstacleLayer;
    public readonly List<Vector2> availableDirections = new();

    [SerializeField]
    public List<NodePathData> nodePathData;


    private void Start()
    {
        availableDirections.Clear();

        nodePathData = new List<NodePathData>();


        StartCoroutine(StartPathCheck());
    }

    IEnumerator StartPathCheck()
    {
        yield return new WaitForSeconds(0.1f);

        CheckAvailableDirection(NodePathSide.Up);
        CheckAvailableDirection(NodePathSide.Down);
        CheckAvailableDirection(NodePathSide.Left);
        CheckAvailableDirection(NodePathSide.Right);

    }
    private void CheckAvailableDirection(NodePathSide side)
    {
        Vector2 direction = Vector2.zero;
        switch (side)
        {
            case NodePathSide.Up:
                direction = Vector2.up;
                break;
            case NodePathSide.Down:
                direction = Vector2.down;
                break;
            case NodePathSide.Left:
                direction = Vector2.left;
                break;
            case NodePathSide.Right:
                direction = Vector2.right;
                break;
            default:
                break;
        }


        //layer should be obstacleLayer and Node layer

        //RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 50f, 0f, direction, 1, obstacleLayer);
        RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(direction.x, direction.y), direction, 100, obstacleLayer);

        // If no collider is hit then there is no obstacle in that direction
        if (hit.collider && hit.collider.gameObject.GetComponent<Obstacle>() == null && hit.collider.gameObject.GetComponent<Node>() != null)
        {
            availableDirections.Add(direction);

            nodePathData.Add(new NodePathData()
            {
                side = side,
                pathDataType = NodePathDataType.OpenPath,
                connectedNode = hit.collider.gameObject.GetComponent<Node>(),
                distance = Vector2.Distance(transform.position, hit.collider.gameObject.transform.position)
            });
        }
        else if (hit.collider)
        {
            Debug.Log("hit: " + hit.collider.gameObject.name);
            nodePathData.Add(new NodePathData()
            {
                side = side,
                pathDataType = NodePathDataType.Obstacle,
                connectedNode = null,
                distance = Vector2.Distance(transform.position, hit.collider.gameObject.transform.position)
            });
        }
    }


    public List<Node> GetNeighbors()
    {
        List<Node> neighbors = new List<Node>();

        foreach (NodePathData pathData in nodePathData)
        {
            if (pathData.pathDataType == NodePathDataType.OpenPath)
            {
                neighbors.Add(pathData.connectedNode);
            }
        }

        return neighbors;
    }

}



[Serializable]
public struct NodePathData
{
    [SerializeField]
    public NodePathSide side;

    [SerializeField]
    public NodePathDataType pathDataType;

    [SerializeField]
    public Node connectedNode;

    [SerializeField]
    public float distance;
}

public enum NodePathSide
{
    Up,
    Down,
    Left,
    Right
}

public enum NodePathDataType
{
    Obstacle,
    OpenPath
}