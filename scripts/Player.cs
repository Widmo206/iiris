using Godot;
using System;

// I have no idea what I'm doing
// - Widmo, 2025.02.04

public partial class Player : CharacterBody2D
{
	public const float MinWalkingSpeed = 10.0f;			// units per second; the lowest speed at which the Player will render with a walking animation
	public const float WalkingSpeed = 100.0f;			// units/second; the speed at which the PC will attempt to walk
	public const float MaxWalkingSpeed = 150.0f;         // units/second; the highest speed at which the PC will be considered as walking
	public const float RunningSpeed = 200.0f;           // units/second; the speed at which the PC will attempt to run
	public const float MaxRunningSpeed = 300.0f;        // units/second; the highest speed at which the PC will be considered as running

	public const float Mass = 40.0f;                    // kilograms;
	public const float AccelerationForce = 800.0f;      // kg*u / s^2; magnitude of the force applied when the player is accelerating
	public const float JumpMomentum = 11200.0f;         // kg*u / s; magnitude of the momentum (mass*velocity) applied to the player when jumping

	public const float DefaultStaticFrictionCoefficient = 0.6f;     // determines friction when coming to a stop; higher coefficient => more friction
	public const float DefaultDynamicFrictionCoefficient = 0.3f;    // determines friction when moving faster than max running speed; higher coefficient => more friction

	public const int InputBuffer = 6;                   // ticks; how early can an input be pressed and still register
	public const int CoyoteTime = 6;                    // ticks; how long after leaving a platform the player can still jump
	public const int InteractionLock = 20;              // ticks; how long to lock the player's movement when interacting with something

	int movementLock = 0;       // ticks; used for preventing player movement for a set time
	//string movementState = "standing";
	//Array movementStates = [""];
	bool isRunning = false;
	bool atRunningSpeed = false;
	bool atWalkingSpeed = false;
	bool isKicking = false;
	bool isGrounded = false;
	int airTime = 0;			// ticks; how long has the character spent in the air
	int jumpBuffer = 0;         // ticks; is a jump action buffered and how much time is left
	int kickBuffer = 0;         // ticks; is a kick action buffered and how much time is left


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
				velocity.Y = -JumpMomentum / Mass;
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
		if (Mathf.Abs(velocity.X) < MaxWalkingSpeed)
		{
			atRunningSpeed = false;
			atWalkingSpeed = true;
		}
		else
		{
			atRunningSpeed = true;
			atWalkingSpeed = false;
		}

		// Handle Movement
		float direction = 0.0f;
		if (movementLock <= 0)
		{
			direction = Input.GetAxis("move_left", "move_right");
		}

		float targetVelocity = 0.0f;
		float dynamicFriction = GetGravity().Y * DefaultDynamicFrictionCoefficient * -Mathf.Sign(velocity.X) * (float)delta;
		if (direction == 0.0f)
		{
			if (Mathf.Abs(dynamicFriction) >= Mathf.Abs(velocity.X))
			{
				velocity.X = 0.0f;
			}
			else
			{
				velocity.X += dynamicFriction;
			}
		}
		else
		{
			if (isRunning)
			{
				targetVelocity = direction * RunningSpeed;
			}
			else
			{
				targetVelocity = direction * WalkingSpeed;
			}

			float acceleration = 0.0f;
			bool directionAlignedWithVelocity = velocity.X * targetVelocity > 0.0f; // moonwalking fix
			if (Mathf.Abs(velocity.X) >= Mathf.Abs(targetVelocity) && directionAlignedWithVelocity)
			{
				// Going faster than desired -> friction to slow down
				// TODO: add different friction based on speed (walking -> static; running -> dynamic)
				acceleration = dynamicFriction;
			}
			else
			{
				acceleration = direction * AccelerationForce / Mass;
				if (Mathf.Abs(velocity.X) > 0.9f * Mathf.Abs(targetVelocity) && directionAlignedWithVelocity)
				{
					// Acceleration fall-off when close to desired speed
					// I used Desmos to find a nice-looking curve
					// It was supposed to be a convolution to blend with a deceleration curve above targetVelocity, but I couldn't get it to zero at targetVelocity, so I just gave up and made it blend to zero instead
					// Also, thanks to whoever made that video about convolutions (3B1B ?)
					acceleration *= 0.5f * (1.0f - Mathf.Cos(Mathf.Pi * (targetVelocity - velocity.X) / (0.2f * targetVelocity)));
				}
			}
			velocity.X += acceleration;

			
		}


		// Animation Handling
		if (isKicking)
		{
			setAnimation("kick");
		}
		else if (Mathf.Abs(velocity.X) >= MinWalkingSpeed)
		{
			if (atRunningSpeed)
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
			else if (direction == 1.0f)
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
