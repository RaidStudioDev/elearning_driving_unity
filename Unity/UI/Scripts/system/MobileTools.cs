using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileTools {

    public static bool IsMobile
    {
        get
        {
            return (Application.platform == RuntimePlatform.IPhonePlayer
              || Application.platform == RuntimePlatform.Android);
        }
    }
}
