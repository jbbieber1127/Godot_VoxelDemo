using Godot;

namespace GodotVoxelTutorial.Scripts
{
    public struct Voxel
    {
        public bool transparent;
        public bool solid;
        public Vector2I texture;
        public Vector2I? textureTop;
        public Vector2I? textureBottom;
        public Vector2I? textureNorth;
        public Vector2I? textureSouth;
        public Vector2I? textureEast;
        public Vector2I? textureWest;
    }
}
