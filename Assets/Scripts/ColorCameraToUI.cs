using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class CameraColorToUI : MonoBehaviour
{
    public Camera mainCamera; // Ta caméra principale
    public Image panelUI;     // Panel ou Image UI

    void Update()
    {
        if (mainCamera != null && panelUI != null)
        {
            panelUI.color = new Color(mainCamera.backgroundColor.r,
                           mainCamera.backgroundColor.g,
                           mainCamera.backgroundColor.b,
                           1f); // alpha forcé
        }
    }
}