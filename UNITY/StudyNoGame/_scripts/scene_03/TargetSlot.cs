using UnityEngine;
using TMPro; // Necesario para interactuar con el componente TextMeshPro.

/// <summary>
/// Define un área de destino ("slot") en el minijuego de arrastrar y soltar (Lluvia de Kanjis).
/// Muestra el Kanji que debe ser soltado en esta posición y dibuja una ayuda visual en el editor.
/// </summary>
public class TargetSlot : MonoBehaviour
{
    // NOTA: Este valor es asignado por el MatchingGameManager al inicio de la ronda.
    public string kanjiAsignado; // El carácter Kanji que este slot está esperando recibir.

    void Start()
    {
        // 1. Asigna el texto al slot.
        // Busca el componente TextMeshPro en los hijos de este GameObject y le asigna el Kanji.
        TextMeshPro textMesh = GetComponentInChildren<TextMeshPro>();
        if (textMesh != null) textMesh.text = kanjiAsignado;
    }

    /// <summary>
    /// Método llamado solo en el editor para dibujar ayudas visuales.
    /// </summary>
    void OnDrawGizmos()
    {
        // 2. Dibuja un gizmo para visualizar el área de solapamiento.
        // Esto ayuda al diseñador a ver el radio donde el FallingKanjiController
        // considerará que el Kanji ha sido soltado correctamente (radio de 1.5f).
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
}