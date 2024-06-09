using Godot;
using GodotVoxelTutorial.Scripts;
using System.Collections.Generic;

public partial class VoxelWorld : Node3D
{
	Dictionary<string, Voxel> voxelDictionary = new(){
			{
				"Stone",
				new Voxel() {
					IsTransparent = false,
					IsSolid = true,
					texture = new Vector2I(0,0),
					Textures = new()
				}
			},
			{
				"Bedrock",
				new Voxel() {
					IsTransparent = false,
					IsSolid = true,
					texture = new Vector2I(2,0),
					Textures = new()
				}
			},
			{
				"Cobble",
				new Voxel() {
					IsTransparent = false,
					IsSolid = true,
					texture = new Vector2I(1,0),
					Textures = new()
				}
			},
			{
				"Dirt",
				new Voxel() {
					IsTransparent = false,
					IsSolid = true,
					texture = new Vector2I(0,1),
					Textures = new()
				}
			},
			{
				"Grass",
				new Voxel() {
					IsTransparent = false,
					IsSolid = true,
					texture = new Vector2I(0,1),
					Textures =
					new() {
						{
							VoxelSide.TOP, new Vector2I(2,1)
						},
						{
							VoxelSide.NORTH, new Vector2I(1,1)
						},
						{
							VoxelSide.SOUTH, new Vector2I(1,1)
						},
						{
							VoxelSide.EAST, new Vector2I(1,1)
						},
						{
							VoxelSide.WEST, new Vector2I(1,1)
						}
					}
				}
			}
		};

	List<string> _voxelList = new();

	[Export]
	public int VoxelTextureSize = 96;
	[Export]
	public int VoxelTextureTileSize = 32;

	public float VoxelTextureUnit;

	PackedScene ChunkScene = GD.Load<PackedScene>("res://Voxel_Terrain_System/Voxel_Chunk.tscn");

	Node3D _chunkHolderNode;

	int VOXEL_UNIT_SIZE = 1;



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_chunkHolderNode = (Node3D)GetNode("Chunks");

		VoxelTextureUnit = 1.0f / (VoxelTextureSize / VoxelTextureTileSize);

		foreach (var voxel_name in voxelDictionary.Keys)
		{
			_voxelList.Add(voxel_name);
		}

		MakeVoxelWorld(new Vector3I(4, 1, 4), new Vector3I(16, 16, 16));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void MakeVoxelWorld(Vector3I WorldSize, Vector3I ChunkSize)
	{
		foreach (var child in _chunkHolderNode.GetChildren())
		{
			child.QueueFree();
		}

		for (int x = 0; x < WorldSize.X; x++)
		{
			for (int y = 0; y < WorldSize.Y; y++)
			{
				for (int z = 0; z < WorldSize.Z; z++)
				{
					VoxelChunk newChunk = (VoxelChunk)ChunkScene.Instantiate();
					_chunkHolderNode.AddChild(newChunk);
					
					var transform = newChunk.GlobalTransform;
					transform.Origin = new Vector3(
							x * ChunkSize.X * VOXEL_UNIT_SIZE,
							y * ChunkSize.Y * VOXEL_UNIT_SIZE,
							z * ChunkSize.Z * VOXEL_UNIT_SIZE
						);
					newChunk.GlobalTransform = transform;

					newChunk.World = this;

					newChunk.Setup(ChunkSize.X, ChunkSize.Y, ChunkSize.Z, VOXEL_UNIT_SIZE);
				}
			}
		}
	}

	public bool TryGetVoxelByName(string voxelName, out Voxel? voxel)
	{

		if (voxelDictionary.ContainsKey(voxelName))
		{
			voxel = voxelDictionary[voxelName];
			return true;
		}
		voxel = null;
		return false;
	}

	public Voxel GetVoxelByIndex(int index)
	{
		return voxelDictionary[_voxelList[index]];
	}

	public int GetVoxelIndex(string voxelName)
	{
		return _voxelList.IndexOf(voxelName);
	}

	public void SetWorldVoxel(Vector3I position, int voxel)
	{
		foreach (var chunk in _chunkHolderNode.GetChildren())
		{
			VoxelChunk voxelChunk = (VoxelChunk)chunk;
			if (voxelChunk.SetVoxelAtPosition(position, voxel))
			{
				break;
			}
		}
	}

}
