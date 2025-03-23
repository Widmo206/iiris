using Godot;
using System;
using static Godot.OpenXRInterface;

public partial class Collectible : Area2D
{
	public override void _Ready()
	{
		GD.Print("LOADED Collectible.cs");
		// this.Connect("CoinCollected", GetNode<Node2D>("Level"), "OnCoinCollected");
	}

	[Signal]
	public delegate void CoinCollectedEventHandler();

	public void OnBodyEntered()
	{
		int coinsCollected = (int)GetNode("/root/Global").Get("coins_collected");
		coinsCollected++;
		GetNode("/root/Global").Set("coins_collected", (Variant)coinsCollected);

		var rand = new Random();
		// Why didn't I add the random pitch earlier? 
		GetNode<AudioStreamPlayer>("CoinPickupSfx").PitchScale = 1.0f + ((float)rand.NextDouble() - 0.5f) * 0.1f;
		GetNode<AudioStreamPlayer>("CoinPickupSfx").Play();
		EmitSignal(SignalName.CoinCollected);
		Hide();
	}

	public void OnCoinPickupSfxFinished()
	{
		QueueFree();
	}


}
