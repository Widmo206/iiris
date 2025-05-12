using Godot;
using System;

// I have no idea what I'm doing
// - Widmo, 2025.02.04

public partial class Player : CharacterBody2D
{
	public const float MinVelocity = 10.0f;				// units per second; if the player is moving slower than that (without input) while on the ground, they'll stop immediately
	public const float WalkingSpeed = 50.0f;			// units/second; the speed at which the PC will attempt to walk
	public const float MaxWalkingSpeed = 75.0f;         // units/second; the highest speed at which the PC will be considered as walking
	public const float RunningSpeed = 200.0f;           // units/second; the speed at which the PC will attempt to run
	public const float MaxRunningSpeed = 300.0f;        // units/second; the highest speed at which the PC will be considered as walking

	public const float Mass = 40.0f;                    // kilograms;
	public const float AccelerationForce = 1600.0f;     // kg*u / s^2; magnitude of the force applied when the player is accelerating
	public const float RunningForceMultiplier = 1.5f;   // how much more acceleration force is applied when trying to run
	public const float JumpMomentum = 11200.0f;         // kg*u / s; magnitude of the momentum (mass*velocity) applied to the player when jumping

	public const float DefaultStaticFrictionCoefficient = 0.6f;     // determines friction when coming to a stop; higher coefficient => more friction
	public const float DefaultDynamicFrictionCoefficient = 0.3f;    // determines friction when moving faster than max running speed; higher coefficient => more friction

	public const float Acceleration = 40.0f;			// units per second^2
	public const float RunningAcceleration = 60.0f;		// units per second^2
	public const float JumpVelocity = -280.0f;			// units per second
	public const float SpeedRetention = 0.8f;			// how much speed the player retains between physics updates; used as a soft speed cap
	public const float Friction = 0.8f;					// how fast the player slows to a stop; lower is faster

	public const int InputBuffer = 6;					// frames; how early can an input be pressed and still register
	public const int CoyoteTime = 6;					// frames; how long after leaving a platform the player can still jump
	public const int InteractionLock = 20;				// frames; how long to lock the player's movement when interacting with something

	int movementLock = 0;       // frames; used for preventing player movement for a set time
	string movementState = "standing";
	//Array movementStates = [""];
	bool isTryingToRun = false;
	bool isRunning = false;
	bool isKicking = false;
	bool isGrounded = false;
	int airTime = 0;			// frames; how long has the character spent in the air
	int jumpBuffer = 0;			// frames; is a jump action buffered and how much time is left
	int kickBuffer = 0;			// frames; is a kick action buffered and how much time is left


	private void setAnimation(string animation)
	{
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play(animation);
	}

	public override void _Ready()
	{
		GD.Print("LOADED Player.cs");
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		isGrounded = IsOnFloor();

		// Gravity
		if (isGrounded)
		{
			airTime = 0;
		}
		else
		{
			velocity += GetGravity() * (float)delta;
			airTime += 1;

		}

		// Handle Interaction
		if (Input.IsActionJustPressed("interact") || kickBuffer > 0) {
			if (isGrounded && movementLock <= 0)
			{
				isKicking = true;
				// kickBuffer = 0;
				movementLock = InteractionLock;
			}
			else if (kickBuffer <= 0) {
				kickBuffer = InputBuffer;
			}
		}

		// Handle Jump
		var rand = new Random();
		if (Input.IsActionJustPressed("jump") || jumpBuffer > 0)
		{
			if ((isGrounded || airTime < CoyoteTime) && movementLock <= 0)
			{
				jumpBuffer = 0;
				airTime += CoyoteTime + 1; // to prevent jumping several times at once
				velocity.Y = JumpMomentum/Mass;
				GetNode<AudioStreamPlayer>("JumpSfx").PitchScale = 1.0f + ((float)rand.NextDouble() - 0.5f) * 0.1f;
				GetNode<AudioStreamPlayer>("JumpSfx").Play();
			}
			else if (jumpBuffer <= 0) {
				jumpBuffer = InputBuffer;
			}
		}

		// Handle Running
		if (Input.IsActionPressed("run"))
		{
			isTryingToRun = true;
		}
		else {
			isTryingToRun = false;
		}
		if (velocity.X < MaxWalkingSpeed)
		{
			isRunning = false;
		}

		// Handle Movement
		float direction = 0.0f;
		if (movementLock <= 0)
		{
			direction = Input.GetAxis("move_left", "move_right");
		}

		if (Mathf.Abs(velocity.X) > MinVelocity)
		{
			if (direction != 0.0f)
			{
				// Slowdown when moving -> soft speed cap
				velocity.X *= SpeedRetention;
			}
			else
			{
				// Deceleration when no movement is applied
				velocity.X *= Friction;
			}
		}
		else
		{
			velocity.X = 0.0f;
		}

		if (direction != 0.0f)
		{
			// inline conditional statement, split for clarity
			// var = bool ? true : false
			velocity.X += isRunning
				? direction * RunningAcceleration	// running
				: direction * Acceleration;			// walking
		}


		// Animation Handling
		if (isKicking)
		{
			setAnimation("kick");
		}
		else if (direction != 0.0f)
		{
			if (isRunning)
			{
				setAnimation("run");
			}
			else
			{
				setAnimation("walk");
			}

			// Handling sprite facing; if direction == 0.0f, the facing stays the same
			if (direction == -1.0f)
			{
				GetNode<AnimatedSprite2D>("AnimatedSprite2D").FlipH = true;
			}
			else
			{
				GetNode<AnimatedSprite2D>("AnimatedSprite2D").FlipH = false;
			}
		}
		else
		{
			setAnimation("idle");
		}
		if (!IsOnFloor())
		{
			setAnimation("jump");
		}

		// Variable management
		if(jumpBuffer > 0)
		{
			jumpBuffer--;
		}
		if(kickBuffer > 0)
		{
			kickBuffer--;
		}
		if (movementLock > 0)
		{
			movementLock--;
		}
		else
		{
			isKicking = false;
		}


		GetNode<Label>("../HUD/PositionDisplay").Text = "Position:\n    x: " + Position.X.ToString() + "\n    y: " + Position.Y.ToString();
		GetNode<Label>("../HUD/VelocityDisplay").Text = "Velocity:\n    x: " + Velocity.X.ToString() + "\n    y: " + Velocity.Y.ToString();

		Velocity = velocity;
		MoveAndSlide();
	}
}
