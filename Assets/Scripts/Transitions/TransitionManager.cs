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
            SceneManager.sceneLoaded += OnSceneLoaded;  // Pour déclencher PlayEntranceEffect
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BlackFadeEffect.Instance?.PlayEntranceEffect();  // Fade IN après load
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
