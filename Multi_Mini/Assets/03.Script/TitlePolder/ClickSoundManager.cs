using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickSoundManager : MonoBehaviour
{
    AudioSource audio;
    public GameObject tutorial;
    bool istuto;

    void Awake()
    {
        tutorial.SetActive(false);
        audio = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            audio.Play();
        }
    }

    public void Tutorial()
    {
        if (!istuto)
        {
            istuto = true;
            tutorial.SetActive(true);
        }
        else
        {
            istuto = false;
            tutorial.SetActive(false);
        }
    }

    public void Exit()
    {
        Application.Quit();
    }
}
