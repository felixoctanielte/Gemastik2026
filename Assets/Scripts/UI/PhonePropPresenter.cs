using UnityEngine;

namespace PeduliTransit.UI
{
    /// <summary>
    /// Shows the cell_phone 3D asset in front of the camera while WhatsApp UI is open.
    /// </summary>
    public class PhonePropPresenter : MonoBehaviour
    {
        [SerializeField] GameObject phonePrefab;
        GameObject _instance;
        Transform _anchor;

        public void Configure(GameObject prefab)
        {
            phonePrefab = prefab;
        }

        public void Show(Camera cam)
        {
            if (cam == null)
                return;

            EnsureInstance(cam.transform);
            if (_instance == null)
                return;

            _instance.SetActive(true);
            _instance.transform.SetParent(cam.transform, false);
            _instance.transform.localPosition = new Vector3(0.22f, -0.18f, 0.55f);
            _instance.transform.localRotation = Quaternion.Euler(8f, 200f, 12f);
            _instance.transform.localScale = Vector3.one * 0.35f;
        }

        public void Hide()
        {
            if (_instance != null)
                _instance.SetActive(false);
        }

        void EnsureInstance(Transform parent)
        {
            if (_instance != null)
                return;

            var prefab = phonePrefab != null ? phonePrefab : TryLoadPhonePrefab();
            if (prefab == null)
                return;

            _instance = Instantiate(prefab, parent);
            _instance.name = "PhoneProp_Runtime";
            StripRuntimeJunk(_instance);
        }

        static void StripRuntimeJunk(GameObject go)
        {
            foreach (var col in go.GetComponentsInChildren<Collider>(true))
                Object.Destroy(col);

            foreach (var rb in go.GetComponentsInChildren<Rigidbody>(true))
                Object.Destroy(rb);

            foreach (var cam in go.GetComponentsInChildren<Camera>(true))
            {
                cam.enabled = false;
                Object.Destroy(cam);
            }

            foreach (var listener in go.GetComponentsInChildren<AudioListener>(true))
                Object.Destroy(listener);
        }

        public static GameObject TryLoadPhonePrefab()
        {
            var fromResources = Resources.Load<GameObject>("CellPhone");
            if (fromResources != null)
                return fromResources;

#if UNITY_EDITOR
            var editorAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/cell_phone.glb");
            if (editorAsset != null)
                return editorAsset;
#endif
            return null;
        }
    }
}
