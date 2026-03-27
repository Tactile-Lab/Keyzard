using System.Collections.Generic;
using UnityEngine;

public abstract class DomainSFXAudioConfig : ScriptableObject
{
    public List<SFXEventAudioEntry> entries = new List<SFXEventAudioEntry>();

    public abstract bool SupportsKey(SFXEventKey key);

    public SFXEventAudioEntry GetEntry(SFXEventKey key)
    {
        if (!SupportsKey(key))
        {
            return null;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            SFXEventAudioEntry entry = entries[i];
            if (entry != null && entry.key == key)
            {
                return entry;
            }
        }

        return null;
    }
}