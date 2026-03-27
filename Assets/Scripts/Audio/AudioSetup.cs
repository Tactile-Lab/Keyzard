using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
public class AudioSetup : MonoBehaviour
{
    [MenuItem("Tools/Audio/Setup Complete Audio System")]
    public static void SetupCompleteAudioSystem()
    {
        // 1. Créer l'AudioManager dans la scène
        CreateAudioManager();
        
        // 2. Créer l'AudioMixer
        CreateAudioMixer();
        
        // 3. Créer les dossiers nécessaires
        CreateAudioFolders();
        
        // 4. Créer des exemples de configurations
        CreateExampleAudioConfigs();

        // 5. Assigner les configs SFX a l'AudioManager
        AssignSfxConfigsToAudioManager();
        
        Debug.Log("✅ Système audio configuré automatiquement !");
        Debug.Log("🎵 AudioManager créé dans la scène");
        Debug.Log("🎚️ AudioMixer créé dans Assets/Audio");
        Debug.Log("📁 Dossiers audio créés");
        Debug.Log("🎯 Exemples de configurations créés");
    }

    private static void AssignSfxConfigsToAudioManager()
    {
        AudioManager audioManager = FindFirstObjectByType<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogWarning("⚠️ AudioManager introuvable pour l'assignation des configs SFX.");
            return;
        }

        audioManager.sfxEventConfig = AssetDatabase.LoadAssetAtPath<SFXEventAudioConfig>("Assets/AudioConfigs/Global/SFXEventAudioConfig.asset");
        audioManager.uiSfxConfig = AssetDatabase.LoadAssetAtPath<UISFXConfig>("Assets/AudioConfigs/UI/UISFXConfig.asset");
        audioManager.typingSfxConfig = AssetDatabase.LoadAssetAtPath<TypingSFXConfig>("Assets/AudioConfigs/Typing/TypingSFXConfig.asset");
        audioManager.playerSfxConfig = AssetDatabase.LoadAssetAtPath<PlayerSFXConfig>("Assets/AudioConfigs/Player/PlayerSFXConfig.asset");
        audioManager.enemySfxConfig = AssetDatabase.LoadAssetAtPath<EnemySFXConfig>("Assets/AudioConfigs/Enemies/EnemySFXConfig.asset");

        EditorUtility.SetDirty(audioManager);
        AssetDatabase.SaveAssets();

