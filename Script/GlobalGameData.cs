using Godot;

// Singleton untuk menyimpan data game global
public partial class GlobalGameData : Node
{
    // Level selection data
    public int SelectedEnemyCount { get; set; } = 5;
    public string SelectedLevelName { get; set; } = "Easy";
    public int SelectedLevelNumber { get; set; } = 1;

    // Level configurations
    public readonly int[] EnemyCounts = { 5, 10, 20, 35, 50 };
    public readonly string[] LevelNames = { "Easy", "Normal", "Hard", "Expert", "Nightmare" };

    // Grappling hook stats
    public int GrapplingLvl { get; set; } = 1;
    public float GrapplingRange { get; set; } = 40.0f;
    public float GrapplingSpeed { get; set; } = 20.0f;
    public int GrapplingUpgradeCost { get; set; } = 10;

    // Golok stats
    public int GolokLvl { get; set; } = 1;
    public int GolokDamage { get; set; } = 15;
    public int GolokUpgradeCost { get; set; } = 15;

    // Keris stats
    public int KerisLvl { get; set; } = 1;
    public float KerisCooldown { get; set; } = 5.0f;
    public int KerisUpgradeCost { get; set; } = 12;

    // HP stats
    public int HpLvl { get; set; } = 1;
    public int MaxHp { get; set; } = 100;
    public float HpRegen { get; set; } = 1.0f;
    public int HpUpgradeCost { get; set; } = 20;

    // Uang pemain
    public int Gold { get; set; } = 0;

    private const string SAVE_FILE = "user://player_stats.save";

    public override void _Ready()
    {
        // DeleteSaveFile();
        LoadPlayerStats();
        GD.Print("GlobalGameData initialized with player stats loaded");
    }

    // Set level yang dipilih
    public void SetLevel(int levelNumber)
    {
        if (levelNumber >= 1 && levelNumber <= 5)
        {
            SelectedLevelNumber = levelNumber;
            SelectedEnemyCount = EnemyCounts[levelNumber - 1];
            SelectedLevelName = LevelNames[levelNumber - 1];
        }
    }

    public void SavePlayerStats()
    {
        var saveFile = FileAccess.Open(SAVE_FILE, FileAccess.ModeFlags.Write);
        if (saveFile != null)
        {
            var saveData = new Godot.Collections.Dictionary<string, Variant>
            {
                {"grappling_lvl", GrapplingLvl},
                {"grappling_range", GrapplingRange},
                {"grappling_speed", GrapplingSpeed},
                {"grappling_upgrade_cost", GrapplingUpgradeCost},

                {"golok_lvl", GolokLvl},
                {"golok_damage", GolokDamage},
                {"golok_upgrade_cost", GolokUpgradeCost},

                {"keris_lvl", KerisLvl},
                {"keris_cooldown", KerisCooldown},
                {"keris_upgrade_cost", KerisUpgradeCost},

                {"hp_lvl", HpLvl},
                {"max_hp", MaxHp},
                {"hp_regen", HpRegen},
                {"hp_upgrade_cost", HpUpgradeCost},

                {"gold", Gold}
            };

            saveFile.StoreString(Json.Stringify(saveData));
            saveFile.Close();
            GD.Print("Player stats saved");
        }
    }

