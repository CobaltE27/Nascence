﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static EnumConstants;
using static InputBuffer.Controls;

public class CharMovement : EntityMovement
{
    public HudUi hud;

    private DebugLogger dLog;

    public ParticleSystem swingParticle;
    public GameObject swingParticlePivot;

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
    public float SWING_LENGTH = 2.0f;

    public Transform swingIndicatorPivot;
    public SpriteRenderer charSprite;
    public CircleCollider2D swingArea;

    private CollisionCalculator charCollCalc;

    private float jumpVelocity;

    public Vector2 velocity = new Vector2(0, 0);
    public Vector2 instantDisplacement = new Vector2(0, 0);

    public bool grounded = false;
    public bool belowFlatCeiling = false;
    public bool canWalkUnobstructedR = false;
    public bool canWalkUnobstructedL = false;

    private int swingFramesHeld;

	private InputBuffer inputBuffer;

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
    public int steamExtraCapacity;

    public int SWING_DAMAGE = 2;
    public int MINI_HIT_DAMAGE = 1;
    public int RECOIL_DURATION_FRAMES = 40;
    public float RECOIL_DECAY = 0.09f;
    public float RECOIL_STRENGTH = 10.0f;
    public Vector2 recoilVelocity = new Vector2();
    public int recoilDurationLeft = 0;

    bool usedFloorVault = false;
    float POST_VAULT_MODIFIER = 0.2f;

    public GameObject coinPrefab;
    private GameObject oldCoin;
	public GameObject bubblePrefab;
	private GameObject oldBubble;

    private int framesSinceGrounded = 50;
    private int COYOTE_TIME = 5;

    private Vector2 lastSafeLocation = new Vector2();
    private bool hasControl = true;

	protected override void Start()
    {
        base.Start();
        hud.SetSteamParameters(baseSteam, steamExtraCapacity);
		hud.UpdateSteamLevel(steam);

		mover.constantVels.Add("recoilVelocity", new Vector2());

        charCollCalc = GetComponent<CollisionCalculator>();

        jumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(GRAVITY) * JUMP_HEIGHT);// v^2 = 2a * (height) -> v = sqrt(2 * a * height)

