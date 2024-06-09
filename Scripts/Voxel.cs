using Godot;
using Godot.Collections;

namespace GodotVoxelTutorial.Scripts
{
	public struct Voxel
	{
		public bool IsTransparent;
		public bool IsSolid;
		public Vector2I texture;
		public Dictionary<VoxelSide, Vector2I> Textures;
	}

	public enum VoxelSide
	{
		TOP,
		BOTTOM, 
		NORTH, 
		SOUTH, 
		EAST,
		WEST
	}
}