    public void LoadPlayerStats()
    {
        if (!FileAccess.FileExists(SAVE_FILE))
        {
            GD.Print("No save file found, using default stats");
            SavePlayerStats();
            return;
        }

        var saveFile = FileAccess.Open(SAVE_FILE, FileAccess.ModeFlags.Read);
        if (saveFile != null)
        {
            var jsonString = saveFile.GetAsText();
            saveFile.Close();

            var json = new Json();
            var parseResult = json.Parse(jsonString);

            if (parseResult == Error.Ok)
            {
                var saveData = json.Data.AsGodotDictionary();

                if (saveData.ContainsKey("grappling_lvl"))
                    GrapplingLvl = saveData["grappling_lvl"].AsInt32();
                if (saveData.ContainsKey("grappling_range"))
                    GrapplingRange = saveData["grappling_range"].AsSingle();
                if (saveData.ContainsKey("grappling_speed"))
                    GrapplingSpeed = saveData["grappling_speed"].AsSingle();
                if (saveData.ContainsKey("grappling_upgrade_cost"))
                    GrapplingUpgradeCost = saveData["grappling_upgrade_cost"].AsInt32();

                if (saveData.ContainsKey("golok_lvl"))
                    GolokLvl = saveData["golok_lvl"].AsInt32();
                if (saveData.ContainsKey("golok_damage"))
                    GolokDamage = saveData["golok_damage"].AsInt32();
                if (saveData.ContainsKey("golok_upgrade_cost"))
                    GolokUpgradeCost = saveData["golok_upgrade_cost"].AsInt32();

                if (saveData.ContainsKey("keris_lvl"))
                    KerisLvl = saveData["keris_lvl"].AsInt32();
                if (saveData.ContainsKey("keris_cooldown"))
                    KerisCooldown = saveData["keris_cooldown"].AsSingle();
                if (saveData.ContainsKey("keris_upgrade_cost"))
                    KerisUpgradeCost = saveData["keris_upgrade_cost"].AsInt32();

                if (saveData.ContainsKey("hp_lvl"))
                    HpLvl = saveData["hp_lvl"].AsInt32();
                if (saveData.ContainsKey("max_hp"))
                    MaxHp = saveData["max_hp"].AsInt32();
                if (saveData.ContainsKey("hp_regen"))
                    HpRegen = saveData["hp_regen"].AsSingle();
                if (saveData.ContainsKey("hp_upgrade_cost"))
                    HpUpgradeCost = saveData["hp_upgrade_cost"].AsInt32();

                if (saveData.ContainsKey("gold"))
                    Gold = saveData["gold"].AsInt32();

                GD.Print("Player stats loaded successfully");
            }
        }
    }

    // Helper methods
    public void AddGold(int amount)
    {
        Gold += amount;
        SavePlayerStats();
    }

    public void SpendGold(int amount)
    {
        Gold = Mathf.Max(0, Gold - amount);
        SavePlayerStats();
    }

    // Upgrade Methods
    public void UpgradeGrappling(float rangeIncrease, float speedIncrease)
    {
        GrapplingLvl++;
        GrapplingRange += rangeIncrease;
        GrapplingSpeed += speedIncrease;
        GrapplingUpgradeCost = (int)(GrapplingUpgradeCost * 1.5f);
        SavePlayerStats();
    }

    public void UpgradeGolok(int damageIncrease)
    {
        GolokLvl++;
        GolokDamage += damageIncrease;
        GolokUpgradeCost = (int)(GolokUpgradeCost * 1.5f);
        SavePlayerStats();
    }

    public void UpgradeKeris(float cooldownReduction)
    {
        KerisLvl++;
        KerisCooldown = Mathf.Max(0.1f, KerisCooldown - cooldownReduction);
        KerisUpgradeCost = (int)(KerisUpgradeCost * 1.5f);
        SavePlayerStats();
    }

    public void UpgradeHP(int hpIncrease, float regenIncrease)
    {
        HpLvl++;
        MaxHp += hpIncrease;
        HpRegen += regenIncrease;
        HpUpgradeCost = (int)(HpUpgradeCost * 1.5f);
        SavePlayerStats();
    }

    public void DeleteSaveFile()
    {
        if (FileAccess.FileExists(SAVE_FILE))
        {
            DirAccess dir = DirAccess.Open("user://");
            if (dir != null)
            {
                dir.Remove("player_stats.save");
                GD.Print("Save file deleted.");
            }
        }
        else
        {
            GD.Print("No save file to delete.");
        }
    }
}
