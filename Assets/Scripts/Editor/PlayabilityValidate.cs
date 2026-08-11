using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PeduliTransit.Editor
{

    public static class PlayabilityValidate
    {
        [MenuItem("PeduliTransit/Validate Playability")]
        public static void Validate()
        {
            int issues = 0;
            void Fail(string msg)
            {
                issues++;
                Debug.LogError("[Playability] " + msg);
            }

            void Ok(string msg) => Debug.Log("[Playability] OK: " + msg);

            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
                Fail("Buka SampleScene dulu.");
            else
                Ok("Scene aktif: " + scene.path);

            var slots = Object.FindObjectOfType<PeduliTransit.World.InteriorAssetSlots>();
            if (slots == null)
                Fail("InteriorAssetSlots tidak ditemukan di scene.");
            else
            {
                if (slots.krlInteriorPrefab == null)
                    Debug.LogWarning("[Playability] KRL prefab kosong — fallback procedural.");
                else Ok("KRL prefab terpasang");
                if (slots.busInteriorPrefab == null)
                    Debug.LogWarning("[Playability] Bus prefab kosong — coba Resources/BusWrapper.");
                else Ok("Bus prefab terpasang");
            }

            var edu = Resources.Load<Sprite>("UI/character_edu");
            if (edu == null)
            {
                var all = Resources.LoadAll<Sprite>("UI/character_edu");
                if (all == null || all.Length == 0)
                    Fail("Resources/UI/character_edu tidak load sebagai Sprite.");
                else Ok("character_edu sprites: " + all.Length);
            }
            else Ok("character_edu sprite load");

            var casual = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Casual1.prefab");
            if (casual == null)
                Fail("MC Casual1 tidak ditemukan di Assets/Prefabs/Casual1.prefab");
            else Ok("MC Casual1 prefab");

            var phone = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/cell_phone.glb");
            if (phone == null)
                Debug.LogWarning("[Playability] cell_phone.glb belum siap (Unity perlu import GLB).");
            else Ok("Phone asset cell_phone.glb");

            var busRes = Resources.Load<GameObject>("BusWrapper");
            if (busRes == null)
                Debug.LogWarning("[Playability] Resources/BusWrapper tidak ada (opsional fallback).");
            else Ok("Resources/BusWrapper");

            string[] requiredTypes =
            {
                "PeduliTransit.Bootstrap.GameBootstrap",
                "PeduliTransit.Events.EventDirector",
                "PeduliTransit.Player.FreeLookCamera",
                "PeduliTransit.Player.PlayerMotor",
                "PeduliTransit.UI.PhoneWhatsAppUI",
                "PeduliTransit.UI.PhonePropPresenter",
                "PeduliTransit.UI.UiAssets",
                "PeduliTransit.World.SeatSlot",
                "PeduliTransit.World.VehicleDoor"
            };

            foreach (var typeName in requiredTypes)
            {
                var t = System.Type.GetType(typeName + ", Assembly-CSharp");
                if (t == null)
                {

                    bool found = false;
                    foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
                    {
                        t = asm.GetType(typeName);
                        if (t != null) { found = true; break; }
                    }
                    if (!found) Fail("Type hilang: " + typeName);
                    else Ok(typeName);
                }
                else Ok(typeName);
            }

            if (issues == 0)
                EditorUtility.DisplayDialog("Playability", "Validasi lulus. Silakan Play SampleScene dan uji KRL + BUS.", "OK");
            else
                EditorUtility.DisplayDialog("Playability", issues + " isu ditemukan. Cek Console.", "OK");
        }
    }
}
