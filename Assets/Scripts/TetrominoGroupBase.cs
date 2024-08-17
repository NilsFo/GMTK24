using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class TetrominoGroupBase : MonoBehaviour
{

    enum State
    {
        Spawned,
        Moving,
        Rotating,
        Placed,
        Graped,
        Welded,
    }
    
    private Grid3D _grid;
    
    [SerializeField] private State _state = State.Spawned;
    
    [SerializeField] private GameObject anchorPoints;
    [SerializeField] private GameObject bottemPoints;
    
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 3f;
    
    [SerializeField] private Vector3Int currentIndex;

    [SerializeField] private Vector3 targetPos;
    [SerializeField] private Quaternion targetRotation;
    [SerializeField] private Vector3 anchorOffset;
    
    // Start is called before the first frame update
    void Start()
    {
        _grid = FindObjectOfType<Grid3D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            RotateRight();
        }
        
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
        else if (_state == State.Rotating)
        {
            float dist = Quaternion.Angle(transform.rotation, targetRotation);
            if (dist < 0.1f)
            {
                _state = State.Graped;
                transform.rotation = targetRotation;
            }
            else
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    public void MoveToPos(Vector3 pos)
    {
        if (_state == State.Graped)
        {
            targetPos = pos;
            _state = State.Moving;
        }
    }

    public void RotateRight()
    {
        if (_state == State.Graped)
        {
            var euler = transform.rotation.eulerAngles;
            transform.rotation =  Quaternion.Euler(euler.x, euler.y+90, euler.z);
            if (anchorOffset != Vector3.zero)
            {
                transform.position = new Vector3(transform.position.x + anchorOffset.y, transform.position.y, transform.position.z + anchorOffset.z);
            }
            _state = State.Graped;
        }
    }
    
    public void RotateLeft()
    {
        if (_state == State.Graped)
        {
            var euler = transform.rotation.eulerAngles;
            targetRotation =  Quaternion.Euler(euler.x, euler.y-90, euler.z);
            _state = State.Rotating;
        }
    }
}
