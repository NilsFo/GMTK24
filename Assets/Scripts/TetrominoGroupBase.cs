using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.UI;
using UnityEngine;

public class TetrominoGroupBase : MonoBehaviour
{

    public enum State
    {
        Spawned,
        Moving,
        Drop,
        Rotating,
        Placed,
        Grabbed,
        Welded,
        Faulty
    }


    public enum TetrominoGroupType
    {
        Unknown,
        I,
        L,
        O,
        T,
        S,
        Special
    }

    private Grid3D _grid;

    public bool isStatic = false;
    
    [Header("General Parameters")][SerializeField]
    private Tetromino.TetrominoType tetrominoType;
    [SerializeField]
    private TetrominoGroupType tetrominoGroupType=TetrominoGroupType.Unknown;
    public Tetromino.TetrominoType Type => tetrominoType;
    public TetrominoGroupType GroupType => tetrominoGroupType;


    [Header("Grid Behaviour")]
    [SerializeField] public State _state = State.Spawned;

    [SerializeField] private GameObject[] shapeBlocks;

    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 3f;

    [SerializeField] private Vector3Int currentIndex;
    [SerializeField] private Vector3Int lastValidIndex;

    [SerializeField] private Vector3 targetPos;

    // Start is called before the first frame update
    void Start()
    {
        _grid = FindObjectOfType<Grid3D>();
        shapeBlocks = transform.GetComponentsInChildren<Tetromino>().Select(t => t.gameObject).ToArray();
        
        currentIndex = _grid.WorldToLocal(transform.position);
        lastValidIndex = currentIndex;

        if (tetrominoType==Tetromino.TetrominoType.House)
        {
            tetrominoGroupType=TetrominoGroupType.Unknown;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            MoveToPos(targetPos);
        }

        if (_state == State.Moving)
        {
            float dist = Vector3.Distance(transform.position, targetPos);

            if (dist < 0.1f)
            {
                _state = State.Placed;
                currentIndex = _grid.WorldToLocal(targetPos);
                transform.position = _grid.LocalToWorld(currentIndex);
                Vector3Int[] currentCenterPointsOnGrid = _grid.ConvertToLocal(GetShapeCenterPoints());
                _grid.PlaceShape(this, currentCenterPointsOnGrid);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                currentIndex = _grid.WorldToLocal(transform.position);
            }
        }
        else if (_state == State.Spawned)
        {
            bool inGrid = _grid.IsInsideDropZone(transform.position);
            if (!inGrid)
            {
                Debug.LogError("" + gameObject.name + " is not inside Grid!");
                _state = State.Faulty;
            }
            else
            {
                Vector3Int[] result = _grid.ConvertToLocal(GetShapeCenterPoints());
                _grid.PlaceShape(this, result);
                _state = State.Placed;
            }
        }
        else if (_state == State.Drop)
        {
            Vector3Int nextIndex;
            if (currentIndex.y - 1 < 0)
            {
                nextIndex = new Vector3Int(currentIndex.x, 0, currentIndex.z);
            }
            else
            {
                nextIndex = new Vector3Int(currentIndex.x, currentIndex.y - 1, currentIndex.z);
            }

            if (lastValidIndex != nextIndex)
            {
                Vector3Int[] centerIndexes = _grid.ConvertToLocal(GetShapeCenterPoints());
                for (int i = 0; i < centerIndexes.Length; i++)
                {
                    if (centerIndexes[i].y - 1 < 0)
                    {
                        centerIndexes[i] = new Vector3Int(centerIndexes[i].x, 0, centerIndexes[i].z);
                    }
                    else
                    {
                        centerIndexes[i] = new Vector3Int(centerIndexes[i].x, centerIndexes[i].y - 1, centerIndexes[i].z);
                    }
                }

                bool canBePlaced = _grid.CanBePlaced(centerIndexes);
                if (canBePlaced)
                {
                    lastValidIndex = nextIndex;
                }
            }

            Vector3 nextPos = _grid.LocalToWorld(lastValidIndex);
            float dist = Vector3.Distance(transform.position, nextPos);

            if (dist < 0.1f)
            {
                _state = State.Placed;
                currentIndex = _grid.WorldToLocal(nextPos);
                transform.position = _grid.LocalToWorld(currentIndex);
                Vector3Int[] currentCenterPointsOnGrid = _grid.ConvertToLocal(GetShapeCenterPoints());
                _grid.PlaceShape(this, currentCenterPointsOnGrid);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, nextPos, moveSpeed * Time.deltaTime);
                currentIndex = _grid.WorldToLocal(transform.position);
            }

        }
    }

    public Vector3[] GetShapeCenterPoints()
    {
        Vector3[] result = new Vector3[shapeBlocks.Length];
        for (int i = 0; i < shapeBlocks.Length; i++)
        {
            result[i] = shapeBlocks[i].transform.position;
        }
        return result;
    }

    
    public Vector3[] GetLocalShapeCenterPoints()
    {
        Vector3[] result = new Vector3[shapeBlocks.Length];
        for (int i = 0; i < shapeBlocks.Length; i++)
        {
            result[i] = shapeBlocks[i].transform.position - transform.position;
        }
        return result;
    }
    
    public void MoveToPos(Vector3 pos)
    {
        if (_state == State.Grabbed)
        {
            targetPos = pos;
            _state = State.Moving;
        }
    }

    public void RotateRight()
    {
        if (_state == State.Grabbed)
        {
            var euler = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(euler.x, euler.y + 90, euler.z);
            _state = State.Grabbed;
        }
    }

    public void RotateLeft()
    {
        if (_state == State.Grabbed)
        {
            var euler = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(euler.x, euler.y - 90, euler.z);
            _state = State.Rotating;
        }
    }
    public bool DropPiece() {
        if (_state == State.Grabbed && _grid.IsInsideDropZone(transform.position)) {
            Vector3Int[] centerIndexes = _grid.ConvertToLocal(GetShapeCenterPoints());
            bool canBePlaced = _grid.CanBePlaced(centerIndexes);
            if (canBePlaced)
            {
                currentIndex = _grid.WorldToLocal(transform.position);
                _state = State.Drop;
                return true;
            }
        }
        return false;
    }

    public bool IsGrabable()
    {
        if (_state == State.Placed && !isStatic)
        {
            Vector3Int[] centerIndexes = _grid.ConvertToLocal(GetShapeCenterPoints());
            for (int i = 0; i < centerIndexes.Length; i++)
            {
                var cell = _grid.GetHighestCell(new Vector2Int(centerIndexes[i].x, centerIndexes[i].z));
                if (cell != null && cell != this)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public TetrominoGroupBase GrabPiece()
    {
        if (!IsGrabable())
        {
            return null;
        }
        Vector3Int[] currentCenterPointsOnGrid = _grid.ConvertToLocal(GetShapeCenterPoints());
        _grid.RemoveShape(currentCenterPointsOnGrid);
        _state = State.Grabbed;
        return this;
    }
    public float GetBlockHeight() {
        var centerPoints = GetShapeCenterPoints();
        var maxHeight = centerPoints.Max(c => c.y);
        var minHeight = centerPoints.Min(c => c.y);
        return maxHeight - minHeight + 3;
    }
}
