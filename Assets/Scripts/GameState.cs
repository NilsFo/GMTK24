using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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



    [Header("Gameplay Parameters")]
    public bool scaffoldingOutlineSolid;

    [Header("Gameplay Variables")]
    public PlayerState currentPlayerState;
    private PlayerState _lastKnownPlayerState;


    // Start is called before the first frame update
    void Start()
    {
        _lastKnownPlayerState = currentPlayerState;
        OnStateChange();
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
    }

    private void OnStateChange()
    {
        Debug.Log("New player state: " + currentPlayerState);

        if (currentPlayerState == PlayerState.Playing)
        {
            // Disable mouse
        }
        else
        {
            // Show mouse
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
    }

    public bool IsWon()
    {
        return currentPlayerState == PlayerState.Win;
    }

    private void OnWin()
    {
        Debug.Log("A winner is you.");
    }

}
