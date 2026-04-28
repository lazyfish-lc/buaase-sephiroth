using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    public bool isOpen = false;
    public CanvasGroup SettingCanvas;
    [Header("精灵图设置")]
    public Sprite MusicOnSprite;
    public Sprite MusicOffSprite;
    public Sprite SoundOnSprite;
    public Sprite SoundOffSprite;    
    [Header("按钮设置")]
    public Button MusicButton;
    public Button SoundButton;
    [Header("滑动条设置")]
    public Slider MusicSlider;
    public Slider SoundSlider;
    [Header("文本设置")]
    public TMP_Text MusicVolumeNumber;
    public TMP_Text SoundVolumeNumber;
    private float LastMusicVolume;
    private float LastSoundVolume;

    private bool isMusicOn;
    private bool isSoundOn;

    public static SettingUI Instance;
    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    void Start()
    {
        SettingCanvas.gameObject.SetActive(false);
    }
    public void UpdateVolume()
    {
        if (isOpen)
        {
            MusicVolumeNumber.text = GetMusicVolume(MusicSlider.value).ToString();
            SoundVolumeNumber.text = GetSoundVolume(SoundSlider.value).ToString();
            AudioManager.Instance.SetBGMVolume(MusicSlider.value);
            AudioManager.Instance.SetSFXVolume(SoundSlider.value);
        }
    }

    void LoadPreferences()
    {
        // 音乐和音效
        MusicSlider.value = LastMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        SoundSlider.value = LastSoundVolume = PlayerPrefs.GetFloat("SoundVolume", 0.8f);
        isMusicOn = PlayerPrefs.GetInt("IsMusicOn", 1) == 1;
        isSoundOn = PlayerPrefs.GetInt("IsSoundOn", 1) == 1;
        MusicButton.image.sprite = isMusicOn ? MusicOnSprite : MusicOffSprite;
        SoundButton.image.sprite = isSoundOn ? SoundOnSprite : SoundOffSprite;
        MusicSlider.interactable = isMusicOn;
        SoundSlider.interactable = isSoundOn;
        AudioManager.Instance.SetBGMVolume(PlayerPrefs.GetFloat("MusicVolume", 0.8f));
        AudioManager.Instance.SetSFXVolume(PlayerPrefs.GetFloat("SoundVolume", 0.8f));
    }

    public void OpenAndClose()
    {
        
        if(!isOpen)
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
        isOpen = true;
        SettingCanvas.gameObject.SetActive(true);
        LoadPreferences();
    }
    public void Close()
    {
        isOpen = false;
        SettingCanvas.gameObject.SetActive(false);
        LoadPreferences();
    }

    public void OpenAndCloseMusic()
    {
        if(isMusicOn)
        {
            MusicButton.image.sprite = MusicOffSprite;
            isMusicOn = false;
            LastMusicVolume = MusicSlider.value;
            MusicSlider.value = 0;
            MusicSlider.interactable = false;
        }
        else
        {
            MusicButton.image.sprite = MusicOnSprite;
            isMusicOn = true;
            MusicSlider.value = LastMusicVolume;
            MusicSlider.interactable = true;
        }

    }

    public void OpenAndCloseSound()
    {
        if(isSoundOn)
        {
            SoundButton.image.sprite = SoundOffSprite;
            isSoundOn = false;
            LastSoundVolume = SoundSlider.value;
            SoundSlider.value = 0;
            SoundSlider.interactable = false;
        }
        else
        {
            SoundButton.image.sprite = SoundOnSprite;
            isSoundOn = true;
            SoundSlider.value = LastSoundVolume;
            SoundSlider.interactable = true;
        }
        
    }

    public void Save()
    {
        // 保存设置逻辑（例如保存到PlayerPrefs）
        Debug.Log("设置已保存");
        //保存音量设置
        Debug.Log("音乐音量: " + GetMusicVolume(MusicSlider.value));
        Debug.Log("音效音量: " + GetSoundVolume(SoundSlider.value));
        PlayerPrefs.SetFloat("MusicVolume", MusicSlider.value);
        PlayerPrefs.SetFloat("SoundVolume", SoundSlider.value);
        PlayerPrefs.SetInt("IsMusicOn", isMusicOn ? 1 : 0);
        PlayerPrefs.SetInt("IsSoundOn", isSoundOn ? 1 : 0);
    }
    public int GetMusicVolume(float volume)
    {
        return (int)(volume * 100);
    }
    public int GetSoundVolume(float volume)
    {
        return (int)(volume * 100);
    }

    public void KeySetting()
    {
        Debug.Log("打开按键设置界面");
        //KeySettingUI.Instance.OpenAndClose();
    }
    
}
