using System;
using System.Collections.Generic;
using PeduliTransit.Audio;
using UnityEngine;

namespace PeduliTransit.Audio
{
    [CreateAssetMenu(menuName = "PeduliTransit/Audio Library", fileName = "AudioLibrary")]
    public class AudioLibrary : ScriptableObject
    {
        [Serializable]
        public class BgmEntry
        {
            public BgmId id;
            public AudioClip clip;
        }

        [Serializable]
        public class SfxEntry
        {
            public SfxId id;
            public AudioClip clip;
            [Range(0f, 1f)] public float volumeScale = 1f;
        }

        public List<BgmEntry> bgm = new List<BgmEntry>();
        public List<SfxEntry> sfx = new List<SfxEntry>();

        public AudioClip GetBgm(BgmId id)
        {
            foreach (var e in bgm)
                if (e != null && e.id == id && e.clip != null)
                    return e.clip;
            return null;
        }

        public SfxEntry GetSfx(SfxId id)
        {
            foreach (var e in sfx)
                if (e != null && e.id == id && e.clip != null)
                    return e;
            return null;
        }
    }
}
