using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WaypointManager : MonoBehaviour
{
    public GameObject[] spawnPoints;
    public List<WaypointSector> sectors = new List<WaypointSector>();

    void Awake()
    {
        // I hope this one wont affect performance ^^
        spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");

        sectors = transform.GetComponentsInChildren<WaypointSector>().ToList();
        sectors.RemoveAll(i => i.type != WaypointType.Standard);
        for (int i = 0; i < sectors.Count; i++)
            sectors[i].id = i;
    }

    public Vector3 GetFirstWaypoint(Vector3 pos)
    {
        WaypointSector currentSector = sectors.Where(i => i.id == 0).FirstOrDefault();
        if (currentSector != null)
        {
            float distance = float.MaxValue;
            Waypoint closestWaypoint = null;
            foreach (Waypoint waypoint in currentSector.waypoints)
            {
                float currentDistance = Vector3.Distance(pos, waypoint.center);
                if (currentDistance < distance)
                {
                    distance = currentDistance;
                    closestWaypoint = waypoint;
                }
            }

            if (closestWaypoint != null)
                return new Vector3(Random.Range(closestWaypoint.min.x, closestWaypoint.max.x), closestWaypoint.center.y, Random.Range(closestWaypoint.min.z, closestWaypoint.max.z));
        }

        return Vector3.zero;
    }

    public WaypointSector GetSector(int sectorId)
    {
        return sectors.Where(i => i.id == sectorId).FirstOrDefault();
    }

    // get next sector waypoints
    // calculate and find closest waypoint
    // get random x coordinate and return it
    public Vector3 GetNextTarget(Vector3 pos, WaypointSector currentSector)
    {
        Waypoint closestWaypoint = currentSector.GetClosestWaypoint(pos);
        if (closestWaypoint != null)
            return new Vector3(Random.Range(closestWaypoint.min.x, closestWaypoint.max.x), closestWaypoint.center.y, Random.Range(closestWaypoint.min.z, closestWaypoint.max.z));

        return Vector3.zero;
    }

    public Vector3 GetNextSpawnPoint(int index)
    {
        if(index >= spawnPoints.Length)
        {
            Debug.LogError("SpawnPoint index out of bounds. " + index);
            Debug.Break();
            return Vector3.zero;
        }

        return spawnPoints[index].transform.position;
    }

    public Vector3 GetFinishPos()
    {
        WaypointSector sector = sectors.Where(i => i.tag == "Finish").FirstOrDefault();
        if(sector == null)
        {
            Debug.LogError("Finish waypoint not found.");
            Debug.Break();
            return Vector3.zero;
        }

        return sector.transform.position;
    }

    public Vector3 GetRandomPosBetween(Vector3 pos1, Vector3 pos2)
    {
        return new Vector3(Random.Range(pos1.x, pos2.x), pos1.y, Random.Range(pos1.z, pos2.z));
    }
}