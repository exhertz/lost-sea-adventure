using Godot;
using System;

public partial class MenuLayer : CanvasLayer
{
	private Button _playButton;

    public override void _Ready()
    {
        _playButton = GetNode<Button>("MenuContainer/PlayButton");
        _playButton.Connect("pressed", new Callable(this, nameof(OnPlayButtonPressed)));
    }

    private void OnPlayButtonPressed()
    {
        var gameScene = ResourceLoader.Load<PackedScene>("res://main.tscn");
        GetTree().ChangeSceneToPacked(gameScene);
    }
}
