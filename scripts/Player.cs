using Godot;
using System;
using System.Text.RegularExpressions;

// I have no idea what I'm doing
// - Widmo, 2025.02.04

public partial class Player : CharacterBody2D
{
	// Physics constants
	public const float BaseSpeed = 250f;                // units/second; how fast the player walks
	public const float CrouchSpeed = 125f;              // units/second; how fast the player walks while crouched


	//public const float MinWalkingSpeed = 10.0f;         // units per second; the lowest speed at which the Player will render with a walking animation
	//public const float WalkingSpeed = 125.0f;           // units/second; the speed at which the PC will attempt to walk
	//public const float MaxWalkingSpeed = 150.0f;        // units/second; the highest speed at which the PC will be considered as walking
	//public const float RunningSpeed = 250.0f;           // units/second; the speed at which the PC will attempt to run
	//public const float MaxRunningSpeed = 350.0f;        // units/second; the highest speed at which the PC will be considered as running

	public const float Mass = 40.0f;                    // kilograms;
	public const float AccelerationForce = 800.0f;      // kg*u / (s*t); magnitude of the force applied when the player is accelerating
	                                                    // (yes, I'm mixing time units; velocities are in u/s, but acceleration is applied each tick)
	public const float JumpMomentum = 11200.0f;         // kg*u / s; magnitude of the momentum (mass*velocity) applied to the player when jumping
	public const float AirControlFactor = 1f;           // the PCs acceleration is multiplied by this in the air

	public const float DefaultStaticFrictionCoefficient = 0.6f;     // determines friction when walking; higher coefficient => more friction
	public const float DefaultDynamicFrictionCoefficient = 0.2f;    // determines friction when running; higher coefficient => more friction

	// Technical constants
	public const int InputBuffer = 6;                   // ticks; how early can an input be pressed and still register
	public const int CoyoteTime = 6;                    // ticks; how long after leaving a platform the player can still jump
	public const int InteractionLock = 20;              // ticks; how long to lock the player's movement when interacting with something
	public const int WalljumpLock = 6;                  // ticks; ** when walljumping
	public const int UpdatesPerSecond = 60;

	// other technical shit
	int movementLock = 0;	// ticks; used for preventing player movement for a set time
	enum State				// does this count as a constant?
	{
		// stationary
		Idle,
		Crouching,
		// moving
		Walking,
		Running,
		Sliding,
		Jumping,
		Walljumping,
		Falling,
		// interactions
		Kicking,
	}
	State currentState = State.Idle;        // the player's current state
	int stateLockCountdown = 0;             // ticks; how long until the state can be changed

	bool isRunning = false;
	bool atRunningSpeed = false;
	bool atWalkingSpeed = false;
	bool isKicking = false;
	bool isGrounded = false;
	bool canWalljump = false;
	int airTime = 0;						// ticks; how long has the character spent in the air
	int jumpBuffer = 0;						// ticks; is a jump action buffered and how much time is left
	int kickBuffer = 0;						// ticks; is a kick action buffered and how much time is left
	float facingDirection = 1f;				// 1 = right, -1 = left
	Vector2 gravityAcceleration = new Vector2(0f, 980f / 60f);		// ~~initialized at _Ready()~~ Initialized now because GetGravity seems to be broken
																	// (divided by 60 to get u/t^2 instead of u/s^2) // wait shouldn't it be 60^2 ?
																	// it worked fine so far ¯\_(?)_/¯

	// Node aliases so I don't go insane
	AnimatedSprite2D PlayerSprite;
	CollisionShape2D InteractionCollider;
	Area2D WalljumpDetector;
	TileMapLayer Ground;
	AnimatedSprite2D WalljumpDebugIndicator;
	AudioStreamPlayer JumpSFX;


	private void setAnimation(string animation)
	{
		PlayerSprite.Play(animation);
	}

