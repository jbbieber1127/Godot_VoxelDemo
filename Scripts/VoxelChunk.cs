using Godot;
using GodotVoxelTutorial.Scripts;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class VoxelChunk : Node3D
{
	public VoxelWorld World { get; set; }


	List<List<List<int?>>> voxels;

	int _chunkSizeX;
	int _chunkSizeY;
	int _chunkSizeZ;

	int _voxelSize = 1;

	ArrayMesh _renderMesh;
	List<Vector3> _renderMeshVertices;
	List<Vector3> _renderMeshNormals;
	List<int> _renderMeshIndices;
	List<Vector2> _renderMeshUvs;

	ArrayMesh _collisionMesh;
	List<Vector3> _collisionMeshVertices;
	List<int> _collisionMeshIndices;

	MeshInstance3D _meshInstance;
	CollisionShape3D _collisionShape;

	SurfaceTool surface_tool;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_meshInstance = (MeshInstance3D)GetNode("MeshInstance");
		_collisionShape = (CollisionShape3D)GetNode("StaticBody/CollisionShape");

		surface_tool = new SurfaceTool();
	}

	public void Setup(int chunkSizeX, int chunkSizeY, int chunkSizeZ, int voxelSize)
	{
		_chunkSizeX = chunkSizeX;
		_chunkSizeY = chunkSizeY;
		_chunkSizeZ = chunkSizeZ;
		_voxelSize = voxelSize;

		voxels = new();
		for (int x = 0; x < _chunkSizeX; x++)
		{
			List<List<int?>> row = new();

			for (int y = 0; y < _chunkSizeY; y++)
			{
				List<int?> column = new();

				for (int z = 0; z < _chunkSizeZ; z++)
				{
					column.Add(null);
				}

				row.Add(column);
			}
			voxels.Add(row);
		}

		MakeStarterTerrain();
	}

	public void MakeStarterTerrain()
	{

		for (int x = 0; x < _chunkSizeX; x++)
		{

			for (int y = 0; y < _chunkSizeY / 2; y++)
			{

				for (int z = 0; z < _chunkSizeZ; z++)
				{
					if (y + 1 == _chunkSizeY / 2)
					{
						voxels[x][y][z] = World.GetVoxelIndex("Grass");
					}
					else if (y >= _chunkSizeY / 4)
					{
						voxels[x][y][z] = World.GetVoxelIndex("Dirt");
					}
					else if (y == 0)
					{
						voxels[x][y][z] = World.GetVoxelIndex("Bedrock");
					}
					else
					{
						voxels[x][y][z] = World.GetVoxelIndex("Stone");
					}
				}
			}
		}
		UpdateMesh();
	}


	private void UpdateMesh()
	{
		_renderMeshVertices = new();
		_renderMeshNormals = new();
		_renderMeshIndices = new();
		_renderMeshUvs = new();
		_collisionMeshVertices = new();
		_collisionMeshIndices = new();

		for (int x = 0; x < _chunkSizeX; x++)
		{
			for (int y = 0; y < _chunkSizeY; y++)
			{
				for (int z = 0; z < _chunkSizeZ; z++)
				{
					MakeVoxel(x, y, z);
				}
			}
		}

		//# Make the render mesh
		//# ********************
		surface_tool.Clear();
		surface_tool.Begin(Mesh.PrimitiveType.Triangles);

		for (int i = 0; i < _renderMeshVertices.Count; i++)
		{
			surface_tool.SetNormal(_renderMeshNormals[i]);
			surface_tool.SetUV(_renderMeshUvs[i]);
			surface_tool.AddVertex(_renderMeshVertices[i]);
		}

		for (int i = 0; i < _renderMeshIndices.Count; i++)
		{
			surface_tool.AddIndex(_renderMeshIndices[i]);
		}

		surface_tool.GenerateTangents();

		_renderMesh = surface_tool.Commit();
		_meshInstance.Mesh = _renderMesh;
		//# ********************
		//# Make the collision mesh
		//# ********************
		surface_tool.Clear();
		surface_tool.Begin(Mesh.PrimitiveType.Triangles);

		for (int i = 0; i < _collisionMeshVertices.Count; i++)
		{
			surface_tool.AddVertex(_collisionMeshVertices[i]);
		}

		for (int i = 0; i < _collisionMeshIndices.Count; i++)
		{
			surface_tool.AddIndex(_collisionMeshIndices[i]);
		}

		_collisionMesh = surface_tool.Commit();
		_collisionShape.Shape = _collisionMesh.CreateTrimeshShape();
	}

	private void MakeVoxel(int x, int y, int z)
	{
		if (voxels[x][y][z] == null || voxels[x][y][z] == -1)
		{
			return;
		}

		if (_getVoxelInBounds(x, y + 1, z))
		{
			if (_checkIfVoxelCausesRender(x, y + 1, z))
			{
				MakeVoxelFace(x, y, z, VoxelSide.TOP);
			}
		}
		else
		{
			MakeVoxelFace(x, y, z, VoxelSide.TOP);
		}

		if (_getVoxelInBounds(x, y - 1, z))
		{
			if (_checkIfVoxelCausesRender(x, y - 1, z))
			{
				MakeVoxelFace(x, y, z, VoxelSide.BOTTOM);
			}
		}
		else
		{
			MakeVoxelFace(x, y, z, VoxelSide.BOTTOM);
		}

		if (_getVoxelInBounds(x + 1, y, z))
		{
			if (_checkIfVoxelCausesRender(x + 1, y, z))
			{
				MakeVoxelFace(x, y, z, VoxelSide.EAST);
			}
		}
		else
		{
			MakeVoxelFace(x, y, z, VoxelSide.EAST);
		}

		if (_getVoxelInBounds(x - 1, y, z))
		{
			if (_checkIfVoxelCausesRender(x - 1, y, z))
			{
				MakeVoxelFace(x, y, z, VoxelSide.WEST);
			}
		}
		else
		{
			MakeVoxelFace(x, y, z, VoxelSide.WEST);
		}

		if (_getVoxelInBounds(x, y, z + 1))
		{
			if (_checkIfVoxelCausesRender(x, y, z + 1))
			{
				MakeVoxelFace(x, y, z, VoxelSide.NORTH);
			}
		}
		else
		{
			MakeVoxelFace(x, y, z, VoxelSide.NORTH);
		}

		if (_getVoxelInBounds(x, y, z - 1))
		{
			if (_checkIfVoxelCausesRender(x, y, z - 1))
			{
				MakeVoxelFace(x, y, z, VoxelSide.SOUTH);
			}
		}
		else
		{
			MakeVoxelFace(x, y, z, VoxelSide.SOUTH);
		}
	}

	private bool _checkIfVoxelCausesRender(int x, int y, int z)
	{
		if (voxels[x][y][z] == null || voxels[x][y][z] == -1)
		{
			return true;
		}
		else
		{
			var tmp_voxel_data = World.GetVoxelByIndex(voxels[x][y][z].Value);
			if (tmp_voxel_data.IsTransparent || !tmp_voxel_data.IsSolid)
			{
				return true;
			}
		}

		return false;
	}

	private void MakeVoxelFace(int x, int y, int z, VoxelSide face)
	{

		var voxel_data = World.GetVoxelByIndex(voxels[x][y][z].Value);

		var uv_position = voxel_data.texture;

		x = x * _voxelSize;
		y = y * _voxelSize;
		z = z * _voxelSize;

		if (voxel_data.Textures.ContainsKey(face))
		{
			uv_position = voxel_data.Textures[face];
		}

		switch (face)
		{
			case VoxelSide.TOP:
				_makeVoxelFaceTop(x, y, z, voxel_data);
				break;
			case VoxelSide.BOTTOM:
				_makeVoxelFaceBottom(x, y, z, voxel_data);
				break;
			case VoxelSide.NORTH:
				_makeVoxelFaceNorth(x, y, z, voxel_data);
				break;
			case VoxelSide.SOUTH:
				_makeVoxelFaceSouth(x, y, z, voxel_data);
				break;
			case VoxelSide.EAST:
				_makeVoxelFaceEast(x, y, z, voxel_data);
				break;
			case VoxelSide.WEST:
				_makeVoxelFaceWest(x, y, z, voxel_data);
				break;
			default:
				Debug.WriteLine("ERROR: Unknown face: " + face);
				return;
		}

		var v_texture_unit = World.VoxelTextureUnit;
		_renderMeshUvs.Add(new Vector2((v_texture_unit * uv_position.X), (v_texture_unit * uv_position.Y) + v_texture_unit));
		_renderMeshUvs.Add(new Vector2((v_texture_unit * uv_position.X) + v_texture_unit, (v_texture_unit * uv_position.Y) + v_texture_unit));
		_renderMeshUvs.Add(new Vector2((v_texture_unit * uv_position.X) + v_texture_unit, (v_texture_unit * uv_position.Y)));
		_renderMeshUvs.Add(new Vector2((v_texture_unit * uv_position.X), (v_texture_unit * uv_position.Y)));

		_renderMeshIndices.Add(_renderMeshVertices.Count - 4);
		_renderMeshIndices.Add(_renderMeshVertices.Count - 3);
		_renderMeshIndices.Add(_renderMeshVertices.Count - 1);
		_renderMeshIndices.Add(_renderMeshVertices.Count - 3);
		_renderMeshIndices.Add(_renderMeshVertices.Count - 2);
		_renderMeshIndices.Add(_renderMeshVertices.Count - 1);

		if (voxel_data.IsSolid)
		{
			_collisionMeshIndices.Add(_renderMeshVertices.Count - 4);
			_collisionMeshIndices.Add(_renderMeshVertices.Count - 3);
			_collisionMeshIndices.Add(_renderMeshVertices.Count - 1);
			_collisionMeshIndices.Add(_renderMeshVertices.Count - 3);
			_collisionMeshIndices.Add(_renderMeshVertices.Count - 2);
			_collisionMeshIndices.Add(_renderMeshVertices.Count - 1);
		}
	}

	private void _makeVoxelFaceTop(int x, int y, int z, Voxel voxel_data)
	{
		_renderMeshVertices.Add(new Vector3(x, y + _voxelSize, z));
		_renderMeshVertices.Add(new Vector3(x + _voxelSize, y + _voxelSize, z));
		_renderMeshVertices.Add(new Vector3(x + _voxelSize, y + _voxelSize, z + _voxelSize));
		_renderMeshVertices.Add(new Vector3(x, y + _voxelSize, z + _voxelSize));

		_renderMeshNormals.Add(new Vector3(0, 1, 0));
		_renderMeshNormals.Add(new Vector3(0, 1, 0));
		_renderMeshNormals.Add(new Vector3(0, 1, 0));
		_renderMeshNormals.Add(new Vector3(0, 1, 0));

		if (voxel_data.IsSolid)
		{
			_collisionMeshVertices.Add(new Vector3(x, y + _voxelSize, z + _voxelSize));
			_collisionMeshVertices.Add(new Vector3(x + _voxelSize, y + _voxelSize, z + _voxelSize));
			_collisionMeshVertices.Add(new Vector3(x + _voxelSize, y + _voxelSize, z));
			_collisionMeshVertices.Add(new Vector3(x, y + _voxelSize, z));
		}
	}

	private void _makeVoxelFaceBottom(int x, int y, int z, Voxel voxel_data)
	{
		_renderMeshVertices.Add(new Vector3(x, y, z + _voxelSize));
		_renderMeshVertices.Add(new Vector3(x + _voxelSize, y, z + _voxelSize));
		_renderMeshVertices.Add(new Vector3(x + _voxelSize, y, z));
		_renderMeshVertices.Add(new Vector3(x, y, z));

		_renderMeshNormals.Add(new Vector3(0, -1, 0));
		_renderMeshNormals.Add(new Vector3(0, -1, 0));
		_renderMeshNormals.Add(new Vector3(0, -1, 0));
		_renderMeshNormals.Add(new Vector3(0, -1, 0));

		if (voxel_data.IsSolid)
		{
			_collisionMeshVertices.Add(new Vector3(x, y, z + _voxelSize));
			_collisionMeshVertices.Add(new Vector3(x + _voxelSize, y, z + _voxelSize));
			_collisionMeshVertices.Add(new Vector3(x + _voxelSize, y, z));
			_collisionMeshVertices.Add(new Vector3(x, y, z));
		}
	}

	private void _makeVoxelFaceNorth(int x, int y, int z, Voxel voxel_data)
	{
		_renderMeshVertices.Add(new Vector3(x + _voxelSize, y, z + _voxelSize));
		_renderMeshVertices.Add(new Vector3(x, y, z + _voxelSize));
		_renderMeshVertices.Add(new Vector3(x, y + _voxelSize, z + _voxelSize));
		_renderMeshVertices.Add(new Vector3(x + _voxelSize, y + _voxelSize, z + _voxelSize));

		_renderMeshNormals.Add(new Vector3(0, 0, 1));
		_renderMeshNormals.Add(new Vector3(0, 0, 1));
		_renderMeshNormals.Add(new Vector3(0, 0, 1));
		_renderMeshNormals.Add(new Vector3(0, 0, 1));

		if (voxel_data.IsSolid)
		{
			_collisionMeshVertices.Add(new Vector3(x, y, z + _voxelSize));
			_collisionMeshVertices.Add(new Vector3(x + _voxelSize, y, z + _voxelSize));
			_collisionMeshVertices.Add(new Vector3(x + _voxelSize, y + _voxelSize, z + _voxelSize));
			_collisionMeshVertices.Add(new Vector3(x, y + _voxelSize, z + _voxelSize));
		}
	}

	private void _makeVoxelFaceSouth(int x, int y, int z, Voxel voxel_data)
	{
		_renderMeshVertices.Add(new Vector3(x, y, z));
		_renderMeshVertices.Add(new Vector3(x + _voxelSize, y, z));
		_renderMeshVertices.Add(new Vector3(x + _voxelSize, y + _voxelSize, z));
		_renderMeshVertices.Add(new Vector3(x, y + _voxelSize, z));

		_renderMeshNormals.Add(new Vector3(0, 0, -1));
		_renderMeshNormals.Add(new Vector3(0, 0, -1));
		_renderMeshNormals.Add(new Vector3(0, 0, -1));
		_renderMeshNormals.Add(new Vector3(0, 0, -1));

		if (voxel_data.IsSolid)
		{

			_collisionMeshVertices.Add(new Vector3(x, y, z));
			_collisionMeshVertices.Add(new Vector3(x + _voxelSize, y, z));
			_collisionMeshVertices.Add(new Vector3(x + _voxelSize, y + _voxelSize, z));
			_collisionMeshVertices.Add(new Vector3(x, y + _voxelSize, z));
		}
	}

	private void _makeVoxelFaceEast(int x, int y, int z, Voxel voxel_data)
	{
		_renderMeshVertices.Add(new Vector3(x + _voxelSize, y, z));
		_renderMeshVertices.Add(new Vector3(x + _voxelSize, y, z + _voxelSize));
		_renderMeshVertices.Add(new Vector3(x + _voxelSize, y + _voxelSize, z + _voxelSize));
		_renderMeshVertices.Add(new Vector3(x + _voxelSize, y + _voxelSize, z));

		_renderMeshNormals.Add(new Vector3(1, 0, 0));
		_renderMeshNormals.Add(new Vector3(1, 0, 0));
		_renderMeshNormals.Add(new Vector3(1, 0, 0));
		_renderMeshNormals.Add(new Vector3(1, 0, 0));

		if (voxel_data.IsSolid)
		{
			_collisionMeshVertices.Add(new Vector3(x + _voxelSize, y, z + _voxelSize));
			_collisionMeshVertices.Add(new Vector3(x + _voxelSize, y, z));
			_collisionMeshVertices.Add(new Vector3(x + _voxelSize, y + _voxelSize, z));
			_collisionMeshVertices.Add(new Vector3(x + _voxelSize, y + _voxelSize, z + _voxelSize));
		}
	}

	private void _makeVoxelFaceWest(int x, int y, int z, Voxel voxel_data)
	{
		_renderMeshVertices.Add(new Vector3(x, y, z + _voxelSize));
		_renderMeshVertices.Add(new Vector3(x, y, z));
		_renderMeshVertices.Add(new Vector3(x, y + _voxelSize, z));
		_renderMeshVertices.Add(new Vector3(x, y + _voxelSize, z + _voxelSize));

		_renderMeshNormals.Add(new Vector3(-1, 0, 0));
		_renderMeshNormals.Add(new Vector3(-1, 0, 0));
		_renderMeshNormals.Add(new Vector3(-1, 0, 0));
		_renderMeshNormals.Add(new Vector3(-1, 0, 0));

		if (voxel_data.IsSolid)
		{
			_collisionMeshVertices.Add(new Vector3(x, y, z + _voxelSize));
			_collisionMeshVertices.Add(new Vector3(x, y, z));
			_collisionMeshVertices.Add(new Vector3(x, y + _voxelSize, z));
			_collisionMeshVertices.Add(new Vector3(x, y + _voxelSize, z + _voxelSize));
		}
	}

	private int? _getVoxelAtPosition(Vector3 position)
	{

		if (IsPositionWithinChunkBounds(position) == true)
		{
			position = position * GlobalTransform;

			position.X = (float)Math.Floor(position.X / _voxelSize);
			position.Y = (float)Math.Floor(position.Y / _voxelSize);
			position.Z = (float)Math.Floor(position.Z / _voxelSize);

			return voxels[(int)position.X][(int)position.Y][(int)position.Z];
		}

		return null;
	}

	public bool SetVoxelAtPosition(Vector3 position, int voxel)
	{
		if (IsPositionWithinChunkBounds(position) == true)
		{

			position = position * GlobalTransform;

			position.X = (float)Math.Floor(position.X / _voxelSize);
			position.Y = (float)Math.Floor(position.Y / _voxelSize);
			position.Z = (float)Math.Floor(position.Z / _voxelSize);

			voxels[(int)position.X][(int)position.Y][(int)position.Z] = voxel;

			UpdateMesh();

			return true;
		}

		return false;
	}

	private bool IsPositionWithinChunkBounds(Vector3 position)
	{

		if (position.X < GlobalTransform.Origin.X + (_chunkSizeX * _voxelSize) && position.X > GlobalTransform.Origin.X)
		{

			if (position.Y < GlobalTransform.Origin.Y + (_chunkSizeY * _voxelSize) && position.Y > GlobalTransform.Origin.Y)
			{

				if (position.Z < GlobalTransform.Origin.Z + (_chunkSizeZ * _voxelSize) && position.Z > GlobalTransform.Origin.Z)
				{

					return true;
				}
			}
		}

		return false;
	}

	private bool _getVoxelInBounds(int x, int y, int z)
	{
		if (x < 0 || x > _chunkSizeX - 1)
		{
			return false;
		}
		else if (y < 0 || y > _chunkSizeY - 1)
		{
			return false;
		}
		else if (z < 0 || z > _chunkSizeZ - 1)
		{
			return false;
		}

		return true;
	}

}
