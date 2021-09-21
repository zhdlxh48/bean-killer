using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public AudioSource bgmEasy;
    public AudioSource bgmNormal;
    public AudioSource bgmHard;
    
    public void PlayMusic(int num)
    {
        bgmEasy.Stop();
        bgmNormal.Stop();
        bgmHard.Stop();
        
        switch (num)
        {
            case 0:
                bgmEasy.Play();
                break;

            case 1:
                bgmNormal.Play();
                break;

            case 2:
                bgmHard.Play();
                break;

            default:
                bgmEasy.Stop();
                bgmNormal.Stop();
                bgmHard.Stop();
                break;
        }
    }
}
