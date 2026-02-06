using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer), typeof(PolygonCollider2D))]
public class ColliderUpdater : MonoBehaviour
{
    PolygonCollider2D pc;
    SpriteRenderer sr;

    void Awake()
    {
        pc = GetComponent<PolygonCollider2D>();
        sr = GetComponent<SpriteRenderer>();
        UpdateCollider();
    }

    void LateUpdate()
    {
        UpdateCollider();
    }

    void UpdateCollider()
    {
        if (sr.sprite == null) return;

        int shapeCount = sr.sprite.GetPhysicsShapeCount();
        pc.pathCount = shapeCount;

        for (int i = 0; i < shapeCount; i++)
        {
            List<Vector2> path = new List<Vector2>();
            sr.sprite.GetPhysicsShape(i, path);
            pc.SetPath(i, path.ToArray());
        }
    }
}
