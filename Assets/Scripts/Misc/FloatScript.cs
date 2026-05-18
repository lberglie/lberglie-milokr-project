using UnityEngine;

public class FloatScript : MonoBehaviour
{
    [SerializeField] private float floatAmplitude = 0.2f; // How far up and down the object moves
    [SerializeField] private float floatFrequency = 1f;    // How fast the object moves up and down
    [SerializeField] private bool randomizeStart = true; // Whether to randomize the starting position

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        if (randomizeStart)
        {
            floatFrequency += Random.Range(-0.5f, 0.5f); // Randomize frequency slightly
            float randomX = Random.Range(-floatAmplitude, floatAmplitude);
            float randomY = Random.Range(-floatAmplitude, floatAmplitude);
            float randomZ = Random.Range(-floatAmplitude, floatAmplitude);
            startPosition += new Vector3(randomX, randomY, randomZ);
        }
    }

    void Update()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}