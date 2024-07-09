using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnumConstants;

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

    Camera currentCam;
    public Transform swingIndicatorPivot;
    public SpriteRenderer charSprite;
    public CircleCollider2D swingArea;

    private CollisionCalculator charCollCalc;
    private PhysicsCastUtility swingCastUtils;

    private float jumpVelocity;

    public Vector2 velocity = new Vector2(0, 0);
    public Vector2 instantDisplacement = new Vector2(0, 0);

    public bool grounded = false;
    public bool belowFlatCeiling = false;
    public bool canWalkUnobstructedR = false;
    public bool canWalkUnobstructedL = false;

    private bool jumpKeyReleased;

    private bool mouse0Pressed;
    private int mouse0FramesHeld;
	private bool mouse1Pressed;

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

	bool downKeyPressed = false;

	protected override void Start()
    {
        base.Start();
        hud.SetSteamParameters(baseSteam, steamExtraCapacity);
		hud.UpdateSteamLevel(steam);

		mover.constantVels.Add("recoilVelocity", new Vector2());

        currentCam = GetComponentInChildren<Camera>();

        charCollCalc = GetComponent<CollisionCalculator>();
        swingCastUtils = new PhysicsCastUtility();

        jumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(GRAVITY) * JUMP_HEIGHT);// v^2 = 2a * (height) -> v = sqrt(2 * a * height)

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

        if (Input.GetKeyUp("space")) //use if statements instead of var = method assignment so that they only get set to false during physics frame
            jumpKeyReleased = true;

        if (Input.GetMouseButtonDown(0))
            mouse0Pressed = true;

		if (Input.GetMouseButtonDown(1))
			mouse1Pressed = true;

        if (Input.GetKeyDown("s"))
            downKeyPressed = true;
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
        }
        else
        {
            framesSinceGrounded++;
        }

        //directional movement input and gravity; everything that will affect velocity based on current state
        //this may eventually be outsourced to a input handler separate from the player object once menus and stuff are made
        #region VELOCITY ADJUSTMENTS
        if (!grounded)
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
            float xVelChange = 0.0f;
            if (Input.GetKey("a") && canWalkUnobstructedL && postLeftWallSwingCooldown == 0)//going left
            {
                if (mover.persistentVel.x > -MAX_SPEED)
                    xVelChange -= RUN_SPEED;
                if (mover.persistentVel.x > 0 && Mathf.Abs(mover.persistentVel.x) < MAX_SPEED)
                    mover.persistentVel.x -= RUN_SPEED / 2;
            }
            if (Input.GetKey("d") && canWalkUnobstructedR && postRightWallSwingCooldown == 0)//going right
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
        else if(grounded || Mathf.Abs(mover.persistentVel.x) <= MAX_SPEED)
        { 
                mover.persistentVel.x *= GROUND_DRAG;
        }

        // applying ground friction if the character is going down a slope, may not want/need this
        if ((charCollCalc.OnRightSlope() || charCollCalc.OnLeftSlope()) && mover.persistentVel.y < 0)
            mover.persistentVel.x *= GROUND_DRAG;

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

        if (downKeyPressed) //fastfall and cornering
        {
            if (grounded)
            {
                if (mover.persistentVel.x != 0)
                    mover.persistentVel.x = -Mathf.Sign(mover.persistentVel.x) * MAX_SPEED * 0.5f;
            }
            else
                mover.persistentVel.y = Mathf.Min(MAX_FALL / 2, mover.persistentVel.y);
            downKeyPressed = false;
        }

        if (charInputBuffer.GetInputDown((grounded || framesSinceGrounded <= COYOTE_TIME) && mover.persistentVel.y <= 0, "space")) //velocity check prevents jumping on frame after ground swing, frames since grounded creates coyote-time
        {
            lastMoveAction = "jump";
            grounded = false;
            mover.persistentVel.y = jumpVelocity;
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
        if (mover.persistentVel.y > MAX_FALL && !grounded && mover.constantVels["recoilVelocity"].y <= 0)
        {
            mover.persistentVel.y += GRAVITY * Time.deltaTime;
        }

        if (Input.GetMouseButton(0))
        {
            mouse0FramesHeld += 1;
        }

        if(mouse0FramesHeld >= SWING_CHARGE_FRAMES)
        {
            charSprite.color = Color.red;
        }
        else 
        {
            charSprite.color = Color.white;
        }

		swingIndicatorDir.x = Mathf.Cos(indicatorAngle * (Mathf.PI / 180.0f) + (Mathf.PI / 2));
		swingIndicatorDir.y = Mathf.Sin(indicatorAngle * (Mathf.PI / 180.0f) + (Mathf.PI / 2));
		swingIndicatorDir.Normalize();

        if (mouse1Pressed)
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
            mouse1Pressed = false;
        }

		dLog.Log("swing indicator direction: " + swingIndicatorDir, "swing indicator");
        //Debug.DrawRay(this.transform.position, swingIndicatorDir, Color.red);
        if (charInputBuffer.GetInputUp(true, "mouse0"))
        {
            if (swingCooldown == 0)
			{
				swingCooldown = SWING_COOLDOWN_FRAMES;

				SetVelocityFromSwing();
                ParticleSystem.MainModule particleMain = swingParticle.main;
                particleMain.startRotation = -indicatorAngle * (Mathf.PI / 180.0f);
                swingParticlePivot.transform.rotation = Quaternion.Euler(0, 0, indicatorAngle);

				swingParticle.Play();
			}
			else
			{
                dLog.Log("on cooldown!", "swing - type");
			}

            mouse0FramesHeld = 0;
        }
        #endregion 

        hud.UpdateSteamLevel(steam);
        if (recoilDurationLeft > 0) dLog.Log("recoilVelocity: " + mover.constantVels["recoilVelocity"] + ", velocity: " + mover.persistentVel, "recoil");
	}

	private void SetVelocityFromSwing()
	{
        //Experimental change to cast from slightly above center of character to stop unwanted collisions w/ ground
        Vector2 offset = Vector2.up * 0.02f;
		RaycastHit2D[] swingCastResults = swingCastUtils.DisplacementShapeCast((Vector2)transform.position + offset, swingIndicatorDir * SWING_LENGTH + offset, swingArea,
		   new string[] { "Environment", "Hurtbox", "Swing" });

		float swingAngle = Vector2.SignedAngle(swingIndicatorDir, Vector2.up);
		Vector2 postSwingVel = new Vector2(swingIndicatorDir.x * SWING_STRENGTH * -1, swingIndicatorDir.y * SWING_STRENGTH * -1);

		float angleToHitNormal = Vector2.Angle(Vector2.up, swingCastUtils.FirstCastNormal(swingCastResults));
		bool wouldHitWall = angleToHitNormal >= 30 && angleToHitNormal <= 140;
		bool wouldHitLeftWall = swingCastUtils.FirstCastNormal(swingCastResults).x > 0;
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
		if (steam >= SWING_STEAM_COST && mouse0FramesHeld >= SWING_CHARGE_FRAMES)
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
                Time.timeScale = 0;
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
				if (mover.constantVels["recoilVelocity"].y != 0)
					postSwingVel.y = 0;

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

    private IEnumerator HitStop(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f;
    }
}
