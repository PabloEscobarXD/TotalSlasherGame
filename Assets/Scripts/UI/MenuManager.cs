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
        AudioManager.GetOrCreate().PlayMusic();
        mainMenuCanvas.SetActive(true);
        optionsCanvas.SetActive(false);
        howToPlayCanvas.SetActive(false);   // ← nuevo
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(startGameButton);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void startGame()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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

        var prevBtn = prevButton?.GetComponent<UnityEngine.UI.Button>();
        var nextBtn = nextButton?.GetComponent<UnityEngine.UI.Button>();

        if (backButton != null)
        {
            var nav = backButton.navigation;
            nav.mode = UnityEngine.UI.Navigation.Mode.Explicit;
            nav.selectOnDown = hasNext ? nextBtn : (hasPrev ? prevBtn : null);
            backButton.navigation = nav;
        }

        if (nextBtn != null)
        {
            var nav = nextBtn.navigation;
            nav.mode = UnityEngine.UI.Navigation.Mode.Explicit;
            nav.selectOnLeft = hasPrev ? prevBtn : null;
            nav.selectOnUp = backButton;
            nextBtn.navigation = nav;
        }

        if (prevBtn != null)
        {
            var nav = prevBtn.navigation;
            nav.mode = UnityEngine.UI.Navigation.Mode.Explicit;
            nav.selectOnRight = hasNext ? nextBtn : null;
            nav.selectOnUp = backButton;
            prevBtn.navigation = nav;
        }

        StartCoroutine(ReselectAfterFrame(
            hasNext ? nextButton : (hasPrev ? prevButton : backButton?.gameObject)
        ));
    }

    private System.Collections.IEnumerator ReselectAfterFrame(GameObject target)
    {
        yield return null;
        if (target != null && target.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(target);
        }
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