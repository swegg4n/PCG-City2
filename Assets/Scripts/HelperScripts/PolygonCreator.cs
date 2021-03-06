using System;
using System.Linq;
using UnityEngine;

public static class PolygonCreator
{
    public static Vector3[] InsetPolygon(Vector2[] points, float inset)
    {
        Vector2 offsetDir = Vector2.zero;

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[points.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            if (inset != 0)
            {
                Vector2 tangent1 = (points[(i + 1) % points.Length] - points[i]).normalized;
                Vector2 tangent2 = (points[i] - points[MathHelper.Mod(i - 1, points.Length)]).normalized;

                Vector2 normal1 = new Vector2(-tangent1.y, tangent1.x).normalized;
                Vector2 normal2 = new Vector2(-tangent2.y, tangent2.x).normalized;

                offsetDir = (normal1 + normal2) / 2;
                offsetDir *= inset / offsetDir.magnitude;
            }
            vertices[i] = new Vector3(points[i].x - offsetDir.x, 0, points[i].y - offsetDir.y);
        }

        return vertices;
    }

    public static Vector3[] InsetPolygon(Vector3[] points, float inset)
    {
        Vector2[] pointsV2 = System.Array.ConvertAll<Vector3, Vector2>(points, V3toV2);
        return InsetPolygon(pointsV2, inset);
    }


    public static Tuple<GameObject, Vector3[]> CreatePolygon(Vector2[] points, Transform parent, Material material, float inset = 0.0f, float heightOffset = 0.0f, bool flipNormals = false)
    {
        Triangulator tr = new Triangulator(points);
        int[] indices = tr.Triangulate();

        Vector2 offsetDir = Vector2.zero;


        float xMax = float.NegativeInfinity;
        float yMax = float.NegativeInfinity;
        Vector2[] uvs = new Vector2[points.Length];

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[points.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            if (inset != 0)
            {
                Vector2 tangent1 = (points[(i + 1) % points.Length] - points[i]).normalized;
                Vector2 tangent2 = (points[i] - points[MathHelper.Mod(i - 1, points.Length)]).normalized;

                Vector2 normal1 = new Vector2(-tangent1.y, tangent1.x).normalized;
                Vector2 normal2 = new Vector2(-tangent2.y, tangent2.x).normalized;

                offsetDir = (normal1 + normal2) / 2;
                offsetDir *= -inset / offsetDir.magnitude;
            }
            vertices[i] = new Vector3(points[i].x - offsetDir.x, heightOffset, points[i].y - offsetDir.y);

            if (points[i].x > xMax)
                xMax = points[i].x;
            if (points[i].y > yMax)
                yMax = points[i].y;

        }
        for (int i = 0; i < uvs.Length; i++)
            uvs[i] = new Vector2(points[i].x / xMax, points[i].y / yMax);

        // Create the mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        if (flipNormals) mesh.triangles = mesh.triangles.Reverse().ToArray();

        // Set up game object with mesh;
        GameObject gameObject = new GameObject("poly", typeof(MeshFilter), typeof(MeshRenderer));
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        gameObject.GetComponent<MeshRenderer>().material = material;
        gameObject.transform.parent = parent;

        return new Tuple<GameObject, Vector3[]>(gameObject, vertices);
    }

    public static Tuple<GameObject, Vector3[]> CreatePolygon(Vector3[] points, Transform parent, Material material, float inset = 0.0f, float heightOffset = 0.0f, bool flipNormals = false)
    {
        Vector2[] pointsV2 = System.Array.ConvertAll<Vector3, Vector2>(points, V3toV2);
        return CreatePolygon(pointsV2, parent, material, inset, heightOffset, flipNormals);
    }


    public static void ExtrudePolygon(Vector3[] points, float height, Transform parent, GameObject prefab, bool flipNormals = false)
    {
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 direction = points[(i + 1) % points.Length] - points[i];

            Vector3 position = points[i];// + new Vector3(0, height, 0);
            Quaternion rotation = Quaternion.Euler(0, -Vector3.SignedAngle(direction, Vector3.right, Vector3.up), 0);

            GameObject curbFill = GameObject.Instantiate(prefab, position, rotation, parent);
            curbFill.transform.localScale = new Vector3(direction.magnitude, height, flipNormals ? -1.0f : 1.0f);
        }
    }

    public static Vector3[,] DividePolygon(Vector3[] mainPolygon, int x_divisions, int y_divisions)
    {
        Vector3[,] vertices = new Vector3[x_divisions + 2, y_divisions + 2];

        Vector3[] side_x_top = DivideSide(mainPolygon[1], mainPolygon[2], x_divisions);
        Vector3[] side_x_bot = DivideSide(mainPolygon[0], mainPolygon[3], x_divisions);
        Vector3[] side_y_left = DivideSide(mainPolygon[0], mainPolygon[1], y_divisions);
        Vector3[] side_y_right = DivideSide(mainPolygon[3], mainPolygon[2], y_divisions);

        for (int i = 0; i < side_y_left.Length; i++)
            vertices[0, i] = side_y_left[i];

        for (int i = 0; i < side_y_right.Length; i++)
            vertices[vertices.GetLength(0) - 1, i] = side_y_right[i];

        for (int x = 1; x <= x_divisions; x++)
        {
            Vector3 from = side_x_bot[x];
            Vector3 to = side_x_top[x];
            Vector3[] vertical_y = DivideSide(from, to, y_divisions);

            for (int y = 0; y < vertical_y.Length; y++)
            {
                vertices[x, y] = vertical_y[y];
            }
        }

        return vertices;
    }

    public static Vector3[] DivideSide(Vector3 from, Vector3 to, int divisions)
    {
        Vector3[] vertices = new Vector3[2 + divisions];
        vertices[0] = from;
        vertices[vertices.Length - 1] = to;

        Vector3 dir = to - from;
        for (int d = 1; d <= divisions; d++)
        {
            Vector3 origDivVertex = dir / (divisions + 1) * d + from;
            vertices[d] = origDivVertex;
        }

        return vertices;
    }



    private static Vector2 V3toV2(Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);
    }
}
