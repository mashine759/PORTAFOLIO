using UnityEngine;
using TMPro; // Necesario para interactuar con componentes TextMeshPro.

/// <summary>
/// Controla el comportamiento de un Kanji individual que cae en el minijuego de arrastrar y soltar.
/// Maneja la ca�da, la detecci�n de arrastre del usuario y la verificaci�n de colisi�n con el l�mite inferior.
/// </summary>
public class FallingKanjiController : MonoBehaviour
{
    public string kanjiTexto;           // El car�cter Kanji asignado a este objeto.
    public float fallSpeed;             // Velocidad de ca�da del Kanji.
    public bool siendoArrastrado = false; // Bandera que indica si el usuario est� arrastrando el objeto con el rat�n.

    private TextMeshPro textMesh;       // Referencia al componente de texto que muestra el Kanji.
    private float bottomLimit;          // L�mite Y en coordenadas del mundo, si el Kanji lo cruza, es un fallo.

    /// <summary>
    /// Inicializa el Kanji con su valor y velocidad, y calcula el l�mite inferior de la pantalla.
    /// </summary>
    /// <param name="kanji">El car�cter Kanji a mostrar.</param>
    /// <param name="speed">La velocidad de ca�da.</param>
    public void Inicializar(string kanji, float speed)
    {
        kanjiTexto = kanji;
        fallSpeed = speed;

        textMesh = GetComponentInChildren<TextMeshPro>();
        if (textMesh != null) textMesh.text = kanjiTexto;

        // Calcula el l�mite inferior de la pantalla (Viewport Y=0) con un peque�o margen extra (-1f).
        bottomLimit = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y - 1f;
    }

    void Update()
    {
        // La ca�da solo ocurre si el Kanji NO est� siendo arrastrado por el usuario.
        if (!siendoArrastrado)
        {
            // Ca�da normal: mueve el objeto hacia abajo.
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

            // Verificar si el Kanji lleg� al fondo (fall�).
            if (transform.position.y <= bottomLimit)
            {
                // Notifica al GameManager que este Kanji cay� al suelo.
                MatchingGameManager.Instance.KanjiLlegoAlFondo(kanjiTexto);

                // Destruye el objeto Kanji.
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Se llama una vez al hacer clic con el rat�n sobre el collider del Kanji.
    /// </summary>
    void OnMouseDown()
    {
        // Activa la bandera de arrastre.
        siendoArrastrado = true;
    }

    /// <summary>
    /// Se llama continuamente mientras el rat�n est� presionado sobre el Kanji.
    /// </summary>
    void OnMouseDrag()
    {
        if (siendoArrastrado)
        {
            // Convierte la posici�n del rat�n en la pantalla a coordenadas del mundo.
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Mueve el Kanji para que siga la posici�n del rat�n (manteniendo la Z en 0).
            transform.position = new Vector3(mousePos.x, mousePos.y, 0);
        }
    }

    /// <summary>
    /// Se llama una vez cuando el usuario suelta el bot�n del rat�n.
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
        // Obtiene la posici�n del slot objetivo del GameManager usando el KanjiTexto.
        Transform targetSlot = MatchingGameManager.Instance.GetTargetSlotForKanji(kanjiTexto);

        // Verifica si hay un slot objetivo y si la distancia es lo suficientemente corta (solapamiento).
        if (targetSlot != null && Vector3.Distance(transform.position, targetSlot.position) < 1.5f)
        {
            // �Acci�n Correcta!

            // Notifica al GameManager para que registre el punto o avance el juego.
            MatchingGameManager.Instance.KanjiArrastradoACorrecto(kanjiTexto);

            // Destruye el Kanji, ya que fue capturado con �xito.
            Destroy(gameObject);
        }
    }
}