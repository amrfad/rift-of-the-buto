namespace riftofbuto;

using Godot;

/// <summary>
/// Grappling Hook Gun controller yang memungkinkan player untuk menembakkan hook,
/// menarik diri ke target, dan mempertahankan momentum saat hook dilepas.
/// </summary>
public partial class GrapplingHookGun : Node3D
{
    #region Exported Properties
    
    [Export] public float HookSpeed = 50.0f;           // Kecepatan hook saat ditembakkan
    [Export] public float MinPullDistance = 2.0f;      // Jarak minimum sebelum berhenti menarik
    [Export] public float MomentumDecay = 0.95f;       // Seberapa cepat momentum berkurang (0.95 = decay 5% per frame)
    [Export] public PackedScene HookProjectileScene;   // Scene untuk hook projectile
    
    #endregion

    #region Private Fields - References
    private GlobalGameData globalData;      // Singleton/autoload global game data
    private RigidBody3D hookProjectile;     // Instance hook yang ditembakkan
    private MeshInstance3D ropeRenderer;    // Visual tali/rope yang menghubungkan gun dan hook
    private Crosshair crosshair;            // Reference ke crosshair untuk menentukan target
    private Player player;                  // Reference ke player untuk movement
    private Marker3D hookSpawnPoint;        // Titik spawn hook pada gun
    
    #endregion

    #region Private Fields - State Management
    
    private bool isHookActive = false;      // Apakah hook sedang aktif (ditembakkan)
    private bool isHookAttached = false;    // Apakah hook sudah menempel pada target
    private bool isPullingPlayer = false;   // Apakah sedang menarik player
    private bool isHookButtonPressed = false; // Status tombol hook (pressed/released)
    private bool hasMomentum = false;       // Apakah player memiliki momentum dari hook
    
    #endregion

    #region Private Fields - Position & Physics
    
    private Vector3 hookAttachedPosition;   // Posisi hook saat menempel
    private Vector3 playerMomentum = Vector3.Zero; // Momentum yang tersimpan untuk player

    #endregion

    #region Initialization

    /// <summary>
    /// Inisialisasi komponen dan setup references
    /// </summary>
    public override void _Ready()
    {
        // Setup komponen internal
        hookSpawnPoint = GetNode<Marker3D>("HookSpawnPoint");

        // Cari references ke node lain di scene
        crosshair = GetNode<Crosshair>("/root/World/HUD/Crosshair");
        player = GetNode<Player>("/root/World/Player");

        // Setup rope renderer yang independent dari movement gun/player
        CreateIndependentRopeRenderer();

        // Sembunyikan rope pada awalnya
        ropeRenderer.Visible = false;

        // Mengambil global game data
        globalData = GetNode<GlobalGameData>("/root/GlobalGameData");
    }

    /// <summary>
    /// Membuat rope renderer yang independent dan tidak terpengaruh movement gun/player
    /// </summary>
    private void CreateIndependentRopeRenderer()
    {
        // Buat rope renderer baru yang independent
        ropeRenderer = new MeshInstance3D();
        ropeRenderer.Name = "IndependentRopeRenderer";

        // Tambahkan ke scene root, bukan sebagai child dari gun
        // Ini memastikan rope tidak ikut bergerak saat gun/player bergerak
        GetTree().CurrentScene.CallDeferred("add_child", ropeRenderer);

        // Setup material dan mesh untuk rope
        SetupRopeRenderer();
    }

    /// <summary>
    /// Setup material dan mesh dasar untuk rope renderer
    /// </summary>
    private void SetupRopeRenderer()
    {
        // Buat mesh kosong untuk tali
        var arrayMesh = new ArrayMesh();
        ropeRenderer.Mesh = arrayMesh;

        // Material hitam untuk tali
        var material = new StandardMaterial3D();
        material.AlbedoColor = Colors.Black;
        material.MetallicSpecular = 0.0f;
        material.Roughness = 1.0f;
        ropeRenderer.MaterialOverride = material;
    }
    
