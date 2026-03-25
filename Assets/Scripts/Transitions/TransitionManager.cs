using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;  // Déclenche PlayEntranceEffect après load
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Pause le jeu avant le fade-in
        Time.timeScale = 0f;

        if (BlackFadeEffect.Instance != null)
        {
            BlackFadeEffect.Instance.PlayEntranceEffect(() =>
            {
                // Remet le jeu en marche après le fade-in
                Time.timeScale = 1f;
            });
        }
        else
        {
            // Si pas de BlackFadeEffect, on remet le TimeScale directement
            Time.timeScale = 1f;
        }
    }

    public void LoadScene(int buildIndex)
    {
        StartCoroutine(LoadSceneCoroutine(buildIndex));
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(int buildIndex)
    {
        if (BlackFadeEffect.Instance != null)
            yield return StartCoroutine(BlackFadeEffect.Instance.PlayExitEffectCoroutine(null));

        yield return null;
        SceneManager.LoadScene(buildIndex);
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        if (BlackFadeEffect.Instance != null)
            yield return StartCoroutine(BlackFadeEffect.Instance.PlayExitEffectCoroutine(null));

        yield return null;
        SceneManager.LoadScene(sceneName);
    }
}