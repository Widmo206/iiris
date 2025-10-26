using Godot;
using System;

public partial class MainMenu : Node2D
{
	[Export(PropertyHint.File, "*.tscn,")]
	public string NextLevel { get; set; } = null;


	public override void _Ready()
	{
		GD.Print("LOADED MainMenu.cs");
		GetNode<Button>("anchor1/Options/StartButton").GrabFocus();

		Label Credits = GetNode<Label>("anchor0/Credits");
		using var file = FileAccess.Open("res://other/CREDITS.txt", FileAccess.ModeFlags.Read);
		Credits.Text = file.GetAsText();
	}

	public override void _Process(double delta)
	{
		// wait, why isn't this in _Ready() ?
		if (!OS.HasFeature("pc"))
		{
			GetNode<Button>("anchor1/Options/FullscreenButton").Hide();
			GetNode<Button>("anchor1/Options/ExitButton").Hide();
		}
	}

	public void OnStartButtonPressed()
	{
		GetTree().ChangeSceneToFile(NextLevel);
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
