using UnityEngine;
using UnityEngine.SocialPlatforms;

public class RoadGenerator : MonoBehaviour
{
    [SerializeField] private Material roadMaterial;
    [SerializeField] private int areaSize = 100;
    [SerializeField] private int tileCount = 10;
    [Range(0, 1.0f)] [SerializeField] private float straightness = 0;
    [SerializeField] private float roadWidth = 3.0f;
    [Range(0, 1.0f)] [SerializeField] private float removeRoadChance = 0.2f;
    [Range(0, 1.0f)] [SerializeField] private float isParkChance = 0.1f;

    private float tileSize;
    public RoadNode[,] RoadNodes { get; private set; }

    Transform roadNetworkParent;



    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        Destroy(GameObject.Find("RoadNetwork"));
        roadNetworkParent = new GameObject("RoadNetwork", typeof(MeshCombiner)).transform;

        tileSize = (float)areaSize / tileCount;

        CreateNodes();
        CreateConnections();
        CreateModelVertices();
        CreateRoadModels();

        roadNetworkParent.GetComponent<MeshCombiner>().CombineMeshes();
    }


    private void CreateNodes()
    {
        RoadNodes = new RoadNode[tileCount, tileCount];

        for (int y = 0; y < tileCount; y++)
        {
            for (int x = 0; x < tileCount; x++)
            {
                float lowerBound = straightness * tileSize / 2;
                float upperBound = tileSize - straightness * tileSize / 2;

                bool isPark = Random.Range(0.0f, 1.0f) < isParkChance;

                RoadNodes[x, y] = new RoadNode(new Vector3(Random.Range(lowerBound, upperBound) + tileSize * x, 0, Random.Range(lowerBound, upperBound) + tileSize * y), isPark);
            }
        }
    }

    private void CreateConnections()
    {
        for (int y = 0; y < tileCount; y++)
        {
            for (int x = 0; x < tileCount; x++)
            {
                bool activeConnection = Random.Range(0.0f, 1.0f) > removeRoadChance || x == 0 || x == tileCount - 1 || y == 0 || y == tileCount - 1;

                if (x != 0)
                {
                    RoadNode.CreateConnection(RoadNodes[x, y], RoadNodes[x - 1, y], Direction.Left, activeConnection);
                }
                if (x != tileCount - 1)
                {
                    RoadNode.CreateConnection(RoadNodes[x, y], RoadNodes[x + 1, y], Direction.Right, activeConnection);
                }
                if (y != 0)
                {
                    RoadNode.CreateConnection(RoadNodes[x, y], RoadNodes[x, y - 1], Direction.Down, activeConnection);
                }
                if (y != tileCount - 1)
                {
                    RoadNode.CreateConnection(RoadNodes[x, y], RoadNodes[x, y + 1], Direction.Up, activeConnection);
                }
            }
        }
    }

    private void CreateModelVertices()
    {
        foreach (var rn in RoadNodes)
        {
            rn.CreateModelVectices(roadWidth);
        }
    }


    private void CreateRoadModels()
    {
        foreach (var rn in RoadNodes)
        {
            if (rn.HasAnyConnections)
            {
                PolygonCreator.CreatePolygon(new Vector3[] { rn.ModelVertices[0], rn.ModelVertices[1], rn.ModelVertices[3], rn.ModelVertices[2] }, roadNetworkParent, roadMaterial);

                if (rn.Up_connection && rn.Up_connection.ActiveConnection)
                {
                    PolygonCreator.CreatePolygon(new Vector3[] { rn.ModelVertices[0], rn.Up_connection.ConnectedNode.ModelVertices[2], rn.Up_connection.ConnectedNode.ModelVertices[3] }, roadNetworkParent, roadMaterial);
                    PolygonCreator.CreatePolygon(new Vector3[] { rn.Up_connection.ConnectedNode.ModelVertices[3], rn.ModelVertices[1], rn.ModelVertices[0] }, roadNetworkParent, roadMaterial);
                }
                if (rn.Right_connection && rn.Right_connection.ActiveConnection)
                {
                    PolygonCreator.CreatePolygon(new Vector3[] { rn.ModelVertices[3], rn.ModelVertices[1], rn.Right_connection.ConnectedNode.ModelVertices[0] }, roadNetworkParent, roadMaterial);
                    PolygonCreator.CreatePolygon(new Vector3[] { rn.ModelVertices[3], rn.Right_connection.ConnectedNode.ModelVertices[0], rn.Right_connection.ConnectedNode.ModelVertices[2] }, roadNetworkParent, roadMaterial);
                }
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (RoadNodes != null)
        {
            foreach (var rn in RoadNodes)
            {
                if (rn != null)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(rn.Position, Gizmos.probeSize * 10);

                    if (rn.Up_connection)
                    {
                        Gizmos.color = (rn.Up_connection.ActiveConnection) ? Color.white : Color.gray;
                        Gizmos.DrawLine(rn.Position, rn.Up_connection.ConnectedNode.Position);
                    }
                    if (rn.Down_connection)
                    {
                        Gizmos.color = (rn.Down_connection.ActiveConnection) ? Color.white : Color.gray;
                        Gizmos.DrawLine(rn.Position, rn.Down_connection.ConnectedNode.Position);
                    }
                    if (rn.Left_connection)
                    {
                        Gizmos.color = (rn.Left_connection.ActiveConnection) ? Color.white : Color.gray;
                        Gizmos.DrawLine(rn.Position, rn.Left_connection.ConnectedNode.Position);
                    }
                    if (rn.Right_connection)
                    {
                        Gizmos.color = (rn.Right_connection.ActiveConnection) ? Color.white : Color.gray;
                        Gizmos.DrawLine(rn.Position, rn.Right_connection.ConnectedNode.Position);
                    }

                    Gizmos.color = Color.red;
                    for (int i = 0; i < rn.ModelVertices.Length; i++)
                    {
                        if (rn.ModelVertices[i] != null)
                        {
                            Gizmos.DrawSphere(rn.ModelVertices[i], Gizmos.probeSize * 5);
                        }
                    }
                }
            }
        }
    }

}
