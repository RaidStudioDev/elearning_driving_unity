using System;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Car
{
    public class BrakeLight : MonoBehaviour
    {
        public CarController car; // reference to the car controller, must be dragged in inspector
        
        // warning CS0414: The private field `Race.CarnivalLoop' is assigned but its value is never used
#pragma warning disable 414
        private Renderer m_Renderer;
#pragma warning restore 414

        private Light rearRightlight;
        private Light rearLeftlight;
        private Material lightMaterial;
        private Color baseLightColor = Color.red;
        private bool isGameNight;


        private void Start()
        {
            isGameNight = PersistentModel.Instance.GameNight;

            m_Renderer = GetComponent<Renderer>();
            
            rearRightlight = gameObject.transform.GetComponent<Light>();
            rearLeftlight = gameObject.transform.GetComponent<Light>();

            lightMaterial = car.GetComponent<Vehicle>().lightMaterial;
        }

        private void Update()
        {
            // enable the Renderer when the car is braking, disable it otherwise.
            if (car.BrakeInput <= 0f) 
            {
                //m_Renderer.material.DisableKeyword("_EMISSION");//.enabled = car.BrakeInput > 0f;
                lightMaterial.SetColor("_EmissionColor", baseLightColor * Mathf.LinearToGammaSpace(0.0f));

                rearRightlight.intensity = (isGameNight) ? 0.0f : 0.0f;
                rearLeftlight.intensity = (isGameNight) ? 0.0f : 0.0f;
            }
            else 
            {
                //m_Renderer.material.EnableKeyword("_EMISSION");
                lightMaterial.SetColor("_EmissionColor", baseLightColor * Mathf.LinearToGammaSpace(1f));

                rearRightlight.intensity = (isGameNight) ? 2f : .95f;
                rearLeftlight.intensity = (isGameNight) ? 2f : .95f;
            }
        }
    }
}
