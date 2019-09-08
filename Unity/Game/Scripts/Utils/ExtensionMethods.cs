using UnityEngine;
using System.Collections.Generic;

public static class ExtensionMethods
{
    public static Transform[] GetChildren(this Transform transform)
    {
        Transform[] transforms = new Transform[transform.childCount];

        int i = 0;
        foreach (Transform childTransform in transform) transforms[i++] = childTransform;
        
        return transforms;
    }

    public static Transform[] GetAllChildren(this Transform transform)
    {
        return transform.gameObject.GetComponentsInChildren<Transform>();
    }

    public static List<GameObject> FindGameObjectChildrenWithPartialName(this GameObject gameObject, string partialName)
    {
        Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
        List<GameObject> list = new List<GameObject>();

        for (int i = 0; i < transforms.Length; i++)
        {
            if (!transforms[i].name.Contains(partialName)) continue;

            list.Add(transforms[i].gameObject);
        }

        return list;
    }
    
    public static GameObject FindGameObjectChildWithName(this GameObject gameObject, string name)
    {
        Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();

        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == name) return transforms[i].gameObject;
        }
        
        return null;
    }

    public static void Shake(this GameObject gameObject, float force = 10f, float jump = 9.5f, float easeOff = 1.6f, float time = 0.42f)
    {
        float height = Mathf.PerlinNoise(jump, 0f) * force;
        height = height * height * 0.3f;

        float shakeAmt = height * 0.2f;         // the degrees to shake
        float shakePeriodTime = time;           // The period of each shake
        float dropOffTime = easeOff;            // How long it takes the shaking to settle down to nothing
        LTDescr shakeTween = LeanTween.rotateAroundLocal(gameObject, Vector3.right, shakeAmt, shakePeriodTime)
            .setEase(LeanTweenType.easeShake) // this is a special ease that is good for shaking
            .setLoopClamp()
            .setRepeat(-1);

        // Slow the shake down to zero
        LeanTween.value(gameObject, shakeAmt, 0f, dropOffTime).setOnUpdate(
            (float val) => {
                shakeTween.setTo(Vector3.right * val);
            }
        ).setEase(LeanTweenType.easeOutQuad);
    }
}