namespace riftofbuto;

using Godot;
using System;

public partial class GameManager : Node3D
{
    [Export] public Aabb SpawnArea = new Aabb(new Vector3(0, 0, 0), new Vector3(510, 0, 510));
    [Export] public int NumberOfEnemies = 10;
    [Export] public PackedScene EnemyScene;
    [Export] public uint TerrainCollisionMask = 1; // Sesuaikan dengan layer collision terrain
    public int EnemiesKilled = 0;
    public int CurrentLevel = 1;
    public string LevelName = "Novice";

    private Label _killsDisplayLabel;
    private Control _winScreen;
    private Button _menuButton;
    private Button _upgradeButton;
    private Button _retryButton;
    private Label _killsLabel;
    private Label _goldLabel;

    private GlobalGameData globalData;

    public override void _Ready()
    {
        // Ambil data level dari GlobalGameData singleton
        globalData = GetNode<GlobalGameData>("/root/GlobalGameData");
        NumberOfEnemies = globalData.SelectedEnemyCount;
        LevelName = globalData.SelectedLevelName;
        CurrentLevel = globalData.SelectedLevelNumber;

        GD.Randomize();
        for (int i = 0; i < NumberOfEnemies; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            if (EnemyScene != null)
            {
                var enemy = EnemyScene.Instantiate<Node3D>();
                AddChild(enemy);
                enemy.GlobalPosition = spawnPos;
            }
        }

        _killsDisplayLabel = GetNode<Label>("PlayerUI/Label");
        UpdateKillsCount();

        // UI Win
        _winScreen = GetNode<Control>("GameOver");
        _winScreen.Visible = false;

        _killsLabel = _winScreen.GetNode<Label>("CenterContainer/WinPanel/VBoxContainer/KillsLabel");
        _goldLabel =  _winScreen.GetNode<Label>("CenterContainer/WinPanel/VBoxContainer/GoldLabel");
    }

    public void UpdateKillsCount()
    {
        if (_killsDisplayLabel != null)
        {
            // Use C# string interpolation to format the text
            _killsDisplayLabel.Text = $"Kills Buto Ijo ({EnemiesKilled}/{NumberOfEnemies})";
        }

        if (EnemiesKilled >= NumberOfEnemies)
        {
            globalData.AddGold(EnemiesKilled * 2);
            ShowWinScreen();
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }

    private void ShowWinScreen()
    {
        _killsLabel.Text = $"{EnemiesKilled} Buto Ijo killed!";
        _goldLabel.Text = $"{EnemiesKilled * 2} gold obtained!";
        _winScreen.Visible = true;
    }

    private void OnMenuButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://MainMenu.tscn");
    }

    private void OnUpgradeButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://UpgradeMenu.tscn");
    }

    private void OnRetryButtonPressed()
    {
        GetTree().ReloadCurrentScene();
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // Ambil XZ acak di dalam Aabb
        float x = (float)GD.RandRange(SpawnArea.Position.X, SpawnArea.Position.X + SpawnArea.Size.X);
        float z = (float)GD.RandRange(SpawnArea.Position.Z, SpawnArea.Position.Z + SpawnArea.Size.Z);

        // Dapatkan Y dengan RayCast dari atas ke bawah
        float y = GetGroundYAtPosition(x, z);

        return new Vector3(x, y, z);
    }

    private float GetGroundYAtPosition(float x, float z)
    {
        Vector3 from = new Vector3(x, 100f, z);   // Titik atas
        Vector3 to = new Vector3(x, -100f, z);    // Titik bawah

        var space = GetWorld3D().DirectSpaceState;

        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollisionMask = TerrainCollisionMask;

        var result = space.IntersectRay(query);

        if (result.Count > 0 && result.TryGetValue("position", out Variant hitPos))
        {
            return ((Vector3)hitPos).Y + 1;
        }

        // Kalau tidak kena tanah, fallback ke 0
        GD.PrintErr($"Spawn di ({x}, {z}) gagal kena raycast, fallback ke y = 0");
        return 0f;
    }
}
