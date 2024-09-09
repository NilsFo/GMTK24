using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fallbreaker : MonoBehaviour
{
    public int falloffYPos = -300;
    public bool destroyOnFall = false;

    private Quaternion _initialRotation;
    private Vector3 _initialPosition;

    // Start is called before the first frame update
    void Start()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y <= falloffYPos)
        {
            if (destroyOnFall)
            {
                Destroy(gameObject);
            }
            else
            {
                transform.position = _initialPosition;
                transform.rotation = _initialRotation;
            }
        }
    }
}