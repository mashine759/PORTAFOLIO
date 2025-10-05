using UnityEngine;
using TMPro; // Para usar TextMeshPro
using UnityEngine.SceneManagement; // Para gestionar el cambio de escenas
using System.Collections.Generic; // Para usar Listas
using UnityEngine.UI; // Para interactuar con botones y UI
using System.Runtime.InteropServices; // Necesario para la comunicación DllImport con JavaScript (WebGL)

// Script Singleton para gestionar la lista de Kanjis y la entrada nativa de WebGL.
public class addkanji : MonoBehaviour
{
    // [Header("DOM Input ID (debe coincidir con el .jslib)")] // Faltaba el campo aquí
    [SerializeField] private string inputId = "KanjiInput"; // ID que usa el JS para encontrar el campo input DOM.

    [Header("Displays de Kanji")]
    [SerializeField] private TMP_Text[] kanjiDisplay; // Referencias a los objetos de texto que muestran los Kanjis seleccionados.

    private int maxKanjis = 5; // Límite máximo de Kanjis que el usuario puede ingresar.
    private static addkanji instance; // Variable estática para implementar el patrón Singleton.

    // BLOQUE DE IMPORTACIÓN DE FUNCIONES JAVASCRIPT (SOLO EN WEBGL)
#if !UNITY_EDITOR && UNITY_WEBGL
    [DllImport("__Internal")] private static extern void CreateNativeInput(string id, string placeholder); // Crea el elemento <input> en el DOM.
    [DllImport("__Internal")] private static extern string GetNativeInputText(string id); // Obtiene el valor del input DOM.
    [DllImport("__Internal")] private static extern void ClearNativeInput(string id); // Borra el texto del input DOM.
    [DllImport("__Internal")] private static extern void FocusNativeInput(string id); // Pone el cursor en el input DOM.
    [DllImport("__Internal")] private static extern void SetNativeInputPosition(string id, float x, float y, float width, float height); // Posiciona y dimensiona el input DOM en la pantalla.
    [DllImport("__Internal")] private static extern void HideNativeInput(string id); // Oculta el input DOM (CSS display: none).
    [DllImport("__Internal")] private static extern void ShowNativeInput(string id); // Muestra el input DOM.
#endif

    private void Awake()
    {
        // 1. Implementación del Singleton (asegura que solo haya una instancia en todo el juego)
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject); // Persiste el objeto entre escenas.

