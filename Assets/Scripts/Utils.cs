using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    public static Vector3 Limit(Vector3 v, float max)
    {
        if (v.sqrMagnitude > max * max) return v.normalized * max;
        return v;
    }
}
