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
    
    public static float moveSpeed = 15f;
    public static float rotationSpeed = 180f;
    
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

    [SerializeField] private GameObject anchorePoint;
    
    [SerializeField] private float dropVelocity = 10f;

    [SerializeField] private Vector3Int currentIndex;
    [SerializeField] private Vector3Int lastValidIndex;

    [SerializeField] private Vector3 targetPos;
    [SerializeField] private Quaternion targetRotation;

    [Header("Effects")]
    [SerializeField] private AudioSource dropSFX;
    
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

            if (isStatic) {
                var tetrominos = GetComponentsInChildren<Tetromino>();
                foreach (var t in tetrominos) {
                    var renderers = t.GetComponentsInChildren<MeshRenderer>();
                    foreach (var meshRenderer in renderers) {
                        foreach (var meshRendererMaterial in meshRenderer.materials) {
                            meshRendererMaterial.color = new Color(0.8f, 0.8f, 0.8f);
                        }
                    }
                }
            }
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
                
                // Drop Effects
                dropSFX.Play();
                if(tetrominoType == Tetromino.TetrominoType.House)
                    FindObjectOfType<CameraShaker>().ShakeCamera(0.1f, 10, 0.5f);
                else 
                    FindObjectOfType<CameraShaker>().ShakeCamera(0.05f, 10, 0.3f);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, nextPos, dropVelocity * Time.deltaTime);
                dropVelocity -= Physics.gravity.y * Time.deltaTime;
                currentIndex = _grid.WorldToLocal(transform.position);
            }

        }
        else if(_state == State.Rotating)
        {
            float diff = Quaternion.Angle(transform.rotation, targetRotation);
            if (diff < 0.1f)
            {
                transform.rotation = targetRotation;
                _state = State.Grabbed;
            }
            else
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
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
            Quaternion nextRota = Quaternion.Euler(0,0,0);
            if(euler.y is >= 0 and < 45)
            {
                nextRota = Quaternion.Euler(euler.x, 90, euler.z);
            }
            else if(euler.y is >= 45 and < 135)
            {
                nextRota = Quaternion.Euler(euler.x, 180, euler.z);
            }
            else if(euler.y is >= 135 and < 225)
            {
                nextRota = Quaternion.Euler(euler.x, 270, euler.z);
            }
            else if(euler.y is >= 255 and < 315)
            {
                nextRota = Quaternion.Euler(euler.x, 0, euler.z);
            }
            else if(euler.y is >= 315 and <= 360)
            {
                nextRota = Quaternion.Euler(euler.x, 90, euler.z);
            }
            targetRotation = nextRota;
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
                dropVelocity = 0;
                return true;
            }
        }
        return false;
    }

    public bool IsGrabable() {
        if (isStatic)
            return false;

        
        if (_state == State.Placed)
        {
            if (IsWelded())
                return false;
            
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

    public bool IsWelded() {
        var welds = GetComponentsInChildren<WeldPoint>();
        return welds.Any(w => w.weldState == WeldPoint.WeldState.WELDED);
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

    public GameObject GetAnchorPoint()
    {
        return anchorePoint;
    }

    public Vector3Int GetCurrentBestIndex()
    {
        var centerPoints = GetShapeCenterPoints();
        Vector3 bestPos = Vector3.positiveInfinity;
        for (int i = 0; i < centerPoints.Length; i++)
        {
            if (centerPoints[i].magnitude < bestPos.magnitude)
            {
                bestPos = centerPoints[i];
            }
        }
        
        return _grid.WorldToLocal(bestPos);
    }

    public Vector3[] GetBaseBlocks()
    {
        var centerPoints = GetShapeCenterPoints();
        Dictionary<string, Vector3> bestBlock = new Dictionary<string, Vector3>();
        for (int i = 0; i < centerPoints.Length; i++)
        {
            var key = $"{centerPoints[i].x},{centerPoints[i].z}";
            if (!bestBlock.ContainsKey(key))
            {
                bestBlock[key] = centerPoints[i];
            }
            else
            {
                if (bestBlock[key].y > centerPoints[i].y)
                {
                    bestBlock[key] = centerPoints[i];
                }
            }
        }
        return bestBlock.Values.ToArray();
    }
}
