﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharMovement : MonoBehaviour
{
    public float GRAVITY = -0.4f;
    public float MAX_FALL = -20.0f;
    public float MAX_SPEED = 0.4f;
    public float RUN_SPEED = 0.05f;
    public float GROUND_DRAG = 0.90f;
    public float LIMITER_DRAG = 0.995f;
    public float JUMP_HEIGHT = 6f;
    public float AERIAL_CONTROL = 0.4f;
    public int SWING_CHARGE_FRAMES = 30;
    public float SWING_STRENGTH = 25.0f;
    public int SWING_STEAM_COST = 50;
    private float MINIMUM_SPEED = 0.01f;

    BoxCollider2D charCollider;
    Rigidbody2D rb;
    Camera currentCam;
    public Transform swingIndicatorPivot;
    public SpriteRenderer charSprite;
    public CircleCollider2D swingArea;

    private CollisionCalculator charCollCalc;
    private MovementCastUtility swingCastUtils;

    private float jumpVelocity;

    public Vector2 velocity = new Vector2(0, 0);
    public Vector2 instantDisplacement = new Vector2(0, 0);

    public bool charGrounded = false;
    public bool belowFlatCeiling = false;
    public bool canWalkUnobstructedR = false;
    public bool canWalkUnobstructedL = false;

    private bool jumpKeyReleased;

    private bool mouse0Pressed;
    private int mouse0FramesHeld;

    private InputBuffer charInputBuffer;

    private float aerialModifier;

    private Vector2 swingIndicatorDir;
    private float indicatorAngle;

    public string lastMoveAction;
    private bool usedWallVault;
    private int POST_WALL_SWING_COOLDOWN = 12;
    private int postLeftWallSwingCooldown;
    private int postRightWallSwingCooldown;

    public int steam;
    public int baseSteam = 100;
    public int steamCapacity;

    private void Start()
    {
        //references to attached components are made
        charCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        currentCam = GetComponentInChildren<Camera>();

        charCollCalc = new CollisionCalculator(charCollider);
        swingCastUtils = new MovementCastUtility(swingArea);

        jumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(GRAVITY) * (50) * JUMP_HEIGHT);// v^2 = 2a (height) -> v = sqrt(2 * a * height)

        charInputBuffer = GetComponent<InputBuffer>();
    }
    // Update is called once per frame
    void Update() 
    {
        UpdateSwingIndicator();

        if (Input.GetKeyUp("space"))
        {
            jumpKeyReleased = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            mouse0Pressed = true;
        }
        else
        {
            mouse0Pressed = false;
        }

    }

    void FixedUpdate()
    {
        //Check whether the character is still on the ground
        charGrounded = charCollCalc.IsOnWalkableGround();
        belowFlatCeiling = charCollCalc.BelowFlatCeiling();
        canWalkUnobstructedR = !charCollCalc.NextToRightWall() && !charCollCalc.OnRightSlope();
        canWalkUnobstructedL = !charCollCalc.NextToLeftWall() && !charCollCalc.OnLeftSlope();
        
        if (charGrounded)
        {
            usedWallVault = false;
            steam = baseSteam;
        }
        
        //directional movement input and gravity; evrything that will affect velocity based on current state
        //this may eventually be outsourced to a input handler separate from the player object once menus and stuff are made
        #region VELOCITY ADJUSTMENTS
        if (!charGrounded)
        {
            aerialModifier = AERIAL_CONTROL;
        }
        else 
        {
            aerialModifier = 1.0f;
        }


        if (postLeftWallSwingCooldown > 0)
            postLeftWallSwingCooldown--;
        if (postRightWallSwingCooldown > 0)
            postRightWallSwingCooldown--;

        if (Input.GetKey("a") || Input.GetKey("d"))
        {
            if (Input.GetKey("a") && canWalkUnobstructedL && postLeftWallSwingCooldown == 0)//going left
            {
                if (velocity.x > -MAX_SPEED)
                {
                    velocity.x -= RUN_SPEED * aerialModifier;
                }

                if (velocity.x > 0 && Mathf.Abs(velocity.x) < MAX_SPEED)
                {
                    velocity.x -= RUN_SPEED / 2;
                }
            }
            if (Input.GetKey("d") && canWalkUnobstructedR && postRightWallSwingCooldown == 0)//going right
            {
                if (velocity.x < MAX_SPEED)
                {
                    velocity.x += RUN_SPEED * aerialModifier;
                }

                if (velocity.x < 0 && Mathf.Abs(velocity.x) < MAX_SPEED)
                {
                    velocity.x += RUN_SPEED / 2;
                }
            }
        }
        else if(charGrounded || Mathf.Abs(velocity.x) <= MAX_SPEED)
        { 
                velocity.x *= GROUND_DRAG;
        }

        // applying ground friction if the character is going down a slope, may not want/need this
        if ((charCollCalc.OnRightSlope() || charCollCalc.OnLeftSlope()) && velocity.y < 0)
            velocity.x *= GROUND_DRAG;

        if (Mathf.Abs(velocity.x) >= MAX_SPEED)
        {
            velocity.x *= LIMITER_DRAG;

            if (charGrounded)
                velocity.x *= GROUND_DRAG;
        }

        if (Mathf.Abs(velocity.x) <= MINIMUM_SPEED)
        {
            velocity.x = 0;
        }


        //detects the spacebar being pressed and adds velocity

        if (charInputBuffer.GetInputDown(charGrounded, "space"))
        {
            lastMoveAction = "jump";
            charGrounded = false;
            velocity.y += jumpVelocity;
        }

        //Debug.Log("yVel = " + velocity.y);
        if (jumpKeyReleased)
        {
            if (velocity.y >= 6.0f && string.Equals(lastMoveAction, "jump"))
            {
                velocity.y = 6.0f;
            }
            jumpKeyReleased = false;
        }
        
        //gravity applied
        if (velocity.y > MAX_FALL && !charGrounded)
        {
            velocity.y += GRAVITY;
        }

        if (Input.GetMouseButton(0))
        {
            mouse0FramesHeld += 1; //this should happen in the physics area?
        }

        swingIndicatorDir.x = Mathf.Cos(indicatorAngle * (Mathf.PI / 180.0f) + (Mathf.PI / 2));
        swingIndicatorDir.y = Mathf.Sin(indicatorAngle * (Mathf.PI / 180.0f) + (Mathf.PI / 2));
        swingIndicatorDir.Normalize();
        if(mouse0FramesHeld >= SWING_CHARGE_FRAMES)
        {
            charSprite.color = Color.red;
        }
        else 
        {
            charSprite.color = Color.white;
        }
        //Debug.Log(swingIndicatorDir);
        //Debug.DrawRay(this.transform.position, swingIndicatorDir, Color.red);
        if (charInputBuffer.GetInputUp(mouse0FramesHeld >= SWING_CHARGE_FRAMES && steam >= SWING_STEAM_COST, "mouse0"))
        {
            mouse0FramesHeld = 0;

            RaycastHit2D[] swingCastResults = new RaycastHit2D[10];
            swingArea.Cast(swingIndicatorDir, swingCastResults, 2.0f);

            float indicatorAngle = Vector2.SignedAngle(swingIndicatorDir, new Vector2(0, 1));
            Vector2 swingNewVel = new Vector2(swingIndicatorDir.x * SWING_STRENGTH * -1, swingIndicatorDir.y * SWING_STRENGTH * -1);

            float angleToHitNormal = Vector2.Angle(Vector2.up, swingCastUtils.FirstCastNormal(swingCastResults));
            bool hitAnything = swingCastResults[0].collider != null;
            bool wouldHitWall = angleToHitNormal >= 60 && angleToHitNormal <= 140;
            bool wouldHitLeftWall = swingCastUtils.FirstCastNormal(swingCastResults).x > 0;
            bool wouldHitFloor = angleToHitNormal < 60; 

            if (wouldHitWall && hitAnything)
            {
                steam -= SWING_STEAM_COST;

                if (wouldHitLeftWall)
                {
                    indicatorAngle *= -1;
                    postLeftWallSwingCooldown = POST_WALL_SWING_COOLDOWN;
                }
				else //hit right wall
				{
                    postRightWallSwingCooldown = POST_WALL_SWING_COOLDOWN;
                }

                if(Mathf.Abs(indicatorAngle) >= 180) // pointing straight down
                {
                    if(!usedWallVault)
                    {
                        swingNewVel.y = 11.0f;
                        usedWallVault = true;
                    }
                    else
                    {
                        steam += SWING_STEAM_COST;
                        swingNewVel.Set(velocity.x, velocity.y);
                    }
                }
                else if (indicatorAngle >= 135)
                {
                    swingNewVel.x *= 0.8f;
                }
                else if (indicatorAngle >= 90)
                {
                    swingNewVel.y += 1.0f;
                }
                else if (indicatorAngle >= 45)
                {
                    swingNewVel.x *= 1.5f;
                }
                else
                {
                    swingNewVel.Set(velocity.x, velocity.y);
                }
            }
            else if (wouldHitFloor && hitAnything)
            {
                if(!charGrounded)
                {
                    steam -= SWING_STEAM_COST;
                }

                indicatorAngle = Mathf.Abs(indicatorAngle);

                if (indicatorAngle >= 180)
                {
                    swingNewVel.y *= 0.6f;
                }
                else if (indicatorAngle >= 135)
                {
                    swingNewVel.y *= 0.7f;
                    swingNewVel.x *= 1.35f;
                }
                else if (indicatorAngle >= 90)
                {
                    swingNewVel.Set(velocity.x, velocity.y);
                }
                else if (indicatorAngle >= 45)
                {
                    swingNewVel.Set(velocity.x, velocity.y);
                }
                else
                {
                    swingNewVel.Set(velocity.x, velocity.y);
                }

                usedWallVault = false;
            }
            else if (velocity.y <= 2.0f)
            {
                steam -= SWING_STEAM_COST;
                swingNewVel.Set(velocity.x * 0.6f, 6.0f);
            }
            else
            {
                swingNewVel.Set(velocity.x, velocity.y);
            }


            lastMoveAction = "swing";
            velocity.Set(swingNewVel.x, swingNewVel.y);
        }
        if (charInputBuffer.GetInputUp(true, "mouse0"))
        {
            mouse0FramesHeld = 0;
        }
        #endregion 

        //movement happens

        rb.MovePosition(rb.position + charCollCalc.MoveAndSlide(velocity, Time.deltaTime));


    }

    /// <summary>
    /// This updates the angle of the swing indicator to match where the mouse is
    /// </summary>
    private void UpdateSwingIndicator()
    {
        Vector2 charPosInCam = currentCam.WorldToScreenPoint(this.transform.position);
        Vector2 relativeMousePosToChar = (Vector2)Input.mousePosition - charPosInCam;
        relativeMousePosToChar.Normalize();
        float angleToMouse = -Vector2.SignedAngle(relativeMousePosToChar, new Vector2(0, 1));
        indicatorAngle = this.RoundAngleToEigths(angleToMouse);
        swingIndicatorPivot.transform.rotation = Quaternion.Euler(0, 0, indicatorAngle);
    }

    /// <summary>
    /// Takes any angle and rounds it to the nearest 1/8 rotation
    /// </summary>
    /// <param name="unrounded"></param>
    /// <returns>The nearest angle which is a multiple of 45</returns>
    private float RoundAngleToEigths(float unrounded)
    {
        for(int centerAng = 180; centerAng >= -180; centerAng -= 45)
        {
            if(unrounded >= centerAng - 22.5f && unrounded <= centerAng + 22.5f)
            {
                return centerAng;
            }
        }
        return 0.0f;
    }

}
