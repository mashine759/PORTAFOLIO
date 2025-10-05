using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla el movimiento horizontal del jugador, la restricci�n de l�mites
/// y la l�gica de disparo para el minijuego tipo Shooter.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 8f;   // Velocidad de movimiento horizontal del jugador.
    public float limiteX = 8f; // L�mite m�ximo en las coordenadas X (izquierda y derecha) para el jugador.

    [Header("Sprites")]
    public Sprite spriteNormal;     // Sprite a usar cuando el jugador est� quieto.
    public Sprite spriteIzquierda;  // Sprite a usar cuando el jugador se mueve a la izquierda.
    public Sprite spriteDerecha;    // Sprite a usar cuando el jugador se mueve a la derecha.

    [Header("Disparo")]
    public GameObject balaPrefab;   // Prefab de la bala que se instanciar� al disparar.
    public Transform puntoDisparo;  // Posici�n desde donde se generar� la bala.
    public float balaVelocidad = 12f; // Velocidad a la que se mover� la bala.

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
        // Se llama en cada frame para gestionar la f�sica del juego.
        Mover();
        Disparar();
    }

    /// <summary>
    /// Maneja la l�gica de movimiento horizontal y el cambio de sprites.
    /// </summary>
    void Mover()
    {
        // Obtiene la entrada horizontal (teclas A/D o flechas). Retorna -1, 0, o 1.
        float inputX = Input.GetAxisRaw("Horizontal");

        // 1. C�LCULO DEL MOVIMIENTO
        // Calcula el desplazamiento basado en la entrada, la velocidad y el tiempo del frame.
        Vector3 movimiento = new Vector3(inputX, 0, 0) * speed * Time.deltaTime;
        transform.Translate(movimiento);

        // 2. RESTRICCI�N DE L�MITES
        // Limita la posici�n X del jugador dentro de los l�mites definidos (limiteX y -limiteX).
        float xClamped = Mathf.Clamp(transform.position.x, -limiteX, limiteX);
        transform.position = new Vector3(xClamped, transform.position.y, transform.position.z);

        // 3. CAMBIO DE SPRITE
        // Cambia el sprite seg�n la direcci�n del movimiento.
        if (inputX > 0)
            spriteRenderer.sprite = spriteDerecha;
        else if (inputX < 0)
            spriteRenderer.sprite = spriteIzquierda;
        else
            spriteRenderer.sprite = spriteNormal; // Vuelve al sprite normal si no hay entrada.
    }

    /// <summary>
    /// Maneja la l�gica de disparo al presionar la barra espaciadora.
    /// </summary>
    void Disparar()
    {
        // Verifica si se presion� la tecla Espacio en este frame.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 1. Instancia el prefab de la bala en la posici�n del puntoDisparo.
            GameObject bala = Instantiate(balaPrefab, puntoDisparo.position, Quaternion.identity);

            // 2. Obtiene el Rigidbody2D y aplica la velocidad.
            Rigidbody2D rb = bala.GetComponent<Rigidbody2D>();
            // Aplica velocidad hacia arriba (Vector2.up) multiplicada por la velocidad configurada.
            rb.velocity = Vector2.up * balaVelocidad;
        }
    }
}