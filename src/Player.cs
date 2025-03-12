using Godot;
using System.Collections.Generic;
using System;

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
    private bool _isFishing = false; // Флаг, указывающий на то, идет ли рыбалка
    private int _fishCount = 0; // Количество пойманной рыбы
    private Label _fishingPrompt; // Ссылка на FishingPrompt
    private Label _fishingCount; // Ссылка на FishingCount
    private Random _random = new Random(); // Для генерации случайных чисел
    private float _nextBiteTime = 0.0f; // Время до следующего клёва
    private float _biteTimer = 0.0f; // Таймер для отслеживания времени до клёва
    private float _penaltyTime = 0.0f; // Время штрафа за слишком частое нажатие
    private bool _isPenalty = false; // Флаг, указывающий на то, наложен ли штраф
    private float _lastCatchTime = 0.0f; // Время последнего нажатия пробела

    public override void _Ready()
    {
        _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
        _water = GetNode<Node3D>("/root/Main/Water");
        _fishingPrompt = GetNode<Label>("/root/Main/UI/FishingPrompt");
        _fishingCount = GetNode<Label>("/root/Main/UI/FishingCount");

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
        HandleFishing(delta);
        _fishingCount.Text = $"Рыб: {_fishCount}";

        // Проверка на частое нажатие пробела
        if (Input.IsActionJustPressed("fishing_catch"))
        {
            float currentTime = Time.GetTicksMsec() / 1000.0f;
            if (currentTime - _lastCatchTime < 1.0f)
            {
                _fishingPrompt.Text = "Подсекайте не так быстро!";
                _penaltyTime = 5.0f; // 5 секунд штрафа
                _isPenalty = true;
            }
            _lastCatchTime = currentTime;
        }
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

    private void HandleFishing(double delta)
    {
        if (Input.IsActionJustPressed("fishing_start"))
        {
            _isFishing = !_isFishing;
            _fishingPrompt.Text = _isFishing ? "Началась рыбалка" : "";
            if (_isFishing)
            {
                _nextBiteTime = (float)_random.NextDouble() * 5.0f + 2.0f;
                _biteTimer = 0.0f;
                _isPenalty = false;
            }
        }

        if (_isFishing)
        {
            _biteTimer += (float)delta;
            if (_isPenalty)
            {
                _penaltyTime -= (float)delta;
                if (_penaltyTime <= 0)
                {
                    _isPenalty = false;
                    _fishingPrompt.Text = "Штраф снят, ждите клёва!";
                }
            }
            else if (_biteTimer >= _nextBiteTime)
            {
                _fishingPrompt.Text = "Клюёт!";
                if (Input.IsActionJustPressed("fishing_catch") && !_isPenalty)
                {
                    if (_random.NextDouble() < 0.1) // 10% шанс на большую рыбу
                    {
                        _fishCount += 2;
                        _fishingPrompt.Text = "Большая рыба поймана!";
                    }
                    else if (_random.NextDouble() < 0.05) // 5% шанс на редкую рыбу
                    {
                        _fishCount += 3;
                        _fishingPrompt.Text = "Редкая рыба поймана!";
                    }
                    else
                    {
                        _fishCount++;
                        _fishingPrompt.Text = "Поймана рыба!";
                    }
                    _fishingCount.Text = $"Рыб: {_fishCount}";
                    _nextBiteTime = (float)_random.NextDouble() * 5.0f + 2.0f;
                    _biteTimer = 0.0f;
                }
                else if (_biteTimer >= _nextBiteTime + 1.0f)
                {
                    _fishingPrompt.Text = "Рыба сорвалась!";
                    _nextBiteTime = (float)_random.NextDouble() * 5.0f + 2.0f;
                    _biteTimer = 0.0f;
                }
            }
        }
    }
}