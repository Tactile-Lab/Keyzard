using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class RoomTrigger : MonoBehaviour
{
    private RoomManager room;
    private Collider2D col;

    void Awake()
    {
        room = GetComponentInParent<RoomManager>();
        col = GetComponent<Collider2D>();
        if (!col.isTrigger) col.isTrigger = true;
    }

    void Start()
    {
        // Si le joueur start déjà dans la room
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && col.OverlapPoint(player.transform.position))
            room.ForceEnterRoom();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            room.OnPlayerEnter();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            room.PlayerExited();
    }
}