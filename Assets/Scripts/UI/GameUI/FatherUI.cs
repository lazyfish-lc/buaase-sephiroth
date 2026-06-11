using UnityEngine;
public class FatherUI : MonoBehaviour
{
    public void PlayOpenSFX()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.PlayOpenSFX();
        }
    }
    public void PlayCloseSFX()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.PlayCloseSFX();
        }
    }
    public void PlayClickSFX()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.PlayClickSFX();
        }
    }
    public void PlayPageTurnSFX()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.PlayPageTurnSFX();
        }
    }
    public void PlayItemGetSFX()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.PlayItemGetSFX();
        }
    }
    public void PlayItemUseSFX()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.PlayItemUseSFX();
        }
    }
    public void PlayHintUISFX()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.PlayHintUISFX();
        }
    }
}