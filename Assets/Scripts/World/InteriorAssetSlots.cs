using PeduliTransit.Core;
using UnityEngine;

namespace PeduliTransit.World
{
    /// <summary>
    /// Slot supaya teman bisa drag prefab interior (Sketchfab/FBX) di Inspector.
    /// Kosong = pakai interior primitif procedural.
    /// </summary>
    public class InteriorAssetSlots : MonoBehaviour
    {
        [Header("Drag prefab interior di sini nanti")]
        public GameObject krlInteriorPrefab;
        public GameObject busInteriorPrefab;
        public GameObject angkutanInteriorPrefab;

        [Header("Anchor spawn")]
        public Transform interiorAnchor;
        public Transform playerSpawn;

        public GameObject GetPrefab(TransportMode mode)
        {
            return mode switch
            {
                TransportMode.Krl => krlInteriorPrefab,
                TransportMode.Bus => busInteriorPrefab,
                TransportMode.AngkutanUmum => angkutanInteriorPrefab,
                _ => null
            };
        }
    }
}
