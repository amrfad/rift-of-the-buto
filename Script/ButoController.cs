namespace riftofbuto;

using Godot;
using System;

public partial class ButoController : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	private readonly float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	private Node3D targetPlayer;
	private bool isPlayerDetected = false;

	public override void _Ready()
	{
		var detectionArea = GetNode<Area3D>("AreaDeteksi");
		detectionArea.BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Gravity
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Gerak ke player jika terdeteksi
		if (isPlayerDetected && targetPlayer != null)
		{
			Vector3 direction = (targetPlayer.GlobalPosition - GlobalPosition).Normalized();
			direction.Y = 0;

			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;

			// Rotasi menghadap player
			if (direction.LengthSquared() > 0.001f)
			{
				LookAt(GlobalPosition + direction, Vector3.Up);
				RotateY(Mathf.Pi); // Biar arah muka model benar
			}
		}
		else
		{
			// Jika tidak deteksi, tetap pakai gravity
			velocity.X = 0;
			velocity.Z = 0;
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void OnBodyEntered(Node3D body)
	{
		if (body is Player)
		{
			targetPlayer = body;
			isPlayerDetected = true;
		}
	}
}
