using Godot;
using GodotVoxelTutorial.Scripts;
using System.Collections.Generic;

public partial class VoxelWorld : Node3D
{
    Dictionary<string, Voxel> voxelDictionary = new(){
        {
            "Stone",
            new Voxel() {
                transparent = false,
                solid = true,
                texture = new Vector2I(0,0)
            }
        },
        {
            "Bedrock",
            new Voxel() {
                transparent = false,
                solid = true,
                texture = new Vector2I(2,0)
            }
        },
        {
            "Cobble",
            new Voxel() {
                transparent = false,
                solid = true,
                texture = new Vector2I(1,0)
            }
        },
        {
            "Dirt",
            new Voxel() {
                transparent = false,
                solid = true,
                texture = new Vector2I(0,1)
            }
        },
        {
            "Grass",
            new Voxel() {
                transparent = false,
                solid = true,
                texture = new Vector2I(0,1),
                textureTop = new Vector2I(2,1),
                textureNorth = new Vector2I(1,1),
                textureSouth = new Vector2I(1,1),
                textureEast = new Vector2I(1,1),
                textureWest = new Vector2I(1,1)
            }
        }
    };

    List<string> _voxelList = new();

    [Export]
    public int VoxelTextureSize = 96;
    [Export]
    public int VoxelTextureTileSize = 32;

    float _voxelTextureUnit;

    private static ResourcePreloader _rpl;
    private static ResourcePreloader RPL
    {
        get
        {
            _rpl ??= new ResourcePreloader();
            return _rpl;
        }
    }

    PackedScene ChunkScene = (PackedScene)RPL.GetResource("res://Voxel_Terrain_System/Voxel_Chunk.tscn");

    Node3D _chunkHolderNode;

    int VOXEL_UNIT_SIZE = 1;



    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        _chunkHolderNode = (Node3D)GetNode("Chunks");

        _voxelTextureUnit = 1.0f / (VoxelTextureSize / VoxelTextureTileSize);

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

                    Transform3D newTransform = new()
                    {
                        Origin = new Vector3(
                            x * ChunkSize.X * VOXEL_UNIT_SIZE,
                            y * ChunkSize.Y * VOXEL_UNIT_SIZE,
                            z * ChunkSize.Z * VOXEL_UNIT_SIZE
                        )
                    };

                    newChunk.GlobalTransform = newTransform;
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

    public void SetWorldVoxel(Vector3I position, Voxel voxel)
    {
        foreach (var chunk in _chunkHolderNode.GetChildren())
        {
            if (chunk.SetVoxelAtPosition(position, voxel))
            {
                break;
            }
        }
    }

}
