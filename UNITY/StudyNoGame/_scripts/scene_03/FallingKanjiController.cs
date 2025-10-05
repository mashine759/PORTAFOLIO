using UnityEngine;
using TMPro; // Necesario para interactuar con componentes TextMeshPro.

/// <summary>
/// Controla el comportamiento de un Kanji individual que cae en el minijuego de arrastrar y soltar.
/// Maneja la caída, la detección de arrastre del usuario y la verificación de colisión con el límite inferior.
/// </summary>
public class FallingKanjiController : MonoBehaviour
{
    public string kanjiTexto;           // El carácter Kanji asignado a este objeto.
    public float fallSpeed;             // Velocidad de caída del Kanji.
    public bool siendoArrastrado = false; // Bandera que indica si el usuario está arrastrando el objeto con el ratón.

    private TextMeshPro textMesh;       // Referencia al componente de texto que muestra el Kanji.
    private float bottomLimit;          // Límite Y en coordenadas del mundo, si el Kanji lo cruza, es un fallo.

    /// <summary>
    /// Inicializa el Kanji con su valor y velocidad, y calcula el límite inferior de la pantalla.
    /// </summary>
    /// <param name="kanji">El carácter Kanji a mostrar.</param>
    /// <param name="speed">La velocidad de caída.</param>
    public void Inicializar(string kanji, float speed)
    {
        kanjiTexto = kanji;
        fallSpeed = speed;

        textMesh = GetComponentInChildren<TextMeshPro>();
        if (textMesh != null) textMesh.text = kanjiTexto;

        // Calcula el límite inferior de la pantalla (Viewport Y=0) con un pequeño margen extra (-1f).
        bottomLimit = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y - 1f;
    }

    void Update()
    {
        // La caída solo ocurre si el Kanji NO está siendo arrastrado por el usuario.
        if (!siendoArrastrado)
        {
            // Caída normal: mueve el objeto hacia abajo.
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

            // Verificar si el Kanji llegó al fondo (falló).
            if (transform.position.y <= bottomLimit)
            {
                // Notifica al GameManager que este Kanji cayó al suelo.
                MatchingGameManager.Instance.KanjiLlegoAlFondo(kanjiTexto);

                // Destruye el objeto Kanji.
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Se llama una vez al hacer clic con el ratón sobre el collider del Kanji.
    /// </summary>
    void OnMouseDown()
    {
        // Activa la bandera de arrastre.
        siendoArrastrado = true;
    }

    /// <summary>
    /// Se llama continuamente mientras el ratón está presionado sobre el Kanji.
    /// </summary>
    void OnMouseDrag()
    {
        if (siendoArrastrado)
        {
            // Convierte la posición del ratón en la pantalla a coordenadas del mundo.
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Mueve el Kanji para que siga la posición del ratón (manteniendo la Z en 0).
            transform.position = new Vector3(mousePos.x, mousePos.y, 0);
        }
    }

    /// <summary>
    /// Se llama una vez cuando el usuario suelta el botón del ratón.
    /// </summary>
    void OnMouseUp()
    {
        // Desactiva la bandera de arrastre.
        siendoArrastrado = false;

        // Comprueba si el Kanji fue soltado sobre el objetivo correcto.
        VerificarSolapamientoConSlot();
    }

    /// <summary>
    /// Comprueba la proximidad del Kanji con el slot objetivo correspondiente al Kanji.
    /// </summary>
    private void VerificarSolapamientoConSlot()
    {
        // Obtiene la posición del slot objetivo del GameManager usando el KanjiTexto.
        Transform targetSlot = MatchingGameManager.Instance.GetTargetSlotForKanji(kanjiTexto);

        // Verifica si hay un slot objetivo y si la distancia es lo suficientemente corta (solapamiento).
        if (targetSlot != null && Vector3.Distance(transform.position, targetSlot.position) < 1.5f)
        {
            // ¡Acción Correcta!

            // Notifica al GameManager para que registre el punto o avance el juego.
            MatchingGameManager.Instance.KanjiArrastradoACorrecto(kanjiTexto);

            // Destruye el Kanji, ya que fue capturado con éxito.
            Destroy(gameObject);
        }
    }
}