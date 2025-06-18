namespace riftofbuto;

using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Export] public float Speed { get; set; } = 5.0f;
    [Export] public float JumpVelocity { get; set; } = 4.5f;
    [Export] public float PushForce { get; set; } = 5.0f;

    private readonly float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    // Referensi ke node kamera dikontrol oleh CameraController terpisah
    private NodePath CameraPivotPath { get; set; }
    public NodePath CameraPath { get; set; }

    private Node3D _cameraPivot;
    private CameraController _cameraController;

    private AnimationPlayer _animPlayer;
    private string _currentAnim = "";
    private bool _isPerformingAction = false;
    private bool _mustStay = false;

    // Sistem serangan combo
    private ulong _lastSlashTime = 0;
    private readonly ulong _comboWindow = 1500;

    private enum SlashState
    {
        Slash1,
        Slash2
    }

    private SlashState _slashState = SlashState.Slash1;

    // Health Bar
    private HealthComponent _health;
    private ProgressBar _uiHealthBar;

    public override void _Ready()
    {
        // Inisialisasi kamera dan pivot, serta hubungkan CameraController
        CameraPivotPath = "CameraPivot";
        _cameraPivot = GetNode<Node3D>(CameraPivotPath);
        CameraPath = CameraPivotPath + "/Camera3D";
        _cameraController = GetNode<CameraController>(CameraPath);

        // Ambil dan hubungkan AnimationPlayer
        _animPlayer = GetNode<AnimationPlayer>("ModelNode/PlayerObject/AnimationPlayer");
        _animPlayer.Connect("animation_finished", new Callable(this, nameof(OnAnimationFinished)));

        _health = GetNode<HealthComponent>("HealthComponent");
        if (_health != null)
        {
            _health.Damaged += UpdateUIHealth;
            _health.Died += OnPlayerDied;
        }

        var world = GetTree().Root.GetNode<Node3D>("World");
        _uiHealthBar = world.GetNode<ProgressBar>("PlayerUI/HealthBar");
        if (_uiHealthBar != null)
        {
            _uiHealthBar.MaxValue = _health.MaxHealth;
            _uiHealthBar.Value = _health.CurrentHealth;
        }
    }

    private void UpdateUIHealth(int currentHealth)
    {
        if (_uiHealthBar != null)
            _uiHealthBar.Value = currentHealth;
    }

    private void OnPlayerDied()
    {
        GD.Print("Player died.");
        // Tambahkan logic seperti menampilkan UI game over
    }

    public void TakeDamage(int amount)
    {
        _health?.TakeDamage(amount);
    }

    private void OnAnimationFinished(string animName)
    {
        if (animName == "Slash-1" || animName == "Slash-2")
        {
            _isPerformingAction = false;
            _mustStay = false;
            PlayAnim("Idle");
            return;
        }

        _isPerformingAction = false;
        _mustStay = false;
        _currentAnim = "";
    }

    public override void _Input(InputEvent @event)
    {
        // Kontrol kamera dipindahkan ke CameraController
        if (Input.IsMouseButtonPressed(MouseButton.Right))
        {
            if (@event is InputEventMouseMotion mouseMotion)
            {
                _cameraController.HandleMouseLook(mouseMotion);
            }

            // Update camera state setiap frame untuk handling perubahan movement state
            _cameraController.UpdateCameraState();
        }

        // Trigger cutscene kamera jika tombol dibatalkan (Escape misalnya)
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            var cameraAnim = GetNode<AnimationPlayer>("CameraMovement");
            cameraAnim.Play("Cut Scene");
        }

        // Serangan ketika klik kiri
        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            if (_currentAnim == "Attack-Run") return;
            if (_currentAnim == "Run" || !IsOnFloor())
            {
                PlayAnim("Attack-Run", 1.2f);
            }
            else
            {
                HandleSlashCombo();
            }
        }

        // Melempar grappling hook
        if (Input.IsActionJustPressed("hook"))
        {
            PlayAnim("Hook");
        }

        // DEBUG: test take damage to player.
        if (Input.IsKeyPressed(Key.X))
            TakeDamage(10);
    }

    /// <summary>
    /// Menangani logika serangan combo dua langkah.
    /// Slash-1 → Slash-2 jika dalam window waktu tertentu.
    /// Jika terlalu lama, kembali ke Slash-1.
    /// </summary>
    private void HandleSlashCombo()
    {
        ulong currentTime = Time.GetTicksMsec();

        if (currentTime - _lastSlashTime <= _comboWindow)
        {
            if (!_isPerformingAction)
            {
                switch (_slashState)
                {
                    case SlashState.Slash1:
                        PlayAnim("Slash-1", 1.5f);
                        _slashState = SlashState.Slash2;
                        break;
                    case SlashState.Slash2:
                        PlayAnim("Slash-2", 1.5f);
                        _slashState = SlashState.Slash1;
                        break;
                }
            }
        }
        else
        {
            _slashState = SlashState.Slash2;
            PlayAnim("Slash-1", 1.5f);
        }

        _lastSlashTime = currentTime;
    }

    /// <summary>
    /// Memainkan animasi yang diberikan, menghindari gangguan aksi lain.
    /// </summary>
    private void PlayAnim(string name, float speed = 1.0f)
    {
        if (_currentAnim == name) return;
        if (_isPerformingAction && (name == "Run" || name == "Idle")) return;

        _animPlayer.SpeedScale = speed;
        _animPlayer.Play(name);
        _currentAnim = name;

        _isPerformingAction = name != "Run" && name != "Idle";
        _mustStay = name == "Slash-1" || name == "Slash-2" || name == "Throw";
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        if (!IsOnFloor())
            velocity.Y -= gravity * (float)delta;

        // Handle Fall.
        // if (!IsOnFloor() && _currentAnim != "Jump" && _currentAnim != "Jump-Run" && _currentAnim != "Attack-Run")
        //     PlayAnim("Fall");
        // else if (IsOnFloor() && _currentAnim == "Fall")
        // {
        //     _animPlayer.Stop();
        //     OnAnimationFinished("Fall");
        // }

        // Melompat saat di tanah
        if (Input.IsActionJustPressed("jump") && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
            if (_currentAnim == "Run")
                PlayAnim("Jump-Run");
            else PlayAnim("Jump", 0.9f);
        }

        if (_mustStay)
        {
            // Hentikan pergerakan jika aksi mengunci karakter
            velocity.X = 0;
            velocity.Z = 0;
        }
        else
        {
            HandleMovement(ref velocity);
        }

        Velocity = velocity;

        // Deteksi dan tangani tabrakan dengan rigidbody
        var collision = MoveAndCollide(velocity * (float)delta);
        HandleCollisionWithRigidBody(collision);

        MoveAndSlide();
    }

    /// <summary>
    /// Menangani input arah gerakan dan memutar animasi sesuai konteks.
    /// </summary>
    private void HandleMovement(ref Vector3 velocity)
    {
        Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_back", "move_forward");
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, -inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;

            if (!_isPerformingAction)
                PlayAnim("Run");

            _cameraController.UpdateCameraState();
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);

            if (!_isPerformingAction)
                PlayAnim("Idle");
        }
    }

    /// <summary>
    /// Mendorong rigidbody jika tertabrak.
    /// </summary>
    private void HandleCollisionWithRigidBody(KinematicCollision3D collision)
    {
        if (collision == null) return;

        var collider = collision.GetCollider();
        if (collider is RigidBody3D rigidBody)
        {
            Vector3 pushDirection = Velocity.Normalized();
            pushDirection.Y = 0;
            rigidBody.ApplyCentralImpulse(pushDirection * PushForce);
        }
    }

    public string GetCurrentAnim()
    {
        return _currentAnim;
    }

    public bool isSlashing()
    {
        return (_currentAnim == "Slash-1") || (_currentAnim == "Slash-2") || (_currentAnim == "Attack-Run");
    }
}
