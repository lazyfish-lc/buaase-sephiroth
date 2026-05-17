using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


public class UISFXPlayer : MonoBehaviour
{
[SerializeField] private AudioClip OpenSFX;
[SerializeField] private AudioClip CloseSFX;
[SerializeField] private AudioClip ClickSFX;
    public void PlayOpenSFX()
    {
        if (AudioManager.Instance != null && OpenSFX != null)
        {
            AudioManager.Instance.PlaySFX(OpenSFX);
        }
    }
    public void PlayCloseSFX()
    {
        if (AudioManager.Instance != null && CloseSFX != null)
        {
            AudioManager.Instance.PlaySFX(CloseSFX);
        }
    }
    public void PlayClickSFX()
    {
        if (AudioManager.Instance != null && ClickSFX != null)
        {
            AudioManager.Instance.PlaySFX(ClickSFX);
        }
    }
}