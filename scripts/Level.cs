using Godot;
using System;

public partial class Level : Node2D
{
	[Export]
	public string LevelName { get; set; } = "Lorem ipsum";
	[Export]
	public int VoidHeight { get; set; } = 2400; // lower boundry of the world; the player dies if they cross it


	public int autoReloadTime = 200; // ticks; how long between player death and the level automatically reloading


	Player player;
	int timeSinceDeath = -1; // ticks; how long since the player died
	int coinsThisLevel = 0;  // how many coins were collected on this level


	public override void _Ready()
	{
		GD.Print("LOADED " + LevelName);
		GetNode<CanvasLayer>("HUD").Call("Level", LevelName);
		UpdateCoinCounter();
		foreach (var coin in GetNode<Node>("Collectibles").GetChildren())
		{
			coin.Connect(Collectible.SignalName.CoinCollected, Callable.From(OnCoinCollected));
		}
		player = GetNode<Player>("Player");
	}


	public void OnDoorPlayerEntered(Variant level)
	{
		int coinsCollected = (int)GetNode("/root/Global").Get("coinsCollected");
		GetNode("/root/Global").Set("coinsCollected", (Variant)(coinsCollected + coinsThisLevel));

		GetTree().CallDeferred("change_scene_to_file", level);
	}


	private void OnCoinCollected()
	{
		coinsThisLevel++;
		UpdateCoinCounter();
	}


	private void UpdateCoinCounter()
	{
		int coinsCollected = (int)GetNode("/root/Global").Get("coinsCollected");
		GetNode<CanvasLayer>("HUD").Call("Coins", coinsCollected + coinsThisLevel);
	}


	public void ResetLevel()
	{
		GetTree().CallDeferred("reload_current_scene");
	}


	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("reset_level"))
		{
			ResetLevel();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!player.isAlive) { timeSinceDeath++; }
		if (timeSinceDeath > autoReloadTime)
		{
			ResetLevel();
		}
	}
}
