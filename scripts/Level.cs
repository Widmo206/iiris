using Godot;
using System;

public partial class Level : Node2D
{
	[Export]
	public string LevelName { get; set; } = "Lorem ipsum";


	public override void _Ready()
	{
		GD.Print("LOADED " + LevelName);
		GetNode<CanvasLayer>("HUD").Call("Level", LevelName);
		UpdateCoinCounter();
		foreach (var coin in GetNode<Node>("Collectibles").GetChildren())
		{
			coin.Connect(Collectible.SignalName.CoinCollected, Callable.From(OnCoinCollected));
		}
	}


	public void OnDoorPlayerEntered(Variant level)
	{
		GetTree().CallDeferred("change_scene_to_file", level);
	}


	private void OnCoinCollected()
	{
		UpdateCoinCounter();
	}


	private void UpdateCoinCounter()
	{
		int coinsCollected = (int)GetNode("/root/Global").Get("coinsCollected");
		GetNode<CanvasLayer>("HUD").Call("Coins", coinsCollected);
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

	public override void _Process(double delta)
	{
	}
}
