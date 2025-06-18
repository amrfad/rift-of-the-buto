namespace riftofbuto;

using Godot;

public partial class LevelSelection : Control
{
    public override void _Ready()
    {
        // Optional: Add any initialization code here
    }

    private void _on_level_1_button_pressed()
    {
        StartLevel(1);
    }

    private void _on_level_2_button_pressed()
    {
        StartLevel(2);
    }

    private void _on_level_3_button_pressed()
    {
        StartLevel(3);
    }

    private void _on_level_4_button_pressed()
    {
        StartLevel(4);
    }

    private void _on_level_5_button_pressed()
    {
        StartLevel(5);
    }

    private void _on_back_button_pressed()
    {
        GetTree().ChangeSceneToFile("res://MainMenu.tscn");
    }

    private void StartLevel(int levelNumber)
    {
        // Set level data di GlobalGameData singleton
        var globalData = GetNode<GlobalGameData>("/root/GlobalGameData");
        globalData.SetLevel(levelNumber);
        
        // Pindah ke game scene
        GetTree().ChangeSceneToFile("res://World.tscn");
    }
}