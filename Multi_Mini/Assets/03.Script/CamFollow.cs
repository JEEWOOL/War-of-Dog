using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollow : MonoBehaviour
{
    public Transform target;

    private void Update()
    {
        //transform.position = target.position;
        transform.position = GameObject.Find("CamPosition").transform.position;
        transform.rotation = GameObject.Find("CamPosition").transform.rotation;
    }
}
