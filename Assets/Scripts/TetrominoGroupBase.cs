using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrominoGroupBase : MonoBehaviour
{

    enum State
    {
        Spawned,
        Moving,
        Placed,
        Welded,
    }
    
    private Grid3D _grid;
    
    [SerializeField] private State _state = State.Spawned;
    [SerializeField] private GameObject anchorPoints;
    [SerializeField] private GameObject bottemPoints;
    [SerializeField] private float moveSpeed = 10f;
    
    [SerializeField] private Vector3Int currentIndex;

    [SerializeField] private Vector3 targetPos;

    // Start is called before the first frame update
    void Start()
    {
        _grid = FindObjectOfType<Grid3D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_state == State.Moving)
        {
            float dist = Vector3.Distance(transform.position, targetPos);
            if (dist < 0.1f)
            {
                _state = State.Placed;
                transform.position = targetPos;
                currentIndex = _grid.WorldToLocal(transform.position);
                _grid.PlaceShape(this, new[] { currentIndex });
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            }
        }
    }

    public void MoveToPos(Vector3 pos)
    {
        targetPos = pos;
        _state = State.Moving;
    }
    
}
