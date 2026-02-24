using System.Collections.Generic;
using UnityEngine;

public class AlertManager : MonoBehaviour
{
    public static AlertManager Instance { get; private set; }

    private List<EnemyAI> enemies = new List<EnemyAI>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        Debug.Log("[AlertManager] Awake. Instance asignada.");
    }

    public void Register(EnemyAI e)
    {
        if (!enemies.Contains(e)) enemies.Add(e);
        Debug.Log($"[AlertManager] Register: {e.name} (total: {enemies.Count})");
    }

    public void Unregister(EnemyAI e)
    {
        if (enemies.Contains(e)) enemies.Remove(e);
        Debug.Log($"[AlertManager] Unregister: {e.name} (total: {enemies.Count})");
    }

    public void BroadcastAlert(Vector3 pos, float duration = 6f)
    {
        Debug.Log($"[AlertManager] BroadcastAlert at {pos} to {enemies.Count} enemies");
        foreach (var e in enemies)
        {
            if (e != null) e.OnAlert(pos, duration);
        }
    }
}