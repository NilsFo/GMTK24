using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TetrominoGroupBase))]
public class TetrominoColor : MonoBehaviour
{


    [SerializeField]
    private List<MeshRenderer> meshRenderers;
    private TetrominoGroupBase _tetromino;


    [Header("Scafolding Colors")]
    public List<Material> tetrominoScafoldingMaterials;

    void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        _tetromino = GetComponent<TetrominoGroupBase>();
        meshRenderers = new List<MeshRenderer>();

        List<Tetromino> myTetrominos = new List<Tetromino>();
        myTetrominos.AddRange(GetComponentsInChildren<Tetromino>());

        foreach (var tet in myTetrominos)
        {
            meshRenderers.Add(tet.outlineMesh);
        }

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
        return tetrominoScafoldingMaterials[UnityEngine.Random.Range(0, tetrominoScafoldingMaterials.Count - 1)];
    }

}
