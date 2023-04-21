using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject effect;

    private void OnCollisionEnter(Collision collision)
    {
        effect.SetActive(true);
        Destroy(this.gameObject, 3f);
    }
}
