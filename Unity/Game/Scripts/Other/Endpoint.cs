using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Endpoint : MonoBehaviour
{
    public delegate void EndEventHandler();
    public event EndEventHandler OnEnded;

    void Start ()
    {	
	}

	void OnTriggerEnter(Collider other) 
	{
        if (other.transform.parent.parent.name != "vehicle") return;
		
        OnEnded();
	}
}
