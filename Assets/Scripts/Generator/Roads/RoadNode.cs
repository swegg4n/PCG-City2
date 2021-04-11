using UnityEngine;

public enum Direction { Up, Down, Left, Right }


public class RoadNodeConnection
{
    public RoadNodeConnection(RoadNode to, bool active)
    {
        this.ConnectedNode = to;
        this.ActiveConnection = active;
    }
    public RoadNode ConnectedNode { get; private set; }
    public bool ActiveConnection { get; private set; }


    public static implicit operator bool(RoadNodeConnection rn_c)
    {
        return rn_c != null;
    }
}


public class RoadNode
{
    public Vector3 Position { get; private set; }

    public RoadNodeConnection Up_connection { get; private set; } = null;
    public RoadNodeConnection Down_connection { get; private set; } = null;
    public RoadNodeConnection Left_connection { get; private set; } = null;
    public RoadNodeConnection Right_connection { get; private set; } = null;

    public bool HasAnyConnections
    {
        get
        {
            return (Up_connection && Up_connection.ActiveConnection)
                || (Down_connection && Down_connection.ActiveConnection)
                || (Left_connection && Left_connection.ActiveConnection)
                || (Right_connection && Right_connection.ActiveConnection);
        }
    }

    public bool IsPark { get; private set; }


    public Vector3[] ModelVertices { get; private set; }



    public RoadNode(Vector3 position, bool isPark)
    {
        this.Position = position;
        this.ModelVertices = new Vector3[4];
        this.IsPark = isPark;
    }


    public static void CreateConnection(RoadNode from, RoadNode to, Direction direction, bool activeConnection)
    {
        switch (direction)
        {
            case Direction.Up:
                from.Up_connection = new RoadNodeConnection(to, activeConnection);
                to.Down_connection = new RoadNodeConnection(from, activeConnection);
                break;

            case Direction.Down:
                from.Down_connection = new RoadNodeConnection(to, activeConnection);
                to.Up_connection = new RoadNodeConnection(from, activeConnection);
                break;

            case Direction.Left:
                from.Left_connection = new RoadNodeConnection(to, activeConnection);
                to.Right_connection = new RoadNodeConnection(from, activeConnection);
                break;

            case Direction.Right:
                from.Right_connection = new RoadNodeConnection(to, activeConnection);
                to.Left_connection = new RoadNodeConnection(from, activeConnection);
                break;
        }
    }

    public void CreateModelVectices(float roadWidth)
    {
        float vertexOffset = Mathf.Sqrt(2) * roadWidth / 2;

        //Vector3? upDir = (Up_connection) ? new Vector3?(Up_connection.ConnectedNode.Position - Position) : null;
        //Vector3? downDir = (Down_connection) ? new Vector3?(Down_connection.ConnectedNode.Position - Position) : null;
        //Vector3? leftDir = (Left_connection) ? new Vector3?(Left_connection.ConnectedNode.Position - Position) : null;
        //Vector3? rightDir = (Right_connection) ? new Vector3?(Right_connection.ConnectedNode.Position - Position) : null;

        //if (upDir == null)
        //    upDir = -downDir;
        //if (downDir == null)
        //    downDir = -upDir;
        //if (leftDir == null)
        //    leftDir = -rightDir;
        //if (rightDir == null)
        //    rightDir = -leftDir;

        //ModelVertices[0] = ((Vector3)upDir + (Vector3)leftDir).normalized * vertexOffset + Position;
        //ModelVertices[1] = ((Vector3)upDir + (Vector3)rightDir).normalized * vertexOffset + Position;
        //ModelVertices[2] = ((Vector3)downDir + (Vector3)leftDir).normalized * vertexOffset + Position;
        //ModelVertices[3] = ((Vector3)downDir + (Vector3)rightDir).normalized * vertexOffset + Position;

        ModelVertices[0] = new Vector3(-vertexOffset, 0.0f, vertexOffset) + Position;
        ModelVertices[1] = new Vector3(vertexOffset, 0.0f, vertexOffset) + Position;
        ModelVertices[2] = new Vector3(-vertexOffset, 0.0f, -vertexOffset) + Position;
        ModelVertices[3] = new Vector3(vertexOffset, 0.0f, -vertexOffset) + Position;
    }

}

