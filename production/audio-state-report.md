# Rapport Audio — Keyzard

**Date:** 2026-03-26
**Équipe:** Audio Director, Sound Designer, Technical Artist
**Stade projet:** Production active

---

## Vue d'ensemble

| Domaine | État | Criticité |
|---------|------|-----------|
| Architecture technique | Solide | — |
| Musique | 1 piste, pas d'états adaptatifs | Moyen |
| SFX sorts | Tya uniquement (4 sorts muets) | **Critique** |
| Feedback frappe clavier | Zéro son | **Critique** |
| Audio ennemis | Zéro son | Haut |
| Audio joueur | Zéro son | Haut |
| UI / menus | Zéro son | Haut |
| Unlock de sorts | Zéro son | Moyen |

---

## Ce qui fonctionne

- **`AudioManager`** singleton avec pool AudioSource (5 SFX + 5 Loop) : architecture propre
- **`SpellAudioConfig`** ScriptableObject : bon pattern, extensible à tous les sorts
- **Muffle low-pass filter** sur pause/glossaire : feature pro, implémentée correctement (le filtre est bien isolé à `musicSource` uniquement — pas de bug)
- **Sort Tya** : design sonore soigné avec variations F1/F2/F3, pitchbend, layers (buildup, breath, scrape, chimes) — template de référence pour les autres sorts
- **Mixer** Master > Music | SFX : structure correcte comme base

---

## Identité sonore cible (Audio Director)

**Registre émotionnel :** Pression intellectuelle arcanique — un érudit en état de siège qui maîtrise les mots-sorts.
**Pas** de la fantasy héroïque, **pas** de l'action frénétique. Précision sous pression.

### Palette sonore par catégorie

| Catégorie | Description |
|-----------|-------------|
| **Frappe clavier** | Cristallin, arcanique. Comme une plume traçant des runes — pas des clics mécaniques. Court, avec une résonance harmonique. Très sec (pas de réverb). |
| **Lancement de sort** | Décisif et directionnel. Relâchement d'une tension accumulée. Doit sembler mérité après le buildup. |
| **Impact ennemi** | Organique et satisfaisant. Os percussifs pour les squelettes. Impact surnaturel pour le vampire (légèrement éthéré). |
| **Joueur touché** | Bref, vulnérable. Pas dramatique — court perturbation magique. |
| **Ambiance** | Le donjon respire sans parler. Drone grave, pierre, mouvement lointain. |
| **Musique** | Sombre et académique. Orchestral ou ensemble de chambre avec touches synthétiques. |
| **UI** | Plus discret que les SFX gameplay. Sobre, net, cohérent. |

---

## Bugs confirmés

### Bug 1 — ShotGunFeu : 0 son au lancement, jusqu'à 13 impacts simultanés
**Sévérité : Haut**
**Fichier :** `Assets/Scripts/Sort/ShotGunFeu.cs`

`LancerSortCible` overrides la méthode de base sans appeler `base.LancerSortCible()` → les appels `PlayLaunchReleaseSFX()` et `StartActiveLoop()` ne s'exécutent jamais. De plus, chaque pellet clone le prefab avec sa référence `audioConfig`, et appelle `PlayImpactSFX()` indépendamment → jusqu'à 13 impacts en un seul frame → distorsion/clipping.

**Corrections requises :**
1. Ajouter l'appel audio launch en tête de `LancerSortCible`
2. Limiter l'impact SFX à 1 appel par volée (flag ou appel sur le pellet central uniquement)

---

### Bug 2 — RingEau : destroy path contourne tout audio
**Sévérité : Moyen**
**Fichier :** `Assets/Scripts/Sort/RingEau.cs`

`DestroySort` retourne immédiatement → `OnImpact` jamais appelé → pas de son d'impact. `OnAnimationFinished` appelle `Destroy(gameObject)` directement sans stopper `activeLoopSource` → fuite de loop source si jamais un clip loop est assigné.

**Correction :** Appeler `AudioManager.Instance?.StopLoop(activeLoopSource)` avant `Destroy`.

---

### Bug 3 — LanceFantom : fuite de activeLoopSource sur early-exit
**Sévérité : Moyen**
**Fichier :** `Assets/Scripts/Sort/LanceFantom.cs`

`DestroySort` retourne sans appeler base si la condition `this.cible == cible || cible == null` est fausse → `activeLoopSource` jamais stoppé → voice occupée définitivement jusqu'à expansion du pool.

**Correction :** Stopper le loop avant le `return` prématuré.

---

## Call sites audio manquants

Ces événements n'ont aucun appel audio dans le code (pas juste des assets manquants).

| Événement | Fichier | Ligne | Priorité |
|-----------|---------|-------|----------|
| Lettre correcte tapée | `TypingSortManager.cs` | 383 | **Critique** |
| Lettre incorrecte tapée | `TypingSortManager.cs` | 424 | **Critique** |
| Joueur touché | `PlayerHealth.cs` | 57 | Haut |
| Joueur mort | `PlayerHealth.cs` | 81 | Haut |
| Ennemi touché | `Enemy.cs` | 364 | Haut |
| Ennemi mort | `Enemy.cs` | 413 | Haut |
| Game Over affiché | `GameOverMenuController.cs` | 66 | Haut |
| Navigation menu | `MainMenuManager.cs` + `GameOverMenuController.cs` | — | Haut |
| Salle nettoyée / porte | `RoomManager.cs` | 193 | Bas |
| Unlock de sort | `SpellUnlockTrigger.cs` | 47 | Moyen |
| Projectile ennemi tiré/impact | `Enemy.cs` | 204 | Moyen |

---

## Assets SFX manquants