        // 2. Suscripción al evento de carga de escena para reconectar la UI.
        SceneManager.sceneLoaded += OnSceneLoaded;

#if !UNITY_EDITOR && UNITY_WEBGL
        // Desactiva la captura de teclado de Unity al inicio (permite que el navegador maneje los inputs).
        // NOTA: Esto se activa y desactiva dinámicamente para los niveles de juego.
        WebGLInput.captureAllKeyboardInput = false;
#endif
    }

    private void OnDestroy()
    {
        // Desuscripción del evento para evitar errores cuando el objeto es destruido.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Inicia una corrutina para reconectar la UI después de que la escena haya terminado de cargar un frame.
        // Esto es crucial para encontrar objetos nuevos o inactivos en la escena.
        StartCoroutine(ReconnectDelayed());
    }

    private System.Collections.IEnumerator ReconnectDelayed()
    {
        yield return null; // Espera un frame.
        LimpiarReferencias(); // Borra referencias de la escena anterior (si existen).
        ReconectarUI(); // Busca elementos de texto (displays) en la nueva escena.
        ReconectarBotones(); // Asigna funciones a los botones de la nueva escena.
        ActualizarDisplays(); // Muestra los Kanjis guardados.
    }

    private void OnEnable()
    {
        // Se llama cuando el objeto se activa. Útil para la primera carga en el editor.
        ReconectarUI();
        ActualizarDisplays();
    }

    private void LimpiarReferencias()
    {
        // Anula la referencia al array de displays para forzar su reconexión en la nueva escena.
        kanjiDisplay = null;
    }

    private void ReconectarBotones()
    {
        // Busca y reasigna los listeners (funciones onClick) a todos los botones de la UI.

        // Botones de juego
        Button shooterBtn = FindInactiveObject("Btn_Shooter")?.GetComponent<Button>();
        if (shooterBtn != null)
        {
            shooterBtn.onClick.RemoveAllListeners();
            shooterBtn.onClick.AddListener(ComenzarJuegoShooter);
        }
        // ... (lógica similar para Btn_Memoria y Btn_Lluvia) ...

        // Memoria
        Button memoriaBtn = FindInactiveObject("Btn_Memoria")?.GetComponent<Button>();
        if (memoriaBtn != null)
        {
            memoriaBtn.onClick.RemoveAllListeners();
            memoriaBtn.onClick.AddListener(ComenzarJuegoMemoria);
        }

        // Lluvia
        Button lluviaBtn = FindInactiveObject("Btn_Lluvia")?.GetComponent<Button>();
        if (lluviaBtn != null)
        {
            lluviaBtn.onClick.RemoveAllListeners();
            lluviaBtn.onClick.AddListener(ComenzarJuegoLluvia);
        }


        // Botones de gestión de Kanjis
        Button vaciarBtn = FindInactiveObject("Btn_VaciarLista")?.GetComponent<Button>();
        if (vaciarBtn != null)
        {
            vaciarBtn.onClick.RemoveAllListeners();
            vaciarBtn.onClick.AddListener(VaciarListaCompleta);
        }

        Button agregarBtn = FindInactiveObject("Btn_AgregarKanji")?.GetComponent<Button>();
        if (agregarBtn != null)
        {
            agregarBtn.onClick.RemoveAllListeners();
            agregarBtn.onClick.AddListener(AgregarKanji);
        }
    }

    private void ReconectarUI()
    {
        // 1. Reconecta los displays de Kanji si no están asignados (o se limpiaron).
        if (kanjiDisplay == null || kanjiDisplay.Length == 0)
        {
            List<TMP_Text> displaysList = new List<TMP_Text>();

            for (int i = 0; i < maxKanjis; i++)
            {
                // Busca los objetos por nombre (KanjiDisplay_0, KanjiDisplay_1, etc.)
                GameObject displayObj = FindInactiveObject("KanjiDisplay_" + i);
                if (displayObj != null)
                {
                    TMP_Text text = displayObj.GetComponent<TMP_Text>();
                    if (text != null)
                        displaysList.Add(text);
                }
            }
            kanjiDisplay = displaysList.ToArray();
        }

        // 2. Gestión del Input DOM (Solo en WebGL)
#if !UNITY_EDITOR && UNITY_WEBGL
        CreateNativeInput(inputId, "..."); // Asegura que el input DOM exista.
        PositionInputForWebGL(); // Lo posiciona (usando el placeholder visual de Unity).
        HideNativeInput(inputId); // Lo oculta por defecto al reconectar (solo debe verse en el menú).
#endif
    }

    /// <summary>
    /// Muestra el Input Field nativo (DOM) y le da foco. Llamado por menuController al abrir el menú de Kanjis.
    /// </summary>
    public void MostrarInputField()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
    // Se reposiciona antes de mostrar, en caso de que el tamaño de la ventana haya cambiado.
    PositionInputForWebGL();
    ShowNativeInput(inputId);
    FocusNativeInput(inputId);
#endif
    }

    /// <summary>
    /// Oculta el Input Field nativo (DOM). Llamado por menuController al cerrar el menú de Kanjis.
    /// </summary>
    public void OcultarInputField()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
    HideNativeInput(inputId);
#endif
    }

    /// <summary>
    /// Calcula la posición y tamaño del input DOM basándose en un RectTransform de Unity (placeholder).
    /// </summary>
    private void PositionInputForWebGL()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
    // 1. Busca el objeto visual (placeholder) en el Canvas de Unity.
    GameObject placeholder = FindInactiveObject("InputFieldKanji");

    if (placeholder != null)
    {
        RectTransform rt = placeholder.GetComponent<RectTransform>(); 

        // 2. Convierte la posición del centro del placeholder a píxeles de pantalla.
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, rt.position);
        
        // 3. Calcula el ancho y alto real en píxeles (incluyendo la escala del Canvas).
        float width = rt.rect.width * rt.lossyScale.x;
        float height = rt.rect.height * rt.lossyScale.y;

        // 4. CALCULA LA POSICIÓN TOP-LEFT (domX, domY)
        
        // domX (left): Posición Central X - mitad del ancho.
        float domX = screenPoint.x - width / 2;

        // domY (top): Invierte el eje Y (Screen.height - Central Y) y luego corrige al top.
        // La inversión es necesaria porque Unity es Bottom-Up y DOM es Top-Down.
        float domY = (Screen.height - screenPoint.y) - height / 2;

        // 5. Envía la posición al JSLIB (el JSLIB añade el offset del canvas).
        SetNativeInputPosition(inputId, domX, domY, width, height);
    }
