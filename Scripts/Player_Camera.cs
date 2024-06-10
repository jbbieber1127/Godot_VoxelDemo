using Godot;
using Godot.Collections;
using System.Linq;

public partial class Player_Camera : Node3D
{
	const int NORMAL_SPEED = 8;
	const int SHIFT_SPEED = 12;

	bool keyboard_control_down = false;

	Camera3D view_cam;

	const double MOUSE_SENSITIVITY = 0.05;
	const int MIN_MAX_ANGLE = 85;

	bool do_raycast = false;
	bool mode_remove_voxel = false;

	[Export]
	NodePath path_to_voxel_world;
	VoxelWorld VoxelWorld;

	string current_voxel = "Cobble";

	[Export]
	PackedScene physics_object_scene;


	public override void _Ready()
	{
		view_cam = GetNode<Camera3D>("View_Camera");
		VoxelWorld = GetNode<VoxelWorld>(path_to_voxel_world);
	}

	public override void _Process(double delta)
	{
		if (Input.IsMouseButtonPressed(MouseButton.Right) == true)
		{
			if (Input.MouseMode != Input.MouseModeEnum.Captured)
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
		}
		else
		{
			if (Input.MouseMode != Input.MouseModeEnum.Visible)
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
		}

		if (Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			var speed = NORMAL_SPEED;
			if (Input.IsKeyPressed(Key.Shift))
			{
				speed = SHIFT_SPEED;
			}

			var dir = new Vector3(0, 0, 0);

			if (Input.IsKeyPressed(Key.W) || Input.IsKeyPressed(Key.Up))
			{
				dir.Z = -1;
			}
			if (Input.IsKeyPressed(Key.S) || Input.IsKeyPressed(Key.Down))
			{
				dir.Z = 1;
			}
			if (Input.IsKeyPressed(Key.A) || Input.IsKeyPressed(Key.Left))
			{
				dir.X = -1;
			}
			if (Input.IsKeyPressed(Key.D) || Input.IsKeyPressed(Key.Right))
			{
				dir.X = 1;
			}
			if (Input.IsKeyPressed(Key.Space))
			{
				dir.Y = 1;
			}

			dir = dir.Normalized();

			GlobalTranslate(view_cam.GlobalTransform.Basis.Z * dir.Z * (float)delta * speed);
			GlobalTranslate(view_cam.GlobalTransform.Basis.X * dir.X * (float)delta * speed);
			GlobalTranslate(new Vector3(0, 1, 0) * dir.Y * (float)delta * speed);
		}

		if (Input.IsKeyPressed(Key.Ctrl))
		{
			if (keyboard_control_down == false)
			{
				keyboard_control_down = true;
			}

			Node3D new_obj = physics_object_scene.Instantiate<Node3D>();
			new_obj.GlobalTransform = view_cam.GlobalTransform;
			GetParent().AddChild(new_obj);
		}
		else
		{
			keyboard_control_down = false;
		}
	}


	public override void _PhysicsProcess(double delta)
	{
		if (do_raycast)
		{
			do_raycast = false;
		}

		var space_state = GetWorld3D().DirectSpaceState;
		var from = view_cam.ProjectRayOrigin(GetViewport().GetMousePosition());
		var to = from + view_cam.ProjectRayNormal(GetViewport().GetMousePosition()) * 100;

		PhysicsRayQueryParameters3D query = new()
		{
			From = from,
			To = to,
			Exclude = new Array<Rid> { new(this) }
		};
		Dictionary result = space_state.IntersectRay(query);
		if (result.Keys.Count > 0)
		{
			Vector3 position = (Vector3)result["position"];
			Vector3 normal = (Vector3)result["normal"];

			if (!mode_remove_voxel)
			{
				var vxl = VoxelWorld.GetVoxelIndex(current_voxel);
				VoxelWorld.SetWorldVoxel(position + (normal / 2), vxl);
			}
			else
			{
				VoxelWorld.SetWorldVoxel(position - (normal / 2), null);
			}
		}
	}


	public override void _UnhandledInput(InputEvent inpt)
	{
		if (Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			if (inpt is InputEventMouseMotion)
			{
				InputEventMouseMotion mtn = inpt as InputEventMouseMotion;
				view_cam.RotateX(Mathf.DegToRad(mtn.Relative.Y * (float)MOUSE_SENSITIVITY * -1));

				this.RotateY(Mathf.DegToRad(mtn.Relative.X * (float)MOUSE_SENSITIVITY * -1));

				var camera_rot = view_cam.RotationDegrees;
				camera_rot.X = Mathf.Clamp(camera_rot.X, -MIN_MAX_ANGLE, MIN_MAX_ANGLE);

				view_cam.RotationDegrees = camera_rot;
			}
		}
		else
		{
			if (inpt is InputEventMouseButton)
			{
				InputEventMouseButton btn = inpt as InputEventMouseButton;
				if (btn.Pressed == true)
				{
					if (btn.ButtonIndex == MouseButton.Left)
					{
						do_raycast = true;
						mode_remove_voxel = false;
					}
					if (btn.ButtonIndex == MouseButton.Middle)
					{
						do_raycast = true;
						mode_remove_voxel = true;
					}
				}
			}
		}
	}
}

