using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid3D : MonoBehaviour
{

    private GameObject[,,] _grid;

    [SerializeField] private int sizeX = 10;
    [SerializeField] private int sizeY = 10;
    [SerializeField] private int sizeZ = 10;

    [SerializeField] private float blockScaleX = 3f;
    [SerializeField] private float blockScaleY = 3f;
    [SerializeField] private float blockScaleZ = 3f;


    private void Awake()
    {
        _grid = new GameObject[sizeX, sizeY, sizeZ];
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int[] WorldToLocal(Vector3 pos)
    {
        return new[] { 0, 0, 0 };
    }
    
    public Vector3 LocalToWorld(int[] index)
    {
        return Vector3.zero;
    }
}
