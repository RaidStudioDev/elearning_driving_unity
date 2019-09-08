using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CameraDrops : MonoBehaviour
{
    private Material material;
    public float intensity = 1;
    public float aspectU = 2;
    public float aspectV = 1;
    public float speed = 0.25f;
    public float size = 1;
    public float distortion = 1;
    public float time = 1;

    // Creates a private material used to the effect
    void Awake()
    {
        material = new Material(Shader.Find("Unlit/Drops_V2"));

        intensity = 1f;
        size = 10f;
        distortion = 1;
        time = 1f;
        speed = 0.25f;
        aspectU = 1f;
        aspectV = 2f;

        material.SetFloat("_AspectU", aspectU);
        material.SetFloat("_AspectV", aspectV);
        material.SetFloat("_Speed", speed);
        material.SetFloat("_Size", size);
        material.SetFloat("_Distortion", distortion);
        material.SetFloat("_T", time);
    }

    // Postprocess the image
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (PersistentModel.Instance == null || intensity == 0)
        {
            Graphics.Blit(source, destination);
            return;
        }

        material.SetFloat("_AspectU", aspectU);
        material.SetFloat("_AspectV", aspectV);
        material.SetFloat("_Speed", speed);
        material.SetFloat("_Size", size);
        material.SetFloat("_T", time);
        material.SetFloat("_Distortion", distortion - (PersistentModel.Instance.CarSpeed / 70) );
        Graphics.Blit(source, destination, material);
    }
}