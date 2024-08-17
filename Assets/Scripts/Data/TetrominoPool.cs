using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Tetromino Pool", menuName = "GMTK24/ScriptableObjects/New Tetromino Pool", order = 1)]
public class TetrominoPool : ScriptableObject
{

    public List<TetrominoPoolEntry> tetrominos;

    public TetrominoGroupBase Next()
    {
        List<TetrominoGroupBase> tetrominoCandidates = new List<TetrominoGroupBase>();

        foreach (TetrominoPoolEntry entry in tetrominos)
        {
            tetrominoCandidates.Add(entry.tetromino);
        }

        return tetrominoCandidates[UnityEngine.Random.Range(0, tetrominoCandidates.Count - 1)];
    }

    [Serializable]
    public class TetrominoPoolEntry
    {

        [SerializeField] public TetrominoGroupBase tetromino;
        [SerializeField] public int weight = 1;


    }

}


