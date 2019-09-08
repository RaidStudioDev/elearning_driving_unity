using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private string weather = Race.WEATHER_DEFAULT;
    
    public delegate void TriggerEventHandler(Checkpoint checkpoint);
    public event TriggerEventHandler OnTriggered;

    public bool Passed; // have we already passed this checkpoint?
    public uint Index;

    public Vector3 SavedVehiclePosition;
    public Quaternion SavedVehicleRotation;

    private Race race;
    private Collider thisCollider;
    private GameObject vehicleGo;
    private Vector3 vehiclePosition = Vector3.zero;
    private Vector3 center = Vector3.zero;

    void Start ()
    {
        race = GameObject.Find("Race").GetComponent<Race>();
        vehicleGo = GameObject.Find("vehicle");
        vehiclePosition = Vector3.zero;
        thisCollider = GetComponent<Collider>();

        string[] parts = name.Split('_');
        Index = System.UInt32.Parse(parts[parts.Length - 1]);

        if (name.StartsWith(Race.WEATHER_HAIL)) weather = Race.WEATHER_HAIL;
        else if (name.StartsWith(Race.WEATHER_NONE)) weather = Race.WEATHER_NONE;
    }

   
    void OnTriggerEnter(Collider other) 
	{
        if (other.transform.parent.parent.name != "vehicle") return;
		
        if (!Passed)
        {
            center = thisCollider.bounds.center;

            vehiclePosition.x = center.x;
            vehiclePosition.y = vehicleGo.transform.position.y + 1f;
            vehiclePosition.z = center.z;

            SavedVehiclePosition = vehiclePosition;
            SavedVehicleRotation = vehicleGo.transform.rotation;
        }

        race.Track.StoreCheckpoint (this.gameObject);
        race.SetWeather(weather);

        if (OnTriggered != null) OnTriggered(this);
	}
}
