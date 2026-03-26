using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tools > Audio > Create Music Config
/// Crée ou recrée l'asset MusicAudioConfig avec les 5 états de musique
/// et wire automatiquement la référence sur l'AudioManager dans toutes les scènes ouvertes.
/// </summary>
public static class MusicConfigSetup
{
    private const string AssetPath = "Assets/AudioConfigs/MusicAudioConfig.asset";
    private const string MusicClipPath = "Assets/Audio/Music/07. Surt R. -- Tower Halls [Lower Floors].mp3";

    [MenuItem("Tools/Audio/Create Music Config")]
    public static void CreateMusicConfig()
    {
        // Créer le dossier si besoin
        if (!AssetDatabase.IsValidFolder("Assets/AudioConfigs"))
        {
            AssetDatabase.CreateFolder("Assets", "AudioConfigs");
        }

        // Charger ou créer l'asset
        var config = AssetDatabase.LoadAssetAtPath<MusicAudioConfig>(AssetPath);
        bool isNew = config == null;
        if (isNew)
        {
            config = ScriptableObject.CreateInstance<MusicAudioConfig>();
        }

        // Charger le seul clip musique disponible
        var dungeonClip = AssetDatabase.LoadAssetAtPath<AudioClip>(MusicClipPath);
        if (dungeonClip == null)
        {
            Debug.LogWarning($"[MusicConfigSetup] Clip introuvable : {MusicClipPath}. L'état Dungeon restera sans clip.");
        }

        // Construire les 5 entrées
        config.entries.Clear();
        config.entries.Add(new MusicAudioEntry { state = GameMusicState.MainMenu,  clip = null,       persistInBackground = false });
        config.entries.Add(new MusicAudioEntry { state = GameMusicState.Dungeon,   clip = dungeonClip, persistInBackground = true  });
        config.entries.Add(new MusicAudioEntry { state = GameMusicState.Combat,    clip = null,       persistInBackground = true  });
        config.entries.Add(new MusicAudioEntry { state = GameMusicState.GameOver,  clip = null,       persistInBackground = false });
        config.entries.Add(new MusicAudioEntry { state = GameMusicState.EndDemo,   clip = null,       persistInBackground = false });

        if (isNew)
        {
            AssetDatabase.CreateAsset(config, AssetPath);
        }
        else
        {
            EditorUtility.SetDirty(config);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[MusicConfigSetup] Asset créé/mis à jour : {AssetPath}");

        // Wire dans l'AudioManager de toutes les scènes ouvertes
        WireAudioManagerInOpenScenes(config);

        // Sélectionner l'asset dans l'Inspector
        Selection.activeObject = config;
        EditorGUIUtility.PingObject(config);

        EditorUtility.DisplayDialog(
            "Music Config créé",
            $"Asset créé : {AssetPath}\n\n" +
            "• Dungeon → Tower Halls (clip assigné)\n" +
            "• MainMenu, Combat, GameOver, EndDemo → null (à assigner quand les clips seront prêts)\n\n" +
            "L'AudioManager dans les scènes ouvertes a été wiré automatiquement.",
            "OK"
        );
    }

    private static void WireAudioManagerInOpenScenes(MusicAudioConfig config)
    {
        var managers = Object.FindObjectsByType<AudioManager>(FindObjectsSortMode.None);
        if (managers.Length == 0)
        {
            Debug.LogWarning("[MusicConfigSetup] Aucun AudioManager trouvé dans les scènes ouvertes. Ouvre ta scène principale et relance le menu.");
            return;
        }

        foreach (var manager in managers)
        {
            var so = new SerializedObject(manager);
            var prop = so.FindProperty("musicConfig");
            if (prop != null)
            {
                prop.objectReferenceValue = config;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(manager);
                Debug.Log($"[MusicConfigSetup] AudioManager wiré dans '{manager.gameObject.scene.name}'");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
    }
}
