using Godot;
using System;

public partial class Hud : CanvasLayer
{
	public float FPS = 0.0f;
	public float UPS = 0.0f;


	public void Level(string levelName)
	{
		GetNode<Label>("CurrentLevel").Text = "Level: " + levelName;
	}


	public void Coins(int count)
	{
		GetNode<Label>("CoinCounter").Text = "Coins: " + count.ToString();
	}


	public override void _Process(double delta)
	{
		FPS = 1 / (float)delta;
		// TODO: Figure out how to force a precision of 2 decimal places
		string text = "FPS: " + FPS.ToString() + "\nUPS: " + UPS.ToString();
		GetNode<Label>("FpsCounter").Text = text;
	}


	public override void _PhysicsProcess(double delta)
	{
		UPS = 1 / (float)delta;
	}
}
