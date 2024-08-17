using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrominoSpawner : MonoBehaviour
{


    public TetrominoPool tetrominoPool;

    [SerializeField]
    private List<TetrominoGroupBase.TetrominoGroupType> spawnTypeQueue;
    public float queueSize = 4;


    // Start is called before the first frame update
    void Start()
    {
        spawnTypeQueue = new List<TetrominoGroupBase.TetrominoGroupType>();
    }

    // Update is called once per frame
    void Update()
    {
        // TODO replace
        if (Input.GetKeyDown(KeyCode.O))
        {
            Next();
        }
    }

    [ContextMenu("Create Next")]
    public GameObject Next(int iterations = 0)
    {
        TetrominoGroupBase element = tetrominoPool.Next();
        var type = element.GroupType;

        if (type==TetrominoGroupBase.TetrominoGroupType.Unknown){
            Debug.LogError("Spawnqueue Conflict. Unknown tetromnios are not spawned!" + " [iterations: " + iterations + "]");
            return Next(iterations + 1);
        }

        if (spawnTypeQueue.Contains(type))
        {
            Debug.LogWarning("Spawnqueue Conflict. Already contains type: " + type + " [iterations: " + iterations + "]");
            return Next(iterations + 1);
        }
        
        if (spawnTypeQueue.Count == queueSize + 1)
        {
            spawnTypeQueue.RemoveAt(0);
        }
        spawnTypeQueue.Add(type);

        GameObject newElement = Instantiate(element.gameObject, transform.position, Quaternion.identity);
        newElement.transform.position = transform.position;
        newElement.transform.parent = null;

        return newElement;
    }



}
