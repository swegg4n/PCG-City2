using UnityEngine;


public class CurbGenerator : MonoBehaviour
{
    [SerializeField] private GameObject curbFiller;
    [SerializeField] private Material curbMaterial;
    [SerializeField] private Material grassMaterial;
    public float curbWidth = 1.0f;
    [SerializeField] private float curbHeight = 0.15f;

    private RoadNode[,] roadNodes;
    private Transform curbsParent;
    private Transform parksParent;


    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        Destroy(GameObject.Find("Curbs"));
        curbsParent = new GameObject("Curbs", typeof(MeshCombiner)).transform;
        Destroy(GameObject.Find("Parks"));
        parksParent = new GameObject("Parks", typeof(MeshCombiner)).transform;

        roadNodes = GetComponent<RoadGenerator>().RoadNodes;

        GenerateCurbs();

        curbsParent.GetComponent<MeshCombiner>().CombineMeshes();
        parksParent.GetComponent<MeshCombiner>().CombineMeshes();
    }


    private void GenerateCurbs()
    {
        foreach (var rn in roadNodes)
        {
            if (rn.Up_connection && rn.Right_connection)
            {
                if (rn.Up_connection.ActiveConnection == false)
                {
                    Vector3[] polygon = new Vector3[4]
                    {
                        rn.ModelVertices[0],
                        rn.Up_connection.ConnectedNode.ModelVertices[2],
                        rn.Up_connection.ConnectedNode.ModelVertices[3],
                        rn.ModelVertices[1],
                    };
                    PolygonCreator.ExtrudePolygon(polygon, curbHeight, curbsParent, curbFiller);
                    PolygonCreator.CreatePolygon(polygon, curbsParent, curbMaterial, 0.0f, curbHeight);
                }

                if (rn.Right_connection.ActiveConnection == false)
                {
                    Vector3[] polygon = new Vector3[4]
                      {
                        rn.ModelVertices[1],
                        rn.Right_connection.ConnectedNode.ModelVertices[0],
                        rn.Right_connection.ConnectedNode.ModelVertices[2],
                        rn.ModelVertices[3]
                      };
                    PolygonCreator.ExtrudePolygon(polygon, curbHeight, curbsParent, curbFiller);
                    PolygonCreator.CreatePolygon(polygon, curbsParent, curbMaterial, 0.0f, curbHeight);
                }

                Vector3[] openAreaPolygon = new Vector3[4]
                {
                    rn.ModelVertices[1],
                    rn.Up_connection.ConnectedNode.ModelVertices[3],
                    rn.Up_connection.ConnectedNode.Right_connection.ConnectedNode.ModelVertices[2],
                    rn.Right_connection.ConnectedNode.ModelVertices[0],
                };

                PolygonCreator.ExtrudePolygon(openAreaPolygon, curbHeight, curbsParent, curbFiller);

                if (rn.IsPark == false)
                {
                    PolygonCreator.CreatePolygon(openAreaPolygon, curbsParent, curbMaterial, 0.0f, curbHeight);
                }
                else
                {
                    Vector3[] insetOpenAreaPolygon = PolygonCreator.InsetPolygon(openAreaPolygon, curbWidth);

                    PolygonCreator.ExtrudePolygon(insetOpenAreaPolygon, curbHeight, curbsParent, curbFiller, true);
                    PolygonCreator.CreatePolygon(insetOpenAreaPolygon, parksParent, grassMaterial);

                    PolygonCreator.CreatePolygon(new Vector3[] { openAreaPolygon[0], openAreaPolygon[1], insetOpenAreaPolygon[1], insetOpenAreaPolygon[0] }, curbsParent, curbMaterial, 0.0f, curbHeight);
                    PolygonCreator.CreatePolygon(new Vector3[] { openAreaPolygon[1], openAreaPolygon[2], insetOpenAreaPolygon[2], insetOpenAreaPolygon[1] }, curbsParent, curbMaterial, 0.0f, curbHeight);
                    PolygonCreator.CreatePolygon(new Vector3[] { openAreaPolygon[2], openAreaPolygon[3], insetOpenAreaPolygon[3], insetOpenAreaPolygon[2] }, curbsParent, curbMaterial, 0.0f, curbHeight);
                    PolygonCreator.CreatePolygon(new Vector3[] { openAreaPolygon[3], openAreaPolygon[0], insetOpenAreaPolygon[0], insetOpenAreaPolygon[3] }, curbsParent, curbMaterial, 0.0f, curbHeight);
                }
            }

            if (rn.HasAnyConnections == false)
            {
                PolygonCreator.CreatePolygon(new Vector3[] { rn.ModelVertices[0], rn.ModelVertices[1], rn.ModelVertices[3], rn.ModelVertices[2] }, curbsParent, curbMaterial, 0.0f, curbHeight);
            }
        }
    }

}
