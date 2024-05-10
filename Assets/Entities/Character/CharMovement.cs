using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumConstants;

public class CharMovement : MonoBehaviour
{

    private DebugLogger dLog;

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
    public int SWING_COOLDOWN_FRAMES = 10;

    EntityMover mover;
    Camera currentCam;
    public Transform swingIndicatorPivot;
    public SpriteRenderer charSprite;
    public CircleCollider2D swingArea;

    private CollisionCalculator charCollCalc;
    private PhysicsCastUtility swingCastUtils;

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
    public int swingCooldown = 0;

    public int steam;
    public int baseSteam = 100;
    public int steamCapacity;

    public float SWING_DAMAGE = 5.0f;
    public float MINI_HIT_DAMAGE = 2.5f;
    public int RECOIL_DURATION_FRAMES = 40;
    public float RECOIL_DECAY = 0.09f;
    public float RECOIL_STRENGTH = 10.0f;
    public Vector2 recoilVelocity = new Vector2();
    public int recoilDurationLeft = 0;

    private void Start()
    {
        //references to attached components are made
        mover = GetComponent<EntityMover>();
        mover.constantVels.Add("recoilVelocity", new Vector2());

        currentCam = GetComponentInChildren<Camera>();

        charCollCalc = GetComponent<CollisionCalculator>();
        swingCastUtils = new PhysicsCastUtility();

        jumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(GRAVITY) * (50) * JUMP_HEIGHT);// v^2 = 2a * (height) -> v = sqrt(2 * a * height)

        charInputBuffer = GetComponent<InputBuffer>();


        dLog = new DebugLogger();
        #region DebugLogger Keys
        dLog.loggableSystems = new Dictionary<string, bool>
        {
            { "swing - type", false },
            { "recoil", false},
            { "swing", false},
            { "swing indicator", false}
        };
        #endregion
    
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

        //directional movement input and gravity; everything that will affect velocity based on current state
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
        if (swingCooldown > 0)
            swingCooldown--;

        if (recoilDurationLeft < 1)
            mover.constantVels["recoilVelocity"] *= 0;
        else
        {
            recoilDurationLeft--;
            mover.constantVels["recoilVelocity"] *= RECOIL_DECAY;
        }

		if (Input.GetKey("a") || Input.GetKey("d"))
        {
            if (Input.GetKey("a") && canWalkUnobstructedL && postLeftWallSwingCooldown == 0)//going left
            {
                if (mover.persistentVel.x > -MAX_SPEED)
                {
                    mover.persistentVel.x -= RUN_SPEED * aerialModifier;
                }

                if (mover.persistentVel.x > 0 && Mathf.Abs(mover.persistentVel.x) < MAX_SPEED)
                {
                    mover.persistentVel.x -= RUN_SPEED / 2;
                }
            }
            if (Input.GetKey("d") && canWalkUnobstructedR && postRightWallSwingCooldown == 0)//going right
            {
                if (mover.persistentVel.x < MAX_SPEED)
                {
                    mover.persistentVel.x += RUN_SPEED * aerialModifier;
                }

                if (mover.persistentVel.x < 0 && Mathf.Abs(mover.persistentVel.x) < MAX_SPEED)
                {
                    mover.persistentVel.x += RUN_SPEED / 2;
                }
            }
        }
        else if(charGrounded || Mathf.Abs(mover.persistentVel.x) <= MAX_SPEED)
        { 
                mover.persistentVel.x *= GROUND_DRAG;
        }

        // applying ground friction if the character is going down a slope, may not want/need this
        if ((charCollCalc.OnRightSlope() || charCollCalc.OnLeftSlope()) && mover.persistentVel.y < 0)
            mover.persistentVel.x *= GROUND_DRAG;

        if (Mathf.Abs(mover.persistentVel.x) >= MAX_SPEED)
        {
            mover.persistentVel.x *= LIMITER_DRAG;

            if (charGrounded)
                mover.persistentVel.x *= GROUND_DRAG;
        }

        if (Mathf.Abs(mover.persistentVel.x) <= MINIMUM_SPEED)
        {
            mover.persistentVel.x = 0;
        }


        //detects the spacebar being pressed and adds velocity

        if (charInputBuffer.GetInputDown(charGrounded, "space"))
        {
            lastMoveAction = "jump";
            charGrounded = false;
            mover.persistentVel.y += jumpVelocity;
        }

        if (jumpKeyReleased)
        {
            if (mover.persistentVel.y >= 6.0f && string.Equals(lastMoveAction, "jump"))
            {
                mover.persistentVel.y = 6.0f;
            }
            jumpKeyReleased = false;
        }
        
        //gravity applied
        if (mover.persistentVel.y > MAX_FALL && !charGrounded && mover.constantVels["recoilVelocity"].y <= 0)
        {
            mover.persistentVel.y += GRAVITY;
        }

