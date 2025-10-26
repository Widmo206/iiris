using Godot;
using System;

public partial class Fishtank : StaticBody2D
{
	public enum Type
	{
		Empty,
		Filled,
		Occupied,
	}
	// I probably could have added the broken version in here too,
	// but that seems like more of a pain, since I'd have to swap colliders too,
	// and I need to make sure the back layer is actually behind the player when they'll be inside...

	[Export]
	public Type TankType { get; set; } = Type.Filled;

	private Sprite2D Fluid;
	private AnimatedSprite2D Bubbles;
	private AnimatedSprite2D Shadow;

	public override void _Ready()
	{
		Fluid	= GetNode<Sprite2D>("Fluid");
		Bubbles	= GetNode<AnimatedSprite2D>("Bubbles");
		Shadow	= GetNode<AnimatedSprite2D>("Shadow");

		if (TankType > Type.Empty) // I love that this just works (enum elements are just integers)
		{
			Fluid.Visible = true;
		}
		else
		{
			Fluid.Visible = false;
		}

		if (TankType > Type.Filled)
		{
			Bubbles.Visible = true;
			Bubbles.Play("default");
			Shadow.Visible = true;
			Shadow.Play("default");
		}
		else
		{
			Bubbles.Visible = false;
			Shadow.Visible = false;
		}
	}

	public override void _Process(double delta)
	{

	}
}
