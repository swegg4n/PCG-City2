using UnityEngine;

public class DecorationsGenerator : MonoBehaviour
{
    [SerializeField] private GameObject streetLamp;
    [SerializeField] private Vector2Int minMaxLampsPerRoad = new Vector2Int(2, 3);

    private RoadNode[,] roadNodes;
    private Transform decorationsParent;



    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        Destroy(GameObject.Find("Decorations"));
        decorationsParent = new GameObject("Decorations").transform;

        roadNodes = GetComponent<RoadGenerator>().RoadNodes;

        GenerateStreetLamps();
    }


    private void GenerateStreetLamps()
    {
        Transform parent = new GameObject("StreetLamps", typeof(MeshCombiner)).transform;
        parent.parent = decorationsParent;

        float lampsInset = 0.5f;
        float curbHeight = GetComponent<CurbGenerator>().curbHeight;

        foreach (var rn in roadNodes)
        {
            if (rn.Up_connection && rn.Up_connection.ActiveConnection)
            {
                int lampsCount = Random.Range(minMaxLampsPerRoad.x, minMaxLampsPerRoad.y + 1);

                (Vector3, Vector3) fromTo_02 = (rn.ModelVertices[0], rn.Up_connection.ConnectedNode.ModelVertices[2]);
                (Vector3, Vector3) fromTo_13 = (rn.ModelVertices[1], rn.Up_connection.ConnectedNode.ModelVertices[3]);

                Vector3 tangent_02 = (fromTo_02.Item2 - fromTo_02.Item1).normalized;
                Vector3 normal_02 = Quaternion.Euler(0, -90.0f, 0) * tangent_02;
                Vector3 tangent_13 = (fromTo_13.Item2 - fromTo_13.Item1).normalized;
                Vector3 normal_13 = Quaternion.Euler(0, 90.0f, 0) * tangent_13;

                Vector3[] lampPositions_02 = PolygonCreator.DivideSide(fromTo_02.Item1, fromTo_02.Item2, lampsCount);
                Vector3[] lampPositions_13 = PolygonCreator.DivideSide(fromTo_13.Item1, fromTo_13.Item2, lampsCount);

                Quaternion rotation_02 = Quaternion.LookRotation(normal_02, Vector3.up);
                Quaternion rotation_13 = Quaternion.LookRotation(normal_13, Vector3.up);

                for (int i = 1; i < lampPositions_02.Length - 1; i++)
                    Instantiate(streetLamp, lampPositions_02[i] + normal_02 * lampsInset + Vector3.up * curbHeight, rotation_02, parent);
                for (int i = 1; i < lampPositions_13.Length - 1; i++)
                    Instantiate(streetLamp, lampPositions_13[i] + normal_13 * lampsInset + Vector3.up * curbHeight, rotation_13, parent);

            }
            if (rn.Right_connection && rn.Right_connection.ActiveConnection)
            {
                int lampsCount = Random.Range(minMaxLampsPerRoad.x, minMaxLampsPerRoad.y + 1);

                (Vector3, Vector3) fromTo_32 = (rn.ModelVertices[3], rn.Right_connection.ConnectedNode.ModelVertices[2]);
                (Vector3, Vector3) fromTo_10 = (rn.ModelVertices[1], rn.Right_connection.ConnectedNode.ModelVertices[0]);

                Vector3 tangent_32 = (fromTo_32.Item2 - fromTo_32.Item1).normalized;
                Vector3 normal_32 = Quaternion.Euler(0, 90.0f, 0) * tangent_32;
                Vector3 tangent_10 = (fromTo_10.Item2 - fromTo_10.Item1).normalized;
                Vector3 normal_10 = Quaternion.Euler(0, -90.0f, 0) * tangent_10;

                Vector3[] lampPositions_32 = PolygonCreator.DivideSide(fromTo_32.Item1, fromTo_32.Item2, lampsCount);
                Vector3[] lampPositions_10 = PolygonCreator.DivideSide(fromTo_10.Item1, fromTo_10.Item2, lampsCount);

                Quaternion rotation_32 = Quaternion.LookRotation(normal_32, Vector3.up);
                Quaternion rotation_10 = Quaternion.LookRotation(normal_10, Vector3.up);

                for (int i = 1; i < lampPositions_32.Length - 1; i++)
                    Instantiate(streetLamp, lampPositions_32[i] + normal_32 * lampsInset + Vector3.up * curbHeight, rotation_32, parent);
                for (int i = 1; i < lampPositions_10.Length - 1; i++)
                    Instantiate(streetLamp, lampPositions_10[i] + normal_10 * lampsInset + Vector3.up * curbHeight, rotation_10, parent);
            }

        }

        parent.GetComponent<MeshCombiner>().CombineMeshes();
    }
}
