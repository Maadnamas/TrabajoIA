using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    
    public float maxSpeed = 5f;
    public float maxForce = 0.5f;
    public float neighborRadius = 3f;
    public float separationRadius = 1f;

    public Transform targetFood; // comida detectada
    public Transform hunter;     // NPC cazador detectado
    public LayerMask boidMask;   // para detectar otros boids

    private Vector3 velocity;
    

}

