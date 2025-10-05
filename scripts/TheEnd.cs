using Godot;
using System;

public partial class TheEnd : Node2D
{
	public override void _Ready()
	{
		GetNode<Button>("anchor1/Options/ReturnToMenuButton").GrabFocus();
		GetNode<Label>("anchor2/Playtime").Text = (string)GetNode("/root/Global").Call("getFormattedtime");
		GetNode<Label>("anchor2/CoinsCollected").Text = $"{GetNode("/root/Global").Get("coinsCollected")} Coins";
	}

	public override void _Process(double delta)
	{
		if (!OS.HasFeature("pc"))
		{
			GetNode<Button>("anchor1/Options/ExitButton").Hide();
		}
	}

	public void OnReturnToMenuButtonPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
	}

	public void OnExitButtonPressed()
	{
		GetTree().Quit();
	}
}
