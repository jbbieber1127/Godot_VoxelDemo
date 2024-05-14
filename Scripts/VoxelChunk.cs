using Godot;
using GodotVoxelTutorial.Scripts;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

public partial class VoxelChunk : Node3D
{
    public VoxelWorld World { get; set; }


    List<List<List<int?>>> voxels;

    int _chunkSizeX;
    int _chunkSizeY;
    int _chunkSizeZ;

    int _voxelSize = 1;

    var render_mesh;
    var render_mesh_vertices;
    var render_mesh_normals;
    var render_mesh_indices;
    var render_mesh_uvs;

    var collision_mesh;
    var collision_mesh_vertices;
    var collision_mesh_indices;

    Node3D mesh_instance;
    Node3D collision_shape;

    SurfaceTool surface_tool;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        mesh_instance = (Node3D)GetNode("MeshInstance");
        collision_shape = (Node3D)GetNode("StaticBody/CollisionShape");

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
        render_mesh_vertices = [];
        render_mesh_normals = [];
        render_mesh_indices = [];
        render_mesh_uvs = [];
        collision_mesh_vertices = [];
        collision_mesh_indices = [];

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

        for (int i = 0; i < render_mesh_vertices.size(); i++)
        {
            surface_tool.SetNormal(render_mesh_normals[i]);
            surface_tool.SetUV(render_mesh_uvs[i]);
            surface_tool.AddVertex(render_mesh_vertices[i]);
        }

        for (int i = 0; i < render_mesh_indices.size(); i++)
        {
            surface_tool.AddIndex(render_mesh_indices[i]);
        }

        surface_tool.GenerateTangents();

        render_mesh = surface_tool.Commit();
        mesh_instance.mesh = render_mesh;
        //# ********************
        //# Make the collision mesh
        //# ********************
        surface_tool.Clear();
        surface_tool.Begin(Mesh.PrimitiveType.Triangles);

        for (int i = 0; i < collision_mesh_vertices.size(); i++)
        {
            surface_tool.AddVertex(collision_mesh_vertices[i]);
        }

        for (int i = 0; i < collision_mesh_indices.size(); i++)
        {
            surface_tool.AddIndex(collision_mesh_indices[i]);
        }

        collision_mesh = surface_tool.Commit();
        collision_shape.shape = collision_mesh.create_trimesh_shape();
    }

}
