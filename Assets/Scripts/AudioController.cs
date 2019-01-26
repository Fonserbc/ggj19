﻿using System.Collections;
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
    public FMODUnity.StudioEventEmitter fmodEventEmmiter;

    float fmodPolarity = 0f;
    float fmodDistance = 0f;

    public bool doPitchDistortion = false;
    public bool doVolumeDrop = false;

    AudioSource audioSource = null;

    public float pitchFactor = 10f;

    public float minWinTime = 4f;
    bool won = false;
    float winTime = 0f;

    float lastDistance = 0f;
    float maxDistance = 0f;

    public void Awake()
    {
        if (audioSource == null)
        {
            FindAudioSource();
        }
        maxDistance = settings.innerRadius + settings.outerRadius;
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

                float distanceFactor = (currentDistance - lastDistance) / maxDistance;
                float speed = distanceFactor * pitchFactor;
                float softFactor = 0.9f;
                softenedSpeed = softenedSpeed * softFactor + speed * (1f - softFactor);

                minSpeed = Mathf.Min(minSpeed, softenedSpeed);
                maxSpeed = Mathf.Max(maxSpeed, softenedSpeed);

                fmodPolarity = Mathf.Sign(softenedSpeed);
                fmodEventEmmiter.SetParameter("Polarity", fmodPolarity);
                fmodDistance = Mathf.Clamp01(currentDistance / maxDistance) * 100f + 1f;

                if (fmodDistance < 2f + Mathf.Abs(settings.innerRadius - settings.outerRadius)) { // 2 is because we are +1 up there
                    winTime += Time.deltaTime;

                    if (winTime >= minWinTime) {
                        Debug.Log("WIN!");
                        PlayerSync.localPlayer.ownState.won = true;
                    }
                }
                else {
                    winTime = 0f;
                    PlayerSync.localPlayer.ownState.won = false;
                }

                fmodEventEmmiter.SetParameter("Distance", fmodDistance);

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

                fmodEventEmmiter.SetParameter("Distance", 100f);
                fmodEventEmmiter.SetParameter("Polarity", 1);
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
