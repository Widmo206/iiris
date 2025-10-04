using Godot;
using System;
using System.Reflection.Emit;

public partial class Cutscene : CanvasLayer
{
	[Export]
	public string NextLevel = "";

	Control Page1;
	Control Page2;

	bool isFirstPage = true;
	bool justSkippedPage = false; // whether the previous page was skipped; -> you need to release the key to skip the next page


	public override void _Ready()
	{
		Page1 = GetNode<Control>("Page1");
		Page2 = GetNode<Control>("Page2");

		Page2.ProcessMode = ProcessModeEnum.Disabled;
		Page2.Visible = false;
	}

	public override void _Process(double delta)
	{
		if (Input.IsAnythingPressed() && !justSkippedPage)
		{
			if (isFirstPage)
			{
				Page1.QueueFree();
				Page2.Visible = true;
				Page2.ProcessMode = ProcessModeEnum.Inherit;
				isFirstPage = false;
				justSkippedPage = true;
			}
			else
			{
				GetTree().CallDeferred("change_scene_to_file", NextLevel);
			}
		}
		else if (!Input.IsAnythingPressed())
		{
			justSkippedPage = false;
		}
	}
}
