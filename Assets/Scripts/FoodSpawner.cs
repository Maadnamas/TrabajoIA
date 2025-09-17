using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab; 
    public int maxFood = 10;
    public float spawnInterval = 3f;
    public float spawnRange = 20f;

    private List<GameObject> spawnedFood = new List<GameObject>();

    void Start()
    {
        for (int i = 0; i < maxFood; i++)
        {
            SpawnFoodImmediate();
        }

        InvokeRepeating(nameof(SpawnFood), spawnInterval, spawnInterval);
    }

    void SpawnFood()
    {
        if (spawnedFood.Count >= maxFood) return;

        SpawnFoodImmediate();
    }

    void SpawnFoodImmediate()
    {
        Vector3 spawnPos = new Vector3(
            Random.Range(-spawnRange, spawnRange),
            0.5f,
            Random.Range(-spawnRange, spawnRange)
        );

        GameObject newFood = Instantiate(foodPrefab, spawnPos, Quaternion.identity);
        spawnedFood.Add(newFood);

        newFood.GetComponent<Food>().OnEaten += HandleFoodEaten;
    }

    void HandleFoodEaten(GameObject food)
    {
        spawnedFood.Remove(food);
    }
}