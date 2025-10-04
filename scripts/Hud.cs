using Godot;
using System;

public partial class Hud : CanvasLayer
{
	Player player;
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


	public string getTime()
	{
		int timerTicks = (int)GetNode<Node>("/root/Global").Get("gameTime");

		int hours = timerTicks / 72000;
		timerTicks = timerTicks % 72000;

		int minutes = timerTicks / 1200;
		timerTicks = timerTicks % 1200;

		int seconds = timerTicks / 20;
		timerTicks = timerTicks % 20;

		int ms = timerTicks * 50;

		return $"{hours}:{minutes.ToString().PadLeft(2, '0')}:{seconds.ToString().PadLeft(2, '0')}.{ms.ToString().PadLeft(3, '0')}";
	}


	public override void _Ready()
	{
		player = GetNode<Player>("/root/Level/Player");
	}


	private void updateHUD()
	{
		// update debug overlay
		string text = "FPS: " + Mathf.Round(FPS).ToString() + "\nUPS: " + Mathf.Round(UPS).ToString();
		GetNode<Label>("topright/FpsCounter").Text = text;
		GetNode<Label>("topright/Timer").Text = getTime();

		GetNode<Label>("PositionDisplay").Text = $"Position:\n    x: {player.Position.X}\n    y: {player.Position.Y}";
		GetNode<Label>("VelocityDisplay").Text = $"Velocity:\n    x: {player.Velocity.X}\n    y: {player.Velocity.Y}";
		GetNode<Label>("StateDisplay").Text = $"State: {player.currentState}\nstateLockCountdown: {player.stateLockCountdown.ToString()}";
	}


	public override void _Process(double delta)
	{
		FPS = 1 / (float)delta;

		updateHUD();
	}


	public override void _PhysicsProcess(double delta) { UPS = 1 / (float)delta; }
}
