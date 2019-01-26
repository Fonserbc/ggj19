using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Voice.PUN;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour {

    public PhotonVoiceView voiceView;
    public AudioMixerGroup voiceGroup;

    AudioSource audioSource = null;

    public void Awake()
    {
        if (audioSource == null)
        {
            FindAudioSource();
        }
    }

    private void Update()
    {
        if (audioSource == null)
        {
            FindAudioSource();
        }

        if (audioSource != null) {
            
        }
    }

    void FindAudioSource()
    {
        audioSource = voiceView.GetComponent<AudioSource>();

        if (audioSource != null) {
            audioSource.outputAudioMixerGroup = voiceGroup;
        }
    }
}
