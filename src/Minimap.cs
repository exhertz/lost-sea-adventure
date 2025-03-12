using Godot;

public partial class Minimap : SubViewportContainer
{
    [Export] private NodePath _playerPath;
    [Export] private float _zoom = 80.0f; // Масштаб мини-карты
    
    private Node3D _player;
    private Camera3D _minimapCamera;
    
    public override void _Ready()
    {
        _player = GetNode<Node3D>(_playerPath);
        _minimapCamera = GetNode<Camera3D>("SubViewport/Camera3D");
        
        // Настраиваем размер и положение мини-карты
        CustomMinimumSize = new Vector2(200, 200); // Размер мини-карты
        SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        SizeFlagsVertical = SizeFlags.ShrinkBegin;
        
        // Настраиваем камеру мини-карты
        _minimapCamera.Projection = Camera3D.ProjectionType.Orthogonal;
        _minimapCamera.Size = _zoom;
    }
    
    public override void _Process(double delta)
    {
        if (_player == null || _minimapCamera == null)
            return;
            
        // Обновляем позицию камеры мини-карты, чтобы следовать за игроком
        Vector3 newPosition = _player.Position;
        newPosition.Y = _minimapCamera.Position.Y; // Сохраняем высоту камеры
        _minimapCamera.Position = newPosition;
    }
} 