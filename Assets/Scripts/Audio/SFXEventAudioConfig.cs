using System.Collections.Generic;
using UnityEngine;

public enum SFXEventKey
{
	None = 0,

	// UI
	UIMenuMove = 1,
	UIMenuConfirm = 2,
	UIOpen = 3,
	UIClose = 4,
	UISceneTransition = 5,
	UIGlossaryOpen = 6,
	UIGlossaryClose = 7,

	// Typing
	TypingHit = 8,
	TypingMiss = 9,
	TypingWordFail = 10,
	TypingTargetLock = 11,
	TypingSpellReady = 12,
	TypingSpellCleared = 13,

	// Player
	PlayerFootstep = 14,
	PlayerHurt = 15,
	PlayerDeath = 16,

	// Enemy generic
	EnemyAttack = 17,
	EnemyHurt = 18,
	EnemyDeath = 19,
	EnemyFootstep = 20,

	// Enemy by type
	EnemyRapideAttack = 21,
	EnemyRapideHurt = 22,
	EnemyRapideDeath = 23,
	EnemyRapideFootstep = 24,
	EnemyLourdAttack = 25,
	EnemyLourdHurt = 26,
	EnemyLourdDeath = 27,
	EnemyLourdFootstep = 28,
	EnemyDistantAttack = 29,
	EnemyDistantHurt = 30,
	EnemyDistantDeath = 31,

	// Progress / misc
	RoomClear = 32,
	NewSpellUnlocked = 33,
	BookPageTurn = 34,
	EndDemoOpen = 35,
	EndDemoConfirm = 36
}

[System.Serializable]
public class SFXEventAudioEntry
{
	public SFXEventKey key;
	public AudioClip clip;
	[Range(0f, 1f)] public float volume = 1f;
	[Range(0.1f, 3f)] public float pitch = 1f;
	[Range(0f, 0.5f)] public float randomPitchVariance = 0f;
}

[CreateAssetMenu(fileName = "SFXEventAudioConfig", menuName = "Audio/SFXEventAudioConfig")]
public class SFXEventAudioConfig : ScriptableObject
{
	public List<SFXEventAudioEntry> entries = new List<SFXEventAudioEntry>();

	public static bool IsMiscKey(SFXEventKey key)
	{
		switch (key)
		{
			case SFXEventKey.RoomClear:
			case SFXEventKey.NewSpellUnlocked:
			case SFXEventKey.BookPageTurn:
			case SFXEventKey.EndDemoOpen:
			case SFXEventKey.EndDemoConfirm:
				return true;
			default:
				return false;
		}
	}

	public SFXEventAudioEntry GetEntry(SFXEventKey key)
	{
		if (!IsMiscKey(key))
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
