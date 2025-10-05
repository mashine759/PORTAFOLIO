using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro; // Para trabajar con TextMeshPro
using UnityEngine.UI; // Para trabajar con elementos de UI como Button

/// <summary>
/// GameManager principal para el minijuego de "Lluvia de Kanjis".
/// Controla el flujo del juego, la dificultad, la puntuaci�n, el spawn de Kanjis y la condici�n de Game Over.
/// Implementa el patr�n Singleton.
/// </summary>
public class MatchingGameManager : MonoBehaviour
{
    public static MatchingGameManager Instance; // Referencia est�tica al Singleton.

    [Header("Referencias de Gameplay")]
    public GameObject kanjiFallingPrefab;    // Prefab de los Kanjis que caen.
    public Transform[] targetSlots;          // Array de Transforms que representan los "tanukis" o �reas de destino.
    public Transform spawnAreaTop;           // Posici�n superior desde donde caen los Kanjis.

    [Header("Dificultad")]
    public float initialSpawnRate = 2f;      // Tiempo inicial entre spawns (segundos).
    public float initialFallSpeed = 2f;      // Velocidad de ca�da inicial.
    public float difficultyIncreaseRate = 0.1f; // Tasa base de aumento de dificultad.

    [Header("UI Elements")]
    public TMP_Text scoreText;              // Display de la puntuaci�n en el juego.
    public GameObject gameOverPanel;        // Panel que se muestra al perder.
    public TMP_Text finalScoreText;         // Texto para mostrar la puntuaci�n en el panel de Game Over.
    public Button restartButton;            // Bot�n para reiniciar el juego.

    // --- Variables de Estado Interno ---
    [SerializeField]
    private List<string> kanjisActivos = new List<string>(); // La sub-selecci�n de Kanjis usados en esta ronda (m�ximo 5).
    [SerializeField]
    // Diccionario que mapea un Kanji a su slot de destino correcto (ej: "Kanji" -> Slot_1_Transform).
    private Dictionary<string, Transform> slotAssignments = new Dictionary<string, Transform>();

    private float currentSpawnRate;         // Tasa de spawn actual (disminuye con el tiempo).
    private float currentFallSpeed;         // Velocidad de ca�da actual (aumenta con el tiempo).
    private int score = 0;                  // Puntuaci�n actual.
    private bool gameRunning = true;        // Bandera para controlar el bucle principal del juego.

    private void Awake()
    {
        // L�gica del Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // NOTA: Se asume que este nivel es cargado despu�s del men� donde se guardan los Kanjis.
        CargarKanjisDesdePlayerPrefs();
        SetupUI();
        IniciarJuego();
    }

    private void SetupUI()
    {
        // Asigna el m�todo ReiniciarJuego al bot�n de reinicio.
        if (restartButton != null)
            restartButton.onClick.AddListener(ReiniciarJuego);

        // Asegura que el panel de Game Over est� oculto al inicio.
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        ActualizarScoreUI();
    }

    /// <summary>
    /// Inicia o reinicia la partida, restableciendo todas las variables de estado.
    /// </summary>
    public void IniciarJuego()
    {
        gameRunning = true;
        score = 0;
        // Restablece la dificultad a los valores iniciales.
        currentSpawnRate = initialSpawnRate;
        currentFallSpeed = initialFallSpeed;

        kanjisActivos.Clear();
        slotAssignments.Clear();

        // Limpia cualquier Kanji que haya quedado de una partida anterior.
        LimpiarTarjetasExistentes();

        // Oculta el panel de Game Over si estaba visible.
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        AsignarKanjisASlots();        // Define qu� Kanji va a qu� slot.
        StartCoroutine(SpawnCoroutine()); // Inicia el bucle de spawn.

        ActualizarScoreUI();
    }

    /// <summary>
    /// Busca y destruye todos los Kanjis que a�n est�n cayendo en la escena.
    /// </summary>
    private void LimpiarTarjetasExistentes()
    {
        FallingKanjiController[] tarjetas = FindObjectsOfType<FallingKanjiController>();
        foreach (var tarjeta in tarjetas)
        {
            Destroy(tarjeta.gameObject);
        }
    }

    /// <summary>
    /// Actualiza el display de puntuaci�n en la UI.
    /// </summary>
    private void ActualizarScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    /// <summary>
    /// Carga los Kanjis seleccionados del men� guardados en PlayerPrefs en GameData.
    /// </summary>
    private void CargarKanjisDesdePlayerPrefs()
    {
        // Limpia la lista est�tica antes de cargar.
        GameData.kanjisSelect.Clear();

        int count = PlayerPrefs.GetInt("KanjiCount", 0);

        if (count == 0)
        {
            Debug.LogWarning("No hay kanjis en PlayerPrefs, usando datos por defecto");
            // Kanjis de fallback si el usuario no ingres� nada (ej: si se inicia la escena directamente).
            GameData.kanjisSelect = new List<string> { "x", "4", "1", "0", "b" };
            return;
        }

        for (int i = 0; i < count; i++)
        {
            string kanji = PlayerPrefs.GetString("Kanji_" + i, "");
            if (!string.IsNullOrEmpty(kanji))
            {
                GameData.kanjisSelect.Add(kanji);
            }
        }

        Debug.Log($"Cargados {GameData.kanjisSelect.Count} kanjis desde PlayerPrefs");
    }

