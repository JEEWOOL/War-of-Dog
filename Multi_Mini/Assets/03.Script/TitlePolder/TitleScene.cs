using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleScene : MonoBehaviour
{
    public Text titleText;
    public string m_Message;
    public float t_speed = 0.2f;

    public AudioSource audioSource;
    public GameObject titleLogo;

    private void Start()
    {
        titleLogo.SetActive(false);
        m_Message = titleText.text;        
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(Typing(titleText, m_Message, t_speed));
    }

    IEnumerator Typing(Text typingText, string message, float speed)
    {        
        for(int i = 0; i < message.Length; i++)
        {
            typingText.text = message.Substring(0, i + 1);
            yield return new WaitForSeconds(speed);
        }
        yield return new WaitForSeconds(2f);
        titleLogo.SetActive(true);
        audioSource.Play();
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Menu");
    }
}
