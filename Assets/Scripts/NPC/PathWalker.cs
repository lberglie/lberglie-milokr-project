using System.Collections.Generic;
using UnityEngine;

public class PathWalker : MonoBehaviour
{

    [SerializeField] float speed = 5f;
    [SerializeField] ProceduralGenerator proceduralGenerator;
    [SerializeField] GameObject player1;
    [SerializeField] GameObject player2; // does not do anything (for now)

    List<Vector2Int> path;
    int currentPathIndex;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector2Int startGrid = WorldToGrid(transform.position);
        Vector2Int endGrid = WorldToGrid(player1.transform.position);
        path = PathFinder.FindShortestPath(startGrid, endGrid, proceduralGenerator.grid);
    }

    // Update is called once per frame
    void Update()
    {
        if (path == null || currentPathIndex >= path.Count) return;

        Vector3 nextWorldPos = GridToWorld(path[currentPathIndex]);
        transform.position = Vector3.MoveTowards(transform.position, nextWorldPos, speed * Time.deltaTime);

        if (transform.position == nextWorldPos) {
            currentPathIndex++;
        }
    }

    Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * 8, 0, gridPos.y * 8);
    }

    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPos.x / 8), Mathf.RoundToInt(worldPos.z / 8));
    }
}
