using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


public class UISFXPlayer : MonoBehaviour
{
[SerializeField] private AudioClip OpenSFX;
[SerializeField] private AudioClip CloseSFX;
[SerializeField] private AudioClip ClickSFX;
[SerializeField] private AudioClip ItemGetSFX;
[SerializeField] private AudioClip ItemUseSFX;
[SerializeField] private AudioClip HintUISFX;
[SerializeField] private AudioClip PageTurnSFX;
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
    public void PlayItemGetSFX()
    {
        if (AudioManager.Instance != null && ItemGetSFX != null)
        {
            AudioManager.Instance.PlaySFX(ItemGetSFX);
        }
    }
    public void PlayItemUseSFX()
    {
        if (AudioManager.Instance != null && ItemUseSFX != null)
        {
            AudioManager.Instance.PlaySFX(ItemUseSFX);
        }
    }
    public void PlayHintUISFX()
    {
        if (AudioManager.Instance != null && HintUISFX != null)
        {
            AudioManager.Instance.PlaySFX(HintUISFX);
        }
    }
    public void PlayPageTurnSFX()
    {
        if (AudioManager.Instance != null && PageTurnSFX != null)
        {
            AudioManager.Instance.PlaySFX(PageTurnSFX);
        }
    }
}