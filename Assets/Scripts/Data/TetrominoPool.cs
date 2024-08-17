using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Tetromino Pool", menuName = "GMTK24/ScriptableObjects/New Tetromino Pool", order = 1)]
public class SpawnManagerScriptableObject : ScriptableObject
{


    public List<TetrominoPoolEntry> tetrominos;

    [Serializable]
    public class TetrominoPoolEntry
    {

        [SerializeField] public TetrominoGroupBase tetromino;
        [SerializeField] public int weight = 1;


    }

}


