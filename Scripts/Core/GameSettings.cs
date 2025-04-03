using Godot;

namespace ChronosDescent.Scripts.Core;

/// <summary>
/// Manages game settings such as audio, video, and controls
/// </summary>
public static class GameSettings
{
    private const string SettingsFile = "user://settings.cfg";
    private const string AudioSection = "audio";
    private const string VideoSection = "video";
    private const string ControlsSection = "controls";
    
    // Default settings
    private const float DefaultMusicVolume = 0.8f;
    private const float DefaultSfxVolume = 1.0f;
    private const bool DefaultFullscreen = false;
    
    // Audio settings
    public static float GetMusicVolume()
    {
        var config = new ConfigFile();
        var err = config.Load(SettingsFile);
        
        if (err == Error.Ok && config.HasSectionKey(AudioSection, "music_volume"))
        {
            return (float)config.GetValue(AudioSection, "music_volume");
        }
        
        return DefaultMusicVolume;
    }
    
    public static void SetMusicVolume(float volume)
    {
        // Clamp volume between 0 and 1
        volume = Mathf.Clamp(volume, 0f, 1f);
        
        // Apply the volume to AudioServer
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), Mathf.LinearToDb(volume));
    }
    
    public static float GetSFXVolume()
    {
        var config = new ConfigFile();
        var err = config.Load(SettingsFile);
        
        if (err == Error.Ok && config.HasSectionKey(AudioSection, "sfx_volume"))
        {
            return (float)config.GetValue(AudioSection, "sfx_volume");
        }
        
        return DefaultSfxVolume;
    }
    
    public static void SetSFXVolume(float volume)
    {
        // Clamp volume between 0 and 1
        volume = Mathf.Clamp(volume, 0f, 1f);
        
        // Apply the volume to AudioServer
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), Mathf.LinearToDb(volume));
    }
    
    // Video settings
    public static bool GetFullscreen()
    {
        var config = new ConfigFile();
        var err = config.Load(SettingsFile);
        
        if (err == Error.Ok && config.HasSectionKey(VideoSection, "fullscreen"))
        {
            return (bool)config.GetValue(VideoSection, "fullscreen");
        }
        
        return DefaultFullscreen;
    }
    
    public static void SetFullscreen(bool fullscreen)
    {
        DisplayServer.WindowSetMode(fullscreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
    }
    
    // Save all settings
    public static void SaveSettings()
    {
        var config = new ConfigFile();
        
        // Try to load existing settings first
        config.Load(SettingsFile);
        
        // Audio settings
        var musicVolume = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Music"));
        var sfxVolume = AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("SFX"));
        config.SetValue(AudioSection, "music_volume", Mathf.DbToLinear(musicVolume));
        config.SetValue(AudioSection, "sfx_volume", Mathf.DbToLinear(sfxVolume));
        
        // Video settings
        config.SetValue(VideoSection, "fullscreen", DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen);
        
        // Save the config file
        config.Save(SettingsFile);
    }
    
    // Reset all settings to default
    public static void ResetToDefault()
    {
        // Audio settings
        SetMusicVolume(DefaultMusicVolume);
        SetSFXVolume(DefaultSfxVolume);
        
        // Video settings
        SetFullscreen(DefaultFullscreen);
        
        // Save the settings
        SaveSettings();
    }
    
    // Initialize settings on game startup
    public static void Initialize()
    {
        // Load and apply saved settings
        SetMusicVolume(GetMusicVolume());
        SetSFXVolume(GetSFXVolume());
        SetFullscreen(GetFullscreen());
    }
}
