using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Type
{
    Parent,
    Child,
    Shortcut
}

public class WaypointGizmo : MonoBehaviour
{
    public Type type;

    void OnDrawGizmos()
    {
        Vector3 pos = transform.position;
        pos.y += 1;

        Gizmos.color = Color.red;
        if (type == Type.Child)
            Gizmos.DrawSphere(pos, 0.1f);
        else if (type == Type.Parent)
            Gizmos.DrawCube(pos, new Vector3(0.5f, 0.5f, 0.5f));
        else if(type == Type.Shortcut)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube(pos, new Vector3(0.5f, 0.5f, 0.5f));
        }
    }
}
