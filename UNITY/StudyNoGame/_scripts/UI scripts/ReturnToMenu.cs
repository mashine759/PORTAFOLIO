using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cargar escenas.
using UnityEngine.UI; // Necesario para trabajar con el componente Button.

/// <summary>
/// Script utilitario encargado de regresar al men� principal desde cualquier escena de juego.
/// Ofrece funcionalidad mediante un bot�n de UI y una tecla de acceso r�pido (shortcut).
/// </summary>
public class ReturnToMenu : MonoBehaviour
{
    [Header("Configuraci�n")]
    public string menuSceneName = "Menu_Game"; // El nombre de la escena del men� principal a cargar.
    public KeyCode shortcutKey = KeyCode.Escape; // La tecla que activar� el regreso al men� (por defecto, Esc).

    [Header("UI")]
    public Button returnButton; // Referencia al bot�n de UI que activa el regreso al men�.

    private void Start()
    {
        // Asigna el m�todo ReturnToMainMenu al evento de clic del bot�n (si el bot�n est� asignado).
        if (returnButton != null)
            returnButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void Update()
    {
        // Comprueba si la tecla de acceso r�pido (shortcutKey) fue presionada en este frame.
        if (Input.GetKeyDown(shortcutKey))
            ReturnToMainMenu();
    }

    /// <summary>
    /// Carga la escena del men� principal. Este m�todo es p�blico para ser llamado
    /// directamente por el bot�n de la UI o por el c�digo (tecla de acceso r�pido).
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("Volviendo al men� principal...");
        // Carga la escena cuyo nombre est� especificado en 'menuSceneName'.
        SceneManager.LoadScene(menuSceneName);
    }
}