#endif
    }

    /// <summary>
    /// Activa o desactiva la captura de teclado de Unity.
    /// False: El navegador maneja el teclado (para usar el input DOM).
    /// True: Unity maneja el teclado (para jugar).
    /// </summary>
    public void SetKeyboardInputCapture(bool capture)
    {
#if !UNITY_EDITOR && UNITY_WEBGL
    UnityEngine.WebGLInput.captureAllKeyboardInput = capture;
    Debug.Log($"WebGL Input Capture: {capture}");
#endif
    }

    /// <summary>
    /// Método utilitario para encontrar GameObjects (incluyendo inactivos) por su nombre.
    /// </summary>
    private GameObject FindInactiveObject(string name)
    {
        Scene scene = SceneManager.GetActiveScene();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            // Busca en todos los hijos, activos o inactivos (true)
            Transform[] all = root.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in all)
            {
                if (t.name == name)
                    return t.gameObject;
            }
        }
        return null; // Devuelve null si no lo encuentra.
    }

    /// <summary>
    /// Obtiene el texto del input DOM y lo añade a la lista de Kanjis seleccionados.
    /// </summary>
    public void AgregarKanji()
    {
        string nuevoKanji = "";

#if !UNITY_EDITOR && UNITY_WEBGL
        nuevoKanji = GetNativeInputText(inputId).Trim(); // Obtiene texto del DOM.
#else
        Debug.LogWarning("En editor, no se usa input DOM. Ingresa texto manual si quieres probar.");
#endif

        if (string.IsNullOrEmpty(nuevoKanji)) return;
        if (GameData.kanjisSelect.Contains(nuevoKanji)) return; // Evita duplicados.
        if (GameData.kanjisSelect.Count >= maxKanjis) return; // Limita la cantidad.

        GameData.kanjisSelect.Add(nuevoKanji);
        ActualizarDisplays();

#if !UNITY_EDITOR && UNITY_WEBGL
        ClearNativeInput(inputId); // Limpia el input DOM después de añadir.
#endif
    }

    /// <summary>
    /// Simplemente borra la lista de Kanjis de la memoria de la sesión.
    /// </summary>
    public void LimpiarKanjis()
    {
        GameData.kanjisSelect.Clear();
        ActualizarDisplays();
    }

    /// <summary>
    /// Actualiza los displays de texto (TMP_Text) de Unity con la lista actual de Kanjis.
    /// </summary>
    private void ActualizarDisplays()
    {
        if (kanjiDisplay == null || kanjiDisplay.Length == 0) ReconectarUI();
        if (kanjiDisplay == null) return;

        // 1. Limpiar todos los displays primero.
        foreach (var display in kanjiDisplay)
        {
            if (display != null) display.text = "";
        }

        // 2. Escribir los Kanjis actuales en los displays disponibles.
        for (int i = 0; i < GameData.kanjisSelect.Count; i++)
        {
            if (i < kanjiDisplay.Length && kanjiDisplay[i] != null)
            {
                kanjiDisplay[i].text = GameData.kanjisSelect[i];
            }
        }
    }

    /// <summary>
    /// Vacía la lista de Kanjis y borra las entradas persistentes de PlayerPrefs.
    /// </summary>
    public void VaciarListaCompleta()
    {
        GameData.kanjisSelect.Clear();
        ActualizarDisplays();

#if !UNITY_EDITOR && UNITY_WEBGL
        ClearNativeInput(inputId);
#endif
        // Borra las entradas guardadas.
        PlayerPrefs.DeleteKey("KanjiCount");
        for (int i = 0; i < maxKanjis; i++)
            PlayerPrefs.DeleteKey("Kanji_" + i);

        PlayerPrefs.Save();
    }

    // --- MÉTODOS DE INICIO DE JUEGO ---

    public void ComenzarJuegoShooter()
    {
        if (GameData.kanjisSelect.Count == 0) return;
        SetKeyboardInputCapture(true); // Activa el teclado de Unity (asumiendo que el juego lo necesita)
        OcultarInputField(); // Asegura que el input DOM está oculto
        SceneManager.LoadScene("Scene_01");
    }

    public void ComenzarJuegoMemoria()
    {
        if (GameData.kanjisSelect.Count == 0) return;
        SceneManager.LoadScene("Scene_02");
    }

    public void ComenzarJuegoLluvia()
    {
        if (GameData.kanjisSelect.Count == 0) return;
        SetKeyboardInputCapture(true); // Activa el teclado de Unity
        OcultarInputField(); // Asegura que el input DOM está oculto
        GuardarKanjisEnPlayerPrefs(); // Guarda la lista antes de ir a la escena
        SceneManager.LoadScene("Scene_03");
    }

    private void GuardarKanjisEnPlayerPrefs()
    {
        // Guarda la lista de Kanjis seleccionados para que sean accesibles en otras escenas
        PlayerPrefs.SetInt("KanjiCount", GameData.kanjisSelect.Count);
        for (int i = 0; i < GameData.kanjisSelect.Count; i++)
        {
            PlayerPrefs.SetString("Kanji_" + i, GameData.kanjisSelect[i]);
        }
        PlayerPrefs.Save();
    }
}