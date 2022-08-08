using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float rotateSpeed = 5f;


    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, 1f * rotateSpeed * Time.deltaTime);
    }
}
