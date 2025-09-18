using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;
    public int maxFood = 10;
    public float spawnInterval = 3f;

    public GameBounds gameBounds;
    public BoidManager manager; // referencia al manager

    private List<GameObject> spawnedFood = new List<GameObject>();

    void Start()
    {
        if (gameBounds == null)
            gameBounds = FindObjectOfType<GameBounds>();

        if (manager == null)
            manager = FindObjectOfType<BoidManager>();

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
        Vector3 spawnPos;

        if (gameBounds != null)
            spawnPos = gameBounds.RandomPosition();
        else
            spawnPos = new Vector3(
                UnityEngine.Random.Range(-10f, 10f),
                0.5f,
                UnityEngine.Random.Range(-10f, 10f)
            );

        GameObject newFood = Instantiate(foodPrefab, spawnPos, Quaternion.identity);
        spawnedFood.Add(newFood);

        // Registrar en el manager
        if (manager != null)
            manager.AddFood(newFood);

        Food foodComp = newFood.GetComponent<Food>();
        if (foodComp != null)
            foodComp.OnEaten += HandleFoodEaten;
    }

    void HandleFoodEaten(GameObject food)
    {
        spawnedFood.Remove(food);

        // Quitar del manager
        if (manager != null)
            manager.RemoveFood(food);
    }
}   