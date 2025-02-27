using Godot;
using System;

public partial class Camera3d : Camera3D
{
    [Export] public NodePath TargetPath;
    [Export] public float SmoothSpeed = 5.0f;
    [Export] public Vector3 Offset = new Vector3(0, 5, 10); // Смещение камеры относительно корабля
    [Export] public float RotationSmoothSpeed = 3.0f; // Плавность поворота камеры
    [Export] public bool LookAtTarget = true; // Смотреть ли на корабль
    
    private Node3D _target;
    private Vector3 _smoothedPosition;
    
    public override void _Ready()
    {
        if (TargetPath != null && !TargetPath.IsEmpty)
        {
            _target = GetNode<Node3D>(TargetPath);
        }
        else
        {
            // Если путь не указан, попытаемся найти объект player_sphere
            _target = GetNode<Node3D>("/root/Main/player_sphere/Player");
            GD.Print("Camera targeting player_sphere");
        }
        
        // Начальная позиция камеры
        if (_target != null)
        {
            _smoothedPosition = new Vector3(_target.GlobalPosition.X, GlobalPosition.Y, _target.GlobalPosition.Z) + Offset;
            GlobalPosition = _smoothedPosition;
            
            if (LookAtTarget)
            {
                // Смотрим на корабль сразу
                LookAt(_target.GlobalPosition, Vector3.Left);
            }
        }
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (_target == null) return;
        
        // Получаем текущую позицию цели
        Vector3 targetPosition = _target.GlobalPosition;
        
        // Вычисляем желаемую позицию камеры, сохраняя ее высоту (Y)
        Vector3 desiredPosition = new Vector3(
            targetPosition.X, 
            GlobalPosition.Y, 
            targetPosition.Z
        ) + Offset;
        
        // Плавно перемещаем камеру к желаемой позиции
        _smoothedPosition = _smoothedPosition.Lerp(desiredPosition, (float)delta * SmoothSpeed);
        GlobalPosition = _smoothedPosition;
        
        // Если нужно, плавно поворачиваем камеру, чтобы смотреть на корабль
        if (LookAtTarget)
        {
            // Сохраняем текущее направление взгляда
            Vector3 currentForward = -GlobalTransform.Basis.Z;
            
            // Вычисляем желаемое направление взгляда (в сторону корабля, но с сохранением горизонтальности)
            Vector3 targetDirection = targetPosition - GlobalPosition;
            targetDirection.Y = 0; // Обнуляем вертикальную составляющую для сохранения горизонтального взгляда
            
            if (targetDirection.LengthSquared() > 0.001f)
            {
                targetDirection = targetDirection.Normalized();
                
                // Плавно интерполируем между текущим направлением и желаемым
                Vector3 newForward = currentForward.Lerp(targetDirection, (float)delta * RotationSmoothSpeed).Normalized();
                
                // Смотрим в новом направлении
                Vector3 lookAtPoint = GlobalPosition + newForward * 10.0f;
                lookAtPoint.Y = targetPosition.Y; // Устанавливаем высоту точки обзора равной высоте цели
                
                LookAt(lookAtPoint, Vector3.Left);
            }
        }
    }
}