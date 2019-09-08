using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIScreen:MonoBehaviour {

    public abstract void PreInitialize();
    public abstract void Initialize(string id);
    public abstract void Load();
    public abstract void Draw();
    public abstract void Remove();

    public abstract void OpenLoadingPanel();
    public abstract void CloseLoadingPanel();
    public abstract void CloseLoadingPanelComplete();
}
