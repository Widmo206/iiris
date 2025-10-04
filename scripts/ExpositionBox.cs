using Godot;
using System;
using System.Diagnostics.Metrics;

public partial class ExpositionBox : Control
{
	[Export(PropertyHint.MultilineText)]
	public string Text = "Missing text";
	[Export]
	public int Delay = 0;			// ticks; time until this EBox is shown
	[Export]
	public int FadeinTime = 0;		// ticks; how long does it take forthe paragraph to fade in

	Label Paragraph;

	int counter = 0;


	public override void _Ready()
	{
		Paragraph = GetNode<Label>("Paragraph");
		Paragraph.Visible = false;
		Paragraph.Text = Text;
	}

	public override void _PhysicsProcess(double delta)
	{
		counter++;
		if (counter > Delay)
		{
			Paragraph.Visible = true;
			if (FadeinTime <= 0)
			{
				return;
			}
			else
			{
				float fadeinProgress = (float)(counter - Delay) / (float)FadeinTime;
				Paragraph.Modulate = new Color(1f, 1f, 1f, fadeinProgress);
			}
		}
	}
}
