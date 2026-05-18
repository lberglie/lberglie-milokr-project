using UnityEngine;

public class DisableCamera : MonoBehaviour
{


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Camera.main.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
