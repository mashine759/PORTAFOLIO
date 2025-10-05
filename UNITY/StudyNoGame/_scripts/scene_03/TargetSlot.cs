using UnityEngine;
using TMPro; // Necesario para interactuar con el componente TextMeshPro.

/// <summary>
/// Define un �rea de destino ("slot") en el minijuego de arrastrar y soltar (Lluvia de Kanjis).
/// Muestra el Kanji que debe ser soltado en esta posici�n y dibuja una ayuda visual en el editor.
/// </summary>
public class TargetSlot : MonoBehaviour
{
    // NOTA: Este valor es asignado por el MatchingGameManager al inicio de la ronda.
    public string kanjiAsignado; // El car�cter Kanji que este slot est� esperando recibir.

    void Start()
    {
        // 1. Asigna el texto al slot.
        // Busca el componente TextMeshPro en los hijos de este GameObject y le asigna el Kanji.
        TextMeshPro textMesh = GetComponentInChildren<TextMeshPro>();
        if (textMesh != null) textMesh.text = kanjiAsignado;
    }

    /// <summary>
    /// M�todo llamado solo en el editor para dibujar ayudas visuales.
    /// </summary>
    void OnDrawGizmos()
    {
        // 2. Dibuja un gizmo para visualizar el �rea de solapamiento.
        // Esto ayuda al dise�ador a ver el radio donde el FallingKanjiController
        // considerar� que el Kanji ha sido soltado correctamente (radio de 1.5f).
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}