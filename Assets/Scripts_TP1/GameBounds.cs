using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBounds : MonoBehaviour
{
    public Vector3 minBounds = new Vector3(-10f, 0f, -10f);
    public Vector3 maxBounds = new Vector3(10f, 0f, 10f);

    public Vector3 ClampPosition(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
        pos.y = 0f;
        pos.z = Mathf.Clamp(pos.z, minBounds.z, maxBounds.z);
        return pos;
    }

    public Vector3 RandomPosition()
    {
        float x = Random.Range(minBounds.x, maxBounds.x);
        float z = Random.Range(minBounds.z, maxBounds.z);
        return new Vector3(x, 0f, z);
    }
}
