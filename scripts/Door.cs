using Godot;
using System;

public partial class Door : Area2D
{
	[Export(PropertyHint.File, "*.tscn,")]
	public string Level = null;

	[Signal]
	public delegate void PlayerEnteredEventHandler(string Level);


	public void OnBodyEntered(Node2D CollidingEntity)
	{
		GD.Print("Player entered door");
		EmitSignal(SignalName.PlayerEntered, Level);
	}
}
