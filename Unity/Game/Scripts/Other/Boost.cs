using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boost : MonoBehaviour
{
    // private int strength;
    
    public delegate void TriggerEventHandler(Boost boost);
    public event TriggerEventHandler OnTriggered;
    
    void Start ()
    {
        // string[] parts = name.Split('_');
        // strength = Int32.Parse(parts[parts.Length - 1]);
    }

	void OnTriggerEnter(Collider other) 
	{
        if (other.transform.parent.parent.name != "vehicle") return;
		
        //Race race = GameObject.Find("Race").GetComponent<Race>();
        //race.Vehicle.Boost(strength);

        if (OnTriggered != null) OnTriggered(this);
	}
}
