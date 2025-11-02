using Godot;
using System;

public partial class Collectible : Area2D
{
	[Export]
	public bool StartHidden { get; set; } = false;

	[Signal]
	public delegate void CoinCollectedEventHandler();

	Random random = new Random();
	bool isExhausted = false;

	public override void _Ready()
	{
		//GD.Print("LOADED Collectible.cs");
		// this.Connect("CoinCollected", GetNode<Node2D>("Level"), "OnCoinCollected");
		if (StartHidden)
		{
			Visible = false;
		}
	}


	public void OnBodyEntered(Node2D collidingEntity)
	{
		if (isExhausted) return;

		// coin collection is handled by the Level
		//int coinsCollected = (int)GetNode("/root/Global").Get("coinsCollected");
		//coinsCollected++;
		//GetNode("/root/Global").Set("coinsCollected", (Variant)coinsCollected);

		if (StartHidden)
		{
			Show();
		}
		else
		{
			Hide();
		}

			EmitSignal(SignalName.CoinCollected);
		isExhausted = true;
		// Why didn't I add the random pitch earlier? 
		GetNode<AudioStreamPlayer>("CoinPickupSfx").PitchScale = 1f + ((float)random.NextDouble() - 0.5f) * 0.1f;
		GetNode<AudioStreamPlayer>("CoinPickupSfx").Play();
	}

	public override void _Process(double delta)
	{
		if (StartHidden && isExhausted)
		{
			Color modulate = Modulate;
			modulate.A += 0.1f;
			Modulate = modulate;
		}
	}

	public void OnCoinPickupSfxFinished()
	{
		QueueFree();
	}


}
