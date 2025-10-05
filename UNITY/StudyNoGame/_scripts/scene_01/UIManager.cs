using UnityEngine;
using System.Collections; // Necesario para usar Coroutines (IEnumerator)

/// <summary>
/// Controlador de la Interfaz de Usuario (UI) espec�fico para la escena de juego.
/// Gestiona la visualizaci�n de mensajes y la pausa temporal del juego ante eventos (ej. error).
/// Implementa el patr�n Singleton.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance; // Referencia est�tica al Singleton.

    [Header("Referencias de UI")]
    public GameObject panelIncorrecto; // Referencia al panel de UI que muestra el mensaje de "Incorrecto".

    private PlayerController player; // Referencia al script de control del jugador.

    void Awake()
    {
        // Implementaci�n del Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Busca la instancia del PlayerController en la escena.
        player = FindObjectOfType<PlayerController>();
    }

    /// <summary>
    /// Muestra el panel de error y pausa temporalmente el control del jugador.
    /// Llamado por KanjiTarget cuando el jugador dispara al Kanji equivocado.
    /// </summary>
    public void MostrarPanelIncorrecto()
    {
        if (panelIncorrecto != null)
        {
            panelIncorrecto.SetActive(true); // Muestra el panel de feedback.

            // Deshabilita el script del jugador para 'pausar' el movimiento y disparo
            // mientras se muestra el mensaje de error.
            if (player != null) player.enabled = false;

            // Inicia la Coroutine para cerrar el panel despu�s de un tiempo.
            StartCoroutine(CerrarPanelIncorrecto());
        }
    }

    /// <summary>
    /// Coroutine que espera un tiempo y luego cierra el panel de error, reanudando el juego.
    /// </summary>
    private IEnumerator CerrarPanelIncorrecto()
    {
        // Espera 2 segundos para que el jugador lea el mensaje.
        yield return new WaitForSeconds(2f);

        if (panelIncorrecto != null)
        {
            panelIncorrecto.SetActive(false); // Oculta el panel.

            // Reactiva el control del jugador para reanudar el juego.
            if (player != null) player.enabled = true;
        }
    }
}