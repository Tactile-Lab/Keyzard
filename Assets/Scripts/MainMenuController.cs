using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    public UIDocument uiDocument;
    private string _inputBuffer = "";

    void Update()
    {
        // Si le clavier envoie quelque chose
        if (!string.IsNullOrEmpty(Input.inputString))
        {
            foreach (char c in Input.inputString)
            {
                // AFFICHE LA LETTRE DANS LA CONSOLE (C'est la ligne espion)
                Debug.Log("Touche reçue : " + c);

                _inputBuffer += c.ToString().ToUpper();
                
                if (_inputBuffer.Length > 10)
                {
                    _inputBuffer = _inputBuffer.Substring(_inputBuffer.Length - 10);
                }

                // AFFICHE LA MÉMOIRE ACTUELLE
                Debug.Log("Mémoire actuelle : " + _inputBuffer);

                CheckSecretCodes();
            }
        }
    }

    void CheckSecretCodes()
    {
        if (_inputBuffer.EndsWith("START"))
        {
            Debug.Log("🔮 VICTOIRE ! Le mot START est détecté !");
        }
    }
}