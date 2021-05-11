using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : PlayerBase
{
    public WaypointManager waypointManager;

    public int perfectShortcutPercent = 100;
    public int jumpShortcutPercent = 80;
    public int missingShortcutPercent = 70;

    float groundY = 0.0f;

    public Vector3 currentWaypoint;
    [SerializeField] int currentWaypointIndex = 0;
    float minDistanceToNextWaypoint = 1.0f;

    int jumpPlankSize = 8;

    void Awake()
    {
        rb.freezeRotation = true;
        rb.useGravity = false;

        if (rb == null)
        {
            Debug.LogError("Bot " + name + "'s rigidBody is null!");
            Debug.Break();
        }

        if (animator == null)
        {
            Debug.LogError("Bot " + name + "'s animator is null!");
            Debug.Break();
        }
    }

    void Update()
    {
        if (!isMultiplierFinished && runningToMultipliers || (canMove && !isFinished))
            animator.SetInteger("AnimState", (int)characterAnimState);
        else animator.SetInteger("AnimState", (int)AnimState.Idle);
    }

    void FixedUpdate()
    {
        if (!isAlive || !canMove || isFinished)
        {
            UpdateCanMove(false);
            return;
        }

        Quaternion tr = Quaternion.LookRotation(currentWaypoint - transform.position);
        Vector3 eulerRotation = tr.eulerAngles;
        eulerRotation.x = 0.0f;
        eulerRotation.z = 0.0f;
        Quaternion targetRotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(eulerRotation), Time.deltaTime * rotatingSpeed);
        transform.rotation = targetRotation;

        transform.position += transform.forward * movementSpeed;
        CheckPlayerFront();

        // if character not grounded then apply gravity
        if (IsGrounded())
        {
            isJumped = false;
            isClimbing = false;
            rb.useGravity = true;
            groundY = transform.position.y;
            goTriggerClimb.SetActive(false);
            characterAnimState = AnimState.Run;
        }
        else
        {
            if (isClimbing)
            {
                rb.useGravity = false;

                if (transform.position.y < groundY)
                    transform.position += transform.up * climbMultiplier;
                else
                {
                    isClimbing = false;
                    transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
                }
            }
            else
            {
                rb.velocity += new Vector3(0, -gravity * rb.mass, 0);
                if (!IsAtMidAir() && rb.velocity.y <= 0.0f)
                {
                    characterAnimState = AnimState.Run;
                    goTriggerClimb.SetActive(false);
                }
            }
        }

        Vector3 tempPos = transform.position;
        tempPos.y = currentWaypoint.y;
        if (Vector3.Distance(tempPos, currentWaypoint) <= minDistanceToNextWaypoint)
            GetNextTarget();
    }

    public void GetFirstWaypoint()
    {
        currentWaypoint = waypointManager.GetFirstWaypoint(transform.position);
        currentWaypointIndex = 1;
    }

    void GetNextTarget()
    {
        bool getNext = true;

        WaypointSector currentSector = waypointManager.GetSector(currentWaypointIndex - 1);
        if (currentSector.mustFollowWaypoint == null)
        {
            if (currentSector == null)
            {
                Debug.LogError("CurrentSector is null!");
                Debug.Break();
            }

            // check if there is a shortcut
            if (currentSector.HasShortcutEnter())
            {
                // bot have more planks than shortcut needs
                if (currentPlankCount >= currentSector.planksToShortcutExit)
                {
                    int chance = Random.Range(0, 100);
                    if (chance >= 100 - perfectShortcutPercent)
                    {
                        currentWaypointIndex = currentSector.shortcutWaypoint.id;
                        currentSector = currentSector.shortcutWaypoint;
                        getNext = false;
                    }
                }
                else if (currentPlankCount + jumpPlankSize >= currentSector.planksToShortcutExit) // if bot can reach to shortcut with a jump
                {
                    int chance = Random.Range(0, 100);
                    if (chance >= 100 - jumpShortcutPercent)
                    {
                        currentWaypointIndex = currentSector.shortcutWaypoint.id;
                        currentSector = currentSector.shortcutWaypoint;
                        getNext = false;
                    }
                }
                else if (currentSector.HasShortcutExit()) // if this shortcut like an island it have to be an exit point
                {
                    // if bot can collect missing planks at the island
                    if (currentPlankCount + jumpPlankSize + currentSector.possiblePlankCollectShortcut >= currentSector.planksToShortcutExit)
                    {
                        int chance = Random.Range(0, 100);
                        if (chance >= 100 - missingShortcutPercent)
                        {
                            currentWaypointIndex = currentSector.shortcutWaypoint.id;
                            currentSector = currentSector.shortcutWaypoint;
                            getNext = false;
                        }
                    }
                }
            }
        }else // there is a must to follow waypoint
        {
            currentWaypointIndex = currentSector.mustFollowWaypoint.id;
            currentSector = currentSector.mustFollowWaypoint;
            getNext = false;
        }

        if(getNext)
        {
            currentSector = waypointManager.GetSector(currentWaypointIndex);
            if (currentSector == null)
            {
                Debug.LogError("nextSector is null!");
                Debug.Break();
            }
        }

        currentWaypoint = waypointManager.GetNextTarget(transform.position, currentSector);
        currentWaypointIndex++;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Plank")
        {
            IncreaseBagPlank();
            other.gameObject.SetActive(false);
        }
        else if (other.tag == "DeathZone")
        {
            transform.position = Vector3.zero;
            rb.useGravity = false;
            bounceDir = Vector3.zero;
            bounceForce = 0.0f;
            hardForce = Vector3.zero;
            rb.velocity = Vector3.zero;
            isAlive = false;

            waterSplashFx.transform.parent = null;
            waterSplashFx.transform.position = new Vector3(waterSplashFx.transform.position.x, 5.5f, waterSplashFx.transform.position.z);
            waterSplashFx.Play();
        }
        else if (other.tag == "Finish")
        {
            isFinished = true;
            goCurrentMultiplerPlatform = other.gameObject;
        }
    }
}