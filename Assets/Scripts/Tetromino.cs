using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Tetromino : MonoBehaviour
{

    public enum TetrominoType
    {
        House, Scaffold
    }

    [SerializeField]
    private TetrominoType tetrominoType;
    public TetrominoType Type => tetrominoType;

    public MeshRenderer outlineMesh;
    public MeshCollider outlineMeshCollider;
    private GameState _gameState;


    void Awake()
    {
        _gameState = FindObjectOfType<GameState>();
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        outlineMeshCollider.enabled = _gameState.scaffoldingOutlineSolid;
    }

    
    public void Rotate90() {
        
    }
}
