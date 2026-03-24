using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Met à jour automatiquement les physiques du PolygonCollider2D pour correspondre
/// aux formes définies dans les sprites importés (Physics Shape en Sprite Editor).
/// Optimisé pour fonctionner uniquement lors de changements de sprite.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(PolygonCollider2D))]
public class ColliderUpdater : MonoBehaviour
{
    private PolygonCollider2D polygonCollider;
    private SpriteRenderer spriteRenderer;
    private Sprite previousSprite;

    private void Awake()
    {
        // Récupérer les références aux composants requis.
        polygonCollider = GetComponent<PolygonCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        previousSprite = null;
        UpdateCollider();
    }

    private void LateUpdate()
    {
        // Mettre à jour le collider uniquement si le sprite a changé pour optimiser les performances.
        if (spriteRenderer.sprite != previousSprite)
        {
            UpdateCollider();
        }
    }

    /// <summary>
    /// Extrait les formes physiques du sprite actuel et les applique au PolygonCollider2D.
    /// </summary>
    private void UpdateCollider()
    {
        // Sécurité: quitter si aucun sprite n'est assigné.
        if (spriteRenderer.sprite == null)
        {
            return;
        }

        Sprite currentSprite = spriteRenderer.sprite;
        int physicsShapeCount = currentSprite.GetPhysicsShapeCount();

        // Adapter le nombre de chemins du collider au nombre de formes physiques.
        polygonCollider.pathCount = physicsShapeCount;

        // Appliquer chaque forme physique au collider polygon.
        for (int shapeIndex = 0; shapeIndex < physicsShapeCount; shapeIndex++)
        {
            List<Vector2> physicShape = new List<Vector2>();
            currentSprite.GetPhysicsShape(shapeIndex, physicShape);
            polygonCollider.SetPath(shapeIndex, physicShape.ToArray());
        }

        // Mémoriser le sprite pour détecter les changements futurs.
        previousSprite = currentSprite;
    }
}
