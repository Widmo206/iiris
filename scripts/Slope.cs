using Godot;
using System;
using System.Collections.Generic;


public partial class Slope : Node2D
{
	[Export]
	public Type SlopeType = Type.R_1x2_Frame;


	public enum Type
	{
		R_1x2_Frame,
		L_1x2_Frame,

		R_1x2_Metal_BG,
		L_1x2_Metal_BG,

		R_1x2_Metal_FG,
		L_1x2_Metal_FG,
	}
	private readonly Dictionary<Type, Vector2> Normals = new Dictionary<Type, Vector2>{
		// Frame
		{ Type.R_1x2_Frame,    new Vector2(-1, -2).Normalized() },
		{ Type.L_1x2_Frame,    new Vector2( 1, -2).Normalized() },
		// metal (background)
		{ Type.R_1x2_Metal_BG, new Vector2(-1, -2).Normalized() },
		{ Type.L_1x2_Metal_BG, new Vector2( 1, -2).Normalized() },
		// metal (foreground)
		{ Type.R_1x2_Metal_FG, new Vector2(-1, -2).Normalized() },
		{ Type.L_1x2_Metal_FG, new Vector2( 1, -2).Normalized() },
	};


	public Vector2 getNormalVector()
	{
		return Normals[SlopeType].Rotated(Rotation);
	}


	public override void _Ready()
	{
		// Remove all variants except the one specified by Slopetype
		// this is probably a bad way to do this, but it should be good enough

		var slopeTypes = Enum.GetValues(typeof(Type));
		foreach (Type slopeType in slopeTypes)
		{
			if (slopeType != SlopeType)
			{ 
				GetNode<TileMapLayer>(slopeType.ToString()).Free();
			}
		}
	}

	public override void _Process(double delta)
	{
	}
}
