using Godot;
using System;

public partial class MainMenu : Node2D
{
	public override void _Ready()
	{
		GD.Print("LOADED MainMenu.cs");
		GetNode<Button>("Options/StartButton").GrabFocus();
	}

	public override void _Process(double delta)
	{
		if (!OS.HasFeature("pc"))
		{
			GetNode<Button>("Options/FullscreenButton").Hide();
			GetNode<Button>("Options/ExitButton").Hide();
		}
	}

	public void OnStartButtonPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/level_1.tscn");
	}

	public void OnFullscreenButtonPressed()
	{
		if (DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen)
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		}
		else
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		}
	}

	public void OnExitButtonPressed()
	{
		GetTree().Quit();
	}
}
