using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveLogic : MonoBehaviour
{
    public Slider slider;
    public TMP_Text text;

    private GameState _gameState;

    public float fillSpeed = 1f;
    private float _fill = 0f;

    public int objectiveTarget = 0;
    public int objectiveProgress = 0;

    public AudioClip winClip;

    void Awake()
    {
        _gameState = FindObjectOfType<GameState>();
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateWinTarget();
    }

    public void UpdateWinTarget()
    {
        var weldPoints = FindObjectsOfType<WeldPoint>();
        objectiveTarget = weldPoints.Length / 2;
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "WELDED: " + objectiveProgress + "/" + objectiveTarget;

        float progressPercentage = (float)objectiveProgress / (float)objectiveTarget;
        _fill = Mathf.MoveTowards(_fill, progressPercentage, fillSpeed * Time.deltaTime);
        slider.value = _fill;
    }

    void LateUpdate()
    {
        CheckWin();
    }

    public void CheckWin()
    {
        if (objectiveProgress == objectiveTarget && _gameState.currentPlayerState == GameState.PlayerState.Playing)
        {
            _gameState.TriggerWin();
            // _gameState.musicManager.CreateAudioClip(winClip, _gameState.transform.position, respectBinning: false);
        }
    }
}