**Existants (Tya uniquement) :** 20 clips WAV (launch x3, buildup x13, impact x1)
**Estimés manquants : ~70 clips WAV**

### Feuille d'événements audio à produire

| Événement | Variants | Priorité | Notes |
|-----------|----------|----------|-------|
| Lettre correcte | 3 | Critique | Arcanique, court, cristallin |
| Lettre incorrecte | 2 | Critique | Dissonant, court |
| Launch BouleMagique (start + release) | 3 × 2 | Critique | Grave, orbe magique |
| Launch LanceFantome (start + release) | 3 × 2 | Critique | Éthéré, fantomatique |
| Launch RingEau (start + release) | 3 × 2 | Critique | Aquatique, bouillonnant |
| Launch ShotGunFeu (start + release) | 3 × 2 | Critique | Craquement de feu agressif |
| Impact × 4 sorts | 2-3 × 4 | Critique | Selon élément |
| Loop actif × 4 sorts | 1 × 4 | Haut | Loopable proprement |
| Ennemi touché (×3 types) | 2 × 3 | Haut | Os/fantôme/vampire |
| Ennemi mort (×3 types) | 2 × 3 | Haut | Effondrement/dissipation |
| Joueur touché | 2-3 | Haut | Synthétique ou vocal |
| Joueur mort | 1 | Haut | Grave, conclusif |
| Navigation menu | 2 (L/R) | Haut | Discret, directionnel |
| Confirmation menu | 1 | Haut | Cloche résolue |
| Game Over stinger | 1 | Haut | Dramatique, ~1.5s |
| Unlock de sort | 1 | Moyen | Séquence montante, récompense |
| Pause on/off | 2 | Moyen | Très discret |

---

## Architecture mixer — gaps

| Gap | Impact | Urgence |
|-----|--------|---------|
| Pas de bus **UI** | Sons menus mis en pause avec `AudioListener.pause` gameplay | Haut — avant tout ajout de SFX UI |
| Pas de **limiter/compressor sur Master** | ShotGun peut causer distorsion, aucun filet de sécurité | Haut |
| Pas de système de **snapshots** | Transitions complexes gérées en code uniquement, fragiles | Moyen |
| Pas d'effet **LowPass sur groupe Music** (mixer) | Muffle fait via filtre sur GameObject — fonctionne mais moins robuste | Bas |

---

## Recommandations techniques (corrections code)

### AudioManager.cs:29
```csharp
// Changer de :
private const int INITIAL_POOL_SIZE = 5;
// En :
private const int INITIAL_POOL_SIZE = 10;
```
Évite les allocations `new GameObject` mid-combat.

### ShotGunFeu.cs — LancerSortCible
Ajouter en tête de la méthode :
```csharp
audioConfig?.Preload();
audioConfig?.PlayLaunchReleaseSFX();
```
Et limiter l'impact SFX à 1 appel par volée (flag `_impactSFXFired`).

### RingEau.cs — OnAnimationFinished
Ajouter avant `Destroy(gameObject)` :
```csharp
AudioManager.Instance?.StopLoop(activeLoopSource);
activeLoopSource = null;
```

### LanceFantom.cs — DestroySort
Ajouter avant le `return` prématuré :
```csharp
AudioManager.Instance?.StopLoop(activeLoopSource);
activeLoopSource = null;
```

### TypingSortManager.cs
Ajouter des champs `[SerializeField] AudioClip correctLetterSFX`, `wrongLetterSFX` et appeler `AudioManager.Instance.PlaySFX(...)` dans `PlayLetterAnimation` et `PlayWrongLetterAnimation`.

---

## Feuille de route

### Semaine 1 — Débloque le playtesting
1. Corriger `ShotGunFeu.LancerSortCible` (bug launch + impact x13)
2. Corriger `RingEau.DestroySort` (loop leak)
3. Corriger `LanceFantom.DestroySort` (loop leak early-exit)
4. Augmenter pool à 10 (`AudioManager.cs:29`)
5. Ajouter bus UI dans le mixer
6. Ajouter call sites dans `TypingSortManager` (lettre correcte/incorrecte)

### Semaine 2 — Contenu audio prioritaire
7. Sourcer/créer ~25 clips : feedback clavier + ennemis + joueur
8. Assigner clips aux 4 `SpellAudioConfig` (BouleMagique, LanceFantome, RingEau, ShotGunFeu)
9. Ajouter call sites `PlayerHealth` et `Enemy`
10. Ajouter limiter sur Master

### Semaine 3 — Polish
11. Remaining spells loops + sons UI menus
12. Game Over stinger + unlock sort
13. Ambiance salle

---

## Fichiers clés

| Fichier | Rôle |
|---------|------|
| `Assets/Scripts/Audio/AudioManager.cs` | Singleton, pool, muffle |
| `Assets/Scripts/Audio/SpellAudioConfig.cs` | Config ScriptableObject sorts |
| `Assets/AudioConfigs/Sorts/` | 4 configs vides à remplir |
| `Assets/Audio/SFX/Tya/` | Référence qualité SFX |
| `Assets/Audio/GameAudioMixer.mixer` | Mixer à compléter (UI bus, limiter) |
| `Assets/Scripts/Typing/TypingSortManager.cs` | Call sites clavier manquants |
| `Assets/Scripts/Enemy/Enemy.cs` | Call sites ennemis manquants |
| `Assets/Scripts/Player/PlayerHealth.cs` | Call sites joueur manquants |
| `Assets/Scripts/Sort/ShotGunFeu.cs` | Bug audio confirmé |
| `Assets/Scripts/Sort/RingEau.cs` | Bug audio confirmé |
| `Assets/Scripts/Sort/LanceFantom.cs` | Bug audio confirmé |
