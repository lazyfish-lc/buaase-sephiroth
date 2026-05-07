using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


public class BGMPlayer : MonoBehaviour
{
[SerializeField] private AudioClip bgmForThisScene;
    void Start()
    {
        if (AudioManager.Instance != null)
        {
            UnityEngine.Debug.Log($"播放 {bgmForThisScene.name}");
            AudioManager.Instance.PlayMusic(bgmForThisScene);
        }
    }
}