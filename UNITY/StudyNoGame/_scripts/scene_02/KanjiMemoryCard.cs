using UnityEngine;
using TMPro; // Necesario para interactuar con componentes TextMeshPro.

/// <summary>
/// Define el comportamiento individual de una tarjeta en el juego de memoria.
/// Almacena su valor (Kanji), gestiona su estado (emparejada/oculta) y maneja la interacción del clic.
/// </summary>
public class KanjiMemoryCard : MonoBehaviour
{
    public string kanjiTexto; // El carácter Kanji asignado a esta tarjeta.
    public bool fueEmparejada = false; // Indica si la tarjeta ya formó parte de un par correcto.

    private TextMeshPro textMesh; // Referencia al componente de texto hijo que muestra el Kanji.
    private MemoryGameManager gameManager; // Referencia al gestor del juego para notificar la selección.

    /// <summary>
    /// Inicializa la tarjeta, asignando su valor y el gestor de juego.
    /// </summary>
    /// <param name="kanji">El carácter Kanji a mostrar.</param>
    /// <param name="manager">La instancia de MemoryGameManager que controla la lógica.</param>
    public void Inicializar(string kanji, MemoryGameManager manager)
    {
        kanjiTexto = kanji;
        gameManager = manager;

        // Busca el componente TextMeshPro en los hijos de la tarjeta.
        textMesh = GetComponentInChildren<TextMeshPro>();
        if (textMesh != null)
        {
            textMesh.text = kanjiTexto;
            textMesh.gameObject.SetActive(false); // Oculta el texto por defecto (carta boca abajo).
        }
        else
        {
            Debug.LogError("No se encontró TextMeshPro en la tarjeta de memoria");
        }
    }

    /// <summary>
    /// Se llama cuando el usuario hace clic con el ratón sobre el collider 2D de esta tarjeta.
    /// </summary>
    void OnMouseDown()
    {
        // Solo permite la selección si la tarjeta no ha sido emparejada y el juego está activo.
        if (!fueEmparejada && gameManager != null && textMesh != null)
        {
            // Muestra el Kanji (voltea la tarjeta).
            textMesh.gameObject.SetActive(true);

            // Notifica al GameManager que esta tarjeta ha sido seleccionada.
            gameManager.OnTarjetaSeleccionada(this);
        }
    }

    /// <summary>
    /// Oculta el texto del Kanji, volteando la tarjeta de nuevo (para pares incorrectos).
    /// </summary>
    public void OcultarTexto()
    {
        // Solo oculta si la tarjeta no ha sido emparejada ya.
        if (textMesh != null && !fueEmparejada)
        {
            textMesh.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Muestra el texto permanentemente e marca la tarjeta como emparejada (para pares correctos).
    /// </summary>
    public void MostrarTextoPermanente()
    {
        if (textMesh != null)
        {
            textMesh.gameObject.SetActive(true);
            fueEmparejada = true; // Marca la tarjeta como resuelta.
        }
    }

    /// <summary>
    /// Restablece el estado de la tarjeta para una nueva partida.
    /// </summary>
    public void Reiniciar()
    {
        fueEmparejada = false;
        if (textMesh != null)
        {
            textMesh.gameObject.SetActive(false); // Oculta el texto.
        }
    }
}