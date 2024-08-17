using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrominoSpawner : MonoBehaviour
{


    public TetrominoPool tetrominoPool;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            Next();
        }
    }

    public GameObject Next()
    {
        TetrominoGroupBase element = tetrominoPool.Next();

        GameObject newElement = Instantiate(element.gameObject, transform.position, Quaternion.identity);
        newElement.transform.position = transform.position;
        newElement.transform.parent = null;

        return newElement;
    }


}