	public override void _Ready()
	{
		//GD.Print("LOADED Player.cs");
		//gravityAcceleration = GetGravity() / UpdatesPerSecond;

		// Populate node aliases
		PlayerSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		InteractionCollider = GetNode<CollisionShape2D>("InteractionTrigger/Collider");
		WalljumpDetector = GetNode<Area2D>("WalljumpDetector");
		Ground = GetNode<TileMapLayer>("../Terrain/Ground");
		WalljumpDebugIndicator = GetNode<AnimatedSprite2D>("WalljumpDetector/Collider/DebugIndicator");
		JumpSFX = GetNode<AudioStreamPlayer>("JumpSfx");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		isGrounded = IsOnFloor();
		

		// Gravity
		if (isGrounded)
		{
			airTime = 0;
			if (currentState == State.Jumping) {
				currentState = State.Idle;
			}
		}
		else
		{
			velocity += gravityAcceleration;
			airTime += 1;
			if (velocity.Y > 0) // -y is down
			{
				currentState = State.Falling;
			}
		}

		// Handle Interaction
		if (Input.IsActionJustPressed("interact") || kickBuffer > 0) {
			if (isGrounded && stateLockCountdown <= 0)
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
		if (currentState == State.Kicking)
		{
			InteractionCollider.Disabled = false;
		}
		else
		{
			InteractionCollider.Disabled = true;
		}


		// Handle Directional Colliders
		float inputDirection = 0f;
		if (stateLockCountdown <= 0)
		{
			inputDirection = Input.GetAxis("move_left", "move_right");
		}
		// Not using Mathf.Sign() because I don't want to change with no input
		if (inputDirection > 0f)
		{
			// Facing right
			facingDirection = 1f;
		}
		else if (inputDirection < 0f)
		{
			// Facing left
			facingDirection = -1f;
		}

		// there's probably a better way, but this was convenient (as in, flipping the parent node)
		WalljumpDetector.Scale = new Vector2(facingDirection, 1f);


		// Check for walljumping
		canWalljump = false;
		if (WalljumpDetector.OverlapsBody(Ground))
		{
			// Touching wall
			if (isGrounded)
			{
				WalljumpDebugIndicator.Play("not_in_air");
			}
			else
			{
				WalljumpDebugIndicator.Play("valid");
				canWalljump = true;
			}

		}
		else
		{
			// Not touching wall
			WalljumpDebugIndicator.Play("no_wall");
		}


		// Handle Jump and Walljump
		// TODO: make jump stronger/weaker based on how long the key is held (jump on rising or falling edge?)
		var rand = new Random();
		if (Input.IsActionJustPressed("jump") || jumpBuffer > 0)
		{
			float JumpVelocity = -JumpMomentum / Mass;
			if ((isGrounded || airTime < CoyoteTime) && stateLockCountdown <= 0 && currentState != State.Jumping)
			{
				jumpBuffer = 0;
				// should be unnecessary due to currentState
				//airTime += CoyoteTime + 1; // to prevent jumping several times at once
				velocity.Y = JumpVelocity;
				currentState = State.Jumping;
				isGrounded = false;
				// TODO: move SFX to a separate function
				JumpSFX.PitchScale = 1f + ((float)rand.NextDouble() - 0.5f) * 0.1f;
				JumpSFX.Play();
			}
			else if (canWalljump && movementLock <= 0)
			{
				jumpBuffer = 0;
				currentState = State.Walljumping;
				stateLockCountdown = WalljumpLock;
				// GD.Print("Walljumpng!");
				// velocity.X *= -0.5f;
				// Another Desmos equation; 
				velocity.Y += (1 + Mathf.Atan(velocity.Y * 0.01f) * 2 / Mathf.Pi) * JumpVelocity / Mathf.Sqrt2;
				velocity.X += facingDirection * JumpVelocity / Mathf.Sqrt2;
				facingDirection *= -1f; // TIL the decimal point isn't required
			}
			else if (Input.IsActionJustPressed("jump"))	// checking again so buffer is only updated on button press
			{
				jumpBuffer = InputBuffer;
			}
		}

		// Handle Movement
		
		// direction moved under "Handle Directional Colliders"

		float dynamicFriction = gravityAcceleration.Y * DefaultDynamicFrictionCoefficient;  // magnitude
		float staticFriction = gravityAcceleration.Y * DefaultStaticFrictionCoefficient;	// yeah I know static friction works differently

		float decelerationFriction = 0f;
		// No friction in the air
		if (velocity.X != 0f && isGrounded)
		{
			if (currentState == State.Sliding)
			{
				decelerationFriction = dynamicFriction * -Mathf.Sign(velocity.X);
			}
			else
			{
				decelerationFriction = staticFriction * -Mathf.Sign(velocity.X);
			}

			if (Mathf.Abs(decelerationFriction) >= Mathf.Abs(velocity.X))
			{
				// to not overshoot and accidentally accelerate the other way
				decelerationFriction = -velocity.X;
			}
		}

		float targetVelocity = 0f;
		if (inputDirection == 0f)
		{
			velocity.X += decelerationFriction;
			if (isGrounded && stateLockCountdown <= 0) {
				if (Input.IsActionPressed("crouch"))
				{
					currentState = State.Crouching;
				}
				else
				{
					currentState = State.Idle;

				}
			}
		}
		else
		{
			if (Input.IsActionPressed("crouch"))
			{
				targetVelocity = inputDirection * CrouchSpeed;
				if (isGrounded)
				{ 
					currentState = State.Walking;
				}
			}
			else
			{
				targetVelocity = inputDirection * BaseSpeed;
				if (isGrounded)
				{
					currentState = State.Running;
				}
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
				{
					acceleration *= AirControlFactor;
				}

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


		// Animation Handling
		switch (currentState)
		{
			case State.Idle:
				setAnimation("idle");
				break;

			case State.Crouching:
				setAnimation("crouched");
				break;

			case State.Kicking:
				setAnimation("kicking");
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

			case State.Jumping:
				setAnimation("jump");
				break;

			case State.Walljumping:
				setAnimation("jump");
				break;

			case State.Falling:
				setAnimation("fall");
				break;
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

		// Variable management
		if (jumpBuffer > 0)
		{
			jumpBuffer--;
		}
		if (kickBuffer > 0)
		{
			kickBuffer--;
		}
		if (stateLockCountdown > 0)
		{
			stateLockCountdown--;
		}
		else
		{
			isKicking = false;
		}

		
		GetNode<Label>("../HUD/PositionDisplay").Text = $"Position:\n    x: {Position.X}\n    y: {Position.Y}";
		GetNode<Label>("../HUD/VelocityDisplay").Text = $"Velocity:\n    x: {Velocity.X}\n    y: {Velocity.Y}";
		GetNode<Label>("../HUD/StateDisplay").Text = $"State: {currentState}\nstateLockCountdown: {stateLockCountdown.ToString()}\njumpBuffer: {jumpBuffer}\nkickBuffer: {kickBuffer}";

		Velocity = velocity;
		MoveAndSlide();
	}
}
