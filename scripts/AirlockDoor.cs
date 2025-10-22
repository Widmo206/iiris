using Godot;
using System;

public partial class AirlockDoor : StaticBody2D
{
	[Export]
	private float ClosedPosition = -2f;
	[Export]
	private float OpenPosition = -56f;
	[Export]
	public float Speed = 1f;
	[Export]
	public bool IsEnabled = true;

	[Signal]
	public delegate void DoorClosedEventHandler();
	[Signal]
	public delegate void DoorOpenedEventHandler();

	enum State
	{
		Closed,
		Opening,
		Open,
		Closing,
	}
	State currentState = State.Closed;


	AnimatedSprite2D GearL;
	AnimatedSprite2D GearR;
	AnimatableBody2D Door;


	public void Open()
	{
		if (IsEnabled)
		{
			currentState = State.Opening;
			TrySetAnimation(GearL, "spinning_left");
			TrySetAnimation(GearR, "spinning_right");
			//GearL.Animation = "spinning_right"; // stoopid
			//GearR.Animation = "spinning_left";  // I tried to set the animation instead of Play()'ing it
		}
	}

	public void Close()
	{
		if (IsEnabled)
		{
			currentState = State.Closing;
			TrySetAnimation(GearL, "spinning_right");
			TrySetAnimation(GearR, "spinning_left");
			//GearL.Animation = "spinning_left";
			//GearR.Animation = "spinning_right";
		}
	}

	public void Toggle()
	{
		if (IsEnabled)
		{
			if (currentState == State.Closed) { Open(); }
			else if (currentState == State.Open) { Close(); }
		}
	}

	private void TrySetAnimation(AnimatedSprite2D sprite, string animationName)
	{
		if (sprite.Animation == animationName) { return; }
		else { sprite.Play(animationName); }
	}


	//EmitSignal(SignalName.CoinCollected);


	public override void _Ready()
	{
		GearL = GetNode<AnimatedSprite2D>("GearL");
		GearR = GetNode<AnimatedSprite2D>("GearR");
		Door  = GetNode<AnimatableBody2D>("Door");

		if (currentState == State.Closed)
		{
			Vector2 position = Door.Position;
			position.Y = ClosedPosition;
			Door.Position = position;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 position = Door.Position;
		float moveSpeed = 0f;

		if (currentState == State.Closing && IsEnabled)
		{
			if (position.Y >= ClosedPosition)
			{
				currentState = State.Closed;
				EmitSignal(SignalName.DoorClosed);
				TrySetAnimation(GearL, "stopped");
				TrySetAnimation(GearR, "stopped");
				//GearL.Animation = "stopped";
				//GearR.Animation = "stopped";
			}
			else
			{
				moveSpeed = Speed;
			}
		}
		else if (currentState == State.Opening && IsEnabled)
		{
			if (position.Y <= OpenPosition)
			{
				currentState = State.Open;
				EmitSignal(SignalName.DoorOpened);
				TrySetAnimation(GearL, "stopped");
				TrySetAnimation(GearR, "stopped");
				//GearL.Animation = "stopped";
				//GearR.Animation = "stopped";
			}
			else
			{
				moveSpeed = -Speed;
			}
		}

		if (moveSpeed != 0f)
		{
			position.Y += moveSpeed;
			Door.Position = position;
		}
	}
}
