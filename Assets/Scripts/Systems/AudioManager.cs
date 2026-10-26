using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArmyLife.Systems
{
    /// <summary>
    /// Studio-grade Audio Manager for Army Life Simulator.
    /// Provides responsive tactical sound effects, military radio squelch,
    /// ceremonial bugle fanfares, drill whistles, weapon discharges, and background marching cadence.
    /// Includes built-in procedural audio synthesis so the game is 100% self-contained with zero missing assets.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private const string PREF_MASTER_VOL = "ArmyLife_MasterVol";
        private const string PREF_SFX_VOL = "ArmyLife_SFXVol";
        private const string PREF_BGM_VOL = "ArmyLife_BgmVol";

        [Header("Volume Configuration")]
        [Range(0f, 1f)] public float masterVolume = 0.85f;
        [Range(0f, 1f)] public float sfxVolume = 0.85f;
        [Range(0f, 1f)] public float bgmVolume = 0.65f;

        [Header("Audio Sources")]
        private AudioSource bgmSource;
        private readonly List<AudioSource> sfxPool = new List<AudioSource>();
        private const int SFX_POOL_SIZE = 6;
        private int currentSfxIndex = 0;

        // Clip Cache
        private readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                InitializeAudioEngine();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Auto-start ambient cadence music if desired
            if (bgmSource != null && !bgmSource.isPlaying)
            {
                PlayBgmCadence();
            }
        }

        /// <summary>
        /// Ensures an AudioManager instance exists even in unit tests or mock scenes.
        /// </summary>
        public static AudioManager EnsureInstance()
        {
            if (Instance == null)
            {
                var existing = FindAnyObjectByType<AudioManager>(FindObjectsInactive.Include);
                if (existing != null)
                {
                    Instance = existing;
                }
                else
                {
                    var go = new GameObject("[AudioManager]");
                    Instance = go.AddComponent<AudioManager>();
                }
            }
            return Instance;
        }

        private void InitializeAudioEngine()
        {
            // Load saved player preferences
            masterVolume = PlayerPrefs.GetFloat(PREF_MASTER_VOL, 0.85f);
            sfxVolume = PlayerPrefs.GetFloat(PREF_SFX_VOL, 0.85f);
            bgmVolume = PlayerPrefs.GetFloat(PREF_BGM_VOL, 0.65f);

            // Setup BGM AudioSource
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.spatialBlend = 0f; // 2D UI Audio
            bgmSource.volume = masterVolume * bgmVolume;

            // Setup SFX Pool
            for (int i = 0; i < SFX_POOL_SIZE; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.loop = false;
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                src.volume = masterVolume * sfxVolume;
                sfxPool.Add(src);
            }

            // Pre-warm audio cache with procedural audio clips
            PrewarmAudioClips();
        }

        private void PrewarmAudioClips()
        {
            GetOrCreateClip("UiClick", GenerateUiClickClip);
            GetOrCreateClip("TurnAdvance", GenerateTurnAdvanceClip);
            GetOrCreateClip("RadioSquelch", GenerateRadioSquelchClip);
            GetOrCreateClip("RifleShot", GenerateRifleShotClip);
            GetOrCreateClip("DrillWhistle", GenerateDrillWhistleClip);
            GetOrCreateClip("BugleFanfare", GenerateBugleFanfareClip);
            GetOrCreateClip("Taps", GenerateTapsClip);
            GetOrCreateClip("Heartbeat", GenerateHeartbeatClip);
            GetOrCreateClip("AlertPing", GenerateAlertPingClip);
            GetOrCreateClip("Payday", GeneratePaydayClip);
            GetOrCreateClip("CadenceBgm", GenerateCadenceBgmClip);
        }

        private AudioClip GetOrCreateClip(string clipKey, Func<AudioClip> generator)
        {
            if (clipCache.TryGetValue(clipKey, out var clip) && clip != null)
            {
                return clip;
            }

            // Check if user has an external audio asset in Resources/Audio/
            var external = Resources.Load<AudioClip>($"Audio/{clipKey}");
            if (external != null)
            {
                clipCache[clipKey] = external;
                return external;
            }

            // Otherwise, synthesize procedural clip
            var generated = generator();
            clipCache[clipKey] = generated;
            return generated;
        }

        // ====================================================================
        // PUBLIC PLAYBACK API
        // ====================================================================

        public static void PlayUiClick(float pitchVariance = 0.05f)
        {
            EnsureInstance()?.PlaySoundInternal("UiClick", 0.6f, pitchVariance);
        }

        public static void PlayTurnAdvance()
        {
            EnsureInstance()?.PlaySoundInternal("TurnAdvance", 0.85f, 0.02f);
        }

        public static void PlayRadioSquelch()
        {
            EnsureInstance()?.PlaySoundInternal("RadioSquelch", 0.75f, 0.03f);
        }

        public static void PlayRifleShot()
        {
            EnsureInstance()?.PlaySoundInternal("RifleShot", 0.95f, 0.08f);
        }

        public static void PlayDrillWhistle()
        {
            EnsureInstance()?.PlaySoundInternal("DrillWhistle", 0.80f, 0.02f);
        }

        public static void PlayBugleFanfare()
        {
            EnsureInstance()?.PlaySoundInternal("BugleFanfare", 0.90f, 0.0f);
        }

        public static void PlayTaps()
        {
            EnsureInstance()?.PlaySoundInternal("Taps", 0.80f, 0.0f);
        }

        public static void PlayHeartbeat()
        {
            EnsureInstance()?.PlaySoundInternal("Heartbeat", 0.90f, 0.03f);
        }

        public static void PlayAlertPing()
        {
            EnsureInstance()?.PlaySoundInternal("AlertPing", 0.75f, 0.02f);
        }

        public static void PlayPayday()
        {
            EnsureInstance()?.PlaySoundInternal("Payday", 0.75f, 0.03f);
        }

        public static void PlayBgmCadence()
        {
            var mgr = EnsureInstance();
            if (mgr == null || mgr.bgmSource == null) return;

            var clip = mgr.GetOrCreateClip("CadenceBgm", mgr.GenerateCadenceBgmClip);
            if (clip != null && mgr.bgmSource.clip != clip)
            {
                mgr.bgmSource.clip = clip;
                mgr.bgmSource.volume = mgr.masterVolume * mgr.bgmVolume;
                mgr.bgmSource.Play();
            }
        }

        public static void StopBgm()
        {
            if (Instance != null && Instance.bgmSource != null)
            {
                Instance.bgmSource.Stop();
            }
        }

        private void PlaySoundInternal(string clipName, float volumeScale = 1.0f, float pitchVariance = 0.0f)
        {
            if (sfxPool.Count == 0) return;

            AudioClip clip = null;
            if (!clipCache.TryGetValue(clipName, out clip) || clip == null)
            {
                PrewarmAudioClips();
                clipCache.TryGetValue(clipName, out clip);
            }
            if (clip == null) return;

            // Pick next pooled source round-robin
            AudioSource src = sfxPool[currentSfxIndex];
            currentSfxIndex = (currentSfxIndex + 1) % sfxPool.Count;

            src.pitch = 1.0f + (pitchVariance > 0 ? UnityEngine.Random.Range(-pitchVariance, pitchVariance) : 0f);
            src.volume = Mathf.Clamp01(masterVolume * sfxVolume * volumeScale);
            src.PlayOneShot(clip);
        }

        // ====================================================================
        // VOLUME SETTINGS API
        // ====================================================================

        public void SetMasterVolume(float vol)
        {
            masterVolume = Mathf.Clamp01(vol);
            PlayerPrefs.SetFloat(PREF_MASTER_VOL, masterVolume);
            PlayerPrefs.Save();
            UpdateAllVolumes();
        }

        public void SetSfxVolume(float vol)
        {
            sfxVolume = Mathf.Clamp01(vol);
            PlayerPrefs.SetFloat(PREF_SFX_VOL, sfxVolume);
            PlayerPrefs.Save();
            UpdateAllVolumes();
        }

        public void SetBgmVolume(float vol)
        {
            bgmVolume = Mathf.Clamp01(vol);
            PlayerPrefs.SetFloat(PREF_BGM_VOL, bgmVolume);
            PlayerPrefs.Save();
            UpdateAllVolumes();
        }

        private void UpdateAllVolumes()
        {
            if (bgmSource != null)
            {
                bgmSource.volume = masterVolume * bgmVolume;
            }
            foreach (var s in sfxPool)
            {
                if (s != null) s.volume = masterVolume * sfxVolume;
            }
        }

        // ====================================================================
        // PROCEDURAL AUDIO SYNTHESIZERS
        // Pure mathematical synthesis generating zero-dependency 44.1kHz clips
        // ====================================================================

        private AudioClip GenerateUiClickClip()
        {
            int sampleRate = 44100;
            float duration = 0.035f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-120f * t);
                float freq = Mathf.Lerp(1200f, 850f, t / duration);
                float sine = Mathf.Sin(2f * Mathf.PI * freq * t);
                float noise = (UnityEngine.Random.value * 2f - 1f) * 0.12f;
                samples[i] = (sine + noise) * env * 0.7f;
            }

            AudioClip clip = AudioClip.Create("Procedural_UiClick", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateTurnAdvanceClip()
        {
            int sampleRate = 44100;
            float duration = 0.22f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                // Low-end desk stamp thump
                float thumpEnv = Mathf.Exp(-22f * t);
                float thumpFreq = Mathf.Lerp(75f, 40f, t / duration);
                float thump = Mathf.Sin(2f * Mathf.PI * thumpFreq * t) * thumpEnv * 0.75f;

                // Paper / mechanical rustle
                float paperEnv = Mathf.Exp(-45f * t);
                float paperNoise = (UnityEngine.Random.value * 2f - 1f) * paperEnv * 0.35f;

                samples[i] = Mathf.Clamp(thump + paperNoise, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Procedural_TurnAdvance", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateRadioSquelchClip()
        {
            int sampleRate = 44100;
            float duration = 0.28f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float val = 0f;

                // Intro DTMF chirp (0.00s to 0.04s)
                if (t < 0.04f)
                {
                    float chirp = (Mathf.Sin(2f * Mathf.PI * 941f * t) + Mathf.Sin(2f * Mathf.PI * 1336f * t)) * 0.25f;
                    val += chirp;
                }
                // Radio atmospheric static (0.03s to 0.24s)
                if (t >= 0.03f && t < 0.24f)
                {
                    float staticEnv = (t < 0.06f) ? (t - 0.03f) / 0.03f : (t > 0.20f) ? (0.24f - t) / 0.04f : 1.0f;
                    // Filtered noise simulating tactical FM radio squelch
                    float rawNoise = UnityEngine.Random.value * 2f - 1f;
                    float squelchMod = Mathf.Sin(2f * Mathf.PI * 1200f * t) * 0.15f;
                    val += (rawNoise * 0.45f + squelchMod) * staticEnv;
                }
                // Mic unkey click (0.24s to 0.28s)
                if (t >= 0.24f)
                {
                    float clickT = t - 0.24f;
                    val += Mathf.Sin(2f * Mathf.PI * 600f * clickT) * Mathf.Exp(-90f * clickT) * 0.4f;
                }

                samples[i] = Mathf.Clamp(val * 0.8f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Procedural_RadioSquelch", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateRifleShotClip()
        {
            int sampleRate = 44100;
            float duration = 0.45f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;

                // 1. Initial supersonic muzzle crack (sharp attack noise)
                float crackEnv = Mathf.Exp(-110f * t);
                float crack = (UnityEngine.Random.value * 2f - 1f) * crackEnv * 0.9f;

                // 2. 120Hz barrel blast thump
                float thumpEnv = Mathf.Exp(-20f * t);
                float thump = Mathf.Sin(2f * Mathf.PI * 120f * t) * thumpEnv * 0.65f;

                // 3. Reverberation echo tail
                float echoEnv = Mathf.Exp(-7f * t);
                float echoNoise = (UnityEngine.Random.value * 2f - 1f) * echoEnv * 0.18f;

                samples[i] = Mathf.Clamp(crack + thump + echoNoise, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Procedural_RifleShot", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateDrillWhistleClip()
        {
            int sampleRate = 44100;
            float duration = 0.45f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;

                // Whistle envelope: rapid attack (0.02s) and steady sustained release
                float env = (t < 0.02f) ? t / 0.02f : (t > 0.38f) ? (duration - t) / 0.07f : 1.0f;

                // Dual resonant whistle frequencies with pea oscillation flutter (24Hz)
                float peaFlutter = 1.0f + 0.35f * Mathf.Sin(2f * Mathf.PI * 24f * t);
                float tone1 = Mathf.Sin(2f * Mathf.PI * 2750f * t);
                float tone2 = Mathf.Sin(2f * Mathf.PI * 3100f * t);

                samples[i] = Mathf.Clamp((tone1 + tone2) * 0.38f * peaFlutter * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Procedural_DrillWhistle", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateBugleFanfareClip()
        {
            int sampleRate = 44100;
            float duration = 2.0f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            // Sequence of brass fanfare notes (Start, End, Freq)
            (float start, float end, float freq)[] notes = new (float, float, float)[]
            {
                (0.00f, 0.22f, 392.00f), // G4
                (0.24f, 0.46f, 523.25f), // C5
                (0.48f, 0.70f, 659.25f), // E5
                (0.72f, 1.10f, 783.99f), // G5
                (1.12f, 1.95f, 1046.50f) // High C6 (Fanfare Resolve)
            };

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float sampleVal = 0f;

                foreach (var note in notes)
                {
                    if (t >= note.start && t <= note.end)
                    {
                        float noteT = t - note.start;
                        float noteDur = note.end - note.start;
                        // ADSR envelope
                        float env = (noteT < 0.02f) ? noteT / 0.02f : (noteT > noteDur - 0.05f) ? (noteDur - noteT) / 0.05f : 1.0f;

                        // Brass harmonics (fundamental + 2nd + 3rd harmonic)
                        float f = note.freq;
                        float brass = Mathf.Sin(2f * Mathf.PI * f * noteT) * 0.55f +
                                      Mathf.Sin(2f * Mathf.PI * (f * 2f) * noteT) * 0.28f +
                                      Mathf.Sin(2f * Mathf.PI * (f * 3f) * noteT) * 0.12f;

                        sampleVal += brass * env;
                    }
                }

                samples[i] = Mathf.Clamp(sampleVal * 0.75f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Procedural_BugleFanfare", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateTapsClip()
        {
            int sampleRate = 44100;
            float duration = 2.8f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            // Taps opening phrase (Solemn tribute for fallen comrades)
            (float start, float end, float freq)[] notes = new (float, float, float)[]
            {
                (0.00f, 0.60f, 196.00f), // G3
                (0.65f, 1.25f, 261.63f), // C4
                (1.30f, 2.70f, 329.63f)  // E4 (Sustained Memorial)
            };

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float sampleVal = 0f;

                foreach (var note in notes)
                {
                    if (t >= note.start && t <= note.end)
                    {
                        float noteT = t - note.start;
                        float noteDur = note.end - note.start;
                        float env = (noteT < 0.08f) ? noteT / 0.08f : (noteT > noteDur - 0.15f) ? (noteDur - noteT) / 0.15f : 1.0f;

                        float f = note.freq;
                        float tone = Mathf.Sin(2f * Mathf.PI * f * noteT) * 0.60f +
                                     Mathf.Sin(2f * Mathf.PI * (f * 2f) * noteT) * 0.20f;

                        sampleVal += tone * env;
                    }
                }

                samples[i] = Mathf.Clamp(sampleVal * 0.70f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Procedural_Taps", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateHeartbeatClip()
        {
            int sampleRate = 44100;
            float duration = 0.85f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float val = 0f;

                // "Lub" thump at 0.05s
                if (t >= 0.05f && t < 0.25f)
                {
                    float dt = t - 0.05f;
                    val += Mathf.Sin(2f * Mathf.PI * 58f * dt) * Mathf.Exp(-20f * dt) * 0.8f;
                }
                // "Dub" thump at 0.30s
                if (t >= 0.30f && t < 0.50f)
                {
                    float dt = t - 0.30f;
                    val += Mathf.Sin(2f * Mathf.PI * 50f * dt) * Mathf.Exp(-22f * dt) * 0.65f;
                }

                samples[i] = Mathf.Clamp(val, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Procedural_Heartbeat", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateAlertPingClip()
        {
            int sampleRate = 44100;
            float duration = 0.45f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env = Mathf.Exp(-9f * t);
                float tone = Mathf.Sin(2f * Mathf.PI * 1760f * t) * 0.6f +
                             Mathf.Sin(2f * Mathf.PI * 3520f * t) * 0.2f;
                samples[i] = Mathf.Clamp(tone * env, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Procedural_AlertPing", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GeneratePaydayClip()
        {
            int sampleRate = 44100;
            float duration = 0.35f;
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float env1 = Mathf.Exp(-14f * t);
                float tone1 = Mathf.Sin(2f * Mathf.PI * 2800f * t) * env1 * 0.5f;

                float tone2 = 0f;
                if (t >= 0.07f)
                {
                    float dt = t - 0.07f;
                    tone2 = Mathf.Sin(2f * Mathf.PI * 3700f * dt) * Mathf.Exp(-16f * dt) * 0.45f;
                }

                samples[i] = Mathf.Clamp(tone1 + tone2, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Procedural_Payday", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateCadenceBgmClip()
        {
            int sampleRate = 44100;
            float duration = 4.0f; // 4-second seamless loop at 120 BPM (0.5s per step)
            int totalSamples = (int)(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float val = 0f;

                // Military marching cadence: footsteps every 0.5s (8 beats)
                float beatInterval = 0.5f;
                float stepT = t % beatInterval;

                // Low bass footstep thump on every beat
                float stepThump = Mathf.Sin(2f * Mathf.PI * 65f * stepT) * Mathf.Exp(-24f * stepT) * 0.45f;
                // Subtle boot-on-gravel noise
                float gravelNoise = (UnityEngine.Random.value * 2f - 1f) * Mathf.Exp(-35f * stepT) * 0.15f;
                val += stepThump + gravelNoise;

                // Snare accent on alternating beats (2, 4, 6, 8)
                int beatIndex = Mathf.FloorToInt(t / beatInterval);
                if (beatIndex % 2 == 1)
                {
                    float snareNoise = (UnityEngine.Random.value * 2f - 1f) * Mathf.Exp(-28f * stepT) * 0.22f;
                    val += snareNoise;
                }

                // Low ambient drone pad
                float ambientDrone = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.08f;
                val += ambientDrone;

                samples[i] = Mathf.Clamp(val * 0.55f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("Procedural_CadenceBgm", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
