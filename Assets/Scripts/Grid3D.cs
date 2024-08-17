using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Grid3D : MonoBehaviour
{

    private TetrominoGroupBase[,,] _grid;

    [SerializeField] private int sizeX = 10;
    [SerializeField] private int sizeY = 10;
    [SerializeField] private int sizeZ = 10;

    [SerializeField] private int bufferX = 3;
    [SerializeField] private int bufferY = 3;
    [SerializeField] private int bufferZ = 3;
    
    [SerializeField] private float blockScaleX = 3f;
    [SerializeField] private float blockScaleY = 3f;
    [SerializeField] private float blockScaleZ = 3f;


    private void Awake()
    {
        _grid = new TetrominoGroupBase[sizeX, sizeY, sizeZ];
    }

    private (int, bool) ToIndex(float pos, int minX, int maxX)
    {
        bool isNotInside = false;
        float reminderX = pos % blockScaleX;
        int index = (int)((pos - reminderX) / blockScaleX);
        if(reminderX < 0) index += 1;
        if (index < minX)
        {
            index = minX;
            isNotInside = true;
        }
        if(index > maxX)
        {
            index = maxX;
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
            if (shape == null)
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
}
