using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpellUnlockVisual : MonoBehaviour
{
    private SpriteRenderer sr;

    [Header("Bobbing Effect")]
    [SerializeField] private float bobAmplitude = 0.15f; // hauteur du bobbing
    [SerializeField] private float bobSpeed = 3f;        // vitesse du bobbing

    private Vector3 startPos;
    private float timer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;

        startPos = transform.localPosition;
        timer = Random.Range(0f, 2f * Mathf.PI); // décalage aléatoire pour ne pas synchro
    }

    void Update()
    {
        if (!sr.enabled) return;

        timer += Time.deltaTime * bobSpeed;

        // Bobbing vertical uniquement
        float yOffset = Mathf.Sin(timer) * bobAmplitude;
        transform.localPosition = startPos + new Vector3(0, yOffset, 0);
    }

    public void SetSprite(Sprite sprite)
    {
        if (sprite == null) return;
        sr.sprite = sprite;
        sr.enabled = true;
    }
}