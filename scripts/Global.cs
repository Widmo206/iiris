using Godot;
using System;

public partial class Global : Node
{
	public int coinsCollected = 0;
	
	public override void _Ready()
	{
		//GD.Print("LOADED Global.cs");
		//GD.Print("Coins collected:", coinsCollected);
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("return_to_main_menu"))
		{
			GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
		}
	}

	public override void _Process(double delta)
	{

	}
}
