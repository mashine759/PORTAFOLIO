using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla el comportamiento de un proyectil o "bala".
/// Gestiona su tiempo de vida y qué sucede al colisionar con otros objetos.
/// </summary>
public class Bullet : MonoBehaviour
{
    public float tiempoVida = 3f; // Duración máxima en segundos antes de que la bala se autodestruya.

    void Start()
    {
        // Destruye el GameObject de la bala después de 'tiempoVida' segundos,
        // previniendo que las balas que fallan sigan existiendo indefinidamente en la escena.
        Destroy(gameObject, tiempoVida);
    }

    /// <summary>
    /// Se llama cuando este Collider 2D (marcado como Is Trigger) entra en contacto con otro Collider 2D.
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Verificación de la colisión

        // Comprueba si el objeto con el que colisionamos tiene el tag "Kanji".
        if (other.CompareTag("Kanji"))
        {
            // 2. Lógica de Impacto

            // NOTA: Aquí debería ir la lógica para notificar al Kanji que fue impactado (ej. other.GetComponent<KanjiTarget>().OnHit()).

            // Destruye la bala inmediatamente al impactar un Kanji (para que no lo atraviese).
            Destroy(gameObject);
        }
    }
}