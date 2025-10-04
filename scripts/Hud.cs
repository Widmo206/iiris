using Godot;
using System;

public partial class Hud : CanvasLayer
{
	public float FPS = 0f;
	public float UPS = 0f;


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
		string text = "FPS: " + Mathf.Round(FPS).ToString() + "\nUPS: " + Mathf.Round(UPS).ToString();
		GetNode<Label>("topright/FpsCounter").Text = text;
	}


	public override void _PhysicsProcess(double delta)
	{
		UPS = 1 / (float)delta;

		int timerTicks = (int)GetNode<Node>("/root/Global").Get("gameTime");

		int hours = timerTicks / 72000;
		timerTicks = timerTicks % 72000;

		int minutes = timerTicks / 1200;
		timerTicks = timerTicks % 1200;

		int seconds = timerTicks / 20;
		timerTicks = timerTicks % 20;

		int ms = timerTicks * 50;

		string humanTime = $"{hours}:{minutes.ToString().PadLeft(2, '0')}:{seconds.ToString().PadLeft(2, '0')}.{ms.ToString().PadLeft(3, '0')}";
		GetNode<Label>("topright/Timer").Text = humanTime;
	}
}
