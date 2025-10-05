using Godot;
using System;

public partial class Collectible : Area2D
{
	Random random = new Random();
	bool isExhausted = false;

	public override void _Ready()
	{
		//GD.Print("LOADED Collectible.cs");
		// this.Connect("CoinCollected", GetNode<Node2D>("Level"), "OnCoinCollected");
	}

	[Signal]
	public delegate void CoinCollectedEventHandler();

	public void OnBodyEntered(Node2D collidingEntity)
	{
		if (isExhausted) return;

		// coin collection is handled by the Level
		//int coinsCollected = (int)GetNode("/root/Global").Get("coinsCollected");
		//coinsCollected++;
		//GetNode("/root/Global").Set("coinsCollected", (Variant)coinsCollected);

		EmitSignal(SignalName.CoinCollected);
		isExhausted = true;
		// Why didn't I add the random pitch earlier? 
		GetNode<AudioStreamPlayer>("CoinPickupSfx").PitchScale = 1f + ((float)random.NextDouble() - 0.5f) * 0.1f;
		GetNode<AudioStreamPlayer>("CoinPickupSfx").Play();
		Hide();
	}

	public void OnCoinPickupSfxFinished()
	{
		QueueFree();
	}


}
