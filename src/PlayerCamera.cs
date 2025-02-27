using Godot;
using System;

public partial class PlayerCamera : Node3D
{
	[Export] private NodePath _playerPath;
    [Export] private float _smoothSpeed = 60.0f;

    private Node3D _player;
    private Vector3 _offset;

    public override void _Ready()
    {
        _player = GetNode<Node3D>(_playerPath);

        _offset = Position - _player.Position;
    }

    public override void _Process(double delta)
    {
        if (_player == null)
            return;

        FollowPlayer((float)delta);
    }

    private void FollowPlayer(float delta)
    {
        Vector3 targetPosition = new Vector3(
            _player.Position.X + _offset.X,
            Position.Y,
            _player.Position.Z + _offset.Z
        );

        Position = Position.Lerp(targetPosition, _smoothSpeed * delta);
    }
}