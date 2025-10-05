using UnityEngine;
using TMPro; // Necesario para interactuar con componentes TextMeshPro
using UnityEngine.SceneManagement; // Necesario para manejar eventos de carga de escena

/// <summary>
/// Gestiona la localizaci�n y traducci�n de textos de UI en la aplicaci�n.
/// Utiliza el patr�n Singleton para mantener el idioma seleccionado entre escenas.
/// </summary>
public class LanguageController : MonoBehaviour
{
    public static LanguageController Instance; // Referencia est�tica al Singleton.

    /// <summary>
    /// Enumeraci�n que define los idiomas soportados.
    /// Sus valores num�ricos (0, 1, etc.) coinciden con los �ndices del Dropdown.
    /// </summary>
    public enum Language { Spanish, English }
    public Language currentLanguage = Language.Spanish; // Idioma seleccionado actualmente.

    [Header("Referencias de UI")]
    public TMP_Dropdown languageDropdown; // Control de men� desplegable para cambiar el idioma.

    [Header("Textos traducibles")]
    // Textos largos de instrucciones para cada minijuego.
    public TMP_Text howToPlayShooter;
    public TMP_Text howToPlayMemory;
    public TMP_Text howToPlayRain;

    // Textos cortos de la interfaz (men�s y botones).
    public TMP_Text textHowtoplay;
    public TMP_Text textLanguaje;
    public TMP_Text txtAdd;
    public TMP_Text btnAdd;
    public TMP_Text txtClear;

    // Referencias a los GameObjects de los paneles de instrucciones (para buscar textos hijos).
    public GameObject HowShooter;
    public GameObject HowMemory;
    public GameObject HowRain;

    private void Awake()
    {
        // Implementaci�n del Singleton (asegura que solo exista una instancia).
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste el controlador de idioma entre escenas.
            SceneManager.sceneLoaded += OnSceneLoaded; // Se suscribe al evento de carga de escena.
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        // Desuscripci�n del evento para limpiar recursos.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // L�gica de inicializaci�n al cargar una nueva escena.
        LimpiarReferencias();      // Anula referencias anteriores.
        ReconectarUI();            // Busca y asigna nuevas referencias de UI en la escena.
        ConfigurarDropdown();      // Inicializa el Dropdown.
        UpdateTexts();             // Aplica las traducciones al idioma actual.
    }

    /// <summary>
    /// Anula todas las referencias para forzar al m�todo ReconectarUI() a buscarlas de nuevo
    /// en la nueva escena. Esto previene errores de "missing references".
    /// </summary>
    private void LimpiarReferencias()
    {
        // Anular referencias de paneles
        HowShooter = null;
        HowMemory = null;
        HowRain = null;

        // Anular referencias de textos largos
        howToPlayShooter = null;
        howToPlayMemory = null;
        howToPlayRain = null;

        // Anular referencias de textos cortos
        textHowtoplay = null;
        textLanguaje = null;
        txtAdd = null;
        btnAdd = null;
        txtClear = null;

        // Anular referencia del Dropdown
        languageDropdown = null;
    }

    /// <summary>
    /// Busca y asigna todas las referencias de UI necesarias en la escena actual.
    /// Usa el operador de fusi�n nula (??) para evitar buscar si la referencia ya existe.
    /// </summary>
    private void ReconectarUI()
    {
        // Reconectar Paneles (como GameObjects base)
        HowShooter = HowShooter ?? FindInScene("PanelHowShooter");
        HowMemory = HowMemory ?? FindInScene("PanelHowMemory");
        HowRain = HowRain ?? FindInScene("PanelHowRain");
        // ... (similares para HowMemory y HowRain) ...

        // Reconectar Textos de Instrucciones (busc�ndolos como hijos de los paneles)
        howToPlayShooter = howToPlayShooter ?? FindTextInChildren(HowShooter, "HowToPlayShooterText");
        howToPlayMemory = howToPlayMemory ?? FindTextInChildren(HowMemory, "HowToPlayMemoryText");
        howToPlayRain = howToPlayRain ?? FindTextInChildren(HowRain, "HowToPlayRainText");
        // ... (similares para howToPlayMemory y howToPlayRain) ...

        // Reconectar Textos de Men� (busc�ndolos directamente en la escena y obteniendo el componente TMP_Text)
        textHowtoplay = textHowtoplay ?? FindInScene("TextHowToPlay")?.GetComponent<TMP_Text>();
        textLanguaje = textLanguaje ?? FindInScene("TextLanguage")?.GetComponent<TMP_Text>();
        txtAdd = txtAdd ?? FindInScene("TextAddKanji")?.GetComponent<TMP_Text>();
        btnAdd = btnAdd ?? FindInScene("BtnAddKanji")?.GetComponent<TMP_Text>();
        txtClear = txtClear ?? FindInScene("TextClearKanji")?.GetComponent<TMP_Text>();
        // ... (similares para textLanguaje, txtAdd, etc.) ...

        // Reconectar el Dropdown
        languageDropdown = languageDropdown ?? FindInScene("LanguageDropdown")?.GetComponent<TMP_Dropdown>();
    }

