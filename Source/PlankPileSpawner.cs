using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlankPileSpawner : MonoBehaviour
{
    public int spawnCooldown = 3;
    bool isOnCooldown = false;

    public void ResetPile()
    {
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(true);

        isOnCooldown = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(!isOnCooldown)
            {
                isOnCooldown = true;
                StartCoroutine(RespawnPlanks());
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (!isOnCooldown)
            {
                isOnCooldown = true;
                StartCoroutine(RespawnPlanks());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            if (!isOnCooldown)
            {
                isOnCooldown = true;
                StartCoroutine(RespawnPlanks());
            }
        }
    }

    IEnumerator RespawnPlanks()
    {
        yield return new WaitForSeconds(spawnCooldown);

        ResetPile();
    }
}