using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Crane : MonoBehaviour {
    public Grid3D grid;
    public TetrominoSpawner tetroSpawner;
    
    public float yLevel = 10f;
    public Vector2Int gridPos;

    public float speed = 1f;

    public enum CraneState {
        MOVING,
        NEW_TILE,
        IDLE
    }
    public CraneState craneState;
    public TetrominoGroupBase grabbedTile;

    
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
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPos, speed * Time.deltaTime);
            
            if(Vector3.Distance(transform.localPosition, targetPos) < 0.01f) {
                craneState = CraneState.IDLE;
            } else {
                craneState = CraneState.MOVING;
            }
        }

        else if (craneState is CraneState.NEW_TILE) {
            var targetPos = tetroSpawner.transform.position;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPos, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.01f) {
                grabbedTile = tetroSpawner.Next().GetComponent<TetrominoGroupBase>();
                grabbedTile.transform.parent = transform;
                grabbedTile.transform.localPosition = new Vector3(0, -3, 0);  // TODO correct for anchor pos
                
                craneState = CraneState.MOVING;
            }
        }
    }

    public void SetTargetPos(int x, int y) {
        SetTargetPos(new Vector2Int(x, y));
    }
    
    public void SetTargetPos(Vector2Int pos) {
        var gridSize = grid.GetSize();
        if (pos.x >= 0 && pos.x < gridSize.x && pos.y >= 0 && pos.y < gridSize.y) {
            gridPos.x = pos.x;
            gridPos.y = pos.y;
        }
    }

    public void Grab() {
        // TODO
    }

    public void Drop() {
        // TODO
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