    #endregion

    #region Input Handling
    
    /// <summary>
    /// Handle input untuk menembakkan dan melepaskan hook
    /// </summary>
    public override void _Input(InputEvent @event)
    {
        // Tombol hook ditekan - tembakkan hook atau mulai menarik
        if (@event.IsActionPressed("hook"))
        {
            isHookButtonPressed = true;

            // Tembakkan hook jika belum aktif
            if (!isHookActive)
            {
                ShootHook();
            }
        }
        // Tombol hook dilepas - selalu retract hook
        else if (@event.IsActionReleased("hook"))
        {
            isHookButtonPressed = false;

            // Selalu retract saat tombol dilepas
            if (isHookActive)
            {
                RetractHook();
            }
        }
    }
    
    #endregion

    #region Hook Shooting
    
    /// <summary>
    /// Menembakkan hook ke arah target crosshair
    /// </summary>
    private void ShootHook()
    {
        // Dapatkan posisi target dari crosshair
        Vector3 targetPosition = crosshair.GetTargetPosition();

        // Spawn hook projectile dari scene
        hookProjectile = HookProjectileScene.Instantiate<RigidBody3D>();
        GetTree().CurrentScene.AddChild(hookProjectile);

        // Set posisi awal hook dari spawn point pada gun
        Vector3 currentSpawnPosition = hookSpawnPoint.GlobalPosition;
        hookProjectile.GlobalPosition = currentSpawnPosition;

        // Hitung arah dan berikan velocity ke hook
        var direction = (targetPosition - currentSpawnPosition).Normalized();
        hookProjectile.LinearVelocity = direction * HookSpeed;

        // Set rotasi hook agar menghadap arah gerak
        hookProjectile.LookAt(targetPosition, Vector3.Up);

        // Connect signal untuk deteksi collision
        hookProjectile.Connect("body_entered", new Callable(this, nameof(OnHookCollision)));

        // Update state
        isHookActive = true;
        isHookAttached = false;
        isPullingPlayer = false;
        ropeRenderer.Visible = true;

        GD.Print($"Hook shot from spawn point: {currentSpawnPosition} towards: {targetPosition}");
    }
    
    #endregion

    #region Main Update Loop
    
    /// <summary>
    /// Update loop utama untuk handle momentum dan hook mechanics
    /// </summary>
    public override void _Process(double delta)
    {
        // Handle momentum bahkan saat hook tidak aktif
        if (hasMomentum && !isPullingPlayer)
        {
            ApplyMomentum((float)delta);
        }

        // Jika hook aktif, update semua mechanics
        if (isHookActive && hookProjectile != null)
        {
            // Mulai menarik player jika hook attached dan tombol masih ditekan
            if (isHookAttached && isHookButtonPressed && !isPullingPlayer)
            {
                StartPullingPlayer();
            }

            // Berhenti menarik jika tombol dilepas
            if (isPullingPlayer && !isHookButtonPressed)
            {
                StopPullingPlayer();
            }

            // Update posisi player jika sedang ditarik
            if (isPullingPlayer && isHookButtonPressed)
            {
                PullPlayerToHook((float)delta);
            }

            // Check jarak maksimum (hanya jika hook belum attached)
            if (!isHookAttached && hookProjectile != null)
            {
                var distance = hookSpawnPoint.GlobalPosition.DistanceTo(hookProjectile.GlobalPosition);
                if (distance > globalData.GrapplingRange)
                {
                    RetractHook();
                }
            }

            // Update visual rope
            UpdateRope();
        }
    }
    
    #endregion

    #region Rope Rendering
    