    /// <summary>
    /// Asigna aleatoriamente los Kanjis cargados a los slots de destino (tanukis).
    /// </summary>
    private void AsignarKanjisASlots()
    {
        if (GameData.kanjisSelect.Count == 0)
        {
            Debug.LogError("No hay kanjis en la lista!");
            return;
        }

        // 1. Mezcla la lista completa de Kanjis disponibles.
        List<string> kanjisMezclados = MezclarLista(new List<string>(GameData.kanjisSelect));

        // 2. Asigna Kanjis a los slots de destino (limitado a 5 o menos slots/Kanjis).
        for (int i = 0; i < Mathf.Min(targetSlots.Length, kanjisMezclados.Count); i++)
        {
            string kanji = kanjisMezclados[i];
            kanjisActivos.Add(kanji); // Agrega a la lista de Kanjis que caer�n.
            slotAssignments[kanji] = targetSlots[i]; // Mapea el Kanji al Transform del slot.

            // 3. Actualiza el texto en el slot de destino para que el jugador sepa d�nde soltarlo.
            TextMeshPro slotText = targetSlots[i].GetComponentInChildren<TextMeshPro>();
            if (slotText != null) slotText.text = kanji;
        }
    }

    /// <summary>
    /// Coroutine principal que maneja el spawn de Kanjis y el aumento de dificultad.
    /// </summary>
    private IEnumerator SpawnCoroutine()
    {
        while (gameRunning)
        {
            SpawnFallingKanji();

            yield return new WaitForSeconds(currentSpawnRate); // Espera seg�n la tasa de spawn actual.

            // Aumento de dificultad por tiempo (disminuye el tiempo de espera).
            currentSpawnRate = Mathf.Max(0.5f, currentSpawnRate - difficultyIncreaseRate * 0.1f);

            // Aumento de la velocidad de ca�da.
            currentFallSpeed += difficultyIncreaseRate;
        }
    }

    /// <summary>
    /// Instancia un Kanji que cae en una posici�n horizontal aleatoria en la parte superior.
    /// </summary>
    private void SpawnFallingKanji()
    {
        if (kanjisActivos.Count == 0) return;

        // 1. Elige un Kanji aleatorio de la lista de Kanjis activos.
        string randomKanji = kanjisActivos[Random.Range(0, kanjisActivos.Count)];

        // 2. Calcula una posici�n de spawn aleatoria a lo largo del eje X.
        Vector3 spawnPos = new Vector3(
            Random.Range(spawnAreaTop.position.x - 5f, spawnAreaTop.position.x + 5f),
            spawnAreaTop.position.y,
            0
        );

        // 3. Instancia y Inicializa el Kanji.
        GameObject fallingKanji = Instantiate(kanjiFallingPrefab, spawnPos, Quaternion.identity);
        FallingKanjiController controller = fallingKanji.GetComponent<FallingKanjiController>();
        controller.Inicializar(randomKanji, currentFallSpeed);
    }

    /// <summary>
    /// Llamado por FallingKanjiController cuando un Kanji es arrastrado y soltado en el slot correcto.
    /// </summary>
    /// <param name="kanji">El Kanji capturado.</param>
    public void KanjiArrastradoACorrecto(string kanji)
    {
        score++;
        Debug.Log($"Score: {score}");

        // Aumento de dificultad por puntuaci�n (ej: cada 5 puntos).
        if (score % 5 == 0)
        {
            currentSpawnRate = Mathf.Max(0.3f, currentSpawnRate - 0.1f);
            currentFallSpeed += 0.2f;
        }

        ActualizarScoreUI();
    }

    /// <summary>
    /// Llamado por FallingKanjiController cuando un Kanji cruza el l�mite inferior (fallo).
    /// Detiene el juego inmediatamente.
    /// </summary>
    /// <param name="kanji">El Kanji que cay� al suelo.</param>
    public void KanjiLlegoAlFondo(string kanji)
    {
        Debug.Log("�Game Over!");
        gameRunning = false;
        StopAllCoroutines(); // Detiene el bucle de spawn.

        MostrarGameOver();
    }

    /// <summary>
    /// Muestra el panel de Game Over con la puntuaci�n final.
    /// </summary>
    private void MostrarGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (finalScoreText != null)
                finalScoreText.text = $"Final Score: {score}";
        }
    }

    /// <summary>
    /// M�todo p�blico llamado por el bot�n de UI para reiniciar la partida.
    /// </summary>
    public void ReiniciarJuego()
    {
        IniciarJuego();
    }

    /// <summary>
    /// Devuelve el Transform del slot de destino correcto para un Kanji dado.
    /// Llamado por FallingKanjiController para verificar el solapamiento.
    /// </summary>
    /// <param name="kanji">El Kanji cuyo slot se busca.</param>
    /// <returns>El Transform del slot objetivo o null.</returns>
    public Transform GetTargetSlotForKanji(string kanji)
    {
        // Usa ContainsKey para verificar si el Kanji tiene un slot asignado y lo devuelve.
        return slotAssignments.ContainsKey(kanji) ? slotAssignments[kanji] : null;
    }

    /// <summary>
    /// Implementaci�n del algoritmo de mezcla (shuffle) Fisher-Yates para aleatorizar la lista.
    /// </summary>
    private List<string> MezclarLista(List<string> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        {
            int rnd = Random.Range(i, lista.Count);
            string temp = lista[i];
            lista[i] = lista[rnd];
            lista[rnd] = temp;
        }
        return lista;
    }
}