#if (UNITY_IOS && !UNITY_EDITOR) || __IOS__
#define DLL_IMPORT_INTERNAL
#endif

using System;
using UnityEngine;

namespace Photon.Voice.Unity
{
    [RequireComponent(typeof(Recorder))]
    public class WebRtcAudioDsp : VoiceComponent
    {
        #region Private Fields

        [SerializeField]
        private bool aec = true;

        [SerializeField]
        private bool aecMobile;

        [SerializeField]
        private bool agc = true;

        [SerializeField]
        private bool vad = true;

        [SerializeField]
        private bool highPass;

        [SerializeField]
        private bool bypass;

        [SerializeField]
        private bool noiseSuppression;

        [SerializeField]
        private int reverseStreamDelayMs = 120;

        private int reverseChannels;
        private WebRTCAudioProcessor proc;

        private AudioOutCapture ac;
        private bool started;

        #endregion

        #region Properties

        public bool AEC
        {
            get { return aec; }
            set
            {
                if (value == aec)
                {
                    return;
                }
                if (value)
                {
                    aecMobile = false;
                }
                aec = value;
                if (proc != null)
                {
                    proc.AEC = aec;
                    proc.AECMobile = aecMobile;
                }
                ToggleOutputListener();
            }
        }

        public bool AECMobile // echo control mobile
        {
            get { return aecMobile; }
            set
            {
                if (value == aecMobile)
                {
                    return;
                }
                if (value)
                {
                    aec = false;
                }
                aec = value;
                if (proc != null)
                {
                    proc.AEC = aec;
                    proc.AECMobile = aecMobile;
                }
                ToggleOutputListener();
            }
        }

        public int ReverseStreamDelayMs
        {
            get { return reverseStreamDelayMs; }
            set
            {
                if (reverseStreamDelayMs == value)
                {
                    return;
                }
                reverseStreamDelayMs = value;
                if (proc != null)
                {
                    proc.AECStreamDelayMs = ReverseStreamDelayMs;
                } 
            }
        }

        public bool NoiseSuppression
        {
            get { return noiseSuppression; }
            set
            {
                if (value == noiseSuppression)
                {
                    return;
                }
                noiseSuppression = value;
                if (proc != null)
                {
                    proc.NoiseSuppression = noiseSuppression;
                }
            }
        }

        public bool HighPass
        {
            get { return highPass; }
            set
            {
                if (value == highPass)
                {
                    return;
                }
                highPass = value;
                if (proc != null)
                {
                    proc.HighPass = highPass;
                }
            }
        }

        public bool Bypass
        {
            get { return bypass; }
            set
            {
                if (value == bypass)
                {
                    return;
                }
                bypass = value;
                if (proc != null)
                {
                    proc.Bypass = bypass;
                }
            }
        }

        public bool AGC
        {
            get { return agc; }
            set
            {
                if (value == agc)
                {
                    return;
                }
                agc = value;
                if (proc != null)
                {
                    proc.AGC = agc;
                }
            }
        }

        public bool VAD
        {
            get { return vad; }
            set
            {
                if (value == vad)
                {
                    return;
                }
                vad = value;
                if (proc != null)
                {
                    proc.VAD = vad;
                }
            }
        }

        #endregion

        #region Private Methods

        protected override void Awake()
        {
            base.Awake();
            AudioListener audioListener = FindObjectOfType<AudioListener>();
            if (audioListener != null)
            {
                ac = audioListener.gameObject.GetComponent<AudioOutCapture>();
                if (ac == null)
                {
                    ac = audioListener.gameObject.AddComponent<AudioOutCapture>();
                }
            }
            else if (this.Logger.IsErrorEnabled)
            {
                this.Logger.LogError("AudioListener component is required");
            }
        }

        private void OnEnable()
        {
            ToggleOutputListener();
        }

        private void OnDisable()
        {
            ToggleOutputListener(false);
        }

        private void ToggleOutputListener()
        {
            ToggleOutputListener(aec || aecMobile);
        }

        private void ToggleOutputListener(bool on)
        {
            if (ac != null && started != on && proc != null)
            {
                if (on)
                {
                    started = true;
                    ac.OnAudioFrame += OnAudioOutFrameFloat;
                }
                else
                {
                    started = false;
                    ac.OnAudioFrame -= OnAudioOutFrameFloat;
                }
            }
        }

        private void OnAudioOutFrameFloat(float[] data, int outChannels)
        {
            if (outChannels != this.reverseChannels)
            {
                if (this.Logger.IsErrorEnabled)
                {
                    this.Logger.LogError("OnAudioOutFrame channel count {0} != initialized {1}.", outChannels, this.reverseChannels);
                }
                return;
            }
            proc.OnAudioOutFrameFloat(data);
        }

        // Message sent by Recorder
        private void PhotonVoiceCreated(Recorder.PhotonVoiceCreatedParams p)
        {
            LocalVoice localVoice = p.Voice;

            if (localVoice.Info.Channels != 1)
            {
                throw new Exception("WebRTCAudioProcessor: only mono audio signals supported.");
            }
            if (!(localVoice is LocalVoiceAudioShort))
            {
                throw new Exception("WebRTCAudioProcessor: only short audio voice supported (Set Recorder.TypeConvert option).");
            }
            LocalVoiceAudioShort v = localVoice as LocalVoiceAudioShort;

            this.reverseChannels = (int) AudioSettings.speakerMode;
            proc = new WebRTCAudioProcessor(this.Logger, localVoice.Info.FrameSize, localVoice.Info.SamplingRate,
                localVoice.Info.Channels, AudioSettings.outputSampleRate, this.reverseChannels);
            proc.AEC = AEC;
            proc.AECMobile = AECMobile;
            proc.AECMRoutingMode = 4;
            proc.AECStreamDelayMs = ReverseStreamDelayMs;
            proc.HighPass = HighPass;
            proc.NoiseSuppression = NoiseSuppression;
            proc.AGC = AGC;
            proc.VAD = VAD;
            proc.Bypass = Bypass;
            v.AddPostProcessor(proc);
            ToggleOutputListener();
            if (this.Logger.IsInfoEnabled)
            {
                this.Logger.LogInfo("Initialized");
            }
        }

        private void PhotonVoiceRemoved()
        {
            Reset();
        }

        private void OnDestroy()
        {
            Reset();
        }

        private void Reset()
        {
            ToggleOutputListener(false);
            if (proc != null)
            {
                proc.Dispose();
                proc = null;
            }
        }

        #endregion
    }
}