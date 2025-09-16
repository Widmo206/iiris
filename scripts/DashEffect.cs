using Godot;
using System;

public partial class DashEffect : Node2D
{
	public const int Duration = 20;

	private int counter = 0;
	private Random random = new Random();
	private Color currentColor;
	private Sprite2D sprite;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		sprite = GetNode<Sprite2D>("Sprite");
		currentColor = sprite.Modulate;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		currentColor.A = 1f - (float)counter / (float)Duration;
		sprite.Modulate = currentColor;
		counter++;
		if (counter >= Duration)
		{ 
			QueueFree();
		}
	}
}
