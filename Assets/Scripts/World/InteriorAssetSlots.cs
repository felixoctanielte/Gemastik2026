using PeduliTransit.Core;
using UnityEngine;

namespace PeduliTransit.World
{

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
            var prefab = mode switch
            {
                TransportMode.Krl => krlInteriorPrefab,
                TransportMode.Bus => busInteriorPrefab,
                TransportMode.AngkutanUmum => angkutanInteriorPrefab,
                _ => null
            };

            if (prefab == null && mode == TransportMode.Bus)
                prefab = Resources.Load<GameObject>("BusWrapper");

            return prefab;
        }
    }
}
