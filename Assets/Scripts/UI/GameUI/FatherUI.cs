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
}