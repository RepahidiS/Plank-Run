using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlankManager : MonoBehaviour
{
    public int poolSize;
    public GameObject goTransparentPlank;
    public GameObject goPlank;
    public List<GameObject> plankPool = new List<GameObject>();
    public int currentIndex = 0;
    public GameManager gameManager;

    void Awake()
    {
        for(int i = 0; i < poolSize; i++)
        {
            GameObject newGo = Instantiate(goPlank, Vector3.zero, Quaternion.identity, transform);
            newGo.SetActive(false);
            plankPool.Add(newGo);
        }
    }

    public void HidePlanks()
    {
        foreach (GameObject go in plankPool)
            go.SetActive(false);

        currentIndex = 0;
    }

    public void SpawnPlank(Vector3 pos, Quaternion rot, Color color)
    {
        GameObject currentGo = plankPool[currentIndex];
        if(currentGo != null)
        {
            pos.y = 5.93f;
            currentGo.transform.position = pos;            
            currentGo.transform.rotation = rot;
            currentGo.GetComponent<MeshRenderer>().material.color = color;
            currentGo.SetActive(true);
        }

        currentIndex++;
        if(currentIndex >= poolSize)
            gameManager.OutOfPoolObjects();
    }
}