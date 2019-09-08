using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
	public ParticleSystem smokeEffect;
	public ParticleSystem explosionEffect;
	public ParticleSystem fireEffect;
	public ParticleSystem smallExplosionEffect;

	void Start ()
    {	
	}

	public void Explode()
	{
		//smokeEffect.Play();
		//fireEffect.Play();
		//explosionEffect.Play();
		//smallExplosionEffect.Play();

//		Camera.current.gameObject.Shake(10);
	}

	public void Stop()
	{
		//smokeEffect.Stop();
		//smokeEffect.Clear ();
		//fireEffect.Stop();
		//fireEffect.Clear ();
		//explosionEffect.Stop();
		///explosionEffect.Clear ();
		//smallExplosionEffect.Stop();
		//smallExplosionEffect.Clear ();
	}

	void Update ()
    {	
	}
}
