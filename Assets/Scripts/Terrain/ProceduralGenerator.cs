using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;

public class ProceduralGenerator : MonoBehaviour
{
    [SerializeField] GameObject FloorObject;
    [SerializeField] GameObject WallObject;
    [SerializeField] bool SpawnRoof = false; // Checkbox for roof instantiation
    [SerializeField] float RoofHeight = 8f; // Configurable roof height

    public TMP_InputField seedInputField;

    // Public seed property, settable from outside
    public int Seed = 0;

    // grid is accessible so pathfinding can use it
    public int[,] grid {  get; set; }

    struct Room
    {
        public int x, y;       // top left corner of the room
        public int width, height;

        public Vector2Int Center()
        {
            return new Vector2Int(x + width / 2, y + height / 2); // returns the center cell of the room, used for connecting rooms with corridors
        }
    }

   
    List<Room> rooms = new List<Room>();

    // Local System.Random instance for deterministic generation
    private System.Random rng;

    public void GenerateWorld()
    {
        grid = new int[50, 50];
        rooms.Clear();
        rng = (seedInputField != null) ? new System.Random(int.Parse(seedInputField.text)) : new System.Random(0);
        Debug.Log($"Using seed: {rng.GetHashCode()}");
        // Reserve 2x2 spawn room at (24,24), (24,25), (25,24), (25,25)
        for (int i = 24; i <= 25; i++)
            for (int j = 24; j <= 25; j++)
                grid[i, j] = 1;
        GenerateRooms(50, 10, 20);
        List<(Room, Room)> connections = CreateConnectionList();
        foreach (var (a, b) in connections)
        {
            Vector2Int startPos = a.Center();
            Vector2Int endPos = b.Center();
            ConnectRooms(startPos, endPos);
        }
        ConnectMiddleToClosestRoom();
        PlaceWalls(50);
        SpawnTiles(50);
    }

    void GenerateRooms(int gridSize, int roomSize, int amountOfRoomGenerations)
    {
        for (int i = 0; i < amountOfRoomGenerations; i++)
        {
            int x = rng.Next(0, gridSize);
            int y = rng.Next(0, gridSize);
            int width = rng.Next(1, roomSize);
            int height = rng.Next(1, roomSize);
            bool valid = false;

            // check if the room is within bounds of the grid
            if (x + width < 50 && y + height < 50)
            {
                valid = true;
                // Prevent overlap with spawn room (24,24)-(25,25)
                if (!(x + width > 24 && x < 26 && y + height > 24 && y < 26))
                {
                    foreach (Room otherRoom in rooms)
                    {
                        if (x < otherRoom.x + otherRoom.width
                            && x + width > otherRoom.x
                            && y < otherRoom.y + otherRoom.height
                            && y + height > otherRoom.y)
                        {
                            valid = false;
                            break;
                        }
                    }
                }
                else
                {
                    valid = false;
                }
            }

            if (valid)
            {
                for (int j = x; j < x + width; j++)
                {
                    for (int k = y; k < y + height; k++)
                    {
                        grid[j, k] = 1;
                    }
                }
                rooms.Add(new Room { x = x, y = y, width = width, height = height });
            }
        }
    }

    Room FindClosestRoom(Room room) // deprecated, keeping here if needed later on.
    {
        Room closestRoom = room;
        float closestDistance = float.MaxValue;
        foreach (Room otherRoom in rooms)
        {
            if (otherRoom.x == room.x && otherRoom.y == room.y) continue; // skip if the same room
            float distance = Vector2Int.Distance(room.Center(), otherRoom.Center());
            // if the distance is less than the current closest distance, update the closest room
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestRoom = otherRoom;
            }
        }
        return closestRoom;
    }

    List<(Room, Room)> CreateConnectionList()
    {
        List<(Room a, Room b, float dist)> ConnectionsWithDistance = new List<(Room, Room, float)>();
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                float distance = Vector2Int.Distance(rooms[i].Center(), rooms[j].Center());
                ConnectionsWithDistance.Add((rooms[i], rooms[j], distance));
            }
        }
        ConnectionsWithDistance.Sort((a, b) => a.dist.CompareTo(b.dist)); // sort connections by shortest distance

        List<(Room, Room)> connections = new List<(Room, Room)>();
        List<Room> connected = new List<Room>();
        connected.Add(rooms[0]);

        while (connected.Count < rooms.Count)
        {
            foreach (var Connection in ConnectionsWithDistance)
            {
                bool aConnected = connected.Contains(Connection.a);
                bool bConnected = connected.Contains(Connection.b);
                if (aConnected != bConnected) // one is connected, one isn't
                {
                    connections.Add((Connection.a, Connection.b));
                    connected.Add(aConnected ? Connection.b : Connection.a);
                    break;
                }
            }
        }

        return connections;
    }

    void ConnectRooms(Vector2Int startPos, Vector2Int endPos)
    {
            int xStep = 1;
            int yStep = 1;
            if (startPos.x > endPos.x)  {  xStep = -1; }
            if (startPos.y > endPos.y)  {  yStep = -1; }
            for (int i = startPos.x; i != endPos.x; i += xStep)
            {
                grid[i, startPos.y] = 1; // might need to check previous value of cell first, we'll see later on
            }
            for (int j = startPos.y; j != endPos.y; j += yStep)
            {
                grid[endPos.x, j] = 1;
            }
    }

    void PlaceWalls(int size)
    {
        for (int i = 0; i != size; i++)
        {
            for (int j = 0; j != size; j++)
            {
                if (grid[i,j] == 0)
                {
                    grid[i, j] = 2; // make all empty cells (0) into walls (2), might change later if needed
                }
            }
        }
    }

    void SpawnTiles(int size)
    {
        for (int i = 0; i != size; i++)
        {
            for (int j = 0; j != size; j++)
            {
                if (grid[i, j] == 1)
                {
                    Instantiate(FloorObject, new Vector3(i*8, 0, j*8), Quaternion.identity, transform);
                    if (SpawnRoof)
                        Instantiate(WallObject, new Vector3(i*8, RoofHeight, j*8), Quaternion.identity, transform);
                }   
                else if (grid[i, j] == 2)
                {
                    Instantiate(WallObject, new Vector3(i*8, 0, j*8), Quaternion.identity, transform);
                    if (SpawnRoof)
                        Instantiate(WallObject, new Vector3(i*8, RoofHeight, j*8), Quaternion.identity, transform);
                }
            }
        }
    }

    // Connect the middle of the spawn room to the closest room with a floor corridor
    void ConnectMiddleToClosestRoom()
    {
        Vector2Int middle = new Vector2Int(25, 25);
        grid[24, 24] = 1;
        grid[24, 25] = 1;
        grid[24, 26] = 1;
        grid[25, 24] = 1;
        grid[25, 25] = 1;
        grid[25, 26] = 1;
        grid[26, 24] = 1;
        grid[26, 25] = 1;
        grid[26, 26] = 1;
        if (rooms.Count == 0) return;
        Room closest = rooms[0];
        float minDist = Vector2Int.Distance(middle, rooms[0].Center());
        foreach (var room in rooms)
        {
            float dist = Vector2Int.Distance(middle, room.Center());
            if (dist < minDist)
            {
                minDist = dist;
                closest = room;
            }
        }
        ConnectRooms(middle, closest.Center());
    }
}
