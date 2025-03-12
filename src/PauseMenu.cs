using Godot;

public partial class PauseMenu : Control
{
    private bool _isPaused = false;
    private Panel _overlay;
    private StyleBoxFlat _overlayStyle;
    private const float FADE_DURATION = 0.3f;
    private float _fadeTimer = 0f;
    private bool _isFading = false;
    private bool _fadeIn = false;
    
    private AudioStreamPlayer _musicPlayer;
    private HSlider _volumeSlider;
    private CheckButton _musicToggle;
    
    public override void _Ready()
    {
        Hide(); // Скрываем меню при старте
        _overlay = GetNode<Panel>("Overlay");
        _overlayStyle = (StyleBoxFlat)_overlay.GetThemeStylebox("panel");
        
        // Получаем ссылки на элементы управления музыкой
        _musicPlayer = GetNode<AudioStreamPlayer>("/root/Main/BackgroundMusic");
        _volumeSlider = GetNode<HSlider>("Panel/VBoxContainer/VolumeControl/VolumeSlider");
        _musicToggle = GetNode<CheckButton>("Panel/VBoxContainer/MusicToggle");
        
        // Устанавливаем начальные значения
        if (_musicPlayer != null)
        {
            _volumeSlider.Value = Mathf.Pow(10.0f, _musicPlayer.VolumeDb / 20.0f);
            _musicToggle.ButtonPressed = !_musicPlayer.StreamPaused;
        }
        
        // Начинаем с полностью прозрачного оверлея
        var color = _overlayStyle.BgColor;
        color.A = 0;
        _overlayStyle.BgColor = color;
    }

    public override void _Process(double delta)
    {
        if (_isFading)
        {
            _fadeTimer += (float)delta;
            float progress = Mathf.Clamp(_fadeTimer / FADE_DURATION, 0.0f, 1.0f);
            
            float easeProgress = Mathf.SmoothStep(0.0f, 1.0f, progress);
            var color = _overlayStyle.BgColor;
            color.A = _fadeIn ? easeProgress * 0.784314f : (1 - progress) * 0.784314f;
            _overlayStyle.BgColor = color;

            if (progress >= 1.0f)
            {
                _isFading = false;
                _fadeTimer = 0f;
                if (!_fadeIn)
                {
                    Hide();
                }
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel")) // ESC
        {
            if (!_isFading) // Предотвращаем двойное нажатие во время анимации
            {
                _isPaused = !_isPaused;
                
                if (_isPaused)
                {
                    Show();
                    StartFade(true);
                    GetTree().Paused = true;
                }
                else
                {
                    StartFade(false);
                    GetTree().Paused = false;
                }
            }
        }
    }

    private void StartFade(bool fadeIn)
    {
        _fadeIn = fadeIn;
        _isFading = true;
        _fadeTimer = 0f;
    }

    // Обработчик кнопки "Продолжить"
    private void _on_continue_button_pressed()
    {
        _isPaused = false;
        StartFade(false);
        GetTree().Paused = false;
    }

    // Обработчик кнопки "Главное меню"
    private void _on_main_menu_button_pressed()
    {
        _isPaused = false;
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://main_menu.tscn");
    }

    // Обработчик кнопки "Выход"
    private void _on_quit_button_pressed()
    {
        GetTree().Quit();
    }
    
    // Обработчики для управления музыкой
    private void _on_volume_slider_value_changed(double value)
    {
        if (_musicPlayer != null)
        {
            _musicPlayer.VolumeDb = 20.0f * Mathf.Log((float)value) / Mathf.Log(10.0f);
        }
    }
    
    private void _on_music_toggle_toggled(bool buttonPressed)
    {
        if (_musicPlayer != null)
        {
            _musicPlayer.StreamPaused = !buttonPressed;
        }
    }
} 