    /// <summary>
    /// Update visual rope/tali antara gun dan hook secara real-time
    /// </summary>
    private void UpdateRope()
    {
        if (hookProjectile == null || hookSpawnPoint == null || ropeRenderer == null) return;

        // Dapatkan posisi real-time dari spawn point dan hook
        // Karena rope renderer independent, tidak terpengaruh movement player
        var startPos = hookSpawnPoint.GlobalPosition; // Posisi real-time spawn point
        var endPos = hookProjectile.GlobalPosition;   // Posisi real-time hook

        // Setup array untuk mesh
        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);

        var vertices = new Vector3[8];
        var normals = new Vector3[8];

        var direction = (endPos - startPos).Normalized();
        var length = startPos.DistanceTo(endPos);
        var radius = 0.01f; // Ketebalan tali

        // Handle edge case jika jarak terlalu kecil
        if (length < 0.001f)
        {
            ropeRenderer.Visible = false;
            return;
        }
        else
        {
            ropeRenderer.Visible = true;
        }

        // Buat cylinder sederhana untuk tali
        var perpendicular = direction.Cross(Vector3.Up).Normalized();
        if (perpendicular.LengthSquared() < 0.1f)
            perpendicular = direction.Cross(Vector3.Right).Normalized();

        var perpendicular2 = direction.Cross(perpendicular).Normalized();

        // Buat 8 vertices (4 di start, 4 di end) untuk cylinder
        for (int i = 0; i < 4; i++)
        {
            float angle = i * Mathf.Pi * 0.5f;
            var offset = (perpendicular * Mathf.Cos(angle) + perpendicular2 * Mathf.Sin(angle)) * radius;

            vertices[i] = startPos + offset;        // Vertices di start
            vertices[i + 4] = endPos + offset;      // Vertices di end
            normals[i] = offset.Normalized();       // Normals untuk lighting
            normals[i + 4] = offset.Normalized();
        }

        // Indices untuk membuat triangles (faces) cylinder
        int[] indicesData = {
            0, 1, 4, 1, 5, 4,  // Side 1
            1, 2, 5, 2, 6, 5,  // Side 2
            2, 3, 6, 3, 7, 6,  // Side 3
            3, 0, 7, 0, 4, 7,  // Side 4
            0, 2, 1, 0, 3, 2,  // Start cap
            4, 5, 6, 4, 6, 7   // End cap
        };

        // Setup arrays dan buat mesh
        arrays[(int)Mesh.ArrayType.Vertex] = vertices;
        arrays[(int)Mesh.ArrayType.Normal] = normals;
        arrays[(int)Mesh.ArrayType.Index] = indicesData;

