using Godot;
using System;

// I have no idea what I'm doing
// - Widmo, 2025.02.04

// Is that comment too unprofessional?

public partial class Player : CharacterBody2D
{
	// Physics constants
	public const float BaseSpeed				= 250f;				// units/second; how fast the player walks
	public const float CrouchSpeed				= 250f;				// units/second; how fast the player walks while crouched
	public const float DashSpeed				= 400f;				// u / s; dash speed is independent of mass

	public const float Mass						= 40f;				// kilograms; the PC's mass, without any carried items (TBA)
	public const float AccelerationForce		= 600f;				// kg*u / (s*t); magnitude of the force applied when the player is accelerating
																	// (yes, I'm mixing time units; velocities are in u/s, but acceleration is applied each tick)
	public const float JumpMomentum				= 12000f;			// kg*u / s; magnitude of the momentum (mass*velocity) applied to the player when jumping
	//public const float MinJumpStrength			= 0.75f;			// multiplier; strength of a jump with no accumulation (i.e. pressed for only 1 tick)
	public const float JumpDecay				= 0.8f;				// multiplier; how quickly the player's vertical velocity decays when the jump key is released (lower is faster)
	public const float AirControlFactor			= 0.5f;				// multiplier; the PCs horizontal acceleration is multiplied by this in the air
	public const float SlopeSpeedFactor			= 1.2f;				// multiplier; how slopes influence dash speed

	public const float DefaultStaticFrictionCoefficient  = 1f;		// determines friction most of the time; higher coefficient => more friction
	public const float DefaultDynamicFrictionCoefficient = 0.1f;	// determines friction when sliding;     higher coefficient => more friction

	// ✧˖°. I identify as a constant ⋆˙⟡
	public readonly Vector2 gravityAcceleration = new Vector2(0f, 980f / 60f);	// ~~initialized at _Ready()~~ Initialized now because GetGravity seems to be broken
																				// divided by 60 to get u / (s*t) -> u/s applied each tick

	// Technical constants
	//public const int MaxJumpAccumulationTime	= 6;				// ticks; how long the jump key needs to be held for a maxiumum strength jump
	public const int DashDuration				= 15;				// ticks; what it says on the tin
	public const int DashCooldown				= 6;				// ticks; how long to wait before allowing the player to dash again
	public const int DashEffectInterval			= 6;				// ticks; how long to wait between successive instances of DashEffect
	public const int InputBuffer				= 6;				// ticks; how early can an input be pressed and still register
	public const int CoyoteTime					= 6;				// ticks; how long after leaving a platform the player can still jump
	public const int InteractionLock			= 20;				// ticks; how long to lock the player's movement when interacting with something
	public const int WalljumpLock				= 5;				// ticks;                                     ** when walljumping
	public const int UpdatesPerSecond			= 60;				// ticks/second;

	// other technical shit
	public enum State
	{
		// stationary
		Idle,
		Crouching,
		Dead,
		// moving
		Walking,
		Running,
		Sliding,
		Dashing,
		Jumping,
		Walljumping,
		Falling,
		// interactions
		Kicking,
	}
	public int VoidDepth;								// lower boundry of the world; the player dies if they cross it
	public State currentState			= State.Idle;	// the player's current state
	string currentHitbox				= "";			// which hitbox is currently enabled; only used by setHitbox to check if the requested hitbox is different from the current one
	public int stateLockCountdown		= 0;			// ticks; how long until the state can be changed again
	public int dashEffectCounter		= 0;			// ticks; how long until a new dash effect "particle" can be spawned
	public int dashCounter				= 0;			// ticks; if positive: how long until the dash runs out; if negative: how much cooldown is left
	public int jumpAccumulation			= 0;			// ticks; how long the jump button has been pressed
	public int airTime					= 0;			// ticks; how long has the character spent in the air
	public int jumpBuffer				= 0;			// ticks; is a jump action buffered and how much time is left
	public int kickBuffer				= 0;			// ticks; is a kick action buffered and how much time is left
	public int dashBuffer				= 0;			// ticks; is a dash action buffered and how much time is left
	public int facingDirection			= 1;			// 1 = right, -1 = left
	public bool isAlive					= true;
	public bool isGrounded				= false;
	public bool dashEnabled				= true;
	public bool canWalljump				= false;
	public bool canDash					= false;


