using Godot;
using System;

public partial class Sign : Node2D
{
	Area2D PlayerDetector;
	Node2D Message;
	Label Text;


	[Export(PropertyHint.MultilineText)]
	public string TextOverride { get; set; }


	public override void _Ready()
	{
		PlayerDetector = GetNode<Area2D>("PlayerDetector");
		Message = GetNode<Node2D>("Message");
		Text = GetNode<Label>("Message/Text");
		Message.Visible = false;
		if (TextOverride != "") 
		{
			Text.Text = TextOverride; // workaround; I can't find a way to expose the property directly
		}
		
	}


	public void OnPlayerEntered(Node2D body)
	{
		Message.Visible = true;
	}

	public void OnPlayerLeft(Node2D body)
	{
		Message.Visible = false;
	}
}
