using Godot;
using System;

public partial class Global : Node
{
	public int coinsCollected = 0;
	PackedScene OptionsMenu;
	public bool optionsMenuOpen = false;
	public int gameTime = 0; // ticks


	public override void _Ready()
	{
		OptionsMenu = GD.Load<PackedScene>("res://scenes/options_menu.tscn");
		//GD.Print("LOADED Global.cs");
		//GD.Print("Coins collected:", coinsCollected);
	}


	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("open_settings") && GetTree().CurrentScene.Name == "Level")
		{
			GD.Print(optionsMenuOpen);
			if (optionsMenuOpen)
			{
				//GD.Print("options already open");
				// doesn't work (because the tree is paused?)
				GetNode("/root/OptionsMenu").Call("closeSettings");
			}
			else
			{
				//GD.Print("open options worked");
				CanvasLayer instance = (CanvasLayer)OptionsMenu.Instantiate();
				GetTree().Root.AddChild(instance);
				//var lst = GetTree().Root.GetChildren();
				//foreach (var child in lst)
				//{
				//	GD.Print(child.Name);
				//}
				optionsMenuOpen = true;
				GetTree().Paused = true;
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (GetTree().CurrentScene == null) return; // NullReferenceException fix

		if (GetTree().CurrentScene.Name == "MainMenu")
		{
			gameTime = 0;
		}
		else if (!GetTree().Paused && GetTree().CurrentScene.Name == "Level")
		{
			gameTime++;
		}
	}
}
