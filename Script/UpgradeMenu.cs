namespace riftofbuto;

using Godot;

public partial class UpgradeMenu : Control
{
    // UI References
    private Label _pointsValue;
    
    private Label _grapplingLevel;
    private Label _grapplingCost;
    private Label _grapplingStats;
    private Button _grapplingButton;
    
    private Label _golokLevel;
    private Label _golokCost;
    private Label _golokStats;
    private Button _golokButton;
    
    private Label _kerisLevel;
    private Label _kerisCost;
    private Label _kerisStats;
    private Button _kerisButton;
    
    private Label _hpLevel;
    private Label _hpCost;
    private Label _hpStats;
    private Button _hpButton;
    
    private GlobalGameData globalData;

    public override void _Ready()
    {
        globalData = GetNode<GlobalGameData>("/root/GlobalGameData");

        // Get UI references
        _pointsValue = GetNode<Label>("MainContainer/HeaderContainer/PointsContainer/PointsValue");
        
        // Grappling Hook references
        _grapplingLevel = GetNode<Label>("MainContainer/UpgradeGrid/GrapplingPanel/GrapplingContainer/GrapplingInfo/GrapplingLevel");
        _grapplingCost = GetNode<Label>("MainContainer/UpgradeGrid/GrapplingPanel/GrapplingContainer/GrapplingInfo/GrapplingCost");
        _grapplingStats = GetNode<Label>("MainContainer/UpgradeGrid/GrapplingPanel/GrapplingContainer/GrapplingInfo/GrapplingStats");
        _grapplingButton = GetNode<Button>("MainContainer/UpgradeGrid/GrapplingPanel/GrapplingContainer/GrapplingInfo/GrapplingButton");
        
        // Golok references
        _golokLevel = GetNode<Label>("MainContainer/UpgradeGrid/GolokPanel/GolokContainer/GolokInfo/GolokLevel");
        _golokCost = GetNode<Label>("MainContainer/UpgradeGrid/GolokPanel/GolokContainer/GolokInfo/GolokCost");
        _golokStats = GetNode<Label>("MainContainer/UpgradeGrid/GolokPanel/GolokContainer/GolokInfo/GolokStats");
        _golokButton = GetNode<Button>("MainContainer/UpgradeGrid/GolokPanel/GolokContainer/GolokInfo/GolokButton");
        
        // Keris references
        _kerisLevel = GetNode<Label>("MainContainer/UpgradeGrid/KerisPanel/KerisContainer/KerisInfo/KerisLevel");
        _kerisCost = GetNode<Label>("MainContainer/UpgradeGrid/KerisPanel/KerisContainer/KerisInfo/KerisCost");
        _kerisStats = GetNode<Label>("MainContainer/UpgradeGrid/KerisPanel/KerisContainer/KerisInfo/KerisStats");
        _kerisButton = GetNode<Button>("MainContainer/UpgradeGrid/KerisPanel/KerisContainer/KerisInfo/KerisButton");
        
        // HP references
        _hpLevel = GetNode<Label>("MainContainer/UpgradeGrid/HPPanel/HPContainer/HPInfo/HPLevel");
        _hpCost = GetNode<Label>("MainContainer/UpgradeGrid/HPPanel/HPContainer/HPInfo/HPCost");
        _hpStats = GetNode<Label>("MainContainer/UpgradeGrid/HPPanel/HPContainer/HPInfo/HPStats");
        _hpButton = GetNode<Button>("MainContainer/UpgradeGrid/HPPanel/HPContainer/HPInfo/HPButton");
        
        // Initialize UI
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update points display
        _pointsValue.Text = globalData.Gold.ToString();
        
        // Update Grappling Hook UI
        _grapplingLevel.Text = $"Level: {globalData.GrapplingLvl}";
        _grapplingCost.Text = $"Cost: {globalData.GrapplingUpgradeCost} Points";
        _grapplingStats.Text = $"Range: {globalData.GrapplingRange}m | Speed: {globalData.GrapplingSpeed}m/s";
        _grapplingButton.Disabled = globalData.Gold < globalData.GrapplingUpgradeCost;
        
        // Update Golok UI
        _golokLevel.Text = $"Level: {globalData.GolokLvl}";
        _golokCost.Text = $"Cost: {globalData.GolokUpgradeCost} Points";
        _golokStats.Text = $"Damage: {globalData.GolokDamage}";
        _golokButton.Disabled = globalData.Gold < globalData.GolokUpgradeCost;
        
        // Update Keris UI
        _kerisLevel.Text = $"Level: {globalData.KerisLvl}";
        _kerisCost.Text = $"Cost: {globalData.KerisUpgradeCost} Points";
        _kerisStats.Text = $"Cooldown: {globalData.KerisCooldown}s";
        _kerisButton.Disabled = globalData.Gold < globalData.KerisUpgradeCost;
        
        // Update HP UI
        _hpLevel.Text = $"Level: {globalData.HpLvl}";
        _hpCost.Text = $"Cost: {globalData.HpUpgradeCost} Points";
        _hpStats.Text = $"Max HP: {globalData.MaxHp} | Regen: {globalData.HpRegen}/s";
        _hpButton.Disabled = globalData.Gold < globalData.HpUpgradeCost;
    }

