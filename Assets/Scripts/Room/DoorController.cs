using UnityEngine;

public class DoorController : MonoBehaviour
{
    public GameObject openDoor;   // prefab porte ouverte
    public GameObject closedDoor; // prefab porte fermée

    public void Open()
    {
        if (openDoor != null) openDoor.SetActive(true);
        if (closedDoor != null) closedDoor.SetActive(false);
    }

    public void Close()
    {
        if (openDoor != null) openDoor.SetActive(false);
        if (closedDoor != null) closedDoor.SetActive(true);
    }
}