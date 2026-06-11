using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseCanvas;
    public GameObject firstPauseSelectable;
    public GameObject optionsButtonInMainMenu;
    public GameObject optionsCanvas;
    public GameObject firstOptionSelectable;

    [Header("Controles")]
    public GameObject controlsCanvas;
    public GameObject[] controlsPages;
    public GameObject controlsPrevButton;
    public GameObject controlsNextButton;
    public UnityEngine.UI.Button controlsBackButton;
    public GameObject firstControlsSelectable;
    private int currentControlsPage = 0;

    private bool isPaused = false;
    private PlayerInput playerInput;

    private void Start()
    {
        playerInput = FindAnyObjectByType<PlayerInput>();
        playerInput.actions["UI/Cancel"].performed += OnCancel;
        playerInput.SwitchCurrentActionMap("Player");
    }

    public void TogglePause()
    {
        if (optionsCanvas.activeSelf)
        {
            optionsCanvas.SetActive(false);
            pauseCanvas.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
            AudioManager.GetOrCreate().StopMusicOverlay();
            AudioManager.GetOrCreate().ResumeMusic();
            playerInput.SwitchCurrentActionMap("Player");
            return;
        }

        if (controlsCanvas != null && controlsCanvas.activeSelf)
        {
            controlsCanvas.SetActive(false);
            pauseCanvas.SetActive(false);
            Time.timeScale = 1f;
            isPaused = false;
            AudioManager.GetOrCreate().StopMusicOverlay();
            AudioManager.GetOrCreate().ResumeMusic();
            playerInput.SwitchCurrentActionMap("Player");
            return;
        }

        isPaused = !isPaused;
        pauseCanvas.SetActive(isPaused);

        if (isPaused)
        {
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            AudioManager.GetOrCreate().PauseMusic();
            AudioManager.GetOrCreate().PlayMusicOverlay("pause");
            playerInput.SwitchCurrentActionMap("UI");
            EventSystem.current.SetSelectedGameObject(firstPauseSelectable);
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            AudioManager.GetOrCreate().StopMusicOverlay();
            AudioManager.GetOrCreate().ResumeMusic();
            playerInput.SwitchCurrentActionMap("Player");
        }
    }

    public void toggleOptionsCanvas()
    {
        if (!optionsCanvas.activeSelf)
        {
            pauseCanvas.SetActive(false);
            optionsCanvas.SetActive(true);
            playerInput.SwitchCurrentActionMap("UI");
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstOptionSelectable);
        }
        else
        {
            optionsCanvas.SetActive(false);
            pauseCanvas.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(optionsButtonInMainMenu);
        }
    }

    // ── Controles ────────────────────────────────────────
    public void toggleControlsCanvas()
    {
        AudioManager.GetOrCreate().PlaySFX("button_click");
        if (!controlsCanvas.activeSelf)
        {
            pauseCanvas.SetActive(false);
            controlsCanvas.SetActive(true);
            currentControlsPage = 0;
            RefreshControlsPages();
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstControlsSelectable);
        }
        else
        {
            controlsCanvas.SetActive(false);
            pauseCanvas.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstPauseSelectable);
        }
    }

    public void ControlsNextPage()
    {
        AudioManager.GetOrCreate().PlaySFX("button_click");
        if (currentControlsPage < controlsPages.Length - 1)
        {
            currentControlsPage++;
            RefreshControlsPages();
        }
    }

    public void ControlsPrevPage()
    {
        AudioManager.GetOrCreate().PlaySFX("button_click");
        if (currentControlsPage > 0)
        {
            currentControlsPage--;
            RefreshControlsPages();
        }
    }

    private void RefreshControlsPages()
    {
        for (int i = 0; i < controlsPages.Length; i++)
            controlsPages[i].SetActive(i == currentControlsPage);

        bool hasPrev = currentControlsPage > 0;
        bool hasNext = currentControlsPage < controlsPages.Length - 1;

        if (controlsPrevButton != null) controlsPrevButton.SetActive(hasPrev);
        if (controlsNextButton != null) controlsNextButton.SetActive(hasNext);

        if (controlsBackButton != null)
        {
            var nav = controlsBackButton.navigation;
            nav.mode = UnityEngine.UI.Navigation.Mode.Explicit;
            if (hasNext && controlsNextButton != null)
                nav.selectOnDown = controlsNextButton.GetComponent<UnityEngine.UI.Button>();
            else if (hasPrev && controlsPrevButton != null)
                nav.selectOnDown = controlsPrevButton.GetComponent<UnityEngine.UI.Button>();
            else
                nav.selectOnDown = null;
            controlsBackButton.navigation = nav;
        }

        if (hasNext && controlsNextButton != null)
            EventSystem.current.SetSelectedGameObject(controlsNextButton);
        else if (hasPrev && controlsPrevButton != null)
            EventSystem.current.SetSelectedGameObject(controlsPrevButton);
    }
    // ────────────────────────────────────────────────────

    public void backToMenu()
    {
        AudioManager.GetOrCreate().StopMusicOverlay();
        AudioManager.GetOrCreate().StopMusic();
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1f;
        playerInput.SwitchCurrentActionMap("Player");
    }

    public void restartGame()
    {
        Time.timeScale = 1f;
        AudioManager.GetOrCreate().StopMusicOverlay();
        AudioManager.GetOrCreate().StopMusic();
        SceneManager.LoadScene("Nivel1");
    }

    private void OnDestroy()
    {
        if (playerInput != null)
            playerInput.actions["UI/Cancel"].performed -= OnCancel;
    }

    private void OnEnable()
    {
        playerInput = FindAnyObjectByType<PlayerInput>();
        if (playerInput != null)
            playerInput.actions["UI/Cancel"].performed += OnCancel;
    }

    private void OnDisable()
    {
        if (playerInput != null)
            playerInput.actions["UI/Cancel"].performed -= OnCancel;
    }

    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        TogglePause();
    }

    public void OnCancel(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (optionsCanvas.activeSelf)
        {
            optionsCanvas.SetActive(false);
            pauseCanvas.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstPauseSelectable);
            return;
        }

        if (controlsCanvas != null && controlsCanvas.activeSelf)
        {
            toggleControlsCanvas(); // ← cierra controles y vuelve a pausa
            return;
        }

        if (pauseCanvas.activeSelf)
            TogglePause();
    }
}