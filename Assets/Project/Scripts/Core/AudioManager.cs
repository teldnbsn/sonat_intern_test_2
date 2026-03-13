using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgmSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip blockedClip;
    [SerializeField] private AudioClip slideClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip loseClip;
    [SerializeField] private AudioClip iceBreakClip;

    [Header("Background Music")]
    [SerializeField] private AudioClip gameplayBgm;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayBlocked()
    {
        PlaySfx(blockedClip);
    }

    public void PlaySlide()
    {
        PlaySfx(slideClip);
    }

    public void PlayWin()
    {
        PlaySfx(winClip);
    }

    public void PlayLose()
    {
        PlaySfx(loseClip);
    }

    public void PlayIceBreak()
    {
        PlaySfx(iceBreakClip);
    }

    public void StopSfx()
    {
        if (sfxSource != null)
        {
            sfxSource.Stop();
        }
    }

    public void PlayGameplayBgm()
    {
        if (bgmSource == null || gameplayBgm == null)
            return;

        if (bgmSource.clip == gameplayBgm && bgmSource.isPlaying)
            return;

        bgmSource.clip = gameplayBgm;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBgm()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
            return;

        sfxSource.PlayOneShot(clip);
    }
}