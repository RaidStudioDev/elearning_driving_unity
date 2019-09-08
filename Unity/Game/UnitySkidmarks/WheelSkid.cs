using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Copyright 2017 Nition, BSD licence (see LICENCE file). http://nition.co
[RequireComponent(typeof(WheelCollider))]
public class WheelSkid : MonoBehaviour {

	// INSPECTOR SETTINGS

	[SerializeField]
	public Rigidbody rb;
	[SerializeField]
    public Skidmarks skidmarksController;

	// END INSPECTOR SETTINGS

	WheelCollider wheelCollider;
	WheelHit wheelHitInfo;

	const float SKID_SLIP_FX_SPEED = 0.25f;             // Min side slip speed in m/s to start showing a skid
	const float SKID_FORWARD_FX_SPEED = 0.5f;           // Min forward side slip speed in m/s to start showing a skid
    const float MAX_SKID_INTENSITY = 6.00f;             // m/s where skid opacity is at full intensity
    const float MAX_FORWARD_SKID_INTENSITY = 42.00f;    // m/s where skid opacity is at full intensity
	int lastSkid = -1;                                  // Index of last skidmark piece this wheel used

    public bool forceBrake = false;                     // apply skid on full brake stop (complete race)

    protected void Awake()
    {
		wheelCollider = GetComponent<WheelCollider>();
	}
    
    protected void Update()
    {
        if (rb == null) return;
        
        if (wheelCollider.GetGroundHit(out wheelHitInfo))
        {
            // Check sideways speed
            // Gives velocity with +Z being our forward axis
            Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
            float skidSpeed = Mathf.Abs(localVelocity.x);
            float intensity = -1;
            Vector3 skidPoint;

            float skidSpeedZ = Mathf.Abs(localVelocity.z);

            if (Mathf.Abs(wheelHitInfo.forwardSlip) >= SKID_FORWARD_FX_SPEED)
            {
                //Debug.Log("forwardSlip" + Mathf.Abs(wheelHitInfo.forwardSlip));

                intensity = Mathf.Clamp01(skidSpeedZ / MAX_FORWARD_SKID_INTENSITY);

                skidPoint = wheelHitInfo.point + (rb.velocity * (Time.fixedDeltaTime));
                lastSkid = skidmarksController.AddSkidMark(skidPoint, wheelHitInfo.normal, intensity, lastSkid);
            }
            /*else if (forceBrake && skidSpeedZ >= 0.1f)
            {
                Debug.Log("skidSpeed" + skidSpeedZ);
                intensity = Mathf.Clamp01(skidSpeedZ / 42.00f);
                
                skidPoint = wheelHitInfo.point + (rb.velocity * (Time.fixedDeltaTime));
                lastSkid = skidmarksController.AddSkidMark(skidPoint, wheelHitInfo.normal, intensity, lastSkid);
            }*/
            else if (skidSpeed >= SKID_SLIP_FX_SPEED || Mathf.Abs(wheelHitInfo.forwardSlip) >= 0.5f)
            {
                // MAX_SKID_INTENSITY as a constant, m/s where skids are at full intensity
                intensity = Mathf.Clamp01(skidSpeed / MAX_SKID_INTENSITY);
                //Debug.Log("intensity" + intensity);
                skidPoint = wheelHitInfo.point + (rb.velocity * (Time.fixedDeltaTime));
                lastSkid = skidmarksController.AddSkidMark(skidPoint, wheelHitInfo.normal, intensity, lastSkid);
            }
            else
            {
                lastSkid = -1;
            }
        }
    }
}