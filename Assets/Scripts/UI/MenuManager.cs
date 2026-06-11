using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    [Header("Canvas")]
    public GameObject mainMenuCanvas;
    public GameObject optionsCanvas;
    public GameObject howToPlayCanvas;      // ← nuevo

    [Header("Páginas Cómo Jugar")]
    public GameObject[] howToPlayPages;     // arrastrar las 3 páginas en orden
    public GameObject prevButton;           // botón flecha izquierda
    public GameObject nextButton;           // botón flecha derecha
    public UnityEngine.UI.Button backButton;
    private int currentPage = 0;

    [Header("Primeros selectables")]
    public GameObject firstOptionSelectable;
    public GameObject optionsButtonInMainMenu;
    public GameObject howToPlayButtonInMainMenu; // ← nuevo
    public GameObject startGameButton;
    public GameObject firstHowToPlaySelectable;  // ← botón flecha derecha o el primero

    void Start()
    {
        var input = FindAnyObjectByType<PlayerInput>();
        input?.SwitchCurrentActionMap("UI");
        AudioManager.GetOrCreate().PlayMusic("menu");
        mainMenuCanvas.SetActive(true);
        optionsCanvas.SetActive(false);
        howToPlayCanvas.SetActive(false);   // ← nuevo
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(startGameButton);
    }

    public void startGame()
    {
        Cursor.visible = false;
        AudioManager.GetOrCreate().PlaySFX("button_click");
        SceneManager.LoadScene("Nivel1");
    }

    public void backToMenu()
    {
        Cursor.visible = false;
        AudioManager.GetOrCreate().PlaySFX("button_click");
        SceneManager.LoadScene("Menu");
    }

    public void toggleOptionsCanvas()
    {
        AudioManager.GetOrCreate().PlaySFX("button_click");
        if (!optionsCanvas.activeSelf)
        {
            mainMenuCanvas.SetActive(false);
            optionsCanvas.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstOptionSelectable);
        }
        else
        {
            optionsCanvas.SetActive(false);
            mainMenuCanvas.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(optionsButtonInMainMenu);
        }
    }

    // ── Cómo Jugar ──────────────────────────────────────
    public void toggleHowToPlayCanvas()
    {
        AudioManager.GetOrCreate().PlaySFX("button_click");
        if (!howToPlayCanvas.activeSelf)
        {
            mainMenuCanvas.SetActive(false);
            howToPlayCanvas.SetActive(true);
            currentPage = 0;
            RefreshPages();
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstHowToPlaySelectable);
        }
        else
        {
            howToPlayCanvas.SetActive(false);
            mainMenuCanvas.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(howToPlayButtonInMainMenu);
        }
    }

    public void NextPage()
    {
        AudioManager.GetOrCreate().PlaySFX("button_click");
        if (currentPage < howToPlayPages.Length - 1)
        {
            currentPage++;
            RefreshPages();
        }
    }

    public void PrevPage()
    {
        AudioManager.GetOrCreate().PlaySFX("button_click");
        if (currentPage > 0)
        {
            currentPage--;
            RefreshPages();
        }
    }
    private void RefreshPages()
    {
        for (int i = 0; i < howToPlayPages.Length; i++)
            howToPlayPages[i].SetActive(i == currentPage);

        bool hasPrev = currentPage > 0;
        bool hasNext = currentPage < howToPlayPages.Length - 1;

        if (prevButton != null) prevButton.SetActive(hasPrev);
        if (nextButton != null) nextButton.SetActive(hasNext);

        // Actualizar navegación del botón Volver según qué flechas están activas
        if (backButton != null)
        {
            var nav = backButton.navigation;
            nav.mode = UnityEngine.UI.Navigation.Mode.Explicit;

            if (hasNext && nextButton != null)
                nav.selectOnDown = nextButton.GetComponent<UnityEngine.UI.Button>();
            else if (hasPrev && prevButton != null)
                nav.selectOnDown = prevButton.GetComponent<UnityEngine.UI.Button>();
            else
                nav.selectOnDown = null;

            backButton.navigation = nav;
        }

        // Seleccionar el botón disponible (prioridad: derecho → izquierdo)
        if (hasNext && nextButton != null)
            EventSystem.current.SetSelectedGameObject(nextButton);
        else if (hasPrev && prevButton != null)
            EventSystem.current.SetSelectedGameObject(prevButton);
    }
    // ────────────────────────────────────────────────────

    public void closeGame()
    {
        AudioManager.GetOrCreate().PlaySFX("button_click");
        Application.Quit();
    }

    private void OnEnable()
    {
        var input = FindAnyObjectByType<PlayerInput>();
        if (input != null)
        {
            input.actions["UI/Cancel"].performed += OnCancel;
        }
    }

    private void OnDisable()
    {
        var input = FindAnyObjectByType<PlayerInput>();
        if (input != null)
        {
            input.actions["UI/Cancel"].performed -= OnCancel;
        }
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        // Si estoy en opciones → volver al menú principal
        if (optionsCanvas.activeSelf)
        {
            toggleOptionsCanvas();
            return;
        }

        // Si estoy en cómo jugar → volver al menú principal
        if (howToPlayCanvas.activeSelf)
        {
            toggleHowToPlayCanvas();
            return;
        }
    }
}