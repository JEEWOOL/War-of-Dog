using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public Image bomb_CoolTime;
    public GameObject bombCoolTime;
    public Image shield_CoolTime;
    public GameObject shieldCoolTime;
    public Slider hp_Slider;
}
