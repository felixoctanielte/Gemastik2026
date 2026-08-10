using UnityEngine;

namespace PeduliTransit.UI
{
    public class CameraControlsSceneHint : MonoBehaviour
    {
        [TextArea(3, 6)]
        public string controls =
            "Kamera (FreeLook):\n" +
            "• Tahan KLIK KANAN + geser mouse = lihat sekitar\n" +
            "• WASD = geser\n" +
            "• Scroll = zoom\n" +
            "• Q / E = naik / turun";
    }
}
