using Godot;
using System;

public partial class Sign : Node2D
{
	Area2D PlayerDetector;
	Node2D Message;


	public override void _Ready()
	{
		PlayerDetector = GetNode<Area2D>("PlayerDetector");
		Message = GetNode<Node2D>("Message");
		Message.Visible = false;
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
