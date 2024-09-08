using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private LevelShare levelShare;
    private GamepadInputDetector _gamepadInputDetector;
    public Slider slider;
    public MusicManager musicManager;
    private GameState _gameState;

    // Start is called before the first frame update
    void Start()
    {
        _gamepadInputDetector = FindObjectOfType<GamepadInputDetector>();

        Time.timeScale = 1f;
        levelShare = FindObjectOfType<LevelShare>();
        musicManager.Play(1, true);

        slider.value = MusicManager.userDesiredMasterVolume;

        if (!_gamepadInputDetector.isGamePad)
        {
            // show cursor here, in case gameplay scene has hidden it
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Cursor.visible = true;
        // Cursor.lockState = CursorLockMode.None;
        MusicManager.userDesiredMasterVolume = slider.value;
    }

    // public void Level1()
    // {
    //     LevelShare.levelChoice = 1;
    //     SceneManager.LoadScene("Scenes/GameplayScene");
    // }
    // 
    // public void Level2()
    // {
    //     LevelShare.levelChoice = 2;
    //     SceneManager.LoadScene("Scenes/GameplayScene");
    // 
    // }
    // 
    // public void LevelTutorial()
    // {
    //     LevelShare.levelChoice = 0;
    //     SceneManager.LoadScene("Scenes/GameplayScene");
    // }
}