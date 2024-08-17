using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Crane : MonoBehaviour {
    public Grid3D grid;
    public TetrominoSpawner tetroSpawner;

    public Transform kranschiene, laufkatze, seil;
    
    public float yLevel = 10f;
    public Vector2Int gridPos;

    public float speed = 1f;

    public enum CraneState {
        MOVING,
        NEW_TILE,
        GRABBING,
        IDLE
    }
    public CraneState craneState;
    public TetrominoGroupBase grabbedTile;

    public Vector3 grabPoint;

    public AnimationCurve speedCurve;
    public float speedCurveWidth = 2f;
    
    // Start is called before the first frame update
    void Start()
    {
        var targetPos = grid.LocalToWorld(new Vector3Int(gridPos.x, 0, gridPos.y));
        
        targetPos.y = yLevel;
        transform.localPosition = targetPos;
    }

    // Update is called once per frame
    void Update() {
        HandleInput();

        if (craneState is CraneState.IDLE or CraneState.MOVING) {
            var targetPos = grid.LocalToWorld(new Vector3Int(gridPos.x, 0, gridPos.y));
            
            targetPos.y = yLevel;
            var speedMod = speedCurve.Evaluate(Vector3.Distance(transform.position, targetPos) / speedCurveWidth) * speed;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speedMod * Time.deltaTime);
            
            if(Vector3.Distance(transform.position, targetPos) < 0.01f) {
                craneState = CraneState.IDLE;
            } else {
                craneState = CraneState.MOVING;
            }
        }

        else if (craneState is CraneState.NEW_TILE) {
            var targetPos = tetroSpawner.transform.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime * 2);

            if (Vector3.Distance(transform.position, targetPos) < 0.01f) {
                GrabNewScaffold();
            }
        }
        
        else if (craneState is CraneState.GRABBING) {
            var targetPos = grabPoint;
            
            var speedMod = speedCurve.Evaluate(Vector3.Distance(transform.position, targetPos) / speedCurveWidth / 2f) * speed;

            transform.position = Vector3.MoveTowards(transform.position, targetPos, speedMod * Time.deltaTime);
            
            if (Vector3.Distance(transform.position, targetPos) < 0.01f) {
                GrabComplete();
            }
        }
        
        // Visuals
        laufkatze.transform.position = new Vector3(transform.position.x, laufkatze.transform.position.y, transform.position.z);
        kranschiene.transform.localPosition = new Vector3(transform.localPosition.x, 0, 0);
        seil.transform.position = (transform.position + laufkatze.position) / 2;
        seil.transform.localScale = new Vector3(seil.transform.localScale.x, Vector3.Distance(transform.position, laufkatze.position), seil.transform.localScale.z);
    }

    public void SetTargetPos(int x, int y) {
        SetTargetPos(new Vector2Int(x, y));
    }
    
    public void SetTargetPos(Vector2Int pos) {
        var gridSize = grid.GetSize();
        if (pos.x >= 0 && pos.x < gridSize.x && pos.y >= 0 && pos.y < gridSize.y) {
            gridPos.x = pos.x;
            gridPos.y = pos.y;

            if (grabbedTile != null) {
                
                var blockHeight = grabbedTile.GetBlockHeight();
                var height = 0f;
                var centerpoints = grabbedTile.GetLocalShapeCenterPoints();
                foreach (var centerpoint in centerpoints) {
                    var c = grid.WorldToLocal(centerpoint + grid.LocalToWorld(new Vector3Int(gridPos.x, 0, gridPos.y)));
                    height = Mathf.Max(
                        height, 
                        grid.GetHighestEmptyCell(new Vector2Int(c.x, c.z)).y
                    );
                }
                yLevel = height + blockHeight + 4;
            } else {
                yLevel = grid.GetHighestEmptyCell(gridPos).y + 4;
            }
        }
    }

    public void ResetTargetPos() {
        SetTargetPos(gridPos);
    }

    public void GrabNewScaffold() {
        
        grabbedTile = tetroSpawner.Next().GetComponent<TetrominoGroupBase>();
        grabbedTile.transform.parent = transform;
        grabbedTile.transform.localPosition = new Vector3(0, -3, 0);  // TODO correct for anchor pos
        grabbedTile._state = TetrominoGroupBase.State.Grabbed;
        
        craneState = CraneState.MOVING;
        Invoke(nameof(ResetTargetPos), 0.1f);
    }

    public void Grab() {
        if (craneState != CraneState.IDLE || HasGrabbedTile())
            return;
        var tetromino = grid.GetHighestCell(gridPos);
        if (tetromino != null) {
            craneState = CraneState.GRABBING;
            grabPoint = tetromino.GetAnchorPoint().transform.position;
            var g = grid.WorldToLocal(grabPoint);
            gridPos = new Vector2Int(g.x, g.z);
        }
    }

    public void GrabComplete() {
        var tetromino = grid.GetHighestCell(gridPos);
        if (tetromino != null) {
            Debug.Log("Grabbing " + tetromino.gameObject.name, tetromino);
            tetromino = tetromino.GrabPiece();
            if (tetromino != null) {
                grabbedTile = tetromino;
                grabbedTile.transform.parent = transform;
                grabbedTile.transform.localPosition = new Vector3(0, 0, 0) - tetromino.GetAnchorPoint().transform.localPosition;  // TODO correct for anchor pos & animate

                Debug.Log("Grabbed piece " + grabbedTile.gameObject.name, grabbedTile);
                craneState = CraneState.MOVING;
            } else {
                Debug.Log("Can't grab!");
                craneState = CraneState.MOVING;
            }
        } else {
            Debug.Log("Can't grab at " + gridPos);
            craneState = CraneState.MOVING;
        }
    }

    public void Drop() {
        if(grabbedTile == null)
            return;
        bool success = grabbedTile.DropPiece();
        if (success) {
            grabbedTile.transform.parent = null;
            grabbedTile = null;
            craneState = CraneState.IDLE;
        }
    }

    public void Rotate() {
        if(HasGrabbedTile())
            grabbedTile.RotateRight();
    }

    public bool HasGrabbedTile() {
        return grabbedTile != null;
    }

    private void HandleInput() {
        var k = Keyboard.current;
        
        // Move Crane
        if (craneState is CraneState.IDLE or CraneState.MOVING) {
            if (k.leftArrowKey.wasPressedThisFrame) {
                SetTargetPos(gridPos.x - 1, gridPos.y);
            } else if (k.rightArrowKey.wasPressedThisFrame) {
                SetTargetPos(gridPos.x + 1, gridPos.y);
            } else if (k.upArrowKey.wasPressedThisFrame) {
                SetTargetPos(gridPos.x, gridPos.y + 1);
            } else if (k.downArrowKey.wasPressedThisFrame) {
                SetTargetPos(gridPos.x, gridPos.y - 1);
            }
        }
        
        // Rotate
        if (craneState is CraneState.IDLE or CraneState.MOVING) {
            if (k.rKey.wasPressedThisFrame) {
                Rotate();
            }
        }

        // Grab & Drop
        if (craneState is CraneState.IDLE) {
            if (k.eKey.wasPressedThisFrame) {
                if (HasGrabbedTile()) {
                    Drop();
                } else {
                    Grab();
                }
            }
        }
        
        // Get Scaffold
        if (k.qKey.wasPressedThisFrame) {
            if (craneState is CraneState.IDLE or CraneState.MOVING && !HasGrabbedTile()) {
                craneState = CraneState.NEW_TILE;
            }
        }
    }
    
}
