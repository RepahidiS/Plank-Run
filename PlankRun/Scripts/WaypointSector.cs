using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Waypoint
{
    public Vector3 center;
    public Vector3 min;
    public Vector3 max;

    // şuanki haliyle z sabit geliyor onun yerine 2 vector arasında random position veren bir sistem yapılacak
    // ve waypoint kontrolleri tek axis yerine vector bazında kontrol edilecek
    public Waypoint(Vector3 _center, Vector3 _min, Vector3 _max)
    {
        center = _center;
        min = _min;
        max = _max;
    }
}

public enum WaypointType
{
    Standard,
    ShortcutCalculate
}

public class WaypointSector : MonoBehaviour
{
    public WaypointType type;
    public int id;
    public List<Waypoint> waypoints = new List<Waypoint>();

    public Transform[] shortcutEnter = new Transform[2];
    public Transform[] shortcutExit = new Transform[2];
    public WaypointSector shortcutWaypoint;
    public WaypointSector mustFollowWaypoint;

    public int planksToShortcutExit = 0;
    public int possiblePlankCollectShortcut = 0;
    float plankSize = 0.7f;

    void Awake()
    {
        if(type == WaypointType.Standard)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform currentWaypoint = transform.GetChild(i);
                Transform tMax = currentWaypoint.GetChild(0);
                Transform tMin = currentWaypoint.GetChild(1);

                waypoints.Add(new Waypoint(currentWaypoint.position, tMin.position, tMax.position));
            }
        }

        if(shortcutEnter.Length > 2 || shortcutExit.Length > 2)
        {
            Debug.LogError("Shortcut arrays could have max 2 elements. " + name);
            Debug.Break();
        }

        if(shortcutEnter.Length == 1 || shortcutExit.Length == 1)
        {
            Debug.LogError("Shortcut arrays need to have 2 elements to calculate distance. " + name);
            Debug.Break();
        }

        // we have a shortcut enter
        if(shortcutEnter.Length == 2)
        {
            if(shortcutWaypoint == null)
            {
                Debug.LogError("Shortcut need a waypoint to follow. " + name);
                Debug.Break();
            }else
            {
                float distance = Vector3.Distance(shortcutEnter[0].position, shortcutEnter[1].position);
                // if this shortcut like an island it have to be an exit point
                if (shortcutExit.Length == 2)
                    distance += Vector3.Distance(shortcutExit[0].position, shortcutExit[1].position);

                planksToShortcutExit = (int)Mathf.Ceil(distance / plankSize);
            }
        }
    }

    public Waypoint GetClosestWaypoint(Vector3 pos)
    {
        float distance = float.MaxValue;
        Waypoint closestWaypoint = null;
        foreach (Waypoint waypoint in waypoints)
        {
            float currentDistance = Vector3.Distance(pos, waypoint.center);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closestWaypoint = waypoint;
            }
        }

        if (closestWaypoint != null)
            return closestWaypoint;
        return null;
    }

    public bool HasShortcutEnter()
    {
        return shortcutEnter.Length == 2;
    }

    public bool HasShortcutExit()
    {
        return shortcutExit.Length == 2;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (shortcutEnter.Length == 2)
            Gizmos.DrawLine(AddY(shortcutEnter[0].position), AddY(shortcutEnter[1].position));

        Gizmos.color = Color.red;
        if (shortcutExit.Length == 2)
            Gizmos.DrawLine(AddY(shortcutExit[0].position), AddY(shortcutExit[1].position));
    }

    Vector3 AddY(Vector3 pos)
    {
        pos.y += 1;
        return pos;
    }
}