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

    public bool doPitchDistortion = false;
    public bool doVolumeDrop = false;

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

    float minSpeed = Mathf.Infinity;
    float maxSpeed = Mathf.NegativeInfinity;
    float softenedSpeed = 0f;

    private void Update()
    {
        if (audioSource == null)
        {
            FindAudioSource();
        }

        if (audioSource != null) {

            if (PlayerSync.otherPlayer != null) {
                float currentDistance = Vector3.Distance(PlayerSync.otherPlayer.dummyPlayerObject.transform.position, PlayerSync.localPlayer.localPlayerObject.transform.position);

                float speed = (currentDistance - lastDistance)/maxDistance * pitchFactor;
                float softFactor = 0.9f;
                softenedSpeed = softenedSpeed * softFactor + speed * (1f - softFactor);

                minSpeed = Mathf.Min(minSpeed, softenedSpeed);
                maxSpeed = Mathf.Max(maxSpeed, softenedSpeed);

                //if (softenedSpeed > 0 && speed > softenedSpeed) softenedSpeed = speed;
                //else if (softenedSpeed < 0 && speed < softenedSpeed) softenedSpeed = speed;

                //Debug.Log(speed+" "+ softenedSpeed+" min: "+minSpeed+" max: "+maxSpeed);

                if (doPitchDistortion)
                {
                    voiceMixer.SetFloat("ReceivedVoicePitch", Mathf.Lerp(0.5f, 2f, 1f - Mathf.InverseLerp(-0.7f, 0.7f, softenedSpeed)));
                }
                else {
                    voiceMixer.SetFloat("ReceivedVoicePitch", 1f);
                }

                if (doVolumeDrop)
                {
                    voiceMixer.SetFloat("ReceivedVoiceVolume", volumeDropCurve.Evaluate(Mathf.Clamp01(currentDistance / maxDistance)));
                }
                else {
                    voiceMixer.SetFloat("ReceivedVoiceVolume", 0f);
                }

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
        audioSource = voiceView.SpeakerInUse.gameObject.GetComponent<AudioSource>();

        if (audioSource != null) {
            audioSource.outputAudioMixerGroup = voiceGroup;
        }
    }
}
