using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{


    public enum PlayerState
    {
        Unknown,
        Playing,
        Paused,
        Win
    }

    [Header("Scafolding Colors")]
    public List<Material> tetrominoScafoldingMaterials;

    [Header("World hookup")]
    public GameObject gameplayUI;
    public GameObject winUI;
    public GameObject pauseUI;
    public CanvasGroup pauseGroup;

    [Header("Level hookup")]
    public GameObject levelTutorial;
    public GameObject level1;
    public GameObject level2;


    [Header("Gameplay Parameters")]
    public bool scaffoldingOutlineSolid;

    [Header("Gameplay Variables")]
    public PlayerState currentPlayerState;
    private PlayerState _lastKnownPlayerState;
    private LevelShare _levelShare;
    private MusicManager _musicManager;
    public MusicManager musicManager => _musicManager;

    private float _winAlpha = 0f;
    public float fadeOutDelay = 3f;
    public float fadeOutSpeed = 0.5f;
    private bool _fadeOutEnabled;


    // Start is called before the first frame update
    void Start()
    {
        _lastKnownPlayerState = currentPlayerState;
        _levelShare = FindAnyObjectByType<LevelShare>();
        _musicManager = FindAnyObjectByType<MusicManager>();
        OnStateChange();

        pauseGroup.alpha = 0;
        _winAlpha = 0;

        levelTutorial.SetActive(false);
        level1.SetActive(false);
        level2.SetActive(false);
        switch (LevelShare.levelChoice)
        {
            case 0:
                levelTutorial.SetActive(true);
                break;
            case 1:
                level1.SetActive(true);
                break;
            case 2:
                level2.SetActive(true);
                break;
            default:
                BackToMenu();
                break;
        }


        _musicManager.Play(0, true);
    }

    // Update is called once per frame
    void Update()
    {
        if (_lastKnownPlayerState != currentPlayerState)
        {
            OnStateChange();
            _lastKnownPlayerState = currentPlayerState;
        }


        if (currentPlayerState == PlayerState.Unknown)
        {
            Time.timeScale = 0f;
            Debug.LogError("YOU ARE IN ERROR STATE!");
            return;
        }

        Time.timeScale = 1f;
        if (currentPlayerState == PlayerState.Paused)
        {
            Time.timeScale = 0f;
        }


        // TODD use input system
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (!IsWon())
            {
                if (currentPlayerState == PlayerState.Paused)
                {
                    currentPlayerState = PlayerState.Playing;
                }
                else
                {
                    currentPlayerState = PlayerState.Paused;
                }
            }
        }

        // TODO use input keys
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentPlayerState == PlayerState.Win || currentPlayerState == PlayerState.Paused)
            {
                BackToMenu();
            }
        }

        if (_fadeOutEnabled)
        {
            _winAlpha = Mathf.MoveTowards(_winAlpha, 1, fadeOutSpeed * Time.deltaTime);
            pauseGroup.alpha = _winAlpha;
        }
    }

    private void OnStateChange()
    {
        Debug.Log("New player state: " + currentPlayerState);

        if (currentPlayerState == PlayerState.Paused)
        {
            // Disable mouse
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;  
        }
        else
        {
            // Show mouse
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }


        if (currentPlayerState == PlayerState.Playing)
        {
            gameplayUI.SetActive(true);
            winUI.SetActive(false);
            pauseUI.SetActive(false);
        }


        if (currentPlayerState == PlayerState.Paused)
        {
            gameplayUI.SetActive(false);
            winUI.SetActive(false);
            pauseUI.SetActive(true);
        }


        if (currentPlayerState == PlayerState.Win)
        {
            gameplayUI.SetActive(false);
            winUI.SetActive(true);
            pauseUI.SetActive(false);
        }
    }

    [ContextMenu("Win")]
    public void TriggerWin()
    {
        currentPlayerState = PlayerState.Win;
        OnWin();
    }

    public bool IsWon()
    {
        return currentPlayerState == PlayerState.Win;
    }

    private void OnWin()
    {
        Debug.Log("A winner is you.");
        Invoke(nameof(WinFadeOut), fadeOutDelay);
    }

    private void WinFadeOut()
    {
        _fadeOutEnabled = true;
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Scenes/MainMenu");
    }

}
