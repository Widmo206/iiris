using Godot;
using System;

public partial class Airlock : Node2D
{
	public enum Type
	{
		Entrance,
		Exit,
		InLevel,
	}
	
	[Export]
	public Type AirlockType { get; set; } = Type.Entrance;
	[Export(PropertyHint.File, "*.tscn,")]
	public string DestinationLevel { get; set; } = null;
	[Export]
	public bool EntranceOnLeftSide { get; set; } = true;
	[Export(PropertyHint.None, "suffix:ticks")]
	public int ActivationDelay { get; set; } = 60;


	private AirlockDoor LeftDoor;
	private AirlockDoor RightDoor;
	private Area2D PlayerPresenceChecker;
	private Area2D Trigger;
	private Level Level;

	private AirlockDoor Entrance;
	private AirlockDoor Exit;


	private int activationCountdown = -1;


	public override void _Ready()
	{
		LeftDoor = GetNode<AirlockDoor>("AirlockDoorL");
		RightDoor = GetNode<AirlockDoor>("AirlockDoorR");
		PlayerPresenceChecker = GetNode<Area2D>("PlayerPresenceChecker");
		Trigger = GetNode<Area2D>("Trigger");
		Level = GetNode<Level>("/root/Level");

		if (EntranceOnLeftSide)
		{
			Entrance = LeftDoor;
			Exit = RightDoor;
		}
		else
		{
			Entrance = RightDoor;
			Exit = LeftDoor;
		}

		if (AirlockType == Type.Entrance)
		{
			Entrance.IsEnabled = false;
			Trigger.GetNode<CollisionShape2D>("Collider").Disabled = true;
			queueActivation();
		}
		else if (AirlockType == Type.Exit)
		{
			Exit.IsEnabled = false;
			Entrance.Open();
		}
		else
		{
			Entrance.Open();
		}
	}


	private void queueActivation()
	{
		activationCountdown = ActivationDelay;

	}


	public void OnPlayeEntered(Variant _player)
	{
		Entrance.Close();
	}


	public void OnLeftDoorClosed()
	{
		if (EntranceOnLeftSide)
		{
			OnEntranceClosed();
		}
		else
		{
			throw new NotImplementedException("Exit shouldn't be closing???");
		}
	}


	public void OnRightDoorClosed()
	{
		if (!EntranceOnLeftSide)
		{
			OnEntranceClosed();
		}
		else
		{
			throw new NotImplementedException("Exit shouldn't be closing???");
		}
	}


	public void OnEntranceClosed()
	{
		if (PlayerPresenceChecker.HasOverlappingBodies())
		{
			// Player still in airlock
			queueActivation();
		}
		else
		{
			// Oops, player escaped
			Entrance.Open();
		}
	}


	public override void _PhysicsProcess(double delta)
	{
		if (activationCountdown == 0)
		{
			if (AirlockType == Type.Exit)
			{
				Level.LoadLevel(DestinationLevel);
			}
			else
			{
				Exit.Open();
			}
			activationCountdown = -1;
		}

		if (activationCountdown > 0) activationCountdown--; // oh I can skip the braces
	}
}
