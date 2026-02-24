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
    public List<GameObject> food = new List<GameObject>();
    public List<GameObject> hunters = new List<GameObject>();

    public GameBounds gameBounds;

    void Start()
    {
        for (int i = 0; i < boidCount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(gameBounds.minBounds.x, gameBounds.maxBounds.x),
                0.2f,
                Random.Range(gameBounds.minBounds.z, gameBounds.maxBounds.z)
            );

            GameObject b = Instantiate(boidPrefab, pos, Quaternion.identity, transform);
            boids.Add(b);

            Boid boidComp = b.GetComponent<Boid>();
            if (boidComp)
            {
                boidComp.manager = this;
                boidComp.gameBounds = gameBounds;
            }
        }
    }

    public void AddFood(GameObject f)
    {
        if (!food.Contains(f))
            food.Add(f);
    }

    public void RemoveFood(GameObject f)
    {
        if (food.Contains(f))
            food.Remove(f);
    }

    public void AddHunter(GameObject h)
    {
        if (!hunters.Contains(h))
            hunters.Add(h);
    }

    public List<GameObject> GetLayerObjects(LayerMask layer)
    {
        List<GameObject> list = new List<GameObject>();
        if (layer == boidLayer) list.AddRange(boids);
        else if (layer == (1 << LayerMask.NameToLayer("Food"))) list.AddRange(food);
        else if (layer == (1 << LayerMask.NameToLayer("Hunter"))) list.AddRange(hunters);
        return list;
    }
}
