using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceBasedMeshCulling : MonoBehaviour
{
    public MeshRenderer myRenderer;
    public float fullyInvisibleDistance = 1f;
    public float cullingDistance = 2f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Color c = myRenderer.material.color;
        c.a = 1f;
        c.a = Time.time % 1;

        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 position = transform.position;

        float distance = Vector3.Distance(position, cameraPos);
        float normalizedValue = 0f;
        if (distance <= fullyInvisibleDistance)
        {
            // If within the inner ring, set to 0
            normalizedValue = 0f;
        }
        else if (distance >= cullingDistance)
        {
            // If beyond the outer ring, set to 1
            normalizedValue = 1f;
        }
        else
        {
            // Otherwise, normalize the value between 0 and 1
            normalizedValue = (distance - fullyInvisibleDistance) / (cullingDistance - fullyInvisibleDistance);
        }

        c.a = normalizedValue;
        myRenderer.material.color = c;
    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, fullyInvisibleDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, cullingDistance);
    }

#endif
}