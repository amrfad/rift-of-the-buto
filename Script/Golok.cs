namespace riftofbuto;

using Godot;
using System;

public partial class Golok : MeshInstance3D
{
	[Export] public Player player;
	private GlobalGameData globalData;      // Singleton/autoload global game data

    public override void _Ready()
    {
        globalData = GetNode<GlobalGameData>("/root/GlobalGameData");
    }

    private void OnSlash(Node3D body)
	{
		var obj = body.GetParent().GetParent().GetParent();
		if (obj is Enemy enemy && player.isSlashing())
		{
			enemy.TakeDamage(globalData.GolokDamage);
		}
	}
}
