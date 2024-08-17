using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TetrominoGroupBase))]
public class TetrominoColor : MonoBehaviour
{

    private GameState _gameState;

    [SerializeField]
    private List<MeshRenderer> meshRenderers;
    private TetrominoGroupBase _tetromino;

    void Awake()
    {
        _tetromino = GetComponent<TetrominoGroupBase>();
        _gameState = FindObjectOfType<GameState>();
        meshRenderers = new List<MeshRenderer>();

        List<Tetromino> myTetrominos = new List<Tetromino>();
        myTetrominos.AddRange(GetComponentsInChildren<Tetromino>());

        foreach (var tet in myTetrominos)
        {
            meshRenderers.Add(tet.outlineMesh);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (_tetromino.Type == Tetromino.TetrominoType.Scaffold)
        {
            NextPaint();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void NextPaint()
    {
        var mat = NextColor();
        foreach (var renderer in meshRenderers)
        {
            renderer.material = mat;
        }
    }


    private Material NextColor()
    {
        return _gameState.tetrominoScafoldingMaterials[UnityEngine.Random.Range(0, _gameState.tetrominoScafoldingMaterials.Count - 1)];
    }

}
