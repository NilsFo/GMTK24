using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class Grid3D : MonoBehaviour
{

    private TetrominoGroupBase[,,] _grid;

    [SerializeField] private int sizeX = 10;
    [SerializeField] private int sizeY = 10;
    [SerializeField] private int sizeZ = 10;

    [SerializeField] private int bufferX = 3;
    [SerializeField] private int bufferZ = 3;
    
    [SerializeField] private float blockScaleX = 3f;
    [SerializeField] private float blockScaleY = 3f;
    [SerializeField] private float blockScaleZ = 3f;


    private void Awake()
    {
        _grid = new TetrominoGroupBase[sizeX, sizeY, sizeZ];
    }

    private (int, bool) ToIndex(float pos, int min, int max)
    {
        bool isNotInside = false;
        float reminderX = pos % blockScaleX;
        int index = (int)((pos - reminderX) / blockScaleX);
        if (reminderX < 0) index += 1;
        if (index < min)
        {
            index = min;
            isNotInside = true;
        }
        if (index > max)
        {
            index = max;
            isNotInside = true;
        }
        return (index, isNotInside);
    }

    private float ToWorld(int index, float scale)
    {
        return index * scale;
    }

    public Vector3Int WorldToLocal(Vector3 pos)
    {
        var indexX = ToIndex(pos.x, 0, sizeX);
        var indexY = ToIndex(pos.y, 0, sizeY);
        var indexZ = ToIndex(pos.z, 0, sizeZ);

        return new Vector3Int(indexX.Item1, indexY.Item1, indexZ.Item1);
    }

    public Vector3 LocalToWorld(Vector3Int pos)
    {
        float posX = ToWorld(pos.x, blockScaleX);
        float posZ = ToWorld(pos.z, blockScaleZ);
        float posY = ToWorld(pos.y, blockScaleY);

        return new Vector3(posX, posY, posZ);
    }

    public bool IsInsideGrid(Vector3 pos)
    {
        var indexX = ToIndex(pos.x, 0, sizeX);
        var indexY = ToIndex(pos.y, 0, sizeY);
        var indexZ = ToIndex(pos.z, 0, sizeZ);

        return !indexX.Item2 && !indexY.Item2 && !indexZ.Item2;
    }
    
    public bool IsInsideDropZone(Vector3 pos)
    {
        var indexX = ToIndex(pos.x, 0+bufferX, sizeX-bufferX);
        var indexY = ToIndex(pos.y, 0, sizeY);
        var indexZ = ToIndex(pos.z, 0+bufferZ, sizeZ-bufferZ);

        return !indexX.Item2 && !indexY.Item2 && !indexZ.Item2;
    }

    public Vector3Int[] ConvertToLocal(Vector3[] pos)
    {
        Vector3Int[] result = new Vector3Int[pos.Length];
        for (int i = 0; i < pos.Length; i++)
        {
            result[i] = WorldToLocal(pos[i]);
        }
        return result;
    }

    public Vector3[] ConvertToWorld(Vector3Int[] pos)
    {
        Vector3[] result = new Vector3[pos.Length];
        for (int i = 0; i < pos.Length; i++)
        {
            result[i] = LocalToWorld(pos[i]);
        }
        return result;
    }

    public bool CanBePlaced(Vector3Int[] indexes)
    {
        bool result = true;
        for (int i = 0; i < indexes.Length; i++)
        {
            var shape = _grid[indexes[i].x, indexes[i].y, indexes[i].z];
            if (shape != null)
            {
                result = false;
                break;
            }
        }
        return result;
    }

    public bool PlaceShape(TetrominoGroupBase shape, Vector3Int[] indexes)
    {
        bool canBePlaced = CanBePlaced(indexes);
        if (!canBePlaced) return false;
        for (int i = 0; i < indexes.Length; i++)
        {
            _grid[indexes[i].x, indexes[i].y, indexes[i].z] = shape;
        }
        return true;
    }

    public Vector3Int GetSize() {
        return new Vector3Int(sizeX, sizeY, sizeZ);
    }
    
    public void RemoveShape(Vector3Int[] indexes)
    {
        for (int i = 0; i < indexes.Length; i++)
        {
            _grid[indexes[i].x, indexes[i].y, indexes[i].z] = null;
        }
    }
    
    public Vector3 GetScale()
    {
        return new Vector3(blockScaleX, blockScaleY, blockScaleZ);
    }

    public Vector3 GetHighestEmptyCell(Vector2Int index)
    {
        for (int i = sizeY-1; i >= 0; i--)
        {
            TetrominoGroupBase cell = _grid[index.x, i, index.y];
            if (cell != null)
            {
                return LocalToWorld(new Vector3Int(index.x, i+1, index.y));
            }
        }
        return LocalToWorld(new Vector3Int(index.x,0,index.y));
    }
    
    public TetrominoGroupBase GetHighestCell(Vector2Int index)
    {
        for (int i = sizeY-1; i >= 0; i--)
        {
            TetrominoGroupBase cell = _grid[index.x, i, index.y];
            if (cell != null)
            {
                return cell;
            }
        }
        return null;
    }
    
    [Obsolete]
    public Bounds ToBounds()
    {
        Vector3 center = new Vector3(
                                     transform.position.x + sizeX / 2,
                                     transform.position.y + sizeY / 2,
                                     transform.position.z + sizeZ / 2
                                     );
        Vector3 size = new Vector3(
                                     sizeX * blockScaleX,
                                     sizeY * blockScaleY,
                                     sizeZ * blockScaleZ
                                     );

        return new Bounds(center, size);
    }

    [Obsolete]
    public Vector3 GetWorldCenter()
    {
        return ToBounds().center;
    }

#if UNITY_EDITOR

    public void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.red;
        //Gizmos.DrawCube(GetWorldCenter(), Vector3.one);

        for (int x = 0; x < sizeX; x++)
        {

            for (int y = 0; y < sizeY; y++)
            {

                for (int z = 0; z < sizeZ; z++)
                {
                    Vector3Int pos = new Vector3Int(x, y, z);
                    string statusString = "x:" + x + ", y:" + y + ", z:" + z;

                    if (Application.isPlaying)
                    {
                        TetrominoGroupBase thing = _grid[x, y, z];
                        if (thing == null)
                        {
                            continue;
                            Gizmos.color = Color.red;
                        }
                        else
                        {
                            Gizmos.color = Color.blue;
                            statusString = statusString + "\n" + thing.gameObject.name;
                        }
                    }
                    else
                    {
                        Gizmos.color = Color.gray;
                    }

                    //Handles.Label(LocalToWorld(pos), statusString);
                    Gizmos.DrawWireCube(LocalToWorld(pos), new Vector3(blockScaleX, blockScaleY, blockScaleZ) * 1f);
                }
            }
        }
    }
    
    private void DrawBounds(Bounds bounds, Color color)
    {
        Vector3[] corners = new Vector3[8];
        Vector3 center = bounds.center;
        Vector3 size = bounds.size;

        corners[0] = center + new Vector3(-size.x, -size.y, -size.z) * 0.5f;
        corners[1] = center + new Vector3(size.x, -size.y, -size.z) * 0.5f;
        corners[2] = center + new Vector3(size.x, -size.y, size.z) * 0.5f;
        corners[3] = center + new Vector3(-size.x, -size.y, size.z) * 0.5f;

        corners[4] = center + new Vector3(-size.x, size.y, -size.z) * 0.5f;
        corners[5] = center + new Vector3(size.x, size.y, -size.z) * 0.5f;
        corners[6] = center + new Vector3(size.x, size.y, size.z) * 0.5f;
        corners[7] = center + new Vector3(-size.x, size.y, size.z) * 0.5f;

        Gizmos.color = color;

        // Draw bottom face
        Gizmos.DrawLine(corners[0], corners[1]);
        Gizmos.DrawLine(corners[1], corners[2]);
        Gizmos.DrawLine(corners[2], corners[3]);
        Gizmos.DrawLine(corners[3], corners[0]);

        // Draw top face
        Gizmos.DrawLine(corners[4], corners[5]);
        Gizmos.DrawLine(corners[5], corners[6]);
        Gizmos.DrawLine(corners[6], corners[7]);
        Gizmos.DrawLine(corners[7], corners[4]);

        // Draw vertical lines
        Gizmos.DrawLine(corners[0], corners[4]);
        Gizmos.DrawLine(corners[1], corners[5]);
        Gizmos.DrawLine(corners[2], corners[6]);
        Gizmos.DrawLine(corners[3], corners[7]);
    }
    
#endif
    
}
