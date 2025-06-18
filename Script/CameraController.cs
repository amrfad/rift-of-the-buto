namespace riftofbuto;

using Godot;
using System;

public partial class CameraController : Camera3D
{
    [Export] public NodePath CameraPivotPath { get; set; }
    [Export] public float MouseSensitivity { get; set; } = 0.003f;
    [Export] public NodePath PlayerPath { get; set; }
    
    private Node3D _cameraPivot;
    private float _pitch = 0.0f;
    private bool _wasMoving = false;
    
    // Posisi default kamera (relatif terhadap CameraPivot)
    private readonly Vector3 _defaultRotation = new Vector3(Mathf.DegToRad(-30), 0, 0);
    
    public override void _Ready()
    {
        _cameraPivot = GetNode<Node3D>(CameraPivotPath);
        
        // Set posisi awal kamera
        Rotation = _defaultRotation;
        _pitch = _defaultRotation.X;
    }
    
    public void HandleMouseLook(InputEventMouseMotion mouseMotion)
    {
        float yaw = -mouseMotion.Relative.X * MouseSensitivity;
        float pitchDelta = -mouseMotion.Relative.Y * MouseSensitivity;
        
        // Cek apakah player sedang bergerak ke arah manapun
        bool isMoving = Input.IsActionPressed("move_forward") || 
                       Input.IsActionPressed("move_back") || 
                       Input.IsActionPressed("move_left") || 
                       Input.IsActionPressed("move_right");
        
        if (isMoving)
        {
            // Saat bergerak: kamera tidak berputar horizontal, tapi pitch tetap bisa
            _pitch = Mathf.Clamp(_pitch + pitchDelta, -Mathf.Pi / 3, Mathf.Pi / 2);
            
            // Mouse yaw langsung mengubah arah hadap player
            Player playerNode = GetNode<Player>(PlayerPath);
            playerNode.RotateY(yaw);
            
            // Update pitch pada CameraPivot
            _cameraPivot.Rotation = new Vector3(_pitch, _cameraPivot.Rotation.Y, 0);
        }
        else
        {
            // Saat tidak bergerak: mouse look normal (pitch dan yaw pada kamera)
            _pitch = Mathf.Clamp(_pitch + pitchDelta, -Mathf.Pi / 3, Mathf.Pi / 2);
            _cameraPivot.Rotation = new Vector3(_pitch, _cameraPivot.Rotation.Y + yaw, 0);
        }
    }
    
    // Method tambahan untuk update tanpa mouse motion
    public void UpdateCameraState()
    {
        // Cek apakah player sedang bergerak ke arah manapun
        bool isMoving = Input.IsActionPressed("move_forward") || 
                       Input.IsActionPressed("move_back") || 
                       Input.IsActionPressed("move_left") || 
                       Input.IsActionPressed("move_right");
        
        // Deteksi mulai bergerak (transisi dari tidak bergerak ke bergerak)
        bool justStartedMoving = isMoving && !_wasMoving;
        
        if (justStartedMoving)
        {
            // Dapatkan global rotation CameraPivot sebelum player dirotasi
            Vector3 cameraPivotGlobalRotation = _cameraPivot.GlobalRotation;
            
            // Rotasi player agar menghadap arah global CameraPivot
            Player playerNode = GetNode<Player>(PlayerPath);
            playerNode.Rotation = new Vector3(playerNode.Rotation.X, cameraPivotGlobalRotation.Y, playerNode.Rotation.Z);
            
            // Reset CameraPivot local rotation agar tidak double-rotate
            _cameraPivot.Rotation = new Vector3(_pitch, 0, 0);
        }
        
        // Update state untuk frame berikutnya
        _wasMoving = isMoving;
    }
}