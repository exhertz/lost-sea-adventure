using Godot;

public partial class FishCounter : Control
{
    private Label _fishLabel;
    private Player _player;
    
    public override void _Ready()
    {
        _fishLabel = GetNode<Label>("FishLabel");
        _player = GetNode<Player>("/root/Main/Player");
    }
    
    public override void _Process(double delta)
    {
        _fishLabel.Text = $"Рыб поймано: {_player.FishCaught}";
    }
} 