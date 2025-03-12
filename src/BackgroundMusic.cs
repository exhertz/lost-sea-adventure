using Godot;

public partial class BackgroundMusic : AudioStreamPlayer
{
    [Export] private float _volume = -10.0f; // Громкость в децибелах
    
    public override void _Ready()
    {
        // Устанавливаем громкость
        VolumeDb = _volume;
        
        // Включаем автоматическое воспроизведение в цикле
        Autoplay = true;
        Stream = GD.Load<AudioStream>("res://src/0eb24fcad1bffed.mp3");
        
        // Проигрываем музыку
        Play();
    }
    
    public override void _Process(double delta)
    {
        // Если музыка закончилась, начинаем сначала
        if (!Playing)
        {
            Play();
        }
    }
} 