using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Crane : MonoBehaviour {
    public Grid3D grid;
    public TetrominoSpawner tetroSpawner;
    private GameState _gameState;

    public Transform kranschiene, laufkatze, seil;

    public AudioSource craneMoveSFX;

    public float yLevel = 10f;
    public Vector2Int gridPos;

    public float speed = 1f;
    public float rotationSpeed = 180f;

    public Quaternion grabTargetRotation;
    public Transform harken;

    public enum CraneState {
        MOVING,
        ROTATING,
        NEW_TILE,
        GRABBING,
        IDLE
    }
    public CraneState craneState;
    public TetrominoGroupBase grabbedTile;

    public Vector3 grabPoint;

    public AnimationCurve speedCurve;
    public float speedCurveWidth = 2f;

    public enum ControlState {
        North,
        South,
        East,
        West
    }

    public ControlState currentControlState = ControlState.North;
    public UnityEvent OnGrabEvent = new UnityEvent(), OnDropEvent = new UnityEvent();
    
    // Start is called before the first frame update
    void Start() {
        _gameState = FindObjectOfType<GameState>();
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

            if (Vector3.Distance(transform.position, targetPos) < 0.01f) {
                craneState = CraneState.IDLE;
                craneMoveSFX.volume = 0.0f;
            } else {
                craneState = CraneState.MOVING;
                craneMoveSFX.volume = 1.0f;
            }
        } else if (craneState is CraneState.NEW_TILE) {
            var targetPos = tetroSpawner.transform.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime * 2);

            if (Vector3.Distance(transform.position, targetPos) < 0.01f) {
                GrabNewScaffold();
            }
        } else if (craneState is CraneState.GRABBING) {
            var targetPos = grabPoint;

            var speedMod = speedCurve.Evaluate(Vector3.Distance(transform.position, targetPos) / speedCurveWidth / 2f) * speed;

            transform.position = Vector3.MoveTowards(transform.position, targetPos, speedMod * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.01f) {
                GrabComplete();
            }
        } else if (craneState is CraneState.ROTATING) {
            if (!harken) craneState = CraneState.IDLE;
            float diff = Quaternion.Angle(harken.rotation, grabTargetRotation);
            if (diff < 0.1f) {
                harken.rotation = grabTargetRotation;
                craneState = CraneState.IDLE;
            } else {
                harken.rotation = Quaternion.RotateTowards(harken.rotation, grabTargetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        // Visuals
        laufkatze.transform.position = new Vector3(transform.position.x, laufkatze.transform.position.y, transform.position.z);
        kranschiene.transform.localPosition = new Vector3(transform.localPosition.x, 0, 0);
        seil.transform.position = (transform.position + laufkatze.position) / 2 + Vector3.up * 1f;
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
        grabbedTile.transform.localPosition = new Vector3(0, -3, 0); // TODO correct for anchor pos
        grabbedTile._state = TetrominoGroupBase.State.Grabbed;

        craneState = CraneState.MOVING;
        Invoke(nameof(ResetTargetPos), 0.1f);
    }

    public void Grab() {
        if (craneState != CraneState.IDLE || HasGrabbedTile())
            return;
        var tetromino = grid.GetHighestCell(gridPos);
        OnGrabEvent.Invoke();
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
                grabbedTile.transform.localPosition = new Vector3(0, 0, 0) - tetromino.GetAnchorPoint().transform.localPosition; // TODO correct for anchor pos & animate

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
        if (grabbedTile == null)
            return;
        
        OnDropEvent.Invoke();
        bool success = grabbedTile.DropPiece();
        if (success) {
            grabbedTile.transform.parent = null;
            grabbedTile = null;
            craneState = CraneState.IDLE;
        }
    }

    public void Rotate() {
        if (craneState != CraneState.IDLE || !HasGrabbedTile())
            return;
        grabbedTile.RotateRight();
        var euler = harken.rotation.eulerAngles;
        Quaternion nextRota = Quaternion.Euler(-90, 0, 0);
        if (euler.y is >= 0 and < 45) {
            nextRota = Quaternion.Euler(euler.x, 90, euler.z);
        } else if (euler.y is >= 45 and < 135) {
            nextRota = Quaternion.Euler(euler.x, 180, euler.z);
        } else if (euler.y is >= 135 and < 225) {
            nextRota = Quaternion.Euler(euler.x, 270, euler.z);
        } else if (euler.y is >= 255 and < 315) {
            nextRota = Quaternion.Euler(euler.x, 0, euler.z);
        } else if (euler.y is >= 315 and <= 360) {
            nextRota = Quaternion.Euler(euler.x, 90, euler.z);
        }
        grabTargetRotation = nextRota;
        craneState = CraneState.ROTATING;
    }

    public bool HasGrabbedTile() {
        return grabbedTile != null;
    }

    private void HandleInput() {
        var k = Keyboard.current;

        if (craneState is CraneState.IDLE) {
            var euler = _gameState.player.transform.rotation.eulerAngles;
            if (euler.y is >= 0 and < 45) {
                currentControlState = ControlState.North;
            } else if (euler.y is >= 45 and < 135) {
                currentControlState = ControlState.East;
            } else if (euler.y is >= 135 and < 225) {
                currentControlState = ControlState.South;
            } else if (euler.y is >= 255 and < 315) {
                currentControlState = ControlState.West;
            } else if (euler.y is >= 315 and <= 360) {
                currentControlState = ControlState.North;
            }
        }

        // Move Crane
        if (craneState is CraneState.IDLE or CraneState.MOVING) {

            if (currentControlState == ControlState.North) {
                if (k.leftArrowKey.wasPressedThisFrame) {
                    SetTargetPos(gridPos.x - 1, gridPos.y);
                } else if (k.rightArrowKey.wasPressedThisFrame) {
                    SetTargetPos(gridPos.x + 1, gridPos.y);
                } else if (k.upArrowKey.wasPressedThisFrame) {
                    SetTargetPos(gridPos.x, gridPos.y + 1);
                } else if (k.downArrowKey.wasPressedThisFrame) {
                    SetTargetPos(gridPos.x, gridPos.y - 1);
                }
            } else if (currentControlState == ControlState.South) {
                if (k.leftArrowKey.wasPressedThisFrame) {
                    SetTargetPos(gridPos.x + 1, gridPos.y);
                } else if (k.rightArrowKey.wasPressedThisFrame) {
                    SetTargetPos(gridPos.x - 1, gridPos.y);
                } else if (k.upArrowKey.wasPressedThisFrame) {
                    SetTargetPos(gridPos.x, gridPos.y - 1);
                } else if (k.downArrowKey.wasPressedThisFrame) {
                    SetTargetPos(gridPos.x, gridPos.y + 1);
                }
            } else if (currentControlState == ControlState.East) {
                if (k.leftArrowKey.wasPressedThisFrame) {
                    SetTargetPos(gridPos.x, gridPos.y + 1);
                } else if (k.rightArrowKey.wasPressedThisFrame) {
                    SetTargetPos(gridPos.x, gridPos.y - 1);
                } else if (k.upArrowKey.wasPressedThisFrame) {
                    SetTargetPos(gridPos.x + 1, gridPos.y);
                } else if (k.downArrowKey.wasPressedThisFrame) {
                    SetTargetPos(gridPos.x - 1, gridPos.y);
                }
            } else if (currentControlState == ControlState.West) {
                if (k.leftArrowKey.wasPressedThisFrame) {
                    SetTargetPos(gridPos.x, gridPos.y - 1);
                } else if (k.rightArrowKey.wasPressedThisFrame) {
                    SetTargetPos(gridPos.x, gridPos.y + 1);
                } else if (k.upArrowKey.wasPressedThisFrame) {
                    SetTargetPos(gridPos.x - 1, gridPos.y);
                } else if (k.downArrowKey.wasPressedThisFrame) {
                    SetTargetPos(gridPos.x + 1, gridPos.y);
                }
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
        /*if (k.qKey.wasPressedThisFrame) {
            if (craneState is CraneState.IDLE or CraneState.MOVING && !HasGrabbedTile()) {
                craneState = CraneState.NEW_TILE;
            }
        }*/
    }

}
