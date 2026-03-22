using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerSetup : EditorWindow
{
    [MenuItem("Tools/Audio/Setup Audio Mixer Groups")]
    public static void SetupAudioMixerGroups()
    {
        // Charger l'Audio Mixer existant
        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/Audio/GameAudioMixer.mixer");
        
        if (mixer == null)
        {
            Debug.LogError("Audio Mixer non trouvé à: Assets/Audio/GameAudioMixer.mixer");
            return;
        }

        // Obtenir le groupe Master
        AudioMixerGroup[] groups = mixer.FindMatchingGroups("");
        AudioMixerGroup masterGroup = null;
        
        foreach (var group in groups)
        {
            if (group.name == "Master")
            {
                masterGroup = group;
                break;
            }
        }

        if (masterGroup == null)
        {
            Debug.LogError("Groupe Master non trouvé dans l'Audio Mixer");
            return;
        }

        // Créer le groupe Music
        AudioMixerGroup musicGroup = CreateOrFindMixerGroup(mixer, "Music", masterGroup);

        // Créer le groupe SFX
        AudioMixerGroup sfxGroup = CreateOrFindMixerGroup(mixer, "SFX", masterGroup);

        // Mettre à jour l'AudioManager avec les nouveaux groupes (seulement si les groupes existent)
        if (musicGroup != null && sfxGroup != null)
        {
            UpdateAudioManagerGroups(musicGroup, sfxGroup);
            Debug.Log("Groupes Audio Mixer configurés avec succès!");
            Debug.Log("- Master: " + masterGroup.name);
            Debug.Log("- Music: " + musicGroup.name);
            Debug.Log("- SFX: " + sfxGroup.name);
        }
        else
        {
            Debug.Log("Configuration partielle. Veuillez créer les groupes manuellement puis ré-exécuter cet outil.");
            
            // Vérifier si les groupes existent maintenant et proposer une ré-exécution
            CheckAndOfferRetry(mixer);
        }
    }

    private static AudioMixerGroup CreateOrFindMixerGroup(AudioMixer mixer, string groupName, AudioMixerGroup parentGroup)
    {
        // Chercher si le groupe existe déjà
        var existingGroups = mixer.FindMatchingGroups(groupName);
        if (existingGroups.Length > 0)
        {
            return existingGroups[0];
        }

        // Créer un nouveau groupe (cette opération doit être faite manuellement dans l'éditeur)
        Debug.LogWarning($"Le groupe {groupName} n'existe pas. Veuillez le créer manuellement dans l'Audio Mixer:");
        Debug.LogWarning("1. Ouvrez Assets/Audio/GameAudioMixer.mixer");
        Debug.LogWarning("2. Cliquez-droit sous 'Groups'");
        Debug.LogWarning("3. Sélectionnez 'Add child group'");
        Debug.LogWarning("4. Nommez-le '" + groupName + "'");
        
        return null;
    }

    private static void CheckAndOfferRetry(AudioMixer mixer)
    {
        // Vérifier si les groupes existent maintenant
        bool musicExists = mixer.FindMatchingGroups("Music").Length > 0;
        bool sfxExists = mixer.FindMatchingGroups("SFX").Length > 0;
        
        if (musicExists && sfxExists)
        {
            Debug.Log("✓ Les groupes Music et SFX existent maintenant !");
            Debug.Log("Vous pouvez ré-exécuter l'outil avec: Tools > Audio > Setup Audio Mixer Groups");
        }
        else if (musicExists || sfxExists)
        {
            Debug.Log("Configuration partielle:");
            if (musicExists) Debug.Log("✓ Music existe");
            else Debug.Log("✗ Music manquant");
            if (sfxExists) Debug.Log("✓ SFX existe");
            else Debug.Log("✗ SFX manquant");
        }
    }

    private static void UpdateAudioManagerGroups(AudioMixerGroup musicGroup, AudioMixerGroup sfxGroup)
    {
        try
        {
            // Trouver l'AudioManager dans la scène
            AudioManager audioManager = FindFirstObjectByType<AudioManager>();
            
            if (audioManager != null)
            {
                // Mettre à jour les groupes
                SerializedObject serializedAudioManager = new SerializedObject(audioManager);
                
                // Trouver les propriétés avec des noms exacts
                SerializedProperty masterProp = serializedAudioManager.FindProperty("masterGroup");
                SerializedProperty musicProp = serializedAudioManager.FindProperty("musicGroup");
                SerializedProperty sfxProp = serializedAudioManager.FindProperty("sfxGroup");
                
                if (musicProp != null && sfxProp != null)
                {
                    if (masterProp != null)
                    {
                        // Trouver le groupe Master
                        AudioMixer mixer = musicGroup.audioMixer;
                        AudioMixerGroup[] groups = mixer.FindMatchingGroups("Master");
                        if (groups.Length > 0) masterProp.objectReferenceValue = groups[0];
                    }
                    
                    musicProp.objectReferenceValue = musicGroup;
                    sfxProp.objectReferenceValue = sfxGroup;
                    serializedAudioManager.ApplyModifiedProperties();
                    
                    Debug.Log("✓ AudioManager mis à jour avec les nouveaux groupes Audio Mixer (Master, Music, SFX)");
                }
                else
                {
                    Debug.LogWarning("Propriétés non trouvées dans l'AudioManager. Vérifiez les noms des champs.");
                    if (musicProp == null) Debug.LogWarning("Champ 'musicGroup' non trouvé");
                    if (sfxProp == null) Debug.LogWarning("Champ 'sfxGroup' non trouvé");
                }
            }
            else
            {
                Debug.LogWarning("AudioManager non trouvé dans la scène. Les groupes seront configurés mais pas assignés automatiquement.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erreur lors de la mise à jour de l'AudioManager: " + e.Message);
        }
    }

    [MenuItem("Tools/Audio/Verify Audio Setup")]
    public static void VerifyAudioSetup()
    {
        // Vérifier l'Audio Mixer
        AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/Audio/GameAudioMixer.mixer");
        
        if (mixer != null)
        {
            Debug.Log("✓ Audio Mixer trouvé: " + mixer.name);
            
            // Vérifier les groupes
            AudioMixerGroup[] groups = mixer.FindMatchingGroups("");
            foreach (var group in groups)
            {
                Debug.Log("✓ Groupe: " + group.name);
            }
        }
        else
        {
            Debug.LogError("✗ Audio Mixer non trouvé");
        }

        // Vérifier l'AudioManager
        AudioManager audioManager = FindFirstObjectByType<AudioManager>();
        if (audioManager != null)
        {
            Debug.Log("✓ AudioManager trouvé dans la scène");
        }
        else
        {
            Debug.LogError("✗ AudioManager non trouvé dans la scène");
        }
    }
}