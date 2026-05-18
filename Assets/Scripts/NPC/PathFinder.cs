using System.Collections.Generic;
using UnityEngine;

public class PathFinder
{
    public static List<Vector2Int> FindShortestPath(Vector2Int startPos, Vector2Int endPos, int[,] grid)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);


        Node[,] nodes = new Node[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                nodes[x, y] = new Node(new Vector2Int(x, y), grid[x, y] == 1); 
            }
        }

        if (!nodes[startPos.x, startPos.y].walkable || !nodes[endPos.x, endPos.y].walkable) {
            return null;
        }

            List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();

        open.Add(nodes[startPos.x, startPos.y]);

        while (open.Count > 0) {
            Node current = open[0];
            foreach (Node node in open) {
                if (node.fCost < current.fCost) {
                    current = node;
                }
            }

            open.Remove(current);
            closed.Add(current);

            if (current.pos == endPos)
            {
                List<Vector2Int> path = new List<Vector2Int>();
                Node retrace = current;
                while (retrace != null)
                {
                    path.Add(retrace.pos);
                    retrace = retrace.parent;
                }
                path.Reverse();
                return path;
            }

            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = current.pos + dir;

                if (neighborPos.x < 0 || neighborPos.x >= width || neighborPos.y < 0 || neighborPos.y >= height){
                    continue;
                }

                Node neighbor = nodes[neighborPos.x, neighborPos.y];

                if (!neighbor.walkable || closed.Contains(neighbor))
                    continue;

                int newG = current.gCost + 1;

                if (newG < neighbor.gCost || !open.Contains(neighbor))
                {
                    neighbor.gCost = newG;
                    neighbor.hCost = Mathf.Abs(neighborPos.x - endPos.x) + Mathf.Abs(neighborPos.y - endPos.y);
                    neighbor.parent = current;

                    if (!open.Contains(neighbor))
                        open.Add(neighbor);
                }
            }
        }
        return null;
    }
}

public class Node
{
    public Vector2Int pos;
    public int gCost, hCost;
    public int fCost => gCost + hCost;
    public Node parent;
    public bool walkable;

    public Node(Vector2Int pos, bool walkable)
    {
        this.pos = pos;
        this.walkable = walkable;
    }
}