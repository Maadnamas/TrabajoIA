using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public GameObject boidPrefab;
    public int boidCount = 20;
    public float spawnRange = 15f;

    [Header("Layers")]
    public LayerMask boidLayer;

    public List<GameObject> boids = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < boidCount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(-spawnRange, spawnRange),
                0.2f,
                Random.Range(-spawnRange, spawnRange)
            );
            GameObject b = Instantiate(boidPrefab, pos, Quaternion.identity, transform);
            boids.Add(b);
            Boid boidComp = b.GetComponent<Boid>();
            if (boidComp) boidComp.manager = this;
        }
    }
}
