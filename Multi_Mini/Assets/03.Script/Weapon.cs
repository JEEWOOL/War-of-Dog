using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type
    {
        Melee,
        Range
    }
    public Type type;
    public float damage;
    public float rate;
    public BoxCollider meleeArea;

    public int actorNumber;

    public void Use()
    {
        if(type == Type.Melee)
        {
            StopCoroutine(Attack());
            StartCoroutine(Attack());
        }
    }

    IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.1f);
        meleeArea.enabled = true;
        yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false;
    }
}
