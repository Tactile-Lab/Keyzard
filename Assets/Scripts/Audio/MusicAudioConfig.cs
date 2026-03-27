using System.Collections.Generic;
using UnityEngine;

public enum GameMusicState
{
    MainMenu,
    Dungeon,
    Combat,
    GameOver,
    EndDemo
}

[System.Serializable]
public class MusicAudioEntry
{
    public GameMusicState state;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Tooltip("Si true : reprend là où on s'était arrêté. Si false : repart toujours du début.")]
    public bool persistInBackground;
}

[CreateAssetMenu(fileName = "MusicAudioConfig", menuName = "Audio/MusicAudioConfig")]
public class MusicAudioConfig : ScriptableObject
{
    public List<MusicAudioEntry> entries = new List<MusicAudioEntry>();

    public MusicAudioEntry GetEntry(GameMusicState state)
    {
        foreach (var entry in entries)
        {
            if (entry.state == state) return entry;
        }
        return null;
    }

    public float GetEntryVolume(GameMusicState state)
    {
        MusicAudioEntry entry = GetEntry(state);
        if (entry == null)
        {
            return 1f;
        }

        return Mathf.Clamp01(entry.volume);
    }
}
