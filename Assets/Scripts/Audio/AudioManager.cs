using PeduliTransit.Core;
using PeduliTransit.Data;
using PeduliTransit.Managers;
using UnityEngine;

namespace PeduliTransit.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] AudioLibrary library;

        AudioSource _bgm;
        AudioSource _sfx;
        AudioSource _ui;
        AudioSource _ambience;

        BgmId _currentBgm = BgmId.None;

        public static void EnsureExists()
        {
            if (Instance != null)
                return;

            var go = new GameObject("AudioManager");
            go.AddComponent<AudioManager>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (library == null)
                library = Resources.Load<AudioLibrary>("Audio/AudioLibrary");

            _bgm = CreateSource("BGM", true);
            _sfx = CreateSource("SFX", false);
            _ui = CreateSource("UI", false);
            _ambience = CreateSource("Ambience", true);

            ApplyVolumes();
        }

        AudioSource CreateSource(string name, bool loop)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            var src = child.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = loop;
            src.spatialBlend = 0f;
            return src;
        }

        GameSettingsData Settings =>
            GameManager.Instance != null ? GameManager.Instance.Settings : null;

        public void ApplyVolumes()
        {
            var s = Settings;
            float master = s != null ? Mathf.Clamp01(s.masterVolume) : 0.8f;
            float bgm = s != null ? Mathf.Clamp01(s.bgmVolume) : 0.7f;
            float sfx = s != null ? Mathf.Clamp01(s.sfxVolume) : 0.85f;
            float ui = s != null ? Mathf.Clamp01(s.uiVolume) : 0.9f;
            bool muted = s != null && s.muteAll;

            AudioListener.volume = muted ? 0f : master;

            if (_bgm != null) _bgm.volume = muted ? 0f : bgm;
            if (_ambience != null) _ambience.volume = muted ? 0f : bgm * 0.55f;
            if (_sfx != null) _sfx.volume = muted ? 0f : sfx;
            if (_ui != null) _ui.volume = muted ? 0f : ui;
        }

        public void PlayBgm(BgmId id, float fadeSeconds = 0.35f)
        {
            if (_currentBgm == id && _bgm != null && _bgm.isPlaying)
                return;

            _currentBgm = id;
            if (_bgm == null)
                return;

            var clip = ResolveBgm(id);
            if (clip == null)
            {
                _bgm.Stop();
                return;
            }

            _bgm.clip = clip;
            ApplyVolumes();
            _bgm.Play();
        }

        public void StopBgm()
        {
            _currentBgm = BgmId.None;
            if (_bgm != null)
                _bgm.Stop();
        }

        public void PlayBgmForMode(TransportMode mode)
        {
            PlayBgm(mode switch
            {
                TransportMode.Krl => BgmId.GameplayKrl,
                TransportMode.Bus => BgmId.GameplayBus,
                _ => BgmId.GameplayAngkot
            });
        }

        public void PlaySfx(SfxId id)
        {
            if (id == SfxId.None || _sfx == null)
                return;

            var entry = ResolveSfx(id);
            if (entry == null || entry.clip == null)
                return;

            ApplyVolumes();
            float scale = Mathf.Clamp01(entry.volumeScale);
            _sfx.PlayOneShot(entry.clip, scale);
        }

        public void PlayUi(SfxId id)
        {
            if (id == SfxId.None || _ui == null)
                return;

            var entry = ResolveSfx(id);
            if (entry == null || entry.clip == null)
                return;

            ApplyVolumes();
            _ui.PlayOneShot(entry.clip, Mathf.Clamp01(entry.volumeScale));
        }

        public void PlayDecisionFeedback(DecisionOutcome outcome)
        {
            switch (outcome)
            {
                case DecisionOutcome.Yes:
                case DecisionOutcome.Negur:
                    PlaySfx(SfxId.ScorePlus);
                    break;
                case DecisionOutcome.WrongReport:
                case DecisionOutcome.No:
                    PlaySfx(SfxId.ScoreMinus);
                    break;
                case DecisionOutcome.Timeout:
                    PlaySfx(SfxId.ScoreTimeout);
                    break;
                default:
                    PlayUi(SfxId.UiBack);
                    break;
            }
        }

        public void PlayIncidentAmbience(NpcRole role)
        {
            switch (role)
            {
                case NpcRole.LoudTalking:
                    PlaySfx(SfxId.LoudTalking);
                    break;
                case NpcRole.PhoneVolume:
                    PlaySfx(SfxId.PhoneSpeakerBuzz);
                    break;
                case NpcRole.HarassmentHint:
                    PlaySfx(SfxId.HarassmentTension);
                    break;
                case NpcRole.Fighting:
                    PlaySfx(SfxId.FightImpact);
                    break;
                default:
                    break;
            }
        }

        AudioClip ResolveBgm(BgmId id)
        {
            if (library != null)
            {
                var c = library.GetBgm(id);
                if (c != null) return c;
            }

            return Resources.Load<AudioClip>($"Audio/Bgm/{id}");
        }

        AudioLibrary.SfxEntry ResolveSfx(SfxId id)
        {
            if (library != null)
            {
                var e = library.GetSfx(id);
                if (e != null) return e;
            }

            var clip = Resources.Load<AudioClip>($"Audio/Sfx/{id}");
            if (clip == null)
                return null;

            return new AudioLibrary.SfxEntry { id = id, clip = clip, volumeScale = 1f };
        }
    }
}
