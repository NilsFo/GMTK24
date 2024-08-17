using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tetromino : MonoBehaviour
{

    public enum TetrominoType
    {
        House, Scaffold
    }

    [SerializeField]
    private TetrominoType tetrominoType;
    public TetrominoType Type => tetrominoType;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