    /// <summary>
    /// Busca un GameObject por nombre en la escena activa, incluyendo objetos inactivos.
    /// </summary>
    private GameObject FindInScene(string name)
    {
        // Itera sobre todos los GameObjects ra�z y sus hijos (activos e inactivos)
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Transform[] allChildren = root.GetComponentsInChildren<Transform>(true); // 'true' incluye inactivos
            foreach (Transform t in allChildren)
            {
                if (t.name == name)
                    return t.gameObject;
            }
        }
        return null;
    }

    /// <summary>
    /// Busca un componente TMP_Text espec�fico por nombre dentro de los hijos de un GameObject padre.
    /// </summary>
    private TMP_Text FindTextInChildren(GameObject parent, string textName)
    {
        if (parent == null) return null;
        TMP_Text[] texts = parent.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text t in texts)
        {
            if (t.name == textName)
                return t;
        }
        return null;
    }

    /// <summary>
    /// Configura el Dropdown: asigna el listener para el cambio de valor
    /// y establece el valor inicial seg�n el idioma actual.
    /// </summary>
    private void ConfigurarDropdown()
    {
        if (languageDropdown != null)
        {
            // Limpia listeners anteriores y a�ade el nuevo m�todo de manejo
            languageDropdown.onValueChanged.RemoveAllListeners();
            languageDropdown.onValueChanged.AddListener(SetLanguageFromDropdown);

            // Establece el Dropdown al idioma actual (convirtiendo el enum a �ndice int)
            languageDropdown.value = (int)currentLanguage;
        }
    }

    /// <summary>
    /// Cambia la variable de idioma y actualiza todos los textos de la UI.
    /// </summary>
    public void SetLanguage(Language lang)
    {
        currentLanguage = lang;
        UpdateTexts();
    }

    /// <summary>
    /// M�todo listener para el Dropdown. Convierte el �ndice int en un valor del enum Language.
    /// </summary>
    public void SetLanguageFromDropdown(int index)
    {
        SetLanguage((Language)index);
    }

    /// <summary>
    /// Aplica las traducciones a todos los textos de la UI seg�n el idioma seleccionado.
    /// Utiliza el operador condicional ternario (isSpanish ? texto_es : texto_en) para la traducci�n.
    /// </summary>
    public void UpdateTexts()
    {
        bool isSpanish = currentLanguage == Language.Spanish;

        // Traducciones de las instrucciones de juego (textos largos)
        if (howToPlayShooter != null) howToPlayShooter.text = isSpanish ?
                // Texto en espa�ol...
                "Dispara al kanji correcto, al comenzar, aparecer� un texto que indica el kanji " +
                "que debes encontrar. Si te equivocas, aparecer� un mensaje de Incorrecto " +
                "\n\nNota: Usa el bot�n en la esquina superior derecha para reordenarlos si " +
                "se superponen." :
                // Texto en ingl�s...
                "Shoot the correct kanji. When you start, a text prompt will appear indicating " +
                "the kanji you need to find. If you miss, an Incorrect message will pop up." +
                "\n\nNote: Use the button in the top right corner to rearrange them if they overlap.";
        if (howToPlayMemory != null) howToPlayMemory.text = isSpanish ?
               "Voltea las tarjetas de dos en dos para encontrar los 5 pares iguales. " +
               "El objetivo es dejarlas todas boca arriba. Si las dos tarjetas volteadas " +
               "son un par, se quedan visibles; si no, se voltean de nuevo. " +
               "\n\nCuando encuentres todos los pares, el juego terminar� y autom�ticamente " +
               "comenzar� una nueva partida." :
               "Flip the cards over two at a time to find all 5 matching pairs. " +
               "The goal is to leave all the cards face-up. If the two cards you " +
               "flip form a pair, they stay visible; if not, they flip back down. " +
               "\n\nOnce you find every pair, the game will end and automatically " +
               "start a new round.";
        if (howToPlayRain != null) howToPlayRain.text = isSpanish ?
                "En este nivel, los kanjis caer�n como lluvia. Debes arrastrarlos r�pidamente " +
                "al tanuki correspondiente que est� esperando para atraparlos. " +
                "El nivel se ir� haciendo m�s dif�cil a medida que la velocidad de ca�da aumente. " +
                "\n\nTen cuidado: si dejas caer un solo kanji al suelo, " +
                "el nivel se reinicia inmediatamente." :
                "In this level, kanji will fall like rain. You must quickly drag them " +
                "to the correct tanuki who is waiting to catch them. The level will become " +
                "more challenging as the falling speed increases. " +
                "\n\nBe careful: if you let a single kanji hit the floor, the level " +
                "immediately restarts.";
        // ... (similares para howToPlayMemory y howToPlayRain) ...

        // Traducciones de textos de men� (textos cortos)
        if (textHowtoplay != null) textHowtoplay.text = isSpanish ? "C�mo jugar" : "How to play";
        if (textLanguaje != null) textLanguaje.text = isSpanish ? "Idioma" : "Language";
        if (txtAdd != null) txtAdd.text = isSpanish ? "Agregar Kanji" : "Add Kanji";
        if (btnAdd != null) btnAdd.text = isSpanish ? "Agregar" : "Add";
        if (txtClear != null) txtClear.text = isSpanish ? "Vaciar" : "Clear";
    }
}