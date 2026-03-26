using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject winImage;

    [SerializeField] private EndController endController;

    public void CheckAllRoomsCleared()
    {
        RoomManager[] rooms = FindObjectsByType<RoomManager>(FindObjectsSortMode.None);

        foreach (var room in rooms)
        {
            if (room.IsCombatRoomNotCleared())
                return;
        }

        Debug.Log("Toutes les rooms Combat sont clear !");
        OnLevelCompleted();
    }


    private void OnLevelCompleted()
    {
        if (winImage != null)
        {
            winImage.SetActive(true);
        }

        if (endController != null)
        {
            endController.EnableReturn();
        }
    }
}