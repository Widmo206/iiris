using Godot;
using System;

public partial class OptionsMenu : CanvasLayer
{
	//Node2D LevelScene;
	int counter = 0;


	public override void _Ready()
	{
		GD.Print("LOADED OptionsMenu.cs");
		GetNode<Button>("anchor1/Options/UnpauseButton").GrabFocus();
	}

	public override void _Process(double delta)
	{
		if (!OS.HasFeature("pc"))
		{
			GetNode<Button>("anchor1/Options/FullscreenButton").Hide();
			GetNode<Button>("anchor1/Options/ExitButton").Hide();
		}

		// workaround, because I can't figure out a better way
		if (Input.IsActionJustPressed("open_settings") && counter > 1)
		{
			closeSettings();
		}

		counter++;
	}


	// this just breaks stuff
	//public override void _Input(InputEvent @event)
	//{
	//	if (@event.IsActionPressed("open_settings"))
	//	{
	//		closeSettings();
	//	}
	//}


	public void closeSettings()
	{
		GetTree().Paused = false;
		GetNode("/root/Global").Set("optionsMenuOpen", (Variant)false);
		QueueFree();
	}


	public void OnUnpauseButtonPressed()
	{
		closeSettings();
	}


	public void OnRestartButtonPressed()
	{
		//GetTree().ChangeSceneToFile("res://scenes/level_2.tscn"); // WTF ??
		GetTree().Paused = false;
		Level level = GetNode<Level>("/root/Level");
		level.ResetLevel();
		closeSettings();
		//LevelScene = GetTree().Root.GetChild<Node2D>(1);
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
