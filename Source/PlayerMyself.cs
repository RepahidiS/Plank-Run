using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMyself : PlayerBase
{
    Vector2 touchStartPos;
    Vector2 touchCurrentPos;
    [SerializeField] Vector2 touchDiff;
    [SerializeField] Vector3 moveDirection;
    bool touchEnded = false;

    float groundY = 0.0f;

    // just for debugging...
    public RawImage startImage;
    public RawImage currentImage;

    void Awake()
    {
        rb.freezeRotation = true;
        rb.useGravity = false;

        if (rb == null)
        {
            Debug.LogError("MainCharacter rigidBody is null!");
            Debug.Break();
        }

        if (animator == null)
        {
            Debug.LogError("MainCharacter animator is null!");
            Debug.Break();
        }

        CreatePoolObjects();
    }

    public void ResetMainCharacter()
    {
        transform.position = new Vector3(2.5f, 6f, 1.0f);
        transform.rotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 0.0f));
        isAlive = true;
        canMove = false;
        isFinished = false;
        isMultiplierFinished = false;
        runningToMultipliers = false;
        touchStartPos = Vector2.zero;
        touchCurrentPos = Vector2.zero;
        touchDiff = Vector2.zero;
        moveDirection = Vector3.forward;
        characterAnimState = AnimState.Idle;
        rb.useGravity = true;
        rb.velocity = Vector3.zero;
        currentPlankCount = 0;
        isJumped = false;
        isClimbing = false;
        multipliersJumpedOnce = false;
        currentMultiplier = 1;
        goCurrentMultiplerPlatform = null;
        goTriggerClimb.SetActive(false);
        HidePoolPlanks();
        DecreaseBagPlank();
    }

    void Update()
    {
        // this feature is not working when we building the game...
        /*if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android
        || EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
        {
            // is there any touch?
            if(Input.touchCount > 0)
            {
                // get first finger touch info
                Touch touch = Input.GetTouch(0);

                // touch just began
                if(touch.phase == TouchPhase.Began)
                {
                    touchEnded = false;
                    isMoving = true;
                    touchStartPos = touch.position;
                }

                // finger is moving to another position
                if(touch.phase == TouchPhase.Moved)
                {
                    touchEnded = false;
                    touchCurrentPos = touch.position;
                }

                // release this finger
                if(touch.phase == TouchPhase.Ended)
                {
                    touchEnded = true;
                    isMoving = false;
                    rb.velocity = Vector3.zero;
                }
            }
        }else if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows)
        {*/
        // left mouse button down
        if (Input.GetMouseButtonDown(0))
        {
            touchEnded = false;
            touchStartPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }

        // dragging with left mouse button
        if (Input.GetMouseButton(0))
        {
            touchEnded = false;
            touchCurrentPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }

        // release left mouse button
        if (Input.GetMouseButtonUp(0))
        {
            touchEnded = true;
            //rb.velocity = Vector3.zero;
        }

        if (touchEnded)
        {
            touchStartPos = Vector2.zero;
            touchCurrentPos = Vector2.zero;
            touchDiff = Vector2.zero;
            moveDirection = Vector3.forward;
        }
        else
        {
            touchDiff = touchCurrentPos - touchStartPos;
            // keep our touchDiff values between 100 & -100
            if (touchDiff.x != 0.0f)
                touchDiff.x = touchDiff.x > 0.0f ? Mathf.Min(touchDiff.x, 100.0f) : Mathf.Max(touchDiff.x, -100.0f);
            if (touchDiff.y != 0.0f)
                touchDiff.y = touchDiff.y > 0.0f ? Mathf.Min(touchDiff.y, 100.0f) : Mathf.Max(touchDiff.y, -100.0f);
            UpdateMoveDirection();

            // just for debugging...
            //startImage.rectTransform.position = touchStartPos;
            //currentImage.rectTransform.position = touchCurrentPos;
        }

        if (!isMultiplierFinished && runningToMultipliers || (canMove && !isFinished))
            animator.SetInteger("AnimState", (int)characterAnimState);
        else animator.SetInteger("AnimState", (int)AnimState.Idle);
    }

    void FixedUpdate()
    {
        if(!isAlive)
            return;

        if (isMultiplierFinished || !runningToMultipliers && (!canMove || isFinished))
            return;

        // rotate character
        if (moveDirection.x != 0)
        {
            Vector3 dir = moveDirection;
            dir.y = 0;

            Quaternion tr = Quaternion.LookRotation(dir);
            Vector3 tempRot = tr.eulerAngles + transform.rotation.eulerAngles;
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(tempRot), Time.deltaTime * rotatingSpeed);
            transform.rotation = targetRotation;
        }

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
            if(isClimbing)
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
    }

    void UpdateMoveDirection()
    {
        if (touchDiff.x > 5)
            moveDirection.x = touchDiff.x / 100.0f;
        else if (touchDiff.x < -5)
            moveDirection.x = -(touchDiff.x / -100.0f);
        else moveDirection.x = 0.0f;

        moveDirection.z = 1.0f;
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
            if (multipliersJumpedOnce)
            {
                isMultiplierFinished = true;
                transform.position = goCurrentMultiplerPlatform.transform.position + new Vector3(0, 6, 0);
            }
            else
            {
                rb.useGravity = false;
                bounceDir = Vector3.zero;
                bounceForce = 0.0f;
                hardForce = Vector3.zero;
                rb.velocity = Vector3.zero;
                isAlive = false;

                waterSplashFx.transform.parent = null;
                waterSplashFx.transform.position = new Vector3(transform.position.x, 5.5f, transform.position.z);
                waterSplashFx.Play();

                Camera.main.transform.parent = null;
                transform.position = Vector3.zero;
            }
        }
        else if (other.tag == "Finish")
        {
            isFinished = true;
            goCurrentMultiplerPlatform = other.gameObject;
            gameManager.PlayerFinished();
        }
        else if (other.tag == "ScoreMultiplier")
        {
            goCurrentMultiplerPlatform = other.gameObject;
            bool isLast = false;
            for (int i = 0; i < scoreMultiplierPlatforms.childCount; i++)
            {
                if (scoreMultiplierPlatforms.GetChild(i).gameObject == goCurrentMultiplerPlatform)
                {
                    currentMultiplier = i + 2;
                    isLast = currentMultiplier == 10;
                }
            }

            if (multipliersJumpedOnce || isLast)
                isMultiplierFinished = true;
        }
    }
}