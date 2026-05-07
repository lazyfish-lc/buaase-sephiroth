using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


public class LifeSFXPlayer : MonoBehaviour
{
[SerializeField] private AudioClip AttackSFX;
[SerializeField] private AudioClip HurtSFX;
[SerializeField] private AudioClip SepakSFX;

[SerializeField] private AudioClip WalkSFX;
    public void PlayAttackSFX()
    {
        if (AudioManager.Instance != null && AttackSFX != null)
        {
            AudioManager.Instance.PlaySFX(AttackSFX);
        }
    }
    public void PlayHurtSFX()
    {
        if (AudioManager.Instance != null && HurtSFX != null)
        {
            AudioManager.Instance.PlaySFX(HurtSFX);
        }
    }
    public void PlaySepakSFX()
    {
        if (AudioManager.Instance != null && SepakSFX != null)
        {
            AudioManager.Instance.PlaySFX(SepakSFX);
        }
    }
    public void PlayWalkSFX()
    {
        if (AudioManager.Instance != null && WalkSFX != null)
        {
            AudioManager.Instance.PlaySFX(WalkSFX);
        }
    }
}