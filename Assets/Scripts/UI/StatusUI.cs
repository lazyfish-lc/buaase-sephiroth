using UnityEngine;
using UnityEngine.UI;
public class StatusUI : MonoBehaviour
{
    public CanvasGroup StatusCanvas;
    public static StatusUI Instance;
    public PlayerSmallObject player;

    public Slider HPSlider;

    
    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public float GetPlayerHealth()
    {
        if (player != null)
        {
            return player.dynamicState.propertyMap.ContainsKey("Health") ? player.playerState.propertyMap["Health"].value : 0f;
        }
        return 0f;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    void Update()
    {
        if (player != null)
        {
            // 超过1000血即为100%，根据实际情况调整
            float Health = GetPlayerHealth(); // 默认值，防止属性缺失导致错误
            if (Health > 1000f)
            {
                HPSlider.value = 1f;
            }
            else
            {
                HPSlider.value = Health / 1000f;
            }
        }
    }
    public void OpenAndClose()
    {
        
        if(StatusCanvas.alpha == 0)
        {
            Open();
        }
        else
        {
            Close();
        }
    }
    public void Open()
    {
        StatusCanvas.gameObject.SetActive(true);
    }
    public void Close()
    {
        StatusCanvas.gameObject.SetActive(false);
    }

    
}
