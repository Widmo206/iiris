using Godot;
using System;

// I have no idea what I'm doing
// - Widmo, 2025.02.04

public partial class Player : CharacterBody2D
{
	public const float MinVelocity = 20.0f;				// units per tick; if the player moves slower than that while on the ground, they'll stop immediately
	public const float Mass = 40.0f;					// kilograms;
	public const float WalkingSpeed = 2.0f;				// meters/second; the speed at which the PC will attempt to walk
	public const float MaxWalkingSpeed = 3.0f;			// meters/second; the highest speed at which the PC will be considered as walking
	public const float RunningSpeed = 5.0f;				// meters/second; the speed at which the PC will attempt to run
	public const float MaxRunningSpeed = 3.0f;			// meters/second; the highest speed at which the PC will be considered as walking


	public const float Acceleration = 40.0f;			// units per tick^2
	public const float RunningAcceleration = 60.0f;		// units per tick^2
	public const float JumpVelocity = -280.0f;			// units per tick
	public const float SpeedRetention = 0.8f;			// how much speed the player retains between physics updates; used as a soft speed cap
	public const float Friction = 0.8f;					// how fast the player slows to a stop; lower is faster

	public const int InputBuffer = 6;					// frames; how early can an input be pressed and still register
	public const int CoyoteTime = 6;					// frames; how long after leaving a platform the player can still jump
	public const int InteractionLock = 20;				// frames; how long to lock the player's movement when interacting with something

	int movementLock = 0;		// frames; used for preventing player movement for a set time
	bool isRunning = false;
	bool isKicking = false;
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

		// Gravity
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
			airTime += 1;
		}
		else {
			airTime = 0;
		}

		// Handle Interaction
		if (Input.IsActionJustPressed("interact") || kickBuffer > 0) {
			if (IsOnFloor() && movementLock <= 0)
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
			if ((IsOnFloor() || airTime < CoyoteTime) && movementLock <= 0)
			{
				jumpBuffer = 0;
				airTime += CoyoteTime + 1; // to prevent jumping several times at once
				velocity.Y = JumpVelocity;
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
			isRunning = true;
		}
		else {
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