        if (Input.GetMouseButton(0))
        {
            mouse0FramesHeld += 1;
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
        dLog.Log("swing indicator direction: " + swingIndicatorDir, "swing indicator");
        //Debug.DrawRay(this.transform.position, swingIndicatorDir, Color.red);
        if (charInputBuffer.GetInputUp(true, "mouse0"))
        {
            if (swingCooldown == 0)
            {
                swingCooldown = SWING_COOLDOWN_FRAMES;

                //Experimental change to cast from slightly above center of character to stop unwanted collisions w/ ground
                RaycastHit2D[] swingCastResults = swingCastUtils.DisplacementShapeCast((Vector2)transform.position + new Vector2(0, 0.2f), swingIndicatorDir * 2.0f + new Vector2(0, 0.2f), swingArea,
                   new string[] { "Environment", "Solid Entity", "Incorporeal Entity", "Swing" });

                float indicatorAngle = Vector2.SignedAngle(swingIndicatorDir, new Vector2(0, 1));
                Vector2 swingNewVel = new Vector2(swingIndicatorDir.x * SWING_STRENGTH * -1, swingIndicatorDir.y * SWING_STRENGTH * -1);

                float angleToHitNormal = Vector2.Angle(Vector2.up, swingCastUtils.FirstCastNormal(swingCastResults));
                bool hitAnything = swingCastResults[0].collider != null;
                bool wouldHitWall = angleToHitNormal >= 60 && angleToHitNormal <= 140;
                bool wouldHitLeftWall = swingCastUtils.FirstCastNormal(swingCastResults).x > 0;
                bool wouldHitFloor = angleToHitNormal < 60;

                GameObject hitObject = null;
                Enemy hitEnemy = null;
				if (hitAnything)
                    hitObject = swingCastResults[0].collider.gameObject;
                if (hitObject != null)
                    hitEnemy = hitObject.GetComponent<Enemy>();
                bool hitWasEnemy = hitEnemy != null;
                dLog.Log("hitAnything?: " + hitAnything + ", hitObject?: " + hitObject + ", hitWasEnemy?: " + hitWasEnemy, "swing");

                if (steam >= SWING_STEAM_COST && mouse0FramesHeld >= SWING_CHARGE_FRAMES)
                {
                    dLog.Log("SWING!", "swing - type");
                    steam -= SWING_STEAM_COST;

                    if (hitWasEnemy)
                    {
                        hitEnemy.DealDamage(SWING_DAMAGE, swingIndicatorDir);
                        steam += SWING_STEAM_COST;
                    }

                    if (wouldHitWall && hitAnything)
                    {

                        if (wouldHitLeftWall)
                        {
                            indicatorAngle *= -1;
                            postLeftWallSwingCooldown = POST_WALL_SWING_COOLDOWN;
                        }
                        else //hit right wall
                        {
                            postRightWallSwingCooldown = POST_WALL_SWING_COOLDOWN;
                        }

                        if (Mathf.Abs(indicatorAngle) >= 180) // pointing straight down
                        {
                            if (!usedWallVault)
                            {
                                swingNewVel.y = 11.0f;
                                usedWallVault = true;
                            }
                            else
                            {
                                steam += SWING_STEAM_COST;
                                swingNewVel.Set(mover.persistentVel.x, mover.persistentVel.y);
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
                            swingNewVel.Set(mover.persistentVel.x, mover.persistentVel.y);
                        }
                    }
                    else if (wouldHitFloor && hitAnything)
                    {
                        if (charGrounded)
                        {
                            steam += SWING_STEAM_COST;
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
                            swingNewVel.Set(mover.persistentVel.x, mover.persistentVel.y);
                        }
                        else if (indicatorAngle >= 45)
                        {
                            swingNewVel.Set(mover.persistentVel.x, mover.persistentVel.y);
                        }
                        else
                        {
                            swingNewVel.Set(mover.persistentVel.x, mover.persistentVel.y);
                        }

                        usedWallVault = false;
                    }
                    else if (mover.persistentVel.y <= 2.0f)
                    {
                        swingNewVel.Set(mover.persistentVel.x * 0.6f, 6.0f);
                    }
                    else
                    {
                        swingNewVel.Set(mover.persistentVel.x, mover.persistentVel.y);
                    }
                }
				else
				{
                    dLog.Log("MINI-HIT!", "swing - type" );
                    if (hitWasEnemy)
					{
                        dLog.Log("swingNewVel: " + swingNewVel + ", recoilVelocity: " + ((swingNewVel / SWING_STRENGTH) * RECOIL_STRENGTH), "recoil");
						mover.constantVels["recoilVelocity"] = (swingNewVel / SWING_STRENGTH) * RECOIL_STRENGTH;
						swingNewVel = VectorUtility.DeflectWithNormal(mover.persistentVel, swingNewVel * -1);
                        recoilDurationLeft = RECOIL_DURATION_FRAMES;
                        if (mover.constantVels["recoilVelocity"].y != 0)
                            swingNewVel.y = 0;

						hitEnemy.DealDamage(MINI_HIT_DAMAGE, swingIndicatorDir);

                        steam += (int)(SWING_STEAM_COST * (SWING_DAMAGE / MINI_HIT_DAMAGE));
                    }
                    else
					{
                        swingNewVel.Set(mover.persistentVel.x, mover.persistentVel.y);
                    }
				}

                lastMoveAction = "swing";
                steam = Mathf.Min(steam, baseSteam + steamCapacity); //I believe this is here to prevent steam from exceeding its max? Maybe this could just be an inline if?
                dLog.Log("Final swingNewVel: " + swingNewVel, "swing");
                mover.persistentVel.Set(swingNewVel.x, swingNewVel.y);
            }
			else
			{
                dLog.Log("on cooldown!", "swing - type");
			}

            mouse0FramesHeld = 0;
        }
        #endregion 

        if (recoilDurationLeft > 0) dLog.Log("recoilVelocity: " + mover.constantVels["recoilVelocity"] + ", velocity: " + mover.persistentVel, "recoil");
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