    // Signal handlers for upgrade buttons
    private void _on_grappling_upgrade_pressed()
    {
        if (globalData.Gold >= globalData.GrapplingUpgradeCost)
        {
            // Deduct points
            globalData.SpendGold(globalData.GrapplingUpgradeCost);

            // Upgrade stats
            globalData.UpgradeGrappling(5.0f, 2.5f);
            
            GD.Print($"Grappling Hook upgraded to level {globalData.GrapplingLvl}!");
            GD.Print($"New stats - Range: {globalData.GrapplingRange}m, Speed: {globalData.GrapplingSpeed}m/s");
            
            UpdateUI();
        }
    }

    private void _on_golok_upgrade_pressed()
    {
        if (globalData.Gold >= globalData.GolokUpgradeCost)
        {
            // Deduct points
            globalData.SpendGold(globalData.GolokUpgradeCost);

            // Upgrade stats
            globalData.UpgradeGolok(5);
            
            GD.Print($"Golok upgraded to level {globalData.GolokLvl}!");
            GD.Print($"New stats - Damage: {globalData.GolokDamage},");
            
            UpdateUI();
        }
    }

    private void _on_keris_upgrade_pressed()
    {
        if (globalData.Gold >= globalData.KerisUpgradeCost)
        {

            // Deduct points
            globalData.SpendGold(globalData.KerisUpgradeCost);

            // Upgrade stats
            globalData.UpgradeKeris(0.3f);
            
            GD.Print($"Keris upgraded to level {globalData.KerisLvl}!");
            GD.Print($"New stats - Cooldown: {globalData.KerisCooldown}s");
            
            UpdateUI();
        }
    }

    private void _on_hp_upgrade_pressed()
    {
        if (globalData.Gold >= globalData.HpUpgradeCost)
        {
            // Deduct points
            globalData.SpendGold(globalData.HpUpgradeCost);

            // Upgrade stats
            globalData.UpgradeHP(10, 0.5f);
            
            GD.Print($"Health Points upgraded to level {globalData.HpLvl}!");
            GD.Print($"New stats - Max HP: {globalData.MaxHp}, Regen: {globalData.HpRegen}/s");
            
            UpdateUI();
        }
    }

    private void _on_back_button_pressed()
    {
        GD.Print("Returning to Main Menu...");
        GetTree().ChangeSceneToFile("res://MainMenu.tscn");
    }

    // Public methods for external access (if needed by other systems)
    public int GetUpgradePoints()
    {
        return globalData.Gold;
    }

    public void AddUpgradePoints(int points)
    {
        globalData.Gold += points;
        UpdateUI();
        GD.Print($"Added {points} upgrade points. Total: {globalData.Gold}");
    }

    public void SetUpgradePoints(int points)
    {
        globalData.Gold = points;
        UpdateUI();
        GD.Print($"Set upgrade points to: {globalData.Gold}");
    }

    // Getter methods for current upgrade levels (for other systems)
    public int GetGrapplingLevel() => globalData.GrapplingLvl;
    public int GetGolokLevel() => globalData.GolokLvl;
    public int GetKerisLevel() => globalData.KerisLvl;
    public int GetHPLevel() => globalData.HpLvl;

    // Getter methods for current stats (for gameplay systems)
    public float GetGrapplingRange() => globalData.GrapplingRange;
    public float GetGrapplingSpeed() => globalData.GrapplingSpeed;
    public int GetGolokDamage() => globalData.GolokDamage;
    public float GetKerisCooldown() => globalData.KerisCooldown;
    public int GetMaxHP() => globalData.MaxHp;
    public float GetHPRegen() => globalData.HpRegen;
}