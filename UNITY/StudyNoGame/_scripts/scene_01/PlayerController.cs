using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla el movimiento horizontal del jugador, la restricción de límites
/// y la lógica de disparo para el minijuego tipo Shooter.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 8f;   // Velocidad de movimiento horizontal del jugador.
    public float limiteX = 8f; // Límite máximo en las coordenadas X (izquierda y derecha) para el jugador.

    [Header("Sprites")]
    public Sprite spriteNormal;     // Sprite a usar cuando el jugador está quieto.
    public Sprite spriteIzquierda;  // Sprite a usar cuando el jugador se mueve a la izquierda.
    public Sprite spriteDerecha;    // Sprite a usar cuando el jugador se mueve a la derecha.

    [Header("Disparo")]
    public GameObject balaPrefab;   // Prefab de la bala que se instanciará al disparar.
    public Transform puntoDisparo;  // Posición desde donde se generará la bala.
    public float balaVelocidad = 12f; // Velocidad a la que se moverá la bala.

    private SpriteRenderer spriteRenderer; // Componente para cambiar el sprite visualmente.

    void Start()
    {
        // Obtiene la referencia al componente SpriteRenderer al inicio.
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Establece el sprite inicial.
        spriteRenderer.sprite = spriteNormal;
    }

    void Update()
    {
        // Se llama en cada frame para gestionar la física del juego.
        Mover();
        Disparar();
    }

    /// <summary>
    /// Maneja la lógica de movimiento horizontal y el cambio de sprites.
    /// </summary>
    void Mover()
    {
        // Obtiene la entrada horizontal (teclas A/D o flechas). Retorna -1, 0, o 1.
        float inputX = Input.GetAxisRaw("Horizontal");

        // 1. CÁLCULO DEL MOVIMIENTO
        // Calcula el desplazamiento basado en la entrada, la velocidad y el tiempo del frame.
        Vector3 movimiento = new Vector3(inputX, 0, 0) * speed * Time.deltaTime;
        transform.Translate(movimiento);

        // 2. RESTRICCIÓN DE LÍMITES
        // Limita la posición X del jugador dentro de los límites definidos (limiteX y -limiteX).
        float xClamped = Mathf.Clamp(transform.position.x, -limiteX, limiteX);
        transform.position = new Vector3(xClamped, transform.position.y, transform.position.z);

        // 3. CAMBIO DE SPRITE
        // Cambia el sprite según la dirección del movimiento.
        if (inputX > 0)
            spriteRenderer.sprite = spriteDerecha;
        else if (inputX < 0)
            spriteRenderer.sprite = spriteIzquierda;
        else
            spriteRenderer.sprite = spriteNormal; // Vuelve al sprite normal si no hay entrada.
    }

    /// <summary>
    /// Maneja la lógica de disparo al presionar la barra espaciadora.
    /// </summary>
    void Disparar()
    {
        // Verifica si se presionó la tecla Espacio en este frame.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 1. Instancia el prefab de la bala en la posición del puntoDisparo.
            GameObject bala = Instantiate(balaPrefab, puntoDisparo.position, Quaternion.identity);

            // 2. Obtiene el Rigidbody2D y aplica la velocidad.
            Rigidbody2D rb = bala.GetComponent<Rigidbody2D>();
            // Aplica velocidad hacia arriba (Vector2.up) multiplicada por la velocidad configurada.
            rb.velocity = Vector2.up * balaVelocidad;
        }
    }
}