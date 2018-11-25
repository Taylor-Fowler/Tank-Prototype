﻿using UnityEngine;

public enum SFX { Start, Running, Fire, Hit, Boom }

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{

    // All Music and SFX are purchased and Free-to-use licence
    // https://www.promusicpack.com/license.html
    // http://www.soundeffectpack.com/license.html

    public AudioClip BGMusic;
    public AudioClip[] SFXFiles = new AudioClip[5];

    private AudioSource[] sources;
    private int SFXChannels = 0;
    private int CurrSFXChannel = 1;

    /// <summary>
    /// DEVELOPER NOTE
    /// Loads up all AudioSources attached to parent object and polulates the sources[] array
    /// sources[0] is reserved for the BackGround Music (and loops)
    /// All others are assigned as SFX channels and used in sequence (as required, and don't loop)
    /// </summary>

    void Start()
    {
        sources = GetComponents<AudioSource>();
        SFXChannels = sources.Length - 1;
        Debug.Log("Sound Manager has " + SFXChannels.ToString() + " SFX Channels");
        // Set up Backing Music
        sources[0].clip = BGMusic;
        sources[0].loop = true;
    }

    public void PlayMusic()
    {
        sources[0].Play(0);
    }

    public void KillMusic()
    {
        sources[0].clip = null;
    }

    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0, 1f);
        sources[0].volume = volume;
    }

    public void PlaySFX(SFX choice)
    {
        sources[CurrSFXChannel].clip = SFXFiles[(int)choice];
        sources[CurrSFXChannel].loop = false;
        sources[CurrSFXChannel].Play(0);
        ToggleChannel();
    }


    public void KillSFX()
    {
        for (int i = 1; i <= SFXChannels; i++)
        {
            sources[i].clip = null;
        }
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0, 1f);
        for (int i = 1; i <= SFXChannels; i++)
        {
            sources[i].volume = volume;
        }
    }

    public void KillAllSounds()
    {
        KillMusic();
        KillSFX();
    }

    private void ToggleChannel()
    {
        if (CurrSFXChannel == SFXChannels) { CurrSFXChannel = 1; }
        else { CurrSFXChannel++; }
    }
   
}