	// Node aliases so I don't go insane
	Node2D				LevelScene;
	AnimatedSprite2D	PlayerSprite;
	CollisionShape2D	InteractionCollider;
	WalljumpDetector	WalljumpDetector;
	Area2D				DamageDetector;
	Area2D				SlopeDetector;
	TileMapLayer		Ground;
	AudioStreamPlayer2D	JumpSFX;
	AudioStreamPlayer2D	DashSFX;
	PackedScene			dashEffect;

	CollisionShape2D	StandingCollider;
	CollisionShape2D	CrouchingCollider;
	CollisionShape2D	DashingCollider;


	//private int dashCooldownPercentage()
	//{
	//	if (dashEffectCounter > 0)
	//	{
	//		return (int)Mathf.Round(100 * (float)dashCounter / (float)DashDuration);
	//	}
	//	else
	//	{
	//		return (int)Mathf.Round(100 * (1f + (float)dashCounter / (float)DashCooldown));
	//	}
	//}

	private bool isntStateLocked()
	{
		return stateLockCountdown <= 0;
	}

	private void setAnimation(string animation)
	{
		if (PlayerSprite.Animation != animation)
		{
			PlayerSprite.Play(animation);
		}
	}

	private void setHitbox(string hitbox)
	{
		// don't bother the nodes if there's no need to change anything
		if (hitbox == currentHitbox) { return; }

		// TODO: implement checks foe whether a collider change is valid -> may fall through world otherwise

		currentHitbox = hitbox;
		switch (hitbox)
		{
			case "standing":
				StandingCollider.Disabled  = false;
				CrouchingCollider.Disabled = true;
				DashingCollider.Disabled   = true;
				break;

			case "crouching":
				StandingCollider.Disabled  = true;
				CrouchingCollider.Disabled = false;
				DashingCollider.Disabled   = true;
				break;

			case "dashing":
				StandingCollider.Disabled  = true;
				CrouchingCollider.Disabled = true;
				DashingCollider.Disabled   = false;
				break;

			default:
				throw new ArgumentException($"Missing or incorrect hitbox state: {hitbox}");
		}
	}


	private void tryCreateDashEffect()
	{
		if (dashEffectCounter <= 0)
		{
			Node2D instance = (Node2D) dashEffect.Instantiate();
			instance.Position = Position;
			LevelScene.AddChild(instance);
			dashEffectCounter = DashEffectInterval;
		}
	}

