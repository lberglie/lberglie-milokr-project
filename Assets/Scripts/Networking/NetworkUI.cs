using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkUI : MonoBehaviour
{

    public TMP_InputField ipInputField;
    public TMP_InputField seedInputField;
    public Button hostButton;
    public Button joinButton;
    public GameObject uiPanel;
    public Camera placeholderCamera;
    
    public ProceduralGenerator generator;

    void Start()
    {
        // Set default IP
        ipInputField.text = "127.0.0.1";

        seedInputField.text = "0";

        hostButton.onClick.AddListener(StartHost);
        joinButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        SetIP();
        generator.GenerateWorld();
        NetworkManager.Singleton.StartHost();
        HideUI();
    }

    private void StartClient()
    {
        SetIP();
        NetworkManager.Singleton.StartClient();
        HideUI();
    }

    private void SetIP()
    {
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Address = ipInputField.text;
    }

    private void HideUI()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
        if (placeholderCamera != null) placeholderCamera.gameObject.SetActive(false);
    }
}