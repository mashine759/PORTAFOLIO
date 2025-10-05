using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cargar escenas.
using UnityEngine.UI; // Necesario para trabajar con el componente Button.

/// <summary>
/// Script utilitario encargado de regresar al menú principal desde cualquier escena de juego.
/// Ofrece funcionalidad mediante un botón de UI y una tecla de acceso rápido (shortcut).
/// </summary>
public class ReturnToMenu : MonoBehaviour
{
    [Header("Configuración")]
    public string menuSceneName = "Menu_Game"; // El nombre de la escena del menú principal a cargar.
    public KeyCode shortcutKey = KeyCode.Escape; // La tecla que activará el regreso al menú (por defecto, Esc).

    [Header("UI")]
    public Button returnButton; // Referencia al botón de UI que activa el regreso al menú.

    private void Start()
    {
        // Asigna el método ReturnToMainMenu al evento de clic del botón (si el botón está asignado).
        if (returnButton != null)
            returnButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void Update()
    {
        // Comprueba si la tecla de acceso rápido (shortcutKey) fue presionada en este frame.
        if (Input.GetKeyDown(shortcutKey))
            ReturnToMainMenu();
    }

    /// <summary>
    /// Carga la escena del menú principal. Este método es público para ser llamado
    /// directamente por el botón de la UI o por el código (tecla de acceso rápido).
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("Volviendo al menú principal...");
        // Carga la escena cuyo nombre está especificado en 'menuSceneName'.
        SceneManager.LoadScene(menuSceneName);
    }
}