        Debug.Log("✅ Configs SFX assignées automatiquement à AudioManager.");
    }
    
    private static void CreateAudioManager()
    {
        // Vérifier si l'AudioManager existe déjà
        var existingManager = FindFirstObjectByType<AudioManager>();
        if (existingManager != null)
        {
            Debug.Log("⚠️ AudioManager existe déjà dans la scène");
            return;
        }
        
        // Créer un nouveau GameObject avec l'AudioManager
        GameObject audioManagerGO = new GameObject("AudioManager");
        audioManagerGO.AddComponent<AudioManager>();
        
        // Positionner correctement dans la hiérarchie
        audioManagerGO.transform.SetAsFirstSibling();
        
        Debug.Log("🎵 AudioManager créé dans la scène");
    }
    
    private static void CreateAudioMixer()
    {
        string audioPath = "Assets/Audio";
        
        // Créer le dossier Audio s'il n'existe pas
        if (!AssetDatabase.IsValidFolder(audioPath))
        {
            AssetDatabase.CreateFolder("Assets", "Audio");
        }
        
        string mixerPath = audioPath + "/GameAudioMixer.mixer";
        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(mixerPath);
        
        if (mixer == null)
        {
            // Créer l'AudioMixer manuellement dans l'éditeur Unity
            // L'AudioMixer doit être créé via l'interface Unity
            Debug.Log("⚠️ Veuillez créer manuellement l'AudioMixer dans Unity:");
            Debug.Log("1. Cliquez-droit dans Assets/Audio");
            Debug.Log("2. Create > Audio Mixer");
            Debug.Log("3. Nommez-le 'GameAudioMixer'");
            Debug.Log("4. Configurez les groupes Master/Music/SFX");
            return;
        }
        
        // Configurer les volumes par défaut
        mixer.SetFloat("MasterVolume", 0f);
        mixer.SetFloat("MusicVolume", -5f);
        mixer.SetFloat("SFXVolume", 0f);
        
        // Assigner l'AudioMixer à l'AudioManager
        AssignMixerToAudioManager(mixerPath);
        
        Debug.Log("🎚️ AudioMixer configuré: " + mixerPath);
    }
    
    private static void CreateMixerGroup(AudioMixer mixer, string groupName)
    {
        // Créer le groupe
        AudioMixerGroup group = mixer.FindMatchingGroups(groupName)[0];
        if (group == null)
        {
            // Si le groupe n'existe pas, le créer via une méthode alternative
            // Les groupes sont créés automatiquement avec des noms standard
            Debug.Log("⚠️ Groupe " + groupName + " créé automatiquement par Unity");
        }
    }
    
    private static void AssignMixerToAudioManager(string mixerPath)
    {
        // Charger l'AudioMixer
        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(mixerPath);
        
        if (mixer != null)
        {
            // Trouver l'AudioManager dans la scène
        var audioManager = FindFirstObjectByType<AudioManager>();
            if (audioManager != null)
            {
                // Assigner les groupes
                var groups = mixer.FindMatchingGroups("");
                foreach (var group in groups)
                {
                    switch (group.name)
                    {
                        case "Master":
                            audioManager.masterGroup = group;
                            break;
                        case "Music":
                            audioManager.musicGroup = group;
                            break;
                        case "SFX":
                            audioManager.sfxGroup = group;
                            break;
                    }
                }
                
                // Marquer comme modifié
                EditorUtility.SetDirty(audioManager);
                Debug.Log("✅ AudioMixer assigné à l'AudioManager");
            }
        }
    }
    
    private static void CreateAudioFolders()
    {
        string[] folders = {
            "Assets/Audio/SFX",
            "Assets/Audio/Music", 
            "Assets/Audio/UI",
            "Assets/AudioConfigs",
            "Assets/AudioConfigs/Global",
            "Assets/AudioConfigs/Typing",
            "Assets/AudioConfigs/Player",
            "Assets/AudioConfigs/Sorts",
            "Assets/AudioConfigs/Enemies",
            "Assets/AudioConfigs/UI"
        };
        
        foreach (string folder in folders)
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string parentFolder = Path.GetDirectoryName(folder);
                string folderName = Path.GetFileName(folder);
                
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    string grandParent = Path.GetDirectoryName(parentFolder);
                    string parentName = Path.GetFileName(parentFolder);
                    AssetDatabase.CreateFolder(grandParent, parentName);
                }
                
                AssetDatabase.CreateFolder(parentFolder, folderName);
                Debug.Log("📁 Dossier créé: " + folder);
            }
        }
    }
    
    private static void CreateExampleAudioConfigs()
    {
        CreateGlobalSfxEventConfig();
        CreateDomainSfxConfigs();

        string configsPath = "Assets/AudioConfigs/Sorts";
        
        // Créer des exemples de configurations pour vos sorts existants
        CreateSpellAudioConfig(configsPath, "BouleMagique");
        CreateSpellAudioConfig(configsPath, "LanceFantome");
        CreateSpellAudioConfig(configsPath, "RingEau");
        CreateSpellAudioConfig(configsPath, "ShotGunFeu");
        
        Debug.Log("🎯 Exemples de configurations créés");
    }

    private static void CreateGlobalSfxEventConfig()
    {
        string configPath = "Assets/AudioConfigs/Global/SFXEventAudioConfig.asset";
        SFXEventAudioConfig existing = AssetDatabase.LoadAssetAtPath<SFXEventAudioConfig>(configPath);
        if (existing != null)
        {
            SanitizeMiscEntries(existing.entries);
            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();
            Debug.Log("🔊 Configuration globale SFX nettoyée (Divers uniquement): " + configPath);
            return;
        }

        SFXEventAudioConfig config = ScriptableObject.CreateInstance<SFXEventAudioConfig>();

        PopulateEntries(config.entries, key => SFXEventAudioConfig.IsMiscKey(key));

        AssetDatabase.CreateAsset(config, configPath);
        AssetDatabase.SaveAssets();
        Debug.Log("🔊 Configuration globale SFX créée: " + configPath);
    }

    private static void CreateDomainSfxConfigs()
    {
        CreateDomainConfig<UISFXConfig>(
            "Assets/AudioConfigs/UI/UISFXConfig.asset",
            key => IsUiKey(key));

        CreateDomainConfig<TypingSFXConfig>(
            "Assets/AudioConfigs/Typing/TypingSFXConfig.asset",
            key => IsTypingKey(key));

        CreateDomainConfig<PlayerSFXConfig>(
            "Assets/AudioConfigs/Player/PlayerSFXConfig.asset",
            key => IsPlayerKey(key));

        CreateDomainConfig<EnemySFXConfig>(
            "Assets/AudioConfigs/Enemies/EnemySFXConfig.asset",
            key => IsEnemyKey(key));
    }

    private static void CreateDomainConfig<T>(string configPath, System.Func<SFXEventKey, bool> predicate)
        where T : DomainSFXAudioConfig
    {
        T existing = AssetDatabase.LoadAssetAtPath<T>(configPath);
        if (existing != null)
        {
            SanitizeDomainEntries(existing.entries, predicate);
            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();
            return;
        }

        T config = ScriptableObject.CreateInstance<T>();
        PopulateEntries(config.entries, predicate);
        AssetDatabase.CreateAsset(config, configPath);
        AssetDatabase.SaveAssets();

        Debug.Log("🔊 Configuration SFX domaine créée: " + configPath);
    }

    private static void PopulateEntries(System.Collections.Generic.List<SFXEventAudioEntry> entries, System.Func<SFXEventKey, bool> predicate)
    {
        foreach (SFXEventKey key in System.Enum.GetValues(typeof(SFXEventKey)))
        {
            if (key == SFXEventKey.None || !predicate(key))
            {
                continue;
            }

            entries.Add(new SFXEventAudioEntry
            {
                key = key,
                clip = null,
                volume = 1f,
                pitch = 1f,
                randomPitchVariance = 0f
            });
        }
    }

    private static bool IsUiKey(SFXEventKey key)
    {
        switch (key)
        {
            case SFXEventKey.UIMenuMove:
            case SFXEventKey.UIMenuConfirm:
            case SFXEventKey.UIOpen:
            case SFXEventKey.UIClose:
            case SFXEventKey.UISceneTransition:
            case SFXEventKey.UIGlossaryOpen:
            case SFXEventKey.UIGlossaryClose:
                return true;
            default:
                return false;
        }
    }

    private static bool IsTypingKey(SFXEventKey key)
    {
        switch (key)
        {
            case SFXEventKey.TypingHit:
            case SFXEventKey.TypingMiss:
            case SFXEventKey.TypingWordFail:
            case SFXEventKey.TypingTargetLock:
            case SFXEventKey.TypingSpellReady:
            case SFXEventKey.TypingSpellCleared:
                return true;
            default:
                return false;
        }
    }

    private static bool IsPlayerKey(SFXEventKey key)
    {
        switch (key)
        {
            case SFXEventKey.PlayerFootstep:
            case SFXEventKey.PlayerHurt:
            case SFXEventKey.PlayerDeath:
                return true;
            default:
                return false;
        }
    }

    private static bool IsEnemyKey(SFXEventKey key)
    {
        switch (key)
        {
            case SFXEventKey.EnemyAttack:
            case SFXEventKey.EnemyHurt:
            case SFXEventKey.EnemyDeath:
            case SFXEventKey.EnemyFootstep:
            case SFXEventKey.EnemyRapideAttack:
            case SFXEventKey.EnemyRapideHurt:
            case SFXEventKey.EnemyRapideDeath:
            case SFXEventKey.EnemyRapideFootstep:
            case SFXEventKey.EnemyLourdAttack:
            case SFXEventKey.EnemyLourdHurt:
            case SFXEventKey.EnemyLourdDeath:
            case SFXEventKey.EnemyLourdFootstep:
            case SFXEventKey.EnemyDistantAttack:
            case SFXEventKey.EnemyDistantHurt:
            case SFXEventKey.EnemyDistantDeath:
                return true;
            default:
                return false;
        }
    }

    private static bool IsMiscKey(SFXEventKey key)
    {
        return SFXEventAudioConfig.IsMiscKey(key);
    }

    private static void SanitizeMiscEntries(System.Collections.Generic.List<SFXEventAudioEntry> entries)
    {
        SanitizeDomainEntries(entries, key => SFXEventAudioConfig.IsMiscKey(key));
    }

    private static void SanitizeDomainEntries(System.Collections.Generic.List<SFXEventAudioEntry> entries, System.Func<SFXEventKey, bool> predicate)
    {
        if (entries == null)
        {
            return;
        }

        for (int i = entries.Count - 1; i >= 0; i--)
        {
            SFXEventAudioEntry entry = entries[i];
            if (entry == null || entry.key == SFXEventKey.None || !predicate(entry.key))
            {
                entries.RemoveAt(i);
            }
        }

        foreach (SFXEventKey key in System.Enum.GetValues(typeof(SFXEventKey)))
        {
            if (key == SFXEventKey.None || !predicate(key))
            {
                continue;
            }

            bool exists = false;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && entries[i].key == key)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                entries.Add(new SFXEventAudioEntry
                {
                    key = key,
                    clip = null,
                    volume = 1f,
                    pitch = 1f,
                    randomPitchVariance = 0f
                });
            }
        }
    }
    
    private static void CreateSpellAudioConfig(string folderPath, string spellName)
    {
        string configPath = folderPath + "/" + spellName + "Audio.asset";
        
        // Vérifier si la configuration existe déjà
        if (AssetDatabase.LoadAssetAtPath<SpellAudioConfig>(configPath) != null)
        {
            return;
        }
        
        // Créer la configuration
        SpellAudioConfig config = ScriptableObject.CreateInstance<SpellAudioConfig>();
        config.spellName = spellName;
        
        AssetDatabase.CreateAsset(config, configPath);
        AssetDatabase.SaveAssets();
        
        Debug.Log("🔊 Configuration créée: " + configPath);
    }
}
#endif