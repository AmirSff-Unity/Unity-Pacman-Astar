using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;

[RequireComponent(typeof(Movement))]
public class Pacman : MonoBehaviour
{
    [SerializeField]
    private AnimatedSprite deathSequence;
    private SpriteRenderer spriteRenderer;
    private Movement movement;
    private new Collider2D collider;


    public Pellet selectedPellet;
    public List<Node> path;
    public Node node1, node2;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponent<Movement>();
        collider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        // Set the new direction based on the current input
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            movement.SetDirection(Vector2.up);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            movement.SetDirection(Vector2.down);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            movement.SetDirection(Vector2.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            movement.SetDirection(Vector2.right);
        }

        // Rotate pacman to face the movement direction
        float angle = Mathf.Atan2(movement.direction.y, movement.direction.x);
        transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
    }

    public void ResetState()
    {
        enabled = true;
        spriteRenderer.enabled = true;
        collider.enabled = true;
        deathSequence.enabled = false;
        movement.ResetState();
        gameObject.SetActive(true);
    }

    public void DeathSequence()
    {
        enabled = false;
        spriteRenderer.enabled = false;
        collider.enabled = false;
        movement.enabled = false;
        deathSequence.enabled = true;
        deathSequence.Restart();
    }




    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Node node))
        {
            //check if selected pelet is null, select a random pelet
            if (selectedPellet == null || node1 == null || node2 == null || !selectedPellet.gameObject.activeSelf)
            {
                var pellets = FindObjectsByType<Pellet>(FindObjectsSortMode.InstanceID).ToList();
                selectedPellet = pellets[Random.Range(0, pellets.Count)];

                //get the node between the current node and the selected pelet
                (node1, node2) = GetPositionBetweenNodes(selectedPellet.transform.position);
            }
            //get position of all the ghosts
            var allGhosts = FindObjectsByType<Ghost>(FindObjectsSortMode.InstanceID).ToList();
            List<Transform> allGhostsPosition = new List<Transform>();
            for (int i = 0; i < allGhosts.Count; i++)
            {
                allGhostsPosition.Add(allGhosts[i].transform);
            }

            //find the path to the selected pelet

            path = AStarPathfinding.FindPath(node, node1, FindObjectsByType<Node>(FindObjectsSortMode.InstanceID).ToList(), GetDangerousNodes(), allGhostsPosition);


            Vector2 side;
            // if user raeched the destination, user gould go to the direction of pelet
            if (path.Count == 0 || path.Count == 1)
            {
                side = GetDirection(node.transform.position, selectedPellet.transform.position);
            }
            else
            {
                //get the direction of the next node
                side = GetDirection(node.transform.position, path[1].transform.position);
            }


            //go to the path
            movement.SetDirection(side);

        }
    }


    private Vector2 GetDirection(Vector3 pos1, Vector3 pos2)
    {
        if (pos1.x == pos2.x)
        {
            if (pos1.y > pos2.y)
            {
                return Vector2.down;
            }
            else
            {
                return Vector2.up;
            }
        }
        else
        {
            if (pos1.x > pos2.x)
            {
                return Vector2.left;
            }
            else
            {
                return Vector2.right;
            }
        }
    }

    private (Node, Node) GetPositionBetweenNodes(Vector3 pos)
    {
        var allNodes = FindObjectsByType<Node>(FindObjectsSortMode.InstanceID).ToList();

        for (int i = 0; i < allNodes.Count; i++)
        {
            if (Mathf.Abs((allNodes[i].transform.position.x) - (pos.x)) < 0.2f)
            {
                var neighbours = allNodes[i].GetNeighbors();
                for (int j = 0; j < neighbours.Count; j++)
                {
                    //if pos.y was between neighbours[j].transform.position.y and allNodes[i].transform.position.y
                    if (neighbours[j].transform.position.x == allNodes[i].transform.position.x &&
                        ((pos.y <= neighbours[j].transform.position.y && pos.y >= allNodes[i].transform.position.y) ||
                        (pos.y >= neighbours[j].transform.position.y && pos.y <= allNodes[i].transform.position.y)))
                    {
                        return (allNodes[i], neighbours[j]);
                    }
                }
            }

            if (Mathf.Abs((allNodes[i].transform.position.y) - (pos.y)) < 0.2f)
            {
                var neighbours = allNodes[i].GetNeighbors();
                for (int j = 0; j < neighbours.Count; j++)
                {
                    if (neighbours[j].transform.position.y == allNodes[i].transform.position.y &&
                        ((pos.x <= neighbours[j].transform.position.x && pos.x >= allNodes[i].transform.position.x) ||
                        (pos.x >= neighbours[j].transform.position.x && pos.x <= allNodes[i].transform.position.x)))
                    {
                        return (allNodes[i], neighbours[j]);
                    }
                }
            }
        }

        return (null, null);
    }

    List<Node> GetDangerousNodes()
    {
        List<Node> returnList = new();
        var allGhosts = FindObjectsByType<Ghost>(FindObjectsSortMode.InstanceID).ToList();

        List<Node> dangerousNodes = new List<Node>();

        for (int i = 0; i < allGhosts.Count; i++)
        {
            if (allGhosts[i].GetComponent<GhostHome>().enabled)
            {
                continue;
            }

            (Node n1, Node n2) = GetPositionBetweenNodes(allGhosts[i].transform.position);
            returnList.Add(n1);
            returnList.Add(n2);
        }

        return returnList;
    }
}
