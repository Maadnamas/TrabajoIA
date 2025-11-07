using System.Collections.Generic;
using UnityEngine;

public class AlertManager : MonoBehaviour
{
    public static AlertManager Instance { get; private set; }
    private List<EnemyAI> enemies = new List<EnemyAI>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Register(EnemyAI e)
    {
        if (!enemies.Contains(e)) enemies.Add(e);
    }

    public void Unregister(EnemyAI e)
    {
        if (enemies.Contains(e)) enemies.Remove(e);
    }

    public void BroadcastAlert(Vector3 pos, float duration = 6f)
    {
        foreach (var e in enemies)
            e.OnAlert(pos, duration);
    }
}