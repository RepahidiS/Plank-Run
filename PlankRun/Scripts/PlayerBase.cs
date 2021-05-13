using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public enum AnimState
{
    Idle = 0,
    Run,
    Jump,
    Climb
}

public class PlayerBase : MonoBehaviour
{
    public GameManager gameManager;

    public bool isAlive = true;
    public bool canMove = false;
    public Rigidbody rb;
    public Animator animator;
    public float movementSpeed = 10.0f;
    public float rotatingSpeed = 15.0f;
    public float jumpMultiplier = 2.0f;
    public float climbMultiplier = 1.0f;
    public float gravity = 5.0f;
    public bool isFinished = false;
    public bool isMultiplierFinished = false;

    public float bounceForce = 0.0f;
    public Vector3 bounceDir = Vector3.zero;
    public Vector3 barrelForce = Vector3.zero;
    public Vector3 hardForce = Vector3.zero;

    public AnimState characterAnimState = AnimState.Idle;

    public GameObject goTriggerClimb;
    public GameObject goTriggerPlank;
    public Transform plankBag;
    public int currentPlankCount = 0;
    public int poolSize = 50;
    private Color _plankColor;
    public List<GameObject> plankPool;
    public TextMeshProUGUI txtPlankCount;
    public TextMeshProUGUI txtName;
    public ParticleSystem waterSplashFx;
    public PlankManager plankManager;

    public bool isJumped = false;
    public bool isClimbing = false;

    public Transform scoreMultiplierPlatforms;
    public bool runningToMultipliers = false;
    public bool multipliersJumpedOnce = false;
    public int currentMultiplier = 1;
    public GameObject goCurrentMultiplerPlatform = null;

    public void CreatePoolObjects()
    {
        Vector3 pos = plankBag.position;
        for (int i = 0; i < poolSize; i++)
        {
            GameObject newGo = Instantiate(plankManager.goTransparentPlank, plankBag);
            pos.y += 0.13f;
            newGo.transform.position = pos;

            plankPool.Add(newGo);
        }
    }

    public void HidePoolPlanks()
    {
        foreach(GameObject go in plankPool)
            go.SetActive(false);
    }

    public void SetPlankBagColor(Color color)
    {
        _plankColor = color;

        foreach (GameObject plank in plankPool)
            plank.GetComponent<MeshRenderer>().material.color = color;
    }

    public void UpdateCanMove(bool _canMove)
    {
        canMove = _canMove;
        characterAnimState = canMove ? AnimState.Run : AnimState.Idle;
        animator.SetInteger("AnimState", (int)characterAnimState);
    }

    public void IncreaseBagPlank()
    {
        if(currentPlankCount < poolSize)
            plankPool[currentPlankCount].SetActive(true);

        currentPlankCount++;

        if(txtPlankCount != null)
            txtPlankCount.text = currentPlankCount.ToString();
    }

    public void DecreaseBagPlank()
    {
        currentPlankCount--;

        if (currentPlankCount <= 0)
            currentPlankCount = 0;

        if (currentPlankCount < poolSize)
            plankPool[currentPlankCount].SetActive(false);

        if(txtPlankCount != null)
        {
            if (currentPlankCount <= 0)
                txtPlankCount.text = "";
            else txtPlankCount.text = currentPlankCount.ToString();
        }
    }

    public bool IsGrounded()
    {
        // using QueryTriggerInteraction.Ignore because we dont want to raycast hit to triggers like deathzone
        Debug.DrawRay(transform.position, Vector3.down * 0.1f, Color.white);
        return Physics.Raycast(transform.position, Vector3.down, 0.1f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    public bool IsAtMidAir()
    {
        Debug.DrawRay(transform.position, Vector3.down * 5.0f, Color.green);
        return !Physics.Raycast(transform.position, Vector3.down, 5.0f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isAlive)
            return;

        // this is not a true way to detect climbing action
        // we need to know which collider of character collided right now
        if(isJumped && goTriggerClimb.activeSelf)
        {
            if(collision.gameObject.tag == "Floor" || collision.gameObject.tag == "PlankWalkable" || collision.gameObject.tag == "ScoreMultiplier")
            {
                goTriggerClimb.SetActive(false);
                characterAnimState = AnimState.Climb;
                isJumped = false;
                isClimbing = true;
            }
        }
    }

    public void SetHardForce(Vector3 force)
    {
        hardForce = force;
    }

    public void SetBarrelForce(Vector3 force)
    {
        barrelForce = force;
    }

    public void HitPlayer(Vector3 velocityF, float time)
    {
        velocityF.y = 0.0f; // just keep our character on ground

        bounceForce = velocityF.magnitude;
        bounceDir = Vector3.Normalize(velocityF);
        StartCoroutine(Decrease(velocityF.magnitude, time));
    }

    private IEnumerator Decrease(float value, float duration)
    {
        float delta = value / duration;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            yield return null;

            bounceForce -= Time.deltaTime * delta;
            bounceForce = bounceForce < 0 ? 0 : bounceForce;
        }
    }

    public void CheckPlayerFront()
    {
        Vector3 pos = goTriggerPlank.transform.position;
        pos -= goTriggerPlank.transform.forward * 0.3f;
        pos.y += 0.2f;

        Debug.DrawRay(pos, Vector3.down, Color.red);
        if (!Physics.Raycast(pos, Vector3.down, 1.0f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (currentPlankCount > 0)
            {
                DecreaseBagPlank();
                plankManager.SpawnPlank(goTriggerPlank.transform.position, transform.rotation, _plankColor);
            }
            else if (!isJumped && !isClimbing)
            {
                characterAnimState = AnimState.Jump;
                rb.velocity = rb.velocity + Vector3.up * jumpMultiplier;
                isJumped = true;
                goTriggerClimb.SetActive(true);

                if (runningToMultipliers)
                    multipliersJumpedOnce = true;
            }
        }
    }
}