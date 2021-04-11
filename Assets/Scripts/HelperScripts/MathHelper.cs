using UnityEngine;

static class MathHelper
{
    public static int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    public static Vector3 Project(Vector3 P, Vector3 v, Vector3 origin)
    {
        Vector3 OP = P - origin;
        Vector3 u_doublePrime = Vector3.Dot(OP, v) / v.sqrMagnitude * v;

        return origin + v * u_doublePrime.magnitude;
    }
}

