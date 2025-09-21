using Godot;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public partial class WalljumpDetector : Node2D
{
	public bool canWalljump;
	public enum Direction {
		None,
		Left,
		Right,
		Both
	}
	public Direction direction = Direction.None;

	private Node2D RootScene;
	private TileMapLayer Ground;
	private Area2D Left;
	private Area2D Right;
	private Player player;


	public override void _Ready()
	{
		RootScene = GetTree().Root.GetChild<Node2D>(1);
		Ground = RootScene.GetNode<TileMapLayer>("Terrain/Ground");
		Left = GetNode<Area2D>("Left");
		Right = GetNode<Area2D>("Right");
		player = GetNode<Player>("..");
	}


	private bool CheckSide(Area2D side)
	{
		AnimatedSprite2D debugSprite = side.GetNode<AnimatedSprite2D>("Collider/DebugIndicator");
		if (side.GetOverlappingBodies().Count() > 0)
		{
			// Touching wall
			if (player.isGrounded)
			{
				debugSprite.Play("not_in_air");
				return false;
			}
			else
			{
				debugSprite.Play("valid");
				return true;
			}

		}
		else
		{
			// Not touching wall
			debugSprite.Play("no_wall");
			return false;
		}
	}


	public void CheckWalljump()
	{
		canWalljump = false;
		direction = Direction.None;

		if (CheckSide(Left))
		{
			canWalljump = true;
			direction = Direction.Left;
		}

		if (CheckSide(Right))
		{
			if (direction == Direction.None)
			{
				canWalljump = true;
				direction = Direction.Right;
			}
			else
			{
				direction = Direction.Both;
			}
		}
	}
}
