using UnityEngine;

namespace PeduliTransit.UI
{
    public static class UiAssets
    {
        static Sprite _eduPortrait;

        public static Sprite EduPortrait
        {
            get
            {
                if (_eduPortrait != null)
                    return _eduPortrait;

                _eduPortrait = Resources.Load<Sprite>("UI/character_edu");
                if (_eduPortrait == null)
                {
                    var all = Resources.LoadAll<Sprite>("UI/character_edu");
                    if (all != null && all.Length > 0)
                        _eduPortrait = all[0];
                }

                return _eduPortrait;
            }
        }
    }
}
