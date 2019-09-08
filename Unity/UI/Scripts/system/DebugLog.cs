using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugLog {

    public static bool isEnabled = false;

    public static void Trace(string msg)
    {
        if (isEnabled) Debug.Log("[SB]" + msg);
    }
	
}
