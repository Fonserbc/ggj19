using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Voice.PUN;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour {

    public GameSettings settings;
    public PhotonVoiceView voiceView;
    public AudioMixerGroup voiceGroup;
    public AudioMixer voiceMixer;
    public AnimationCurve volumeDropCurve;

    AudioSource audioSource = null;

    public float pitchFactor = 10f;

    float lastDistance = 0f;
    float maxDistance = 0f;

    public void Awake()
    {
        if (audioSource == null)
        {
            FindAudioSource();
        }
        maxDistance = settings.radiuses[0] + settings.radiuses[1];
    }

    private void Update()
    {
        if (audioSource == null)
        {
            FindAudioSource();
        }

        if (audioSource != null) {

            if (PlayerSync.otherPlayer != null) {
                float currentDistance = Vector3.Distance(PlayerSync.otherPlayer.dummyPlayerObject.transform.position, PlayerSync.localPlayer.localPlayerObject.transform.position);

                float speed = (currentDistance - lastDistance)/maxDistance;

                voiceMixer.SetFloat("ReceivedVoicePitch", Mathf.Clamp(speed * pitchFactor, -0.5f, 2f));
                voiceMixer.SetFloat("ReceivedVoiceVolume", volumeDropCurve.Evaluate(Mathf.Clamp01(currentDistance / maxDistance)));

                lastDistance = currentDistance;
            }
            else {
                voiceMixer.SetFloat("ReceivedVoicePitch", 1f);
                voiceMixer.SetFloat("ReceivedVoiceVolume", -80f);
            }
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
