using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TimeObstacle : MonoBehaviour 
{
	private bool hit = false;
	private float speed = 200f;
	private AudioSource audioSource;
    
    void Start ()
    {
        var meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.convex = true;
        meshCollider.isTrigger = true;
        
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = Resources.Load("ding") as AudioClip;
        
        transform.RotateAround(meshCollider.bounds.center, Vector3.up, UnityEngine.Random.Range(0, 500));   
	}

	void OnTriggerEnter(Collider other) 
	{
		if (other.transform.parent.parent.name == "vehicle" && !hit)
		{
			hit = true;

            GameObject.Find("GameScreen(Clone)").transform.GetComponent<GameScreen>().ShowBoom();

            audioSource = GetComponent<AudioSource>();
            audioSource.Play();
            
            string[] parts = name.Split('_');
            var time = Int32.Parse(parts[parts.Length - 1]);
            
            Race race = GameObject.Find("Race").GetComponent<Race>();
            race.Time = race.Time + time;
		}
	}

	void Update () 
	{
        Collider collider = gameObject.GetComponent<Collider>();
    
        transform.RotateAround(collider.bounds.center, Vector3.up, Time.deltaTime * speed); 
    
		if (hit) transform.Translate(Vector3.forward * Time.deltaTime * (speed / 20));

        if (hit && !audioSource.isPlaying) Destroy(gameObject);
	}
}
