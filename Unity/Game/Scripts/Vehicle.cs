using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Vehicles.Car;

public class Vehicle : MonoBehaviour
{
    private CarController carController;
    private CarAudio carAudio;

    [HideInInspector]
    public Material lightMaterial;

    public AudioClip AudioCrashClip { get; private set; }
    public AudioSource AudioWallBump { get; private set; }
    public GameObject Probe { get; private set; }

    public int Speed
    {
        get { return (int)carController.CurrentSpeed;}
        set { carController.m_Topspeed = value; }
    }

    private bool stopPlaySkidSoundOnce = false;
    private bool stop = false;
    public bool CanMove { get; set; }

    void Start ()
    {
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        rigidBody.mass = 1000;

        carController = GetComponent<CarController>();
        carAudio = GetComponent<CarAudio>();
        name = "vehicle";

        // Apply vehicle reflection and update materials
        ProcessVehicleMaterial();

        // Process vehicle lights
        ProcessLightMaterial();

        // Add Bumps and Crash audio clips
        ProcessCarAudioEffects();
    }

    private void FixedUpdate()
    {
        float handbrake = 0.0f;

        PersistentModel.Instance.CarSpeed = carController.CurrentSpeed;

        // pass the input to the car!
        float h = CrossPlatformInputManager.GetAxis("Horizontal") * .5f;
        float v = CrossPlatformInputManager.GetAxis("Vertical");

        if (stop)
        {
            // carController.StartSkidTrail();
            carController.Brake(h, true);
            return;
        }

        if (!CanMove)
        {
            if (v > 0)
            {
                // carAudio.useDoppler = true;
                carController.UpdateMove(v);
            }
            return;
        }
        
        #if !MOBILE_INPUT
            // if (CanMove) v = 1; //try this later...
            carController.Move(h, v, v, handbrake);
        #else
            carController.Move(h, v, v, 0f);
        #endif
    }
    
    public void Stop()
    {
        carAudio.Disable();
        stop = true;
        CanMove = false;

        carController.ForceSkidBrakeStop();

        if (!stopPlaySkidSoundOnce)
        {
            stopPlaySkidSoundOnce = true;
            carController.PlaySkidAudio();
        }

        LeanTween.delayedCall(1.75f, () =>
        {
            carController.StopWheelSpinAudio();
        });
    }

    public void SlowDown()
    {
        carController.SlowDown();
    }

    private void ProcessVehicleMaterial()
    {
        // Look for ReflectionProbe in Main Scene
        // We are aplying this to only the Vehicle
        Probe = GameObject.Find("ReflectionProbe");
        Probe.transform.SetParent(this.transform);

        Probe.GetComponent<ReflectionProbe>().intensity = 0.5f;
        Probe.GetComponent<ReflectionProbe>().boxProjection = true;
        Probe.GetComponent<ReflectionProbe>().RenderProbe();

        // set material effects * using Stander Shader
        GameObject carBody = gameObject.FindGameObjectChildWithName("Body");
        MeshRenderer carMatRender = carBody.GetComponent<MeshRenderer>();

        // apply layer masking for reflection probe
        carBody.layer = LayerMask.NameToLayer("Vehicle");

        // this can be set for each individual vehicle
        // but for convenience doing this way
        // some cars dont look right on certain scenes
        switch (PersistentModel.Instance.GameVehicle)
        {
            case "TOYOTA_CAMRY__LE_2009":
            case "CADILLAC_ESCALADE_2013":
            case "MAZDA_MIATA_2011":
            case "BMW_X5_4.4i_2005":
            case "FORD_F250":
            case "GMC_SIERRA_2007":
            case "DODGE_RAM_3500":
            case "NISSAN_ALTIMA_2008":
            case "CHEVROLET_CAMARO_2011":
            case "FORD_MUSTANG_GT_2010":
            case "MERCEDES_AMG_2012":
            case "LEXUS_ES350_2011":
                carMatRender.material.SetColor("_Color", Color.white);
                carMatRender.material.SetFloat("_Metallic", 0.09f);
                carMatRender.material.SetFloat("_Glossiness", 0.9f);
                break;
            default:
                carMatRender.material.SetColor("_Color", Color.white);
                carMatRender.material.SetFloat("_Metallic", 0.09f);
                carMatRender.material.SetFloat("_Glossiness", 0.75f);
                break;
        }
    }

    // process vehicle head and brake lights
    void ProcessLightMaterial()
    {
        // hard coded values, searches for any of these in the vehicles provided
        string[] names = new string[] { "RearLights", "RearLight", "Rear_Light", "rearlight",
            "rearLight", "brakelight", "tail_light", "light", "LAMP", "lamp", "three" };

        for (int i = 0; i < names.Length; i++)
        {
            lightMaterial = gameObject.FindGameObjectChildrenWithPartialName(names[i])[0].GetComponent<Renderer>().materials[0];
            if (lightMaterial)
            {
                // Debug.Log("Material Light Found");
                break;
            }
        }
        
        // headlight controls
        Light light1 = gameObject.FindGameObjectChildWithName("HeadlightLeftSpot").transform.GetComponent<Light>();
        Light light2 = gameObject.FindGameObjectChildWithName("HeadlightRightSpot").transform.GetComponent<Light>();
        if (PersistentModel.Instance.GameNight)
        {
            light1.intensity = 2f;
            light1.spotAngle = 70.0f;
            light1.range = 100f;
            light2.intensity = 2f;
            light2.spotAngle = 70.0f;
            light2.range = 100f;
        }

        // rearlight controls
        gameObject.FindGameObjectChildWithName("RearlightLeftSpot").AddComponent<BrakeLight>().car = carController;
        gameObject.FindGameObjectChildWithName("RearlightRightSpot").AddComponent<BrakeLight>().car = carController;
    }

    private void ProcessCarAudioEffects()
    {
        // Add Car Wall Bump Sound, sound play see DetectVehicleStuck
        AudioClip audioWallBumpClip = Instantiate(Resources.Load("Car_Bump")) as AudioClip;
        AudioWallBump = gameObject.AddComponent<AudioSource>();
        AudioWallBump.playOnAwake = false;
        AudioWallBump.clip = audioWallBumpClip;

        // Add Car Crash Sound, plays in Track.PositionVehicleAtCheckpoint
        AudioCrashClip = Instantiate(Resources.Load("CarCrash")) as AudioClip;
    }
}
