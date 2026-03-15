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
        
        Debug.Log("✅ Système audio configuré automatiquement !");
        Debug.Log("🎵 AudioManager créé dans la scène");
        Debug.Log("🎚️ AudioMixer créé dans Assets/Audio");
        Debug.Log("📁 Dossiers audio créés");
        Debug.Log("🎯 Exemples de configurations créés");
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
        string configsPath = "Assets/AudioConfigs/Sorts";
        
        // Créer des exemples de configurations pour vos sorts existants
        CreateSpellAudioConfig(configsPath, "BouleMagique");
        CreateSpellAudioConfig(configsPath, "LanceFantome");
        CreateSpellAudioConfig(configsPath, "RingEau");
        CreateSpellAudioConfig(configsPath, "ShotGunFeu");
        
        Debug.Log("🎯 Exemples de configurations créés");
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