        var arrayMesh = new ArrayMesh();
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        ropeRenderer.Mesh = arrayMesh;
    }
    
    #endregion

    #region Hook Collision & Attachment
    
    /// <summary>
    /// Callback saat hook collision dengan object
    /// </summary>
    private void OnHookCollision(Node body)
    {
        // Attach hook jika belum attached
        if (hookProjectile != null && !isHookAttached)
        {
            // Stop hook movement dan freeze di posisi
            hookProjectile.LinearVelocity = Vector3.Zero;
            hookProjectile.GravityScale = 0;
            hookProjectile.Freeze = true;

            // Update state
            isHookAttached = true;
            hookAttachedPosition = hookProjectile.GlobalPosition;

            GD.Print("Hook attached to: " + body.Name);
        }
    }
    
    #endregion

    #region Player Pulling Mechanics
    
    /// <summary>
    /// Mulai menarik player ke arah hook
    /// </summary>
    private void StartPullingPlayer()
    {
        isPullingPlayer = true;
        GD.Print("Started pulling player");
    }
    
    /// <summary>
    /// Berhenti menarik player dan handle momentum preservation
    /// </summary>
    private void StopPullingPlayer()
    {
        if (isPullingPlayer)
        {
            // Simpan momentum hanya jika player tidak di ground
            if (!player.IsOnFloor())
            {
                var playerPos = player.GlobalPosition;
                var hookPos = hookProjectile.GlobalPosition;
                var pullDirection = (hookPos - playerPos).Normalized();
                playerMomentum = pullDirection * globalData.GrapplingSpeed;
                hasMomentum = true;
                GD.Print("Stopped pulling player - momentum captured (player in air)");
            }
            else
            {
                // Player di ground, tidak ada momentum
                playerMomentum = Vector3.Zero;
                hasMomentum = false;
                GD.Print("Stopped pulling player - no momentum (player on ground)");
            }
        }
        
        isPullingPlayer = false;
    }
    
    /// <summary>
    /// Update posisi player saat sedang ditarik ke hook
    /// </summary>
    private void PullPlayerToHook(float delta)
    {
        if (player == null || hookProjectile == null) return;
        
        var playerPos = player.GlobalPosition;
        var hookPos = hookProjectile.GlobalPosition;
        var distance = playerPos.DistanceTo(hookPos);
        
        // Berhenti menarik jika sudah cukup dekat
        if (distance <= MinPullDistance)
        {
            StopPullingPlayer();
            GD.Print("Reached hook destination");
            return;
        }
        
        // Hitung dan apply pull velocity
        var pullDirection = (hookPos - playerPos).Normalized();
        var pullVelocity = pullDirection * globalData.GrapplingSpeed;
        
        player.GlobalPosition += pullVelocity * delta;
    }
    
    #endregion

    #region Momentum System
    
    /// <summary>
    /// Apply momentum yang tersimpan ke player
    /// </summary>
    private void ApplyMomentum(float delta)
    {
        if (player == null) return;
        
        // Berhenti momentum jika player menyentuh ground
        if (player.IsOnFloor())
        {
            playerMomentum = Vector3.Zero;
            hasMomentum = false;
            GD.Print("Momentum stopped - player touched ground");
            return;
        }
        
        // Apply momentum ke posisi player
        player.GlobalPosition += playerMomentum * delta;
        
        // Decay momentum seiring waktu
        playerMomentum *= MomentumDecay;
        
        // Berhenti momentum jika sudah terlalu kecil
        if (playerMomentum.Length() < 0.1f)
        {
            playerMomentum = Vector3.Zero;
            hasMomentum = false;
            GD.Print("Momentum stopped - velocity too small");
        }
    }
    
    #endregion

    #region Hook Retraction & Cleanup
    
    /// <summary>
    /// Retract hook dan cleanup semua state
    /// </summary>
    private void RetractHook()
    {
        // Simpan momentum jika player sedang ditarik dan tidak di ground
        if (isPullingPlayer && hookProjectile != null && !player.IsOnFloor())
        {
            var playerPos = player.GlobalPosition;
            var hookPos = hookProjectile.GlobalPosition;
            var pullDirection = (hookPos - playerPos).Normalized();
            playerMomentum = pullDirection * globalData.GrapplingSpeed;
            hasMomentum = true;
            GD.Print("Hook retracted - momentum preserved (player in air)");
        }
        else
        {
            // Player di ground atau tidak menarik, no momentum
            playerMomentum = Vector3.Zero;
            hasMomentum = false;
            GD.Print("Hook retracted - no momentum preservation");
        }
        
        // Cleanup hook projectile
        if (hookProjectile != null)
        {
            hookProjectile.QueueFree();
            hookProjectile = null;
        }
        
        // Hide rope renderer
        if (ropeRenderer != null)
        {
            ropeRenderer.Visible = false;
        }
        
        // Reset semua state
        isHookActive = false;
        isHookAttached = false;
        isPullingPlayer = false;
    }
    
    /// <summary>
    /// Cleanup saat node keluar dari tree
    /// </summary>
    public override void _ExitTree()
    {
        // Cleanup independent rope renderer
        if (ropeRenderer != null && IsInstanceValid(ropeRenderer))
        {
            ropeRenderer.QueueFree();
        }
    }
    
    #endregion
}