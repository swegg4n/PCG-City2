using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildingsGenerator : MonoBehaviour
{
    [SerializeField] private Material floorMaterial;
    [SerializeField] private Material mainMat;
    [SerializeField] private Material subMat;

    [SerializeField] private float buildingsInset = 0.3f;
    [SerializeField] private Vector2Int minMaxBuildingHeight = new Vector2Int(6, 20);

    [SerializeField] private Vector2Int minMaxBuildingDivisions = new Vector2Int(0, 2);

    private (Material, Material)[] materialsArr = new (Material, Material)[20];

    private GameObject[] walls2m;
    private GameObject[] walls4m;
    [SerializeField] private GameObject roofPrefab;
    private float shortWallChance = 0.4f;

    private RoadNode[,] roadNodes;
    private Transform buildingsParent;



    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        Destroy(GameObject.Find("Buildings"));
        buildingsParent = new GameObject("Buildings").transform;

        roadNodes = GetComponent<RoadGenerator>().RoadNodes;

        walls2m = GetWallModels(new string[] { "Assets/Prefabs/2m" }, "");
        walls4m = GetWallModels(new string[] { "Assets/Prefabs/4m" }, "");

        GenerateMaterials();
        GenerateBuildings();
    }


    private void GenerateMaterials()
    {
        for (int i = 0; i < materialsArr.Length; i++)
        {
            (Color, Color) colors = ColorSwatchGenerator.RandomMatchingColors();
            Material m1 = new Material(mainMat) { color = colors.Item1 };
            Material m2 = new Material(subMat) { color = colors.Item2 };
            materialsArr[i] = (m1, m2);
        }
    }

    private GameObject[] GetWallModels(string[] foldersToSearch, string filter)
    {
        string[] guids = AssetDatabase.FindAssets(filter, foldersToSearch);
        List<GameObject> a = new List<GameObject>();
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            a.Add(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        }
        return a.ToArray();
    }

    private void GenerateBuildings()
    {
        float curbInset = GetComponent<CurbGenerator>().curbWidth * 0.5f;

        foreach (var rn in roadNodes)
        {
            if (rn.IsPark == false && rn.Up_connection && rn.Right_connection)
            {
                Vector3[] openAreaVerts = new Vector3[4]
                {
                    rn.ModelVertices[1],
                    rn.Up_connection.ConnectedNode.ModelVertices[3],
                    rn.Up_connection.ConnectedNode.Right_connection.ConnectedNode.ModelVertices[2],
                    rn.Right_connection.ConnectedNode.ModelVertices[0]
                };

                Vector3[] buildingVerts = PolygonCreator.InsetPolygon(openAreaVerts, buildingsInset);

                int x_divisions = Random.Range(minMaxBuildingDivisions.x, minMaxBuildingDivisions.y + 1);
                int y_divisions = Random.Range(minMaxBuildingDivisions.x, minMaxBuildingDivisions.y + 1);

                Vector3[,] subPolys = PolygonCreator.DividePolygon(buildingVerts, x_divisions, y_divisions);

                for (int x = 0; x < subPolys.GetLength(0) - 1; x++)
                {
                    for (int y = 0; y < subPolys.GetLength(1) - 1; y++)
                    {
                        Vector3[] subBuilding = PolygonCreator.InsetPolygon(new Vector3[] { subPolys[x, y], subPolys[x, y + 1], subPolys[x + 1, y + 1], subPolys[x + 1, y] }, curbInset);
                        GenerateBuilding(subBuilding);
                    }
                }
            }
        }
    }

    static int c = 0;
    private void GenerateBuilding(Vector3[] vertices)
    {
        Transform parent = new GameObject($"Building_{c++}", typeof(MeshCombiner)).transform;
        parent.parent = buildingsParent;

        int floors = Random.Range(minMaxBuildingHeight.x, minMaxBuildingHeight.y);

        (Material m1, Material m2) = materialsArr[Random.Range(0, materialsArr.Length - 1)];

        for (int i = 0; i < vertices.Length; i++)
        {
            GenerateWall(vertices[(i + 1) % vertices.Length], vertices[i], floors, m1, m2, parent);
            GenerateRoof(vertices[(i + 1) % vertices.Length], vertices[i], floors, m1, parent);
        }

        PolygonCreator.CreatePolygon(vertices, parent, floorMaterial, -0.2f, 0.01f);
        for (int f = 0; f < floors; f++)
        {
            PolygonCreator.CreatePolygon(vertices, parent, floorMaterial, -0.2f, 2.6f * (f + 1), true);
            PolygonCreator.CreatePolygon(vertices, parent, floorMaterial, -0.2f, 2.6f * (f + 1), false);
        }

        parent.GetComponent<MeshCombiner>().CombineMeshes();
    }


    private void GenerateWall(Vector3 from, Vector3 to, int floors, Material m1, Material m2, Transform parent)
    {
        Vector3 direction = (to - from).normalized;
        Quaternion rotation = Quaternion.Euler(0, -Vector3.SignedAngle(direction, Vector3.right, Vector3.up), 0);

        Vector3 divisionPoint = from;
        Vector3 floorOffset = new Vector3(0.0f, 2.6f, 0.0f);

        while (Vector3.Distance(divisionPoint, to) >= 4)
        {
            int divisionLength = Random.Range(0, 1.0f) < shortWallChance ? 4 : 2;

            GameObject wallPrefab = GetWallPrefab(walls2m, walls4m, divisionLength < 3 ? true : false);
            GameObject instance = GameObject.Instantiate(wallPrefab, divisionPoint, rotation, parent);
            divisionPoint += direction * divisionLength;


            Material[] mats = instance.GetComponent<MeshRenderer>().materials;
            for (int i = 0; i < mats.Length; i++)
            {
                switch (mats[i].name)
                {
                    case "MainMat (Instance)":
                        mats[i] = m1;
                        break;

                    case "SubMat (Instance)":
                        mats[i] = m2;
                        break;
                }
            }
            instance.GetComponent<MeshRenderer>().materials = mats;

            for (int f = 0; f < floors; f++)
            {
                GameObject.Instantiate(instance, instance.transform.position + floorOffset * f, rotation, parent);
            }
        }

        float restDistance = Vector3.Distance(divisionPoint, to);
        GameObject restWall = GameObject.Instantiate(walls4m[0], divisionPoint, rotation, parent);
        restWall.transform.localScale = new Vector3(restDistance / 4, 1, 1);

        Material[] RestMats = restWall.GetComponent<MeshRenderer>().materials;
        RestMats[0] = m1;
        restWall.GetComponent<MeshRenderer>().materials = RestMats;

        for (int f = 0; f < floors; f++)
        {
            GameObject.Instantiate(restWall, restWall.transform.position + floorOffset * f, rotation, parent);
        }
    }


    private void GenerateRoof(Vector3 from, Vector3 to, int floors, Material m1, Transform parent)
    {
        float roofLengthIncrease = 0;
        float roofLength = Vector3.Distance(from, to) + roofLengthIncrease;
        Vector3 direction = (to - from).normalized;

        Quaternion rotation = Quaternion.Euler(0, -Vector3.SignedAngle(direction, Vector3.right, Vector3.up), 0);

        GameObject roof = GameObject.Instantiate(roofPrefab, from - roofLengthIncrease / 2 * direction + new Vector3(0.0f, 2.6f * floors, 0.0f), rotation, parent);
        roof.transform.localScale = new Vector3(roofLength / 4, 1, 1);

        Material[] mats = roof.GetComponent<MeshRenderer>().materials;
        mats[0] = m1;
        roof.GetComponent<MeshRenderer>().materials = mats;
    }


    private GameObject GetWallPrefab(GameObject[] walls2m, GameObject[] walls4m, bool shortWall)
    {
        int index;
        float random = Random.Range(0, 1.0f);

        if (random < 0.6f)
            index = 0;
        else if (random < 0.75f)
            index = 1;
        else if (random < 0.9f)
            index = 2;
        else
            index = 3;

        return shortWall ? walls2m[index] : walls4m[index];
    }
}
