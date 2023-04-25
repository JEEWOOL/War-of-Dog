using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public Image bomb_CoolTime;
    public GameObject bombCoolTime;
    public Image shield_CoolTime;
    public GameObject shieldCoolTime;
    public Slider hp_Slider;
    public GameObject dBP;

    ///////////////////////////////////////////////

    private void Awake()
    {
        // 출현 위치 정보를 배열에 생성
        Transform[] points = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>();
        int idx = Random.Range(1, points.Length);

        // 네트워크상에 캐릭터 생성
        PhotonNetwork.Instantiate("Player", points[idx].position, points[idx].rotation, 0);
    }
}
