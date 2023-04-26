using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject effect;
    public AudioSource bombAudio;
    bool isOnsound = false;
    int count = 0;
    private void OnCollisionEnter(Collision collision)
    {
        if(isOnsound == false)
        {
            isOnsound = true;
            effect.SetActive(true);
            bombAudio.Play();
            Debug.Log(count++);
            Destroy(this.gameObject, 3f);
        }
    }
}
