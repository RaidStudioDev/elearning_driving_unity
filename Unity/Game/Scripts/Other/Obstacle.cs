using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Obstacle : MonoBehaviour 
{
    private bool hit = false;
    private Race race;
    private Rigidbody objRigibody;
    private Renderer objRenderer;
    private float mag = 0f;
    private Vector3 force;
    private float volControl = 0f;
    private Color endPhaseColor;
    private Vector3 torqueRotation;
    
    public bool hasRigidBody = true;
   
    private void Awake()
    {
        race = GameObject.Find("Race").GetComponent<Race>();
        objRigibody = gameObject.GetComponent<Rigidbody>();
        objRenderer = transform.GetComponent<Renderer>();
        force = Vector3.zero;
        torqueRotation = Vector3.zero;
    }

    void Start ()
    {
        #if !UNITY_IOS
            objRenderer.material.shader = Shader.Find("Transparent/Diffuse ZWrite");
         #endif
    }

    void OnCollisionEnter(Collision collision) 
	{
        if (hit) return;

        hit = collision.gameObject == race.Vehicle.gameObject && !hit;

        if (collision == null || race == null) return;
        if (race != null && race.Vehicle == null) return;
        if (race.Vehicle.AudioWallBump == null) return;

        if (hit && !race.Vehicle.AudioWallBump.isPlaying && collision.relativeVelocity.magnitude >= 0.1f)
        {
            // set volume according to vehicle speed
            volControl = collision.relativeVelocity.magnitude / 10;
            race.Vehicle.AudioWallBump.volume = volControl;
            race.Vehicle.AudioWallBump.Play();

            if (hasRigidBody)
            {
                objRigibody.mass = 10f;
                objRigibody.drag = 1f;
                objRigibody.useGravity = true;

                mag = 5000 * (collision.relativeVelocity.magnitude / 30); // 30 is avg mag at top speed
                force = transform.position - collision.transform.position;
                force.Normalize();
                objRigibody.AddForce(force * mag);
            }

            if (gameObject.name == "car_prop_00")
            {
                mag = 5000 * (collision.relativeVelocity.magnitude / 30); // 30 is avg mag at top speed
                force = transform.position - collision.transform.position;
                force.Normalize();
                objRigibody.AddForce(force * mag);
            }
         
            // UPDATE: RAFAEL: quick function to slowdown car when hitting obstacles
            // called from Vehicle.cs
            race.Vehicle.SlowDown();
        }
    }

	void Update () 
	{
        if (!hit) return;

        if (hasRigidBody)
        {
            torqueRotation.Set(125f, 180f, 360f);
            objRigibody.AddTorque(torqueRotation, ForceMode.Impulse);

            if (objRenderer.material.color.a < .01f)
            {
                race = null;
                Destroy(gameObject);
                return;
            }

            endPhaseColor = objRenderer.material.color;
            endPhaseColor.a = objRenderer.material.color.a - .015f;
            objRenderer.material.color = endPhaseColor;
        }   
        else
        {
            // do nothing
        }
	}

    private void OnDestroy()
    {
        race = null;
        objRigibody = null;
        objRenderer = null;
    }
}
