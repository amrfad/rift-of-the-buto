namespace riftofbuto;

using Godot;

public partial class HookProjectile : RigidBody3D
{
    [Signal] public delegate void HookCollidedEventHandler(Node body);

	public override void _Ready()
	{
		// Setup collision detection
		Connect("body_entered", new Callable(this, nameof(OnBodyEntered)));

		// Setup physics
		GravityScale = 0.5f; // Sedikit gravitasi untuk efek realistis
		CanSleep = false;
		ContactMonitor = true;
		MaxContactsReported = 10;
    }

	private void OnBodyEntered(Node body)
	{
		EmitSignal(SignalName.HookCollided, body);
    }
}