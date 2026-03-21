using UnityEngine;
using System.Collections;

public enum CrawlDirection
{
    Up,
    Down,
    Left,
    Right
}

public class RoomCrawling : MonoBehaviour
{
    [SerializeField]
    private CrawlDirection crawlDirection;
    private Camera mainCamera;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

    }

    private void OnTriggerEnter2D (Collider2D other)
    {
        Debug.Log("Player entered crawl trigger: " + other.name);
        if (other.CompareTag("Player"))
        {            
            Vector3 newPos = transform.position;
            Vector3 cameraOffset = new Vector3(0,0,0);

            switch (crawlDirection)
            {
                case CrawlDirection.Up:
                    newPos.y += 1f;
                    cameraOffset += new Vector3(0, 10f, 0);
                    break;
                case CrawlDirection.Down:
                    newPos.y -= 1f;
                    cameraOffset -= new Vector3(0, 10f, 0);
                    break;
                case CrawlDirection.Left:
                    newPos.x -= 1f; 
                    cameraOffset -= new Vector3(18f, 0, 0);
                    break;
                case CrawlDirection.Right:
                    newPos.x += 1f;
                    cameraOffset += new Vector3(18f, 0, 0);
                    break;
            }

            StartCoroutine(LerpCamera(mainCamera.transform.position + cameraOffset, 0.2f));
            other.transform.position = newPos;
        }
    }

    private IEnumerator LerpCamera(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = mainCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = targetPosition;
    }
}
