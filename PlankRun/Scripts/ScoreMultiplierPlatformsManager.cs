using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreMultiplierPlatformsManager : MonoBehaviour
{
    public float defaultZ;
    public int minZ;
    public int maxZ;
    public bool movePlatforms = false;
    public float movementSpeed = 0.2f;

    private int _currentIndex = 0;

    void Awake()
    {
        defaultZ = transform.GetChild(0).position.z;
        ResetPlatforms();
    }

    public void ResetPlatforms()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            Vector3 pos = transform.GetChild(i).position;
            pos.y = -2.0f;
            pos.z = defaultZ + Random.Range(minZ, maxZ);
            transform.GetChild(i).position = pos;
        }
    }

    void FixedUpdate()
    {
        if (!movePlatforms)
            return;

        Transform currentPlatform = transform.GetChild(_currentIndex);
        Vector3 pos = currentPlatform.position;
        pos.y += movementSpeed;
        if(pos.y >= 0.0f)
        {
            pos.y = 0.0f;
            if(_currentIndex + 1 <= transform.childCount - 1)
                _currentIndex++;
            else
            {
                _currentIndex = 0;
                movePlatforms = false;
            }
        }

        currentPlatform.position = pos;
    }
}