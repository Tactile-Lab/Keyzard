using UnityEngine;
using TMPro;
using System.Collections;

public class LetterAndWordParticles : MonoBehaviour
{
    [Header("Références")]
    public TextMeshProUGUI textMeshPro;       // TMP texte principal
    public GameObject letterParticlePrefab;   // Prefab particule par lettre
    public GameObject wordParticlePrefab;     // Prefab particule pour mot entier

    // ---------------- Spawn particule lettre ----------------
   public void SpawnLetterParticleSafe()
{
    if (textMeshPro == null || letterParticlePrefab == null)
        return;

    StartCoroutine(SpawnLetterNextFrame());
}

private IEnumerator SpawnLetterNextFrame()
{
    // attendre la fin de la frame pour que TMP rende la lettre
    yield return null;

    textMeshPro.ForceMeshUpdate();
    TMP_TextInfo textInfo = textMeshPro.textInfo;

    if (textInfo.characterCount == 0)
        yield break;

    int lastValidCharIndex = -1;
    for (int i = textInfo.characterCount - 1; i >= 0; i--)
    {
        TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
        char c = charInfo.character;
        if (charInfo.isVisible && c != '-' && !char.IsWhiteSpace(c))
        {
            lastValidCharIndex = i;
            break;
        }
    }

    if (lastValidCharIndex == -1)
        lastValidCharIndex = 0;

    TMP_CharacterInfo targetChar = textInfo.characterInfo[lastValidCharIndex];
    Vector3 charMidPos = (targetChar.bottomLeft + targetChar.topRight) / 2;
    Vector3 worldPos = textMeshPro.transform.TransformPoint(charMidPos);

    GameObject particle = Instantiate(letterParticlePrefab, worldPos, Quaternion.identity);
    if (particle.TryGetComponent(out ParticleSystem ps))
        Destroy(particle, ps.main.startLifetime.constant + 0.5f);
}

    // ---------------- Spawn particules mot entier ----------------
    public void SpawnWordParticle()
    {
        if (textMeshPro == null || wordParticlePrefab == null)
            return;

        textMeshPro.ForceMeshUpdate();
        TMP_TextInfo textInfo = textMeshPro.textInfo;

        if (textInfo.characterCount == 0)
            return;

        // Calcul bounding box du mot (ignorer "-" et espaces)
        Vector3 bottomLeft = Vector3.positiveInfinity;
        Vector3 topRight = Vector3.negativeInfinity;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            char c = charInfo.character;
            if (c == '-' || char.IsWhiteSpace(c)) continue;

            bottomLeft = Vector3.Min(bottomLeft, charInfo.bottomLeft);
            topRight = Vector3.Max(topRight, charInfo.topRight);
        }

        // Si aucun caractère visible
        if (bottomLeft == Vector3.positiveInfinity || topRight == Vector3.negativeInfinity)
            return;

        // Centre du mot
        Vector3 wordCenter = (bottomLeft + topRight) / 2;
        Vector3 worldWordPos = textMeshPro.transform.TransformPoint(wordCenter);

        GameObject wordParticle = Instantiate(wordParticlePrefab, worldWordPos, Quaternion.identity);

        if (wordParticle.TryGetComponent(out ParticleSystem ps))
            Destroy(wordParticle, ps.main.startLifetime.constant + 0.5f);
    }
}