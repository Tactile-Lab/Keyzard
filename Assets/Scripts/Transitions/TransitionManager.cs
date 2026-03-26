using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;
    public static bool IsTransitioning { get; private set; }

    private bool firstLoad = true; // <- ignore le fade d'entrée au lancement

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ignore le fade d'entrée la première scène
        if (firstLoad)
        {
            firstLoad = false;
            return;
        }

        if (BlackFadeEffect.Instance != null)
        {
            IsTransitioning = true;
            Time.timeScale = 0f; // bloque tout pendant le fade

            BlackFadeEffect.Instance.PlayFadeIn(() =>
            {
                Time.timeScale = 1f;
                IsTransitioning = false;
            });
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
        {
            IsTransitioning = true;
            Time.timeScale = 0f;
            yield return StartCoroutine(FadeOutCoroutine());
        }

        SceneManager.LoadScene(buildIndex);
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        if (BlackFadeEffect.Instance != null)
        {
            IsTransitioning = true;
            Time.timeScale = 0f;
            yield return StartCoroutine(FadeOutCoroutine());
        }

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeOutCoroutine()
    {
        bool finished = false;
        BlackFadeEffect.Instance.PlayFadeOut(() => finished = true);
        while (!finished)
            yield return null;
    }
}