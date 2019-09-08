using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TimeBoost : MonoBehaviour 
{
	private bool hit = false;
	private int boostAmount = 4;
	private float rotationSpeed = 200f;
	private AudioSource audioSource;

    [HideInInspector]
    public Material lightMaterial;
    private Color baseLightColor;
    private MeshCollider thisMeshCollider;
    private GameScreen gameScreen;
    private Race race;
    private Color finalGlowColor;
    private float emissionAmount;
    private bool isGameNight;

    void Start ()
    {
        isGameNight = PersistentModel.Instance.GameNight;

        gameScreen = GameObject.Find("GameScreen(Clone)").transform.GetComponent<GameScreen>();
        race = GameObject.Find("Race").GetComponent<Race>();

        thisMeshCollider = gameObject.AddComponent<MeshCollider>();
        thisMeshCollider.convex = true;
        thisMeshCollider.isTrigger = true;
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = Resources.Load("pickup_collect_chime_01") as AudioClip;
        
        transform.RotateAround(thisMeshCollider.bounds.center, Vector3.up, UnityEngine.Random.Range(0, 500));

        // emission light 
        baseLightColor = new Color(2.96967f, 0.69f, 0f);
        lightMaterial = gameObject.GetComponent<Renderer>().materials[0];

        if (!isGameNight)
        {
            baseLightColor = new Color(2.96967f, 0.69f, 0f);
        }
    }

	void OnTriggerEnter(Collider other) 
	{
		if (other.transform.parent.parent.name == "vehicle" && !hit)
		{
			hit = true;

            rotationSpeed = 950f;

            gameScreen.ShowBonus();

            audioSource.Play();
            
            race.Time = race.Time - boostAmount;
		}
	}

    private float timeToDestroy = 2;
    void Update () 
	{
        if (isGameNight) StartEmission(0.1f, 2.5f, 0.5f, 0.5f);
        else StartEmission(0.1f, 0.5f, 0.5f, 0.5f);

        transform.RotateAround(thisMeshCollider.bounds.center, Vector3.up, Time.deltaTime * rotationSpeed);

        if (hit)
        {
            rotationSpeed = 1200f;
            transform.Translate(Vector3.forward * Time.deltaTime * (190f / 20));
            
            timeToDestroy -= Time.deltaTime;
            if (timeToDestroy <= 0)
            {
                Destroy(gameObject);
            }
        }
	}

    
    void StartEmission(float minIntensity, float maxIntensity, float pulsateSpeed, float pulsateMaxDistance)
    {
        emissionAmount = Mathf.Lerp(minIntensity, maxIntensity, Mathf.PingPong(Time.time * pulsateSpeed, pulsateMaxDistance));
        finalGlowColor = baseLightColor * Mathf.LinearToGammaSpace(emissionAmount);
        lightMaterial.SetColor("_EmissionColor", finalGlowColor);
    }

    private void OnDestroy()
    {
        gameScreen = null;
        race = null;
        thisMeshCollider = null;
        audioSource = null;
    }
}
