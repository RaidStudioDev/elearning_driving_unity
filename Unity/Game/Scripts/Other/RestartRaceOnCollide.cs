using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartRaceOnCollide : MonoBehaviour
{
	private float time = 5f;
    private Race race;
    private GameScreen gameScreen;

    void Start ()
    {
        race = GameObject.Find("Race").GetComponent<Race>();
        gameScreen = GameObject.Find("GameScreen(Clone)").GetComponent<GameScreen>();
    }

	void OnCollisionEnter(Collision collision)
	{
		if (time < 5) return;

        time = 0;

	    gameScreen.ShowBoom(13f);

		race.Track.PositionVehicleAtCheckpoint (false);
	}

	void OnCollisionStay(Collision collision)
	{
		if (time < 5) return;

        time = 0;

	    gameScreen.ShowBoom();

        race.Track.PositionVehicleAtCheckpoint(true);
	}

	void Update ()
    {
		time += Time.deltaTime;
	}
}