        inputBuffer = GetComponent<InputBuffer>();


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
	}

    void FixedUpdate()
    {
        //Check whether the character is still on the ground
		grounded = charCollCalc.IsOnWalkableGround();
		belowFlatCeiling = charCollCalc.BelowFlatCeiling();
        canWalkUnobstructedR = !charCollCalc.NextToRightWall() && !charCollCalc.OnRightSlope();
        canWalkUnobstructedL = !charCollCalc.NextToLeftWall() && !charCollCalc.OnLeftSlope();

        if (grounded)
        {
            usedWallVault = false;
            usedFloorVault = false;
            steam = baseSteam;
            framesSinceGrounded = 0;

            if (Vector2.Distance(transform.position, lastSafeLocation) > 1)
            {
                LayerMask env = LayerMask.GetMask(new string[] { "Environment" });
                RaycastHit2D leftProbe = Physics2D.Raycast((Vector2)transform.position + new Vector2(-0.5f, 0), Vector2.down, 0.6f, env);
				RaycastHit2D rightProbe = Physics2D.Raycast((Vector2)transform.position + new Vector2(0.5f, 0), Vector2.down, 0.6f, env);

                if (leftProbe && rightProbe)
				    lastSafeLocation = transform.position;
            }
        }
        else
        {
            framesSinceGrounded++;
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

        //directional movement input and gravity; everything that will affect velocity based on current state
        //this may eventually be outsourced to a input handler separate from the player object once menus and stuff are made
        #region VELOCITY ADJUSTMENTS

        bool tryingToStrafe = inputBuffer.GetInput(LEFT) || inputBuffer.GetInput(RIGHT);
		if (!tryingToStrafe && (grounded || Mathf.Abs(mover.persistentVel.x) <= MAX_SPEED))
        { 
            mover.persistentVel.x *= GROUND_DRAG;
        }

        if (Mathf.Abs(mover.persistentVel.x) >= MAX_SPEED)
        {
            mover.persistentVel.x *= LIMITER_DRAG;

            if (grounded)
                mover.persistentVel.x *= GROUND_DRAG;
        }

        if (Mathf.Abs(mover.persistentVel.x) <= MINIMUM_SPEED)
        {
            mover.persistentVel.x = 0;
        }

		swingIndicatorDir.x = Mathf.Cos(indicatorAngle * (Mathf.PI / 180.0f) + (Mathf.PI / 2));
		swingIndicatorDir.y = Mathf.Sin(indicatorAngle * (Mathf.PI / 180.0f) + (Mathf.PI / 2));
		swingIndicatorDir.Normalize();

		//gravity applied
		if (mover.persistentVel.y > MAX_FALL && !grounded && mover.constantVels["recoilVelocity"].y <= 0)
		{
			mover.persistentVel.y += GRAVITY * Time.deltaTime;
		}

        //CONTROLS
        if (hasControl)
        {
            if (inputBuffer.GetInput(LEFT) || inputBuffer.GetInput(RIGHT))
            {
                float xVelChange = 0.0f;
                if (inputBuffer.GetInput(LEFT) && canWalkUnobstructedL && postLeftWallSwingCooldown == 0)//going left
                {
                    if (mover.persistentVel.x > -MAX_SPEED)
                        xVelChange -= RUN_SPEED;
                    if (mover.persistentVel.x > 0 && Mathf.Abs(mover.persistentVel.x) < MAX_SPEED)
                        mover.persistentVel.x -= RUN_SPEED / 2;
                }
                if (inputBuffer.GetInput(RIGHT) && canWalkUnobstructedR && postRightWallSwingCooldown == 0)//going right
                {
                    if (mover.persistentVel.x < MAX_SPEED)
                        xVelChange += RUN_SPEED;
                    if (mover.persistentVel.x < 0 && Mathf.Abs(mover.persistentVel.x) < MAX_SPEED)
                        mover.persistentVel.x += RUN_SPEED / 2;
                }
                if (!grounded)
                    xVelChange *= AERIAL_CONTROL;
                if (usedFloorVault)
                    xVelChange *= POST_VAULT_MODIFIER;

                mover.persistentVel.x += xVelChange;
            }

            if (inputBuffer.GetInputDown(DOWN)) //fastfall and cornering
            {
                //if (grounded)
                //{
                //    if (mover.persistentVel.x != 0)
                //        mover.persistentVel.x = -Mathf.Sign(mover.persistentVel.x) * MAX_SPEED * 0.5f;
                //}
                //else
                //    mover.persistentVel.y = Mathf.Min(MAX_FALL / 2, mover.persistentVel.y);
            }

            if (inputBuffer.GetBufferedInputDown((grounded || framesSinceGrounded <= COYOTE_TIME) && mover.persistentVel.y <= 0, JUMP)) //velocity check prevents jumping on frame after ground swing, frames since grounded creates coyote-time
            {
                lastMoveAction = "jump";
                grounded = false;
                mover.persistentVel.y = jumpVelocity;
            }

            if (inputBuffer.GetInputUp(JUMP))
            {
                if (mover.persistentVel.y >= 6.0f && string.Equals(lastMoveAction, "jump"))
                {
                    mover.persistentVel.y = 6.0f;
                }
            }

            if (inputBuffer.GetInput(SWING))
                swingFramesHeld++;

			if (inputBuffer.GetInputDown(PLAT_PROJECTILE))
            {
                if (grounded)
                {
                    CoinToss();
                }
                else if (steam >= SWING_STEAM_COST)
                {
                    steam -= SWING_STEAM_COST;
                    CreateBubble();
                }
            }

            dLog.Log("swing indicator direction: " + swingIndicatorDir, "swing indicator");
            //Debug.DrawRay(this.transform.position, swingIndicatorDir, Color.red);
            bool swingRelease = inputBuffer.GetBufferedInputUp(swingCooldown == 0, SWING);
			if (swingRelease || inputBuffer.GetBufferedInputDown(swingCooldown == 0, SWING))
            {
                swingCooldown = SWING_COOLDOWN_FRAMES;

                SetVelocityFromSwing();
                ParticleSystem.MainModule particleMain = swingParticle.main;
                particleMain.startRotation = -indicatorAngle * (Mathf.PI / 180.0f);
                swingParticlePivot.transform.rotation = Quaternion.Euler(0, 0, indicatorAngle);

                swingParticle.Play();
            }

            if (swingRelease || inputBuffer.GetInputUp(SWING))
			    swingFramesHeld = 0;
		}
		#endregion
		if (swingFramesHeld >= SWING_CHARGE_FRAMES)
		{
			charSprite.color = Color.red;
		}
		else
		{
			charSprite.color = Color.white;
		}

		hud.UpdateSteamLevel(steam);
        if (recoilDurationLeft > 0) dLog.Log("recoilVelocity: " + mover.constantVels["recoilVelocity"] + ", velocity: " + mover.persistentVel, "recoil");
	}

    public void GoToLastSafe()
    {
        transform.position = (Vector3)lastSafeLocation;
        mover.persistentVel *= 0;
        grounded = true;
        StartCoroutine(BreathingRoom());
    }

    private IEnumerator BreathingRoom()
    {
        hasControl = false;
        //immunity
        yield return new WaitForSeconds(1);
        hasControl = true;
    }
	private void SetVelocityFromSwing()
	{
        //Experimental change to cast from slightly above center of character to stop unwanted collisions w/ ground
        Vector2 offset = Vector2.up * 0.02f;
		List<RaycastHit2D> swingCastResults = PhysicsCastUtility.DisplacementShapeCast((Vector2)transform.position + offset, swingIndicatorDir * SWING_LENGTH + offset, swingArea,
		   new string[] { "Environment", "Hurtbox", "Swing" }).ToList();

        for (int i = 0; i < swingCastResults.Count; i++) //Filter out player hits
        {
            if (swingCastResults[i].collider.CompareTag("Player"))
            {
                swingCastResults.RemoveAt(i);
                i--;
            }
        }
        if (swingCastResults.Count == 0)
            swingCastResults.Add(new RaycastHit2D());


		float swingAngle = Vector2.SignedAngle(swingIndicatorDir, Vector2.up);
		Vector2 postSwingVel = new Vector2(swingIndicatorDir.x * SWING_STRENGTH * -1, swingIndicatorDir.y * SWING_STRENGTH * -1);

		float angleToHitNormal = Vector2.Angle(Vector2.up, PhysicsCastUtility.FirstCastNormal(swingCastResults.ToArray()));
		bool wouldHitWall = angleToHitNormal >= 30 && angleToHitNormal <= 140;
		bool wouldHitLeftWall = PhysicsCastUtility.FirstCastNormal(swingCastResults.ToArray()).x > 0;
		bool wouldHitFloor = angleToHitNormal < 30;

		bool hitAnything = swingCastResults[0].collider != null;
		bool hitAnyEnemy = false;
        List<EntityHealth> hitEntitiesHealths = new List<EntityHealth>();
        foreach (RaycastHit2D result in swingCastResults)
        {
            if (result.collider == null)
                break; //should indicate that the list of real results has ended

            string hitTag = result.collider.gameObject.tag;
			if (hitTag == "Enemy" || hitTag == "Non-Enemy Entity")
            {
                hitEntitiesHealths.Add(result.collider.gameObject.transform.parent.gameObject.GetComponent<EntityHealth>());
                hitAnyEnemy = true;
            }
        }
		dLog.Log("hitAnything?: " + hitAnything + ", num hitEntities?: " + hitEntitiesHealths.Count + ", hitAnyEnemy?: " + hitAnyEnemy, "swing");

		bool floorVaulted = false;
		if (steam >= SWING_STEAM_COST && swingFramesHeld >= SWING_CHARGE_FRAMES)
        {
            dLog.Log("SWING!", "swing - type");
            steam -= SWING_STEAM_COST;

            if (hitEntitiesHealths.Count > 0)
				steam += SWING_STEAM_COST; //for now, only get steam from one enemy at a time.

            foreach (EntityHealth hitEntHealth in hitEntitiesHealths)
                hitEntHealth.DealDamage(SWING_DAMAGE, swingIndicatorDir, 1.5f);

            if (wouldHitWall && hitAnything)
            {
                if (wouldHitLeftWall)
                {
                    swingAngle *= -1;
                    postLeftWallSwingCooldown = POST_WALL_SWING_COOLDOWN;
                }
                else //hit right wall
                {
                    postRightWallSwingCooldown = POST_WALL_SWING_COOLDOWN;
                }

                if (Mathf.Abs(swingAngle) >= 180) // pointing straight down
                {
                    if (!usedWallVault)
                    {
                        postSwingVel.y = 11.0f;
                        usedWallVault = true;
                    }
                    else
                    {
                        steam += SWING_STEAM_COST;
                        postSwingVel.Set(mover.persistentVel.x, mover.persistentVel.y);
                    }
                }
                else if (swingAngle >= 135)
                {
                    postSwingVel.x *= 0.8f;
                }
                else if (swingAngle >= 90)
                {
                    postSwingVel.y += 1.0f;
                }
                else if (swingAngle >= 45)
                {
                    postSwingVel.x *= 1.5f;
                }
                else
                {
                    postSwingVel.Set(mover.persistentVel.x, mover.persistentVel.y);
                }
            }
            else if (wouldHitFloor && hitAnything)
            {
                if (grounded)
                {
                    steam += SWING_STEAM_COST;
                }

                swingAngle = Mathf.Abs(swingAngle);

                if (swingAngle >= 180)
                {
                    float minY = postSwingVel.y * 0.55f;
                    postSwingVel.y *= 0.3f;
                    postSwingVel.y += Mathf.Abs(mover.persistentVel.x);
                    if (postSwingVel.y < minY)
                        postSwingVel.y = minY;
                    postSwingVel.x *= 0;
                    floorVaulted = true;
                    usedFloorVault = true;
                }
                else if (swingAngle >= 135)
                {
                    postSwingVel.y *= 0.7f;
                    postSwingVel.x *= 1.35f;
                }
                else if (swingAngle >= 90)
                {
                    postSwingVel.Set(mover.persistentVel.x, mover.persistentVel.y);
                }
                else if (swingAngle >= 45)
                {
                    postSwingVel.Set(mover.persistentVel.x, mover.persistentVel.y);
                }
                else
                {
                    postSwingVel.Set(mover.persistentVel.x, mover.persistentVel.y);
                }

                usedWallVault = false;
                grounded = false;
            }
            else if (mover.persistentVel.y <= 2.0f && !grounded)
            {
                postSwingVel.Set(mover.persistentVel.x * 0.6f, 6.0f);
            }
            else
            {
                postSwingVel.Set(mover.persistentVel.x, mover.persistentVel.y);
            }

            if (!floorVaulted)
                usedFloorVault = false;

            if (hitAnything)
            {
                StartCoroutine(HitStop(0.2f));
            }
		}
		else
		{
			dLog.Log("MINI-HIT!", "swing - type");
			if (hitAnyEnemy)
			{
				dLog.Log("swingNewVel: " + postSwingVel + ", recoilVelocity: " + ((postSwingVel / SWING_STRENGTH) * RECOIL_STRENGTH), "recoil");
				mover.constantVels["recoilVelocity"] = (postSwingVel / SWING_STRENGTH) * RECOIL_STRENGTH;
				postSwingVel = VectorUtility.DeflectWithNormal(mover.persistentVel, postSwingVel * -1);
				recoilDurationLeft = RECOIL_DURATION_FRAMES;

                foreach (EntityHealth hitEntHealth in hitEntitiesHealths)
                {
					hitEntHealth.DealDamage(MINI_HIT_DAMAGE, swingIndicatorDir);

                    steam += (int)(hitEntHealth.steamOnHit * (MINI_HIT_DAMAGE / (float)SWING_DAMAGE));
                }
			}
			else
			{
				postSwingVel.Set(mover.persistentVel.x, mover.persistentVel.y);
			}
		}

		lastMoveAction = "swing";
		steam = Mathf.Min(steam, baseSteam + steamExtraCapacity); //I believe this is here to prevent steam from exceeding its max? Maybe this could just be an inline if?
		dLog.Log("Final swingNewVel: " + postSwingVel, "swing");
		mover.persistentVel.Set(postSwingVel.x, postSwingVel.y);
	}

    private void CoinToss()
    {
        if (oldCoin != null)
            Destroy(oldCoin);

        GameObject newCoin = Instantiate(coinPrefab);
        newCoin.transform.position = (Vector2)transform.position + (swingIndicatorDir * 1.0f) + Vector2.up * 0.2f;
        CoinBehavior newCoinBehavior = newCoin.GetComponent<CoinBehavior>();
        Vector2 coinVel = swingIndicatorDir;
        //if (Mathf.Abs(coinVel.x) > 0.1f) //Should make it so that diagonal toss goes just as high
		//	coinVel.x /= Mathf.Abs(coinVel.x); 
		//if (Mathf.Abs(coinVel.y) > 0.1f)
		//	coinVel.y /= Mathf.Abs(coinVel.y);
        coinVel *= 9.0f;
        newCoinBehavior.InitVelocity(coinVel + mover.persistentVel * 0.3f); //swingIndicatorDir * 9.0f + mover.persistentVel * 0.3f
        oldCoin = newCoin;
	}

	private void CreateBubble()
	{
		if (oldBubble != null)
			Destroy(oldBubble);

		GameObject newBubble = Instantiate(bubblePrefab);
		newBubble.transform.position = (Vector2)transform.position + (swingIndicatorDir * 1.0f) + Vector2.up * 0.2f;
		BubbleBehavior newBubbleBehavior = newBubble.GetComponent<BubbleBehavior>();
		newBubbleBehavior.InitVelocity(mover.persistentVel);

		Vector2 newVel = VectorUtility.ReflectOffNormal(mover.persistentVel, -swingIndicatorDir, 0.7f);
        mover.persistentVel = newVel + (-swingIndicatorDir * 4.0f);

		oldBubble = newBubble;
	}

	/// <summary>
	/// This updates the angle of the swing indicator to match where the mouse is
	/// </summary>
	private void UpdateSwingIndicator()
    {
        Vector2 swingDirInput = inputBuffer.SwingDir;
        indicatorAngle = Vector2.SignedAngle(Vector2.up, swingDirInput);
		swingIndicatorPivot.transform.rotation = Quaternion.Euler(0, 0, indicatorAngle);
	}

    private IEnumerator HitStop(float duration)
    {
		Time.timeScale = 0;
		yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f;
    }
}
