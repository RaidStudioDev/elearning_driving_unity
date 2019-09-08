using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectVehicleStuck : MonoBehaviour {

	private float collisionTime = 0f;

    private Race race;
	// Use this for initialization
	void Start ()
    {
        race = GameObject.Find("Race").GetComponent<Race>();
    }

    void OnCollisionEnter(Collision collision)
	{
		collisionTime = 0f;
	}

	void OnCollisionStay(Collision collision)
	{
        if (collision.gameObject != race.Vehicle.gameObject) return;

        /*DebugLog.Trace("race: " + race);
        DebugLog.Trace("race.Vehicle: " + race.Vehicle);
        DebugLog.Trace("race.Vehicle.AudioWallBump: " + race.Vehicle.AudioWallBump);
        DebugLog.Trace("collision: " + collision);*/

        if (race == null || collision == null || race.Vehicle.AudioWallBump == null) return;

        // Car Audio Bump on Walls
        if (!race.Vehicle.AudioWallBump.isPlaying && collision.relativeVelocity.magnitude >= 0.25f)
        {
            // set volume according to vehicle speed
            float volControl = collision.relativeVelocity.magnitude / 10;
            race.Vehicle.AudioWallBump.volume = volControl;
            race.Vehicle.AudioWallBump.Play();
        }

        collisionTime += Time.deltaTime;

		if (collisionTime > 3) 
        {
			race.Track.PositionVehicleAtCheckpoint (true);

			collisionTime = 0;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
