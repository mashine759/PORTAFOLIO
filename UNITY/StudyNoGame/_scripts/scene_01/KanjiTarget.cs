using UnityEngine;

/// <summary>
/// Componente que se adjunta a cada tarjeta/enemigo en la escena.
/// Almacena el Kanji que representa y verifica si fue el objetivo correcto al ser impactado.
/// </summary>
public class KanjiTarget : MonoBehaviour
{
    // [SerializeField] permite que esta variable privada aparezca en el Inspector.
    [SerializeField]
    public string kanjiTexto; // El car�cter Kanji espec�fico asignado a esta tarjeta.

    /// <summary>
    /// M�todo llamado por el GameManager al spawnear la tarjeta para asignarle su identidad.
    /// </summary>
    /// <param name="kanji">El car�cter Kanji que la tarjeta debe representar.</param>
    public void Inicializar(string kanji)
    {
        kanjiTexto = kanji;
        Debug.Log($"KanjiTarget inicializado con: {kanjiTexto}");
    }

    void Start()
    {
        // Validaci�n de depuraci�n para asegurar que el Kanji fue asignado correctamente antes de empezar.
        if (string.IsNullOrEmpty(kanjiTexto))
        {
            Debug.LogError($"KanjiTarget {name} no recibi� un kanji asignado!");
        }
        else
        {
            Debug.Log($"KanjiTarget {name} tiene kanji: {kanjiTexto}");
        }
    }

    /// <summary>
    /// Se llama cuando el Collider 2D de esta tarjeta (marcado como Is Trigger)
    /// entra en contacto con otro Collider 2D.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Verifica si la colisi�n fue causada por una "Bala".
        if (other.CompareTag("Bala"))
        {
            // Destruye la bala al impactar, independientemente de si el objetivo es correcto o incorrecto.
            Destroy(other.gameObject);

            // 2. L�GICA DE VERIFICACI�N DEL OBJETIVO

            // Compara el Kanji de esta tarjeta con el Kanji objetivo actual en GameData.
            // Se usa Trim() para eliminar posibles espacios en blanco.
            if (kanjiTexto.Trim() == GameData.kanjiObjetivo.Trim())
            {
                // Objetivo Correcto

                // Llama al GameManager para iniciar la transici�n/siguiente ronda.
                GameManager.Instance.KanjiCorrecto();

                // Destruye la tarjeta objetivo (esta tarjeta).
                Destroy(gameObject);
            }
            else
            {
                // Objetivo Incorrecto

                // Llama al UIManager (asumiendo su existencia) para mostrar un mensaje de error o penalizaci�n.
                // NOTA: Aseg�rate de que UIManager.Instance exista en la escena.
                // UIManager.Instance.MostrarPanelIncorrecto();
            }
        }
    }
}