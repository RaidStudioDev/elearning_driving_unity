using UnityEngine;
using System.Collections;

public class Weather : MonoBehaviour
{
    private string[] parts;
    private string weather;
    private Race race;

    void Start()
	{
        race = GameObject.Find("Race").GetComponent<Race>();

        parts = new string[]{ "" };
        weather = "";

        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.convex = true;
        meshCollider.isTrigger = true;
        
       if (!PersistentModel.Instance.DEBUG) GetComponent<MeshRenderer>().enabled = false;
	}
    
    void OnTriggerEnter(Collider other) 
    {
        if (other.transform.parent.parent.name != "vehicle") return;
        
        parts = name.Split('_');
        weather = parts[1];

        race.SetWeather(weather);
    }

    void OnTriggerExit(Collider other)
    {
        race.SetWeather(Race.WEATHER_DEFAULT);
    }
}
