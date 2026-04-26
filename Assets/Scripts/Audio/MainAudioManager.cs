using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;  // 单例

    [Header("-------- 音频设置 ----------")]
    [SerializeField] private AudioMixer MainAudioMixer;     // 音频总控
    [Header("-------- 音频源 ----------")]
    [SerializeField] private AudioSource musicSource;   // BGM专用源
    [SerializeField] private AudioSource sfxSource;     // 音效专用源

    

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
        if (musicSource != null)
        {
            musicSource.loop = true;        // 背景音乐循环播放
            musicSource.playOnAwake = false;
            musicSource.outputAudioMixerGroup = MainAudioMixer.FindMatchingGroups("Music")[0];
        }
        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.outputAudioMixerGroup = MainAudioMixer.FindMatchingGroups("SFX")[0];
        }
    }
    // 设置背景音乐音量，参数范围0-1
    public void SetMusicVolume(float volume)
    {
        // Mixer的BGM通道音量设置，转换为分贝,因为AudioMixer使用分贝为单位，0.0001f是为了避免对数运算中的负无穷大
        MainAudioMixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20);
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
        if (clip == null) return;
        // 如果已经在播放这首音乐，直接返回
        if (musicSource.isPlaying && musicSource.clip == clip) return;
        
        musicSource.clip = clip;
        musicSource.Play();
    }

    // 播放一次性音效（用于攻击、跳跃、UI点击等）
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}