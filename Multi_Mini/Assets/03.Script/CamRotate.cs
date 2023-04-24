using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamRotate : MonoBehaviour
{
    public float rotSpeed = 200;

    float mx = 0;
    float my = 0;

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            rotSpeed -= 1;
            Debug.Log($"마우스감도 -1 : {rotSpeed}");
        }
        else if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            rotSpeed += 1;
            Debug.Log($"마우스감도 +1 : {rotSpeed}");
        }

        float h = Input.GetAxisRaw("Mouse X");
        float v = Input.GetAxisRaw("Mouse Y");

        mx += h * rotSpeed * Time.deltaTime;
        my += v * rotSpeed * Time.deltaTime;

        my = Mathf.Clamp(my, -90, 90);

        transform.eulerAngles = new Vector3(-my, mx, 0);
    }
}
