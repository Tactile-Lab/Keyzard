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
    private const string DungeonClipPath = "Assets/Audio/Music/07. Surt R. -- Tower Halls [Lower Floors].mp3";
    private const string CombatClipPath = "Assets/Audio/Music/07. Surt R. -- Tower Halls [Lower Floors].mp3";
    private const string EndDemoClipPath = "Assets/Audio/Music/01. Surt R. -- The Devil Tower.mp3";

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

        // Charger les clips de base (fallback sur dungeon si un clip manque)
        var dungeonClip = AssetDatabase.LoadAssetAtPath<AudioClip>(DungeonClipPath);
        var combatClip = AssetDatabase.LoadAssetAtPath<AudioClip>(CombatClipPath);
        var endDemoClip = AssetDatabase.LoadAssetAtPath<AudioClip>(EndDemoClipPath);

        if (dungeonClip == null)
        {
            Debug.LogWarning($"[MusicConfigSetup] Clip introuvable : {DungeonClipPath}. L'etat Dungeon restera sans clip.");
        }

        if (combatClip == null)
        {
            combatClip = dungeonClip;
            Debug.LogWarning($"[MusicConfigSetup] Clip introuvable : {CombatClipPath}. Combat utilisera Dungeon en fallback.");
        }

        if (endDemoClip == null)
        {
            endDemoClip = dungeonClip;
            Debug.LogWarning($"[MusicConfigSetup] Clip introuvable : {EndDemoClipPath}. EndDemo utilisera Dungeon en fallback.");
        }

        // Construire/mettre a jour les 5 entrees sans ecraser les clips deja assignes manuellement.
        UpsertEntry(config, GameMusicState.MainMenu, null, false, 1f, keepExistingClip: true);
        UpsertEntry(config, GameMusicState.Dungeon, dungeonClip, true, 1f, keepExistingClip: true);
        UpsertEntry(config, GameMusicState.Combat, combatClip, true, 1f, keepExistingClip: true);
        UpsertEntry(config, GameMusicState.GameOver, null, false, 1f, keepExistingClip: true);
        UpsertEntry(config, GameMusicState.EndDemo, endDemoClip, false, 1f, keepExistingClip: true);

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
            "• Volumes individuels actifs sur chaque etat\n" +
            "• Dungeon, Combat et EndDemo ont un clip par defaut (avec fallback)\n" +
            "• MainMenu et GameOver restent personnalisables\n\n" +
            "L'AudioManager dans les scènes ouvertes a été wiré automatiquement.",
            "OK"
        );
    }

    private static void UpsertEntry(
        MusicAudioConfig config,
        GameMusicState state,
        AudioClip defaultClip,
        bool persistInBackground,
        float volume,
        bool keepExistingClip)
    {
        MusicAudioEntry entry = config.GetEntry(state);
        if (entry == null)
        {
            entry = new MusicAudioEntry { state = state };
            config.entries.Add(entry);
        }

        if (!keepExistingClip || entry.clip == null)
        {
            entry.clip = defaultClip;
        }

        entry.persistInBackground = persistInBackground;
        entry.volume = Mathf.Clamp01(volume);
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
