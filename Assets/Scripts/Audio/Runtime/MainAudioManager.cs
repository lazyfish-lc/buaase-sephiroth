using System.Diagnostics;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;  // 单例

    [Header("-------- 音频设置 ----------")]
    [SerializeField] private AudioMixer MainAudioMixer;     // 音频总控
    [Header("-------- 音频源 ----------")]
    [SerializeField] private AudioSource BGMSource;   // BGM专用源
    [SerializeField] private AudioSource SFXSource;     // 音效专用源

    

    private void Awake()
    {
        // 1. 实现单例模式，确保全局只有一个 AudioManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 跨场景不销毁，音乐无缝衔接
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 2. 初始化音频源设置
        if (BGMSource != null)
        {
            BGMSource.loop = true;        // 背景音乐循环播放
            BGMSource.playOnAwake = false;
            BGMSource.outputAudioMixerGroup = MainAudioMixer.FindMatchingGroups("BGM")[0];
        }
        if (SFXSource != null)
        {
            SFXSource.playOnAwake = false; // 音效不自动播放
            SFXSource.outputAudioMixerGroup = MainAudioMixer.FindMatchingGroups("SFX")[0];
        }
    }
    // 设置背景音乐音量，参数范围0-1
    public void SetBGMVolume(float volume)
    {
        // Mixer的BGM通道音量设置，转换为分贝,因为AudioMixer使用分贝为单位，0.0001f是为了避免对数运算中的负无穷大
        MainAudioMixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20);
        UnityEngine.Debug.Log($"设置BGM音量: {volume} (dB: {Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20})");
    }
    // 设置音效音量，参数范围0-1
    public void SetSFXVolume(float volume)
    {
        // Mixer的SFX通道音量设置，转换为分贝
        MainAudioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20); // 转换为分贝
    }

    // 播放背景音乐
    public void PlayMusic(AudioClip clip)
    {
        SetBGMVolume(PlayerPrefs.GetFloat("MusicVolume", 0.8f)); // 加载保存的音乐音量设置
        SetSFXVolume(PlayerPrefs.GetFloat("SoundVolume", 0.8f)); // 加载保存的音效音量设置

        if (clip == null) return;
        // 如果已经在播放这首音乐，直接返回
        if (BGMSource.isPlaying && BGMSource.clip == clip) return;
        BGMSource.clip = clip;
        BGMSource.Play();
    }

    // 播放一次性音效（用于攻击、跳跃、UI点击等）
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        SFXSource.PlayOneShot(clip);
    }
    public void Stop()
    {
        BGMSource.Stop();
        SFXSource.Stop();
    }
}