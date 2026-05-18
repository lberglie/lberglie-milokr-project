using System;
using System.Collections.Generic;
using UnityEngine;

public class PathWalker : MonoBehaviour
{

    [SerializeField] float speed = 5f;
    [SerializeField] ProceduralGenerator proceduralGenerator;


    List<Vector2Int> path;
    int currentPathIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        proceduralGenerator.GenerateWorld();
        GameObject player1 = GameObject.FindWithTag("Player (Clone)");
        GameObject player2 = GameObject.FindWithTag("Player (Clone)(1)"); // unused (for now)
        transform.position = new Vector3(25 * 8, transform.position.y, 25 * 8);
        Debug.Log("moved npc to spawn");
        Vector2Int startGrid = WorldToGrid(transform.position);
        Vector2Int endGrid = WorldToGrid(player1.transform.position);
        path = PathFinder.FindShortestPath(startGrid, endGrid, proceduralGenerator.grid);
    }

    // Update is called once per frame
    void Update()
    {
        if (path == null || currentPathIndex >= path.Count)
        {
            Debug.Log("path == null || currentPathIndex >= path.Count");
            return;
        }

        Vector3 nextWorldPos = GridToWorld(path[currentPathIndex]);
        Debug.Log("moving");
        transform.position = Vector3.MoveTowards(transform.position, nextWorldPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, nextWorldPos) < 0.1f)
            {
            Debug.Log("moved 1 step");
            currentPathIndex++;
        }
    }

    Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * 8, transform.position.y, gridPos.y * 8);
    }

    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPos.x / 8), Mathf.RoundToInt(worldPos.z / 8));
    }
}
