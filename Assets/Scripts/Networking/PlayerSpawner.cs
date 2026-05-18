using UnityEngine;
using Unity.Netcode;
using NUnit.Framework;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviour
{

    public Transform spawnPointsContainer;

    private List<Transform> spawnPoints = new List<Transform>();
    private int nextSpawnIndex = 0;


    private void Awake()
    {
        // Get all spawn points from the container
        foreach (Transform child in spawnPointsContainer)
        {
            spawnPoints.Add(child);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        }
    }
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // 1. Approve the connection
        response.Approved = true;

        // 2. Tell NGO to create the player object
        response.CreatePlayerObject = true;

        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points found! Please add spawn points as children of the spawnPointsContainer.");
            return;
        }
        else
        {

            Transform currentSpawn = spawnPoints[nextSpawnIndex];

            // You could use request.ClientNetworkId to assign different spawns to different players.
            response.Position = currentSpawn.position;
            response.Rotation = currentSpawn.rotation;

            nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Count; // Move to the next spawn point for the next player
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
    }

}
