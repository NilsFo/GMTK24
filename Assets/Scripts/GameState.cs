using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour {


    public enum PlayerState {
        Unknown,
        Menu,
        Playing,
        Paused,
        Win
    }

    [Header("World hookup")]
    public GameObject gameplayUI;
    public GameObject menuUI;
    public GameObject winUI;
    public GameObject pauseUI;
    public CanvasGroup pauseGroup;

    [Header("Menu hookup")]
    public GameObject cameraHolders;
    public GameObject player;

    [Header("Level hookup")]
    public GameObject levelMainMenu;
    public GameObject levelTutorial;
    public GameObject level1;
    public GameObject level2;
    public GameObject level3;
    public GameObject level4;


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
    private Grid3D _grid;


    // Start is called before the first frame update
    void Start() {
        _grid = FindObjectOfType<Grid3D>();
        currentPlayerState = PlayerState.Menu;
        _lastKnownPlayerState = currentPlayerState;
        _levelShare = FindAnyObjectByType<LevelShare>();
        _musicManager = FindAnyObjectByType<MusicManager>();
        OnStateChange();
        Time.timeScale = 1f;

        pauseGroup.alpha = 0;
        _winAlpha = 0;

        levelTutorial.SetActive(false);
        level1.SetActive(false);
        level2.SetActive(false);
        level3.SetActive(false);
        level4.SetActive(false);
        cameraHolders.SetActive(true);
        player.SetActive(false);
        levelMainMenu.SetActive(true);

        _musicManager.Play(1, true);
    }

    public void StartLevel(int levelChoice) {
        cameraHolders.SetActive(false);
        levelMainMenu.SetActive(false);
        player.SetActive(true);
        _musicManager.Play(0, true);
        _grid.CleanUpGrid();

        switch(levelChoice) {
            case 0:
                levelTutorial.SetActive(true);
                break;
            case 1:
                level1.SetActive(true);
                break;
            case 2:
                level2.SetActive(true);
                break;
            case 3:
                level3.SetActive(true);
                break;
            case 4:
                level4.SetActive(true);
                break;
            default:
                BackToMenu();
                break;
        }

        ObjectiveLogic objective = FindObjectOfType<ObjectiveLogic>();
        objective.UpdateWinTarget();
        currentPlayerState = PlayerState.Playing;
    }

    // Update is called once per frame
    void Update() {
        if (_lastKnownPlayerState != currentPlayerState) {
            OnStateChange();
            _lastKnownPlayerState = currentPlayerState;
        }

        if (currentPlayerState == PlayerState.Unknown) {
            Time.timeScale = 0f;
            Debug.LogError("YOU ARE IN ERROR STATE!");
            return;
        }

        Time.timeScale = 1f;
        if (currentPlayerState == PlayerState.Paused) {
            Time.timeScale = 0f;
        }

        // TODD use input system
        if (Keyboard.current.backspaceKey.wasPressedThisFrame) {
            if (!IsWon()) {
                if (currentPlayerState == PlayerState.Paused) {
                    currentPlayerState = PlayerState.Playing;
                } else {
                    currentPlayerState = PlayerState.Paused;
                }
            }
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame) {
            if (currentPlayerState == PlayerState.Win || currentPlayerState == PlayerState.Paused) {
                BackToMenu();
            }
        }

        if (_fadeOutEnabled) {
            _winAlpha = Mathf.MoveTowards(_winAlpha, 1, fadeOutSpeed * Time.deltaTime);
            pauseGroup.alpha = _winAlpha;
        }
    }

    private void OnStateChange() {
        Debug.Log("New player state: " + currentPlayerState);

        if (currentPlayerState == PlayerState.Paused || currentPlayerState == PlayerState.Menu) {
            // Disable mouse
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        } else {
            // Show mouse
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (currentPlayerState == PlayerState.Playing) {
            gameplayUI.SetActive(true);
            winUI.SetActive(false);
            pauseUI.SetActive(false);
            menuUI.SetActive(false);
        }

        if (currentPlayerState == PlayerState.Paused) {
            gameplayUI.SetActive(false);
            winUI.SetActive(false);
            pauseUI.SetActive(true);
            menuUI.SetActive(false);
        }

        if (currentPlayerState == PlayerState.Menu) {
            gameplayUI.SetActive(false);
            winUI.SetActive(false);
            pauseUI.SetActive(false);
            menuUI.SetActive(true);
        }

        if (currentPlayerState == PlayerState.Win) {
            gameplayUI.SetActive(false);
            winUI.SetActive(true);
            pauseUI.SetActive(false);
            menuUI.SetActive(false);
        }
    }

    /*void OnDisable(){
        Debug.LogError("IS DISABLED");
    }

    void OnDestroy(){
        Debug.LogError("IS DESTROYED");
    }*/

    [ContextMenu("Win")]
    public void TriggerWin() {
        currentPlayerState = PlayerState.Win;
        OnWin();
    }

    public bool IsWon() {
        return currentPlayerState == PlayerState.Win;
    }

    private void OnWin() {
        Debug.Log("A winner is you.");
        Invoke(nameof(WinFadeOut), fadeOutDelay);
    }

    private void WinFadeOut() {
        _fadeOutEnabled = true;
    }

    public void BackToMenu() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Scenes/GameplayScene");
    }

}
