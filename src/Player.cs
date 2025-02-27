using Godot;
using System.Collections.Generic;

public partial class Player : RigidBody3D
{
    [Export] public float FloatForce = 1.0f; // Сила выталкивания из воды
    [Export] public float WaterDrag = 0.04f; // Сопротивление воды (замедление лодки)
    [Export] public float WaterAngularDrag = 0.06f; // Сопротивление воды на поворот
    [Export] public float ForwardSpeed = 90.0f; // Скорость вперед 100
    [Export] public float BackwardSpeed = 65.0f; // Скорость назад 75
    [Export] public float RotationSpeed = 75.0f; // Скорость разворота 85

    [Export] public float Acceleration = 0.5f; // Ускорение для плавного разгона

    private float _gravity;
    private Node3D _water;
    private List<Node3D> _probes = new List<Node3D>();
    private bool _submerged = false;

    private float _currentSpeed = 0.0f; // Текущая скорость лодки

    public override void _Ready()
    {
        _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
        _water = GetNode<Node3D>("/root/Main/Water");

        foreach (Node3D probe in GetNode("ProbeContainer").GetChildren())
        {
            _probes.Add(probe);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        _submerged = false;

        foreach (Node3D p in _probes)
        {
            float depth = GetWaterHeight(p.GlobalPosition) - p.GlobalPosition.Y;

            if (depth > 0)
            {
                _submerged = true;
                ApplyForce(Vector3.Up * FloatForce * _gravity * depth, p.GlobalPosition - GlobalPosition);
            }
        }

        HandleMovement(delta);
    }

    private void HandleMovement(double delta)
    {
        if (Input.IsActionPressed("ui_up")) // W
        {
            // ApplyCentralForce(-Transform.Basis.Z * ForwardSpeed);
            _currentSpeed = (float)Mathf.Lerp(_currentSpeed, ForwardSpeed, Acceleration * delta);
        } else if (Input.IsActionPressed("ui_down")) // S
        {
            // ApplyCentralForce(Transform.Basis.Z * BackwardSpeed);
            _currentSpeed = (float)Mathf.Lerp(_currentSpeed, -BackwardSpeed, Acceleration * delta);
        } else // (generated LLM)
        {
            _currentSpeed = (float)Mathf.Lerp(_currentSpeed, 0, WaterDrag * delta);
        }

        if (_currentSpeed != 0)
        {
            ApplyCentralForce(-Transform.Basis.Z * _currentSpeed);
        }

        if (Input.IsActionPressed("ui_left")) // A
        {
            // ApplyTorque(Vector3.Up * RotationSpeed);
            ApplyTorque(Vector3.Up * RotationSpeed * _currentSpeed / ForwardSpeed); // (generated LLM)
        }
        if (Input.IsActionPressed("ui_right")) // D
        {
            // ApplyTorque(Vector3.Down * RotationSpeed);
            ApplyTorque(Vector3.Down * RotationSpeed * _currentSpeed / ForwardSpeed); // (generated LLM)
        }
    }

    private float GetWaterHeight(Vector3 position)
    {
        return (float)_water.Call("get_height", position);
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        if (_submerged)
        {
            state.LinearVelocity *= 1.0f - WaterDrag;
            state.AngularVelocity *= 1.0f - WaterAngularDrag;
        }
    }
}