	public override void _Ready()
	{
		//GD.Print("LOADED Player.cs");
		//gravityAcceleration = GetGravity() / UpdatesPerSecond;

		// Populate node aliases
		LevelScene			= GetNode<Level>("/root/Level"); // bruh this just works
		PlayerSprite		= GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		InteractionCollider	= GetNode<CollisionShape2D>("InteractionTrigger/Collider");
		WalljumpDetector	= GetNode<WalljumpDetector>("WalljumpDetector");
		SlopeDetector		= GetNode<Area2D>("SlopeDetector");
		DamageDetector		= GetNode<Area2D>("DamageDetector");
		Ground				= LevelScene.GetNode<TileMapLayer>("Terrain/Ground");
		JumpSFX				= GetNode<AudioStreamPlayer2D>("JumpSFX");
		DashSFX				= GetNode<AudioStreamPlayer2D>("DashSFX");

		StandingCollider	= GetNode<CollisionShape2D>("StandingCollider");
		CrouchingCollider	= GetNode<CollisionShape2D>("CrouchingCollider");
		DashingCollider		= GetNode<CollisionShape2D>("DashingCollider");


		// other resources
		dashEffect			= GD.Load<PackedScene>("res://scenes/dash_effect.tscn");
		VoidDepth			= (int)LevelScene.Get("VoidDepth");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 position = Position;
		Vector2 velocity = Velocity;
		isGrounded = IsOnFloor();


		// Dash: Handle colliding with a slope
		if (currentState == State.Dashing)
		{
			var slopes = SlopeDetector.GetOverlappingBodies();
			foreach (var slopeTileMap in slopes)
			{
				Slope slope = slopeTileMap.GetNode<Slope>("..");
				// dot product is negative if we're moving into the slope, 0 if we're riding along it, positive if we're moving away
				if (velocity.Dot(slope.getNormalVector()) <= 0.01f)
				//if (velocity.AngleTo(slope.getNormalVector()) >= Mathf.Pi/2)
				{
					Vector2 orthogonal = slope.getNormalVector().Rotated(-Mathf.Pi / 2); // orthoganal to the normal vector => parallel to slope surface
					orthogonal *= Mathf.Sign(orthogonal.Dot(velocity)); // flip if necessary
					velocity = orthogonal * DashSpeed * SlopeSpeedFactor;

					// refresh dash
					dashCounter = DashDuration;
					stateLockCountdown = DashDuration;

					break; // only considering the first slope we can dash off of
				}
			}
		}


		// Handle dash expiring (at the beginning, so it doesn't break shit)
		// checking for lower-than-expected velocity to prevent getting stuck on walls
		if (currentState == State.Dashing &&  (dashCounter <= 0 || (float)velocity.Length() < 0.75*DashSpeed)) {
			currentState = State.Falling;
			dashCounter = -DashCooldown;
			stateLockCountdown = 0;
		}
		

		// Gravity
		if (currentState == State.Dashing)
		{
			isGrounded = false;
			airTime += 1;
		}
		else if (isGrounded)
		{
			airTime = 0;
			if (currentState == State.Jumping) {
				currentState = State.Idle;
				//jumpAccumulation = 0;
			}
		}
		else
		{
			velocity += gravityAcceleration;
			airTime += 1;
			if (velocity.Y > 0) // +y is down
			{
				// GD.Print(currentState);
				currentState = State.Falling;
				jumpAccumulation = 0;
			}
		}


		// Handle damage
		if (DamageDetector.HasOverlappingBodies() || position.Y > VoidDepth)
		{
			isAlive = false;
			currentState = State.Dead;
		}


		// Handle Interaction
		/*
		if ((Input.IsActionJustPressed("interact") || kickBuffer > 0) && isAlive)
		{
			if (isGrounded && isntStateLocked())
			{
				currentState = State.Kicking;
				// kickBuffer = 0;
				stateLockCountdown = InteractionLock;
			}
			else if (Input.IsActionJustPressed("interact")) // checking again so buffer is only updated on button press
			{
				// TODO: create a single object for managing input buffers
				kickBuffer = InputBuffer;
			}
		}
		*/
		if (currentState == State.Kicking)
		{
			InteractionCollider.Disabled = false;
		}
		else
		{
			InteractionCollider.Disabled = true;
		}


		// Handle Directional Actions
		float inputDirection = 0f;
		if (isntStateLocked() && isAlive)
		{
			inputDirection = Input.GetAxis("move_left", "move_right");
		}
		// Not using Mathf.Sign() because I don't want to change with no input
		if (inputDirection > 0f)
		{
			// Facing right
			facingDirection = 1;
		}
		else if (inputDirection < 0f)
		{
			// Facing left
			facingDirection = -1;
		}


		// Handle Dash
		// dash expiring is checked at the beginning, so you can potentially start a new dash in the same tick
		// except you can't because of cooldown
		if (dashCounter == 0 && isGrounded) { canDash = true; }

		if ((Input.IsActionJustPressed("dash") && inputDirection != 0f || dashBuffer > 0) && isAlive)
		{
			if (canDash)
			{
				currentState = State.Dashing;
				dashCounter = DashDuration;
				velocity = new Vector2(inputDirection * DashSpeed, 0f);
				isGrounded = false;
				stateLockCountdown = DashDuration;
				canDash = false;
				DashSFX.Play();
			}
			else if (Input.IsActionJustPressed("dash"))
			{
				// checking the input again to only buffer if we're jumping due to input
				// results in chain-buffering otherwise
				dashBuffer = InputBuffer;
			}
			
		}


		// Handle Jump and Walljump

		// Check for walljumping
		WalljumpDetector.CheckWalljump();
		canWalljump = WalljumpDetector.canWalljump && !Input.IsActionPressed("crouch");

		if (!Input.IsActionPressed("jump") && currentState == State.Jumping /*&& velocity.Y < 0f*/)
		{
			// thanks to https://www.gamemakerkitchen.com/tutorials/jordan-guillou/10-levels-of-platformer-jumps/#5.-variable-jump-height
			velocity.Y *= JumpDecay;
		}

		if ((Input.IsActionJustPressed("jump") || jumpBuffer > 0) && isAlive)
		{
			//GD.Print("traying to jump");
			float JumpVelocity = JumpMomentum / Mass;
			if ((isGrounded || airTime < CoyoteTime) && isntStateLocked() && currentState != State.Jumping)
			{
				// Jumping
				jumpBuffer = 0;
				// should be unnecessary due to currentState
				//airTime += CoyoteTime + 1; // to prevent jumping several times at once
				velocity.Y = -JumpVelocity/* * MinJumpStrength*/;

				// Exponential decay of horizontal velocity to prevent bunnyhopping
				//velocity.X += JumpVelocity * inputDirection * 0.5f * Mathf.Pow(2f, -Mathf.Pow(velocity.X, 2f) / BaseSpeed);
				velocity.X += BaseSpeed * inputDirection / 10;


				currentState = State.Jumping;
				isGrounded = false;
				JumpSFX.Play();
			}
			else if (canWalljump && isntStateLocked())
			{
				// Walljumping
				int walljumpDirection;
				if (WalljumpDetector.direction == WalljumpDetector.Direction.Left)
				{
					walljumpDirection = -1;
				}
				else if (WalljumpDetector.direction == WalljumpDetector.Direction.Right)
				{
					walljumpDirection = 1;
				}
				else
				{
					walljumpDirection = facingDirection;
				}

				//GD.Print("walljumping!!!");
				jumpBuffer = 0;
				currentState = State.Walljumping;
				stateLockCountdown = WalljumpLock;
				// velocity.X *= -0.5f;
				// Another Desmos equation; 
				velocity.Y += (1 + Mathf.Atan(velocity.Y * 0.01f) * 2 / Mathf.Pi) * -JumpVelocity / Mathf.Sqrt2;
				velocity.X = walljumpDirection * -JumpVelocity / Mathf.Sqrt2 /*- 0.5f* walljumpDirection*Mathf.Abs(velocity.X)*/;
				facingDirection = -walljumpDirection; // TIL the decimal point isn't required // that comment is out of place now because I changed this to an int // it's not even a literal anymore
				JumpSFX.Play();
			}
			else if (Input.IsActionJustPressed("jump")) // checking again so buffer is only updated on button press
			{
				jumpBuffer = InputBuffer;
			}
		}


		// Handle Horizontal Movement
		// direction moved under "Handle Directional Actions"

		// Friction
		float dynamicFriction = gravityAcceleration.Y * DefaultDynamicFrictionCoefficient;  // magnitude
		float staticFriction = gravityAcceleration.Y * DefaultStaticFrictionCoefficient;	// yeah I know static friction works differently

		float decelerationFriction = 0f;
		if (velocity.X != 0f && isGrounded)     // No friction in the air
		{
			if (currentState == State.Sliding)
			{ decelerationFriction = dynamicFriction * -Mathf.Sign(velocity.X); }
			else
			{ decelerationFriction = staticFriction * -Mathf.Sign(velocity.X); }

			if (Mathf.Abs(decelerationFriction) >= Mathf.Abs(velocity.X))
			{
				// to not overshoot and accidentally accelerate the other way
				decelerationFriction = -velocity.X;
			}
		}

		// Sliding
		if (isntStateLocked() && isAlive)
		{
			if (Mathf.Abs(velocity.X) > 1.25f * BaseSpeed && isGrounded && Input.IsActionPressed("crouch") ||
				Mathf.Abs(velocity.X) > 2f * BaseSpeed && isGrounded)
			{ currentState = State.Sliding; }
			else if (currentState == State.Sliding)
			{ currentState = State.Idle; }
		}

		float targetVelocity = 0f;
		if (currentState == State.Sliding)
		{ velocity.X += decelerationFriction; }
		// Stopping
		else if (inputDirection == 0f)
		{
			velocity.X += decelerationFriction;
			if (isGrounded && isntStateLocked() && isAlive)
			{
				if (Input.IsActionPressed("crouch"))
				{ currentState = State.Crouching; }
				else
				{ currentState = State.Idle; }
			}
		}
		// Walking, running, crouching, etc.
		else
		{
			if (Input.IsActionPressed("crouch"))
			{
				targetVelocity = inputDirection * CrouchSpeed;
				if (isGrounded)
				{ currentState = State.Walking; }
			}
			else
			{
				targetVelocity = inputDirection * BaseSpeed;
				if (isGrounded)
				{ currentState = State.Running; }
			}

			float acceleration = 0f;
			bool directionAlignedWithVelocity = velocity.X * targetVelocity > 0f; // moonwalking fix
			if (Mathf.Abs(velocity.X) >= Mathf.Abs(targetVelocity) && directionAlignedWithVelocity)
			{
				// Going faster than desired -> friction to slow down
				acceleration = decelerationFriction;
			}
			else
			{
				acceleration = inputDirection * AccelerationForce / Mass;
				if (!isGrounded)
				{ acceleration *= AirControlFactor; }

				if (Mathf.Abs(velocity.X) > 0.9f * Mathf.Abs(targetVelocity) && directionAlignedWithVelocity)
				{
					// Acceleration fall-off when close to desired speed
					// I used Desmos to find a nice-looking curve
					// It was supposed to be a convolution to blend with a deceleration curve above targetVelocity, but I couldn't get it to zero at targetVelocity,
					// so I just gave up and made it blend to zero instead		// actuallynot a problem, since above targetVelocity, friction gets applied
					// Also, thanks to whoever made that video about convolutions (3B1B ?)
					acceleration *= 0.5f * (1f - Mathf.Cos(Mathf.Pi * (targetVelocity - velocity.X) / (0.2f * targetVelocity)));
					// TODO: revisit accel curve so momentum matters more
				}
			}
			velocity.X += Mathf.Clamp(acceleration, -staticFriction, staticFriction);


		}

		// Collision Box Handling
		switch (currentState)
		{
			case State.Idle:
				setHitbox("standing");
				break;

			case State.Crouching:
				setHitbox("crouching");
				break;

			case State.Dead:
				setHitbox("crouching");
				break;

			case State.Kicking:
				setHitbox("standing");
				break;

			case State.Walking:
				setHitbox("standing");
				break;

			case State.Running:
				setHitbox("standing");
				break;

			case State.Sliding:
				setHitbox("crouching");
				break;

			case State.Dashing:
				setHitbox("dashing");
				tryCreateDashEffect();
				break;

			case State.Jumping:
				setHitbox("standing");
				break;

			case State.Walljumping:
				setHitbox("standing");
				break;

			case State.Falling:
				setHitbox("standing");
				break;

			default:
				// I forgot to add a case
				throw new NotImplementedException($"Missing hitbox implementation for {currentState}");
		}


		if (!isAlive) { currentState = State.Dead; } // just in case
		// Animation Handling
		switch (currentState)
		{
			case State.Idle:
				setAnimation("idle");
				break;

			case State.Crouching:
				setAnimation("crouched");
				break;

			case State.Dead:
				setAnimation("die");
				break;

			case State.Kicking:
				setAnimation("kick");
				break;

			case State.Walking:
				setAnimation("sneak");
				break;

			case State.Running:
				setAnimation("walk");
				break;

			case State.Sliding:
				setAnimation("slide");
				break;

			case State.Dashing:
				setAnimation("dash");
				tryCreateDashEffect();
				break;

			case State.Jumping:
				setAnimation("jump");
				break;

			case State.Walljumping:
				setAnimation("jump");
				break;

			case State.Falling:
				setAnimation("fall");
				break;

			default:
				// I forgot to add a case
				throw new NotImplementedException($"Missing animation implementation for {currentState}");
		}


		// Handling sprite facing; facing == 1f is right
		if (facingDirection == -1f)
		{
			PlayerSprite.FlipH = true;
		}
		else if (facingDirection == 1f)
		{
			PlayerSprite.FlipH = false;
		}


		// Frame-counter management
		if (jumpBuffer > 0)			{ jumpBuffer--; }

		if (kickBuffer > 0)			{ kickBuffer--; }

		if (stateLockCountdown > 0)	{ stateLockCountdown--; }

		if (dashEffectCounter > 0)	{ dashEffectCounter--; }

		if (dashCounter > 0)		{ dashCounter--; }
		else if (dashCounter < 0)	{ dashCounter++; }

		if (dashBuffer > 0)			{ dashBuffer--; }


		// non-debug UI management
		//string percentage = dashCooldownPercentage().ToString().PadLeft(3, '0');
		string state;
		if (currentState == State.Dashing) { state = "ACTIVE"; }
		else if (dashEnabled)
		{
			if (dashCounter == 0) { state = "READY"; }
			else { state = "RECHARGING"; }
		}
		else { state = "UNAVAILABLE"; }
		GetNode<Label>("../HUD/DashIndicator").Text = $"Dash: {state}";


		Velocity = velocity;
		MoveAndSlide();
	}
}
