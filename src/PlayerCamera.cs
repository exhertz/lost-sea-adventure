using Godot;
using System;

public partial class PlayerCamera : Node3D
{
	[Export] private NodePath _playerPath;
    [Export] private float _smoothSpeed = 60.0f;
    [Export] private float _targetZoom = 20.0f;  // Увеличиваем конечное расстояние камеры
    [Export] private float _zoomDuration = 2.0f;  // Длительность анимации в секундах

    private Node3D _player;
    private Vector3 _offset;
    private float _zoomTimer = 0.0f;
    private bool _isZooming = true;
    private Camera3D _camera;
    private float _initialZoom;

    public override void _Ready()
    {
        _player = GetNode<Node3D>(_playerPath);
        _camera = GetNode<Camera3D>("Camera3D");
        
        if (_camera == null)
        {
            GD.PrintErr("Camera3D not found!");
            return;
        }

        // Устанавливаем начальное положение камеры
        _offset = Position - _player.Position;
        
        // Устанавливаем и выводим начальные значения
        _initialZoom = 10.0f; // Начальное значение
        _camera.Size = _initialZoom;
        
        GD.Print($"Initial zoom: {_initialZoom}");
        GD.Print($"Target zoom: {_targetZoom}");
        GD.Print($"Camera size: {_camera.Size}");
    }

    public override void _Process(double delta)
    {
        if (_player == null || _camera == null)
            return;

        if (_isZooming)
        {
            HandleStartupZoom((float)delta);
        }

        FollowPlayer((float)delta);
    }

    private void HandleStartupZoom(float delta)
    {
        _zoomTimer += delta;
        float progress = Mathf.Clamp(_zoomTimer / _zoomDuration, 0.0f, 1.0f);
        
        // Используем плавную интерполяцию для более естественного движения
        float easeProgress = Mathf.SmoothStep(0.0f, 1.0f, progress);
        float newSize = Mathf.Lerp(_initialZoom, _targetZoom, easeProgress);
        _camera.Size = newSize;
        
        // Отладочный вывод
        if (_zoomTimer % 0.5f < delta) // Выводим каждые 0.5 секунд
        {
            GD.Print($"Zoom progress: {progress:F2}, Size: {newSize:F2}");
        }

        if (progress >= 1.0f)
        {
            _isZooming = false;
            GD.Print("Zooming completed");
        }
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