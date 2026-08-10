using PeduliTransit.Audio;
using UnityEditor;
using UnityEngine;

namespace PeduliTransit.Editor
{
    public static class AudioSetupMenu
    {
        const string LibraryPath = "Assets/Resources/Audio/AudioLibrary.asset";

        [MenuItem("PeduliTransit/Create Audio Library Asset")]
        public static void CreateLibrary()
        {
            EnsureFolders();
            var existing = AssetDatabase.LoadAssetAtPath<AudioLibrary>(LibraryPath);
            if (existing != null)
            {
                Selection.activeObject = existing;
                EditorUtility.DisplayDialog("Audio", "AudioLibrary sudah ada di Resources/Audio.", "OK");
                return;
            }

            var asset = ScriptableObject.CreateInstance<AudioLibrary>();
            foreach (BgmId id in System.Enum.GetValues(typeof(BgmId)))
            {
                if (id == BgmId.None) continue;
                asset.bgm.Add(new AudioLibrary.BgmEntry { id = id });
            }

            foreach (SfxId id in System.Enum.GetValues(typeof(SfxId)))
            {
                if (id == SfxId.None) continue;
                asset.sfx.Add(new AudioLibrary.SfxEntry { id = id, volumeScale = 1f });
            }

            AssetDatabase.CreateAsset(asset, LibraryPath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            EditorUtility.DisplayDialog("Audio",
                "AudioLibrary dibuat. Drag AudioClip ke slot BGM/SFX, atau taruh file di Resources/Audio/Bgm & Sfx dengan nama enum.",
                "OK");
        }

        static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Audio"))
                AssetDatabase.CreateFolder("Assets/Resources", "Audio");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Audio/Bgm"))
                AssetDatabase.CreateFolder("Assets/Resources/Audio", "Bgm");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Audio/Sfx"))
                AssetDatabase.CreateFolder("Assets/Resources/Audio", "Sfx");
        }
    }
}
