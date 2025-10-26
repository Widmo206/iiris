using Godot;
using System;

public partial class TheEnd : Node2D
{
	[Export(PropertyHint.File, "*.tscn,")]
	public string NextLevel = null;

	public override void _Ready()
	{
		GetNode<Button>("anchor1/Options/ReturnToMenuButton").GrabFocus();
		GetNode<Label>("anchor2/Playtime").Text = (string)GetNode("/root/Global").Call("getFormattedtime");
		GetNode<Label>("anchor2/CoinsCollected").Text = $"{GetNode("/root/Global").Get("coinsCollected")} Coins";

		Label Credits = GetNode<Label>("anchor0/Credits");
		using var file = FileAccess.Open("res://other/CREDITS.txt", FileAccess.ModeFlags.Read);
		Credits.Text = file.GetAsText();
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
		GetTree().ChangeSceneToFile(NextLevel);
	}

	public void OnExitButtonPressed()
	{
		GetTree().Quit();
	}
}
