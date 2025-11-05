using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public Action<GameObject> OnEaten;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boid"))
        {
            OnEaten?.Invoke(gameObject);
            Destroy(gameObject);
        }
    }
}