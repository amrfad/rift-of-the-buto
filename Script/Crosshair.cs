namespace riftofbuto;

using Godot;
using System;

public partial class Crosshair : TextureRect
{
	private Vector2 crosshairPosition;
	private bool justChange = false;

	public override void _Ready()
	{
		// Mulai di tengah viewport
		var screenSize = GetViewport().GetVisibleRect().Size;
		crosshairPosition = screenSize / 2.0f;

		GlobalPosition = crosshairPosition;
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		if (!Input.IsMouseButtonPressed(MouseButton.Right) && @event is InputEventMouseMotion motion)
		{
			crosshairPosition += motion.Relative;

			// Clamp biar tidak keluar layar
			var size = GetViewport().GetVisibleRect().Size;
			crosshairPosition.X = Mathf.Clamp(crosshairPosition.X, 0, size.X);
			crosshairPosition.Y = Mathf.Clamp(crosshairPosition.Y, 0, size.Y);

			GlobalPosition = crosshairPosition;
		}
	}

	public Vector3 GetTargetPosition()
	{
		// Dapatkan kamera utama
		var camera = GetViewport().GetCamera3D();
		if (camera == null)
			return Vector3.Zero;

		// Konversi posisi crosshair ke ray dari kamera
		var from = camera.ProjectRayOrigin(crosshairPosition);
		var to = from + camera.ProjectRayNormal(crosshairPosition) * 1000f;

		// Buat raycast
		var spaceState = camera.GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(from, to);
		var result = spaceState.IntersectRay(query);

		// Jika ada collision, return posisi collision
		if (result.Count > 0)
		{
			return result["position"].AsVector3();
		}

		// Jika tidak ada collision, return posisi jauh di arah ray
		return to;
	}
}