using System;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Car
{
    internal enum CarDriveType
    {
        FrontWheelDrive,
        RearWheelDrive,
        FourWheelDrive
    }

    internal enum SpeedType
    {
        MPH,
        KPH
    }

    public class CarController : MonoBehaviour
    {
        private PersistentModel pModel;

        [SerializeField] private CarDriveType m_CarDriveType = CarDriveType.FourWheelDrive;
        [SerializeField] private WheelCollider[] m_WheelColliders = new WheelCollider[4];
        [SerializeField] private GameObject[] m_WheelMeshes = new GameObject[4];
        [SerializeField] private WheelEffects[] m_WheelEffects = new WheelEffects[4];
        [SerializeField] private Vector3 m_CentreOfMassOffset;
        [SerializeField] private float m_MaximumSteerAngle;
        [Range(0, 1)] private float m_SteerHelper = 0.644f; // 0 is raw physics , 1 the car will grip in the direction it is facing
        [Range(0, 1)] private float m_TractionControl = 1; // 0 is no traction control, 1 is full interference
        private float m_FullTorqueOverAllWheels = 4000f;
        [SerializeField] private float m_ReverseTorque;
        [SerializeField] private float m_MaxHandbrakeTorque;
        private float m_Downforce = 200f;
        [SerializeField] private SpeedType m_SpeedType;
        //private float m_Topspeed = 70;
        public float m_Topspeed { get; set; }
        public float m_CurrentTopspeed { get; set; }
        [SerializeField] private static int NoOfGears = 5;
        [SerializeField] private float m_RevRangeBoundary = 1f;
        [SerializeField] private float m_SlipLimit;
        [SerializeField] private float m_BrakeTorque;

        private Quaternion[] m_WheelMeshLocalRotations;
        private Vector3 m_Prevpos, m_Pos;
        private float m_SteerAngle;
        private int m_GearNum;
        private float m_GearFactor;
        private float m_OldRotation;
        private float m_CurrentTorque;
        private Rigidbody m_Rigidbody;
        private const float k_ReversingThreshold = 0.01f;
        
        public bool Skidding { get; private set; }
        public float BrakeInput { get; private set; }
        public float CurrentSteerAngle{ get { return m_SteerAngle; }}
        public float CurrentSpeed{ get { return m_Rigidbody.velocity.magnitude*2.23693629f; }}
        public float MaxSpeed{get { return m_Topspeed; }}
        public float Revs { get; private set; }
        public float AccelInput { get; private set; }

        private Skidmarks skidMarksController;

		public void Reset()
		{
			if (!m_Rigidbody) m_Rigidbody = GetComponent<Rigidbody>();
			m_Rigidbody.velocity = Vector3.zero;
			m_Rigidbody.angularVelocity = Vector3.zero;

			m_SteerAngle = 0;
			AccelInput = 0;
			Revs = 0;
			m_CurrentTorque = 0;
            m_Topspeed = m_CurrentTopspeed = 70;
        }

        // UPDATE: RAFAEL: quick function to slowdown car when hitting obstacles
        // called from Vehicle.cs
        public void SlowDown()
        {
            if (!m_Rigidbody) m_Rigidbody = GetComponent<Rigidbody>();
            m_Rigidbody.velocity /= 1.65f;
            m_Rigidbody.angularVelocity /= 1.75f;

            m_SteerAngle = 0;
            AccelInput = 0;
            Revs = 0;
            m_CurrentTorque = 0;
        }

        // Use this for initialization
        private Race race;
        private void Start()
        {
            race = GameObject.Find("Race").GetComponent<Race>();

            pModel = PersistentModel.Instance;

            stop = false;

            m_Topspeed = m_CurrentTopspeed = 70;

            m_WheelMeshLocalRotations = new Quaternion[4];
            for (int i = 0; i < 4; i++)
            {
                m_WheelMeshLocalRotations[i] = m_WheelMeshes[i].transform.localRotation;
            }
            m_WheelColliders[0].attachedRigidbody.centerOfMass = m_CentreOfMassOffset;

            m_MaxHandbrakeTorque = float.MaxValue;

            m_Rigidbody = GetComponent<Rigidbody>();
            m_CurrentTorque = m_FullTorqueOverAllWheels - (m_TractionControl * m_FullTorqueOverAllWheels);

            this.skidMarksController = GameObject.Find("SkidMarksController").GetComponent<Skidmarks>();

            // right
            m_WheelColliders[2].gameObject.GetComponent<WheelSkid>().rb = m_Rigidbody;
            m_WheelColliders[2].gameObject.GetComponent<WheelSkid>().skidmarksController = this.skidMarksController;

            // left wheel    
            m_WheelColliders[3].gameObject.GetComponent<WheelSkid>().rb = m_Rigidbody;
            m_WheelColliders[3].gameObject.GetComponent<WheelSkid>().skidmarksController = this.skidMarksController;
        }

        public void ForceSkidBrakeStop()
        {
            m_WheelColliders[2].gameObject.GetComponent<WheelSkid>().forceBrake = true;
            m_WheelColliders[3].gameObject.GetComponent<WheelSkid>().forceBrake = true;
        }

        public void DisableSkidBrakeStop()
        {
            m_WheelColliders[2].gameObject.GetComponent<WheelSkid>().forceBrake = false;
            m_WheelColliders[3].gameObject.GetComponent<WheelSkid>().forceBrake = false;
        }

        private void GearChanging()
        {
            float f = Mathf.Abs(CurrentSpeed/MaxSpeed);
            float upgearlimit = (1/(float) NoOfGears)*(m_GearNum + 1);
            float downgearlimit = (1/(float) NoOfGears)*m_GearNum;

            if (m_GearNum > 0 && f < downgearlimit)
            {
                m_GearNum--;
            }

            if (f > upgearlimit && (m_GearNum < (NoOfGears - 1)))
            {
                m_GearNum++;
            }
        }

        // simple function to add a curved bias towards 1 for a value in the 0-1 range
        private static float CurveFactor(float factor)
        {
            return 1 - (1 - factor)*(1 - factor);
        }

        // unclamped version of Lerp, to allow value to exceed the from-to range
        private static float ULerp(float from, float to, float value)
        {
            return (1.0f - value)*from + value*to;
        }

        private void CalculateGearFactor()
        {
            float f = (1/(float) NoOfGears);
            // gear factor is a normalised representation of the current speed within the current gear's range of speeds.
            // We smooth towards the 'target' gear factor, so that revs don't instantly snap up or down when changing gear.
            var targetGearFactor = Mathf.InverseLerp(f*m_GearNum, f*(m_GearNum + 1), Mathf.Abs(CurrentSpeed/MaxSpeed));
            m_GearFactor = Mathf.Lerp(m_GearFactor, targetGearFactor, Time.deltaTime*5f);
        }

        private void CalculateRevs()
        {
            // calculate engine revs (for display / sound)
            // (this is done in retrospect - revs are not used in force/power calculations)
            CalculateGearFactor();
            float gearNumFactor = m_GearNum/(float) NoOfGears;
            float revsRangeMin = ULerp(0f, m_RevRangeBoundary, CurveFactor(gearNumFactor));
            float revsRangeMax = ULerp(m_RevRangeBoundary, 1f, gearNumFactor);
            Revs = ULerp(revsRangeMin, revsRangeMax, m_GearFactor);
        }

        public void UpdateMove(float v)
        {
            Revs = v;
            AccelInput = v;
        }

        public void Move(float steering, float accel, float footbrake, float handbrake)
        {
            Quaternion quat;
            Vector3 position;

            for (int i = 0; i < 4; i++)
            {
                m_WheelColliders[i].GetWorldPose(out position, out quat);
                m_WheelMeshes[i].transform.position = position;
                m_WheelMeshes[i].transform.rotation = quat;
            }

            //clamp input values
            steering = Mathf.Clamp(steering, -1, 1);
            AccelInput = accel = Mathf.Clamp(accel, 0, 1);
            BrakeInput = footbrake = -1 * Mathf.Clamp(footbrake, -1, 0);
            handbrake = Mathf.Clamp(handbrake, 0, 1);

            //Set the steer on the front wheels.
            //Assuming that wheels 0 and 1 are the front wheels.
            m_SteerAngle = steering*m_MaximumSteerAngle;
            m_WheelColliders[0].steerAngle = m_SteerAngle;
            m_WheelColliders[1].steerAngle = m_SteerAngle;

            SteerHelper();
            ApplyDrive(accel, footbrake);
            CapSpeed();

            // Set the handbrake.
            // Assuming that wheels 2 and 3 are the rear wheels.
            if (handbrake > 0f)
            {
                float hbTorque = handbrake * m_MaxHandbrakeTorque;
                m_WheelColliders[2].brakeTorque = hbTorque;
                m_WheelColliders[3].brakeTorque = hbTorque;
            }

            CalculateRevs();
            GearChanging();

            if (stop) return;

            AddDownForce();
            CheckForWheelSpin();
            TractionControl();
        }

        public bool stop { get; set; }

        public void Brake(float steering, bool runSkidTrail = true)
        {
            Quaternion quat;
            Vector3 position;
            for (int i = 0; i < 4; i++)
            {
                m_WheelColliders[i].GetWorldPose(out position, out quat);
                m_WheelMeshes[i].transform.position = position;
                m_WheelMeshes[i].transform.rotation = quat;
            }

            //clamp input values
            steering = Mathf.Clamp(steering, -1, 1);
            AccelInput = 0;
            BrakeInput = 1;

            //Set the steer on the front wheels.
            //Assuming that wheels 0 and 1 are the front wheels.
            m_SteerAngle = steering * m_MaximumSteerAngle;
            m_WheelColliders[0].steerAngle = m_SteerAngle;
            m_WheelColliders[1].steerAngle = m_SteerAngle;

            SteerHelper();
            ApplyDrive(0, 1);
            CapSpeed2();

            // Set the handbrake.
            m_WheelColliders[0].brakeTorque = m_MaxHandbrakeTorque;
            m_WheelColliders[1].brakeTorque = m_MaxHandbrakeTorque;
            m_WheelColliders[2].brakeTorque = m_MaxHandbrakeTorque;
            m_WheelColliders[3].brakeTorque = m_MaxHandbrakeTorque;

            CalculateRevs();
            GearChanging();

            AddDownForce();
            TractionControl();

            if (!runSkidTrail) return;

            // loop through all wheels
            WheelHit wheelHit;
            Vector3 skidPoint;
            for (int i = 0; i < 4; i++)
            {
                m_WheelColliders[i].GetGroundHit(out wheelHit);

                // Debug.Log("wheelHit: " + wheelHit.point);

                skidPoint = wheelHit.point + (m_Rigidbody.velocity * Time.fixedDeltaTime);

                m_WheelEffects[i].EmitTireBrakeSmoke(wheelHit, skidPoint);

                // avoiding all four tires screeching at the same time
                // if they do it can lead to some strange audio artefacts
                if (!AnySkidSoundPlaying())
                {
                    if (pModel.GameModeID != "winter")
                    {
                        if (!stop) m_WheelEffects[i].PlayAudio();
                    }
                }
            }
        }

        private void CapSpeed2()
        {
            m_Rigidbody.velocity = m_Rigidbody.velocity / 1.019f;
        }
        
        private void CapSpeed()
        {
            float speed = m_Rigidbody.velocity.magnitude;

            // set speed gradually, we can set m_Topspeed 
            m_CurrentTopspeed = ULerp(m_CurrentTopspeed, m_Topspeed, Time.deltaTime * 0.25f);
            float topspeed = m_CurrentTopspeed;
            switch (m_SpeedType)
            {
                case SpeedType.MPH:

                    speed *= 2.23693629f;
                    if (speed > topspeed)
                        m_Rigidbody.velocity = (topspeed/2.23693629f) * m_Rigidbody.velocity.normalized;
                    break;

                case SpeedType.KPH:
                    speed *= 3.6f;
                    if (speed > topspeed)
                        m_Rigidbody.velocity = (topspeed/3.6f) * m_Rigidbody.velocity.normalized;
                    break;
            }
        }
        
        private void ApplyDrive(float accel, float footbrake)
        {
            float currentTorque = m_CurrentTorque;
            float thrustTorque;
            switch (m_CarDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    thrustTorque = accel * (currentTorque / 4f);
                    for (int i = 0; i < 4; i++)
                    {
                        m_WheelColliders[i].motorTorque = thrustTorque;
                    }
                    break;

                case CarDriveType.FrontWheelDrive:
                    thrustTorque = accel * (currentTorque / 2f);
                    m_WheelColliders[0].motorTorque = m_WheelColliders[1].motorTorque = thrustTorque;
                    break;

                case CarDriveType.RearWheelDrive:
                    thrustTorque = accel * (currentTorque / 2f);
                    m_WheelColliders[2].motorTorque = m_WheelColliders[3].motorTorque = thrustTorque;
                    break;

            }

            for (int i = 0; i < 4; i++)
            {
                if (CurrentSpeed > 5 && Vector3.Angle(transform.forward, m_Rigidbody.velocity) < 50f)
                {
                    m_WheelColliders[i].brakeTorque = m_BrakeTorque*footbrake;
                }
                else if (footbrake > 0)
                {
                    m_WheelColliders[i].brakeTorque = 0f;
                    m_WheelColliders[i].motorTorque = -m_ReverseTorque*footbrake;
                }
            }
        }
        
        private void SteerHelper()
        {
            WheelHit wheelhit;
            for (int i = 0; i < 4; i++)
            {
                m_WheelColliders[i].GetGroundHit(out wheelhit);
                if (wheelhit.normal == Vector3.zero)
                    return; // wheels arent on the ground so dont realign the rigidbody velocity
            }

            float turnadjust = 0f;
            // this if is needed to avoid gimbal lock problems that will make the car suddenly shift direction
            if (Mathf.Abs(m_OldRotation - transform.eulerAngles.y) < 10f)
            {
                turnadjust = (transform.eulerAngles.y - m_OldRotation) * m_SteerHelper;
                Quaternion velRotation = Quaternion.AngleAxis(turnadjust, Vector3.up);
                m_Rigidbody.velocity = velRotation * m_Rigidbody.velocity;
            }
            m_OldRotation = transform.eulerAngles.y;
        }
        
        // this is used to add more grip in relation to speed
        private void AddDownForce()
        {
            m_WheelColliders[0].attachedRigidbody.AddForce(-transform.up*m_Downforce*
                                                         m_WheelColliders[0].attachedRigidbody.velocity.magnitude);
        }

        // checks if the wheels are spinning and is so does three things
        // 1) emits particles
        // 2) plays tiure skidding sounds
        // 3) leaves skidmarks on the ground
        // these effects are controlled through the WheelEffects class
        private void CheckForWheelSpin()
        {
            // loop through all wheels
            WheelHit wheelHit;
            for (int i = 0; i < 4; i++)
            {
                m_WheelColliders[i].GetGroundHit(out wheelHit);

                if (Mathf.Abs(wheelHit.forwardSlip) >= 0.5f) 
                {
                    //ForceSkidBrakeStop();
                }
                else
                {
                    //DisableSkidBrakeStop();
                }

                // is the tire slipping above the given threshhold
                if (Mathf.Abs(wheelHit.forwardSlip) >= 0.5 || Mathf.Abs(wheelHit.sidewaysSlip) >= 0.25)
                {
                    // THE ACTUAL SKID MARKS
                    m_WheelEffects[i].EmitTireSmoke();

                    // avoiding all four tires screeching at the same time
                    // if they do it can lead to some strange audio artefacts
                    if (!AnySkidSoundPlaying())
                    {
                        if (pModel.GameModeID != "winter")
                        {
                            if (!stop && this.CurrentSpeed > 50)
                            {
                                m_WheelEffects[i].PlayAudio();
                            }
                        }
                    }
                    continue;
                }

                // if it wasnt slipping stop all the audio
                if (m_WheelEffects[i].PlayingAudio)
                {
                    m_WheelEffects[i].StopAudio();

                    // end the trail generation
                    m_WheelEffects[i].EndSkidTrail();
                }
            }
        }

        public void StopWheelSpinAudio()
        {
            stop = true;

            for (int i = 0; i < 4; i++)
            {
                // *************** SKID SMOKE
                m_WheelEffects[i].EmitTireSmoke();

                m_WheelEffects[i].StopAudio();
            }
        }

        public void EndSkidTrail()
        {
            //Debug.Log("stop skid trail");
            // loop through all wheels
            for (int i = 0; i < 4; i++)
            {
                // if it wasnt slipping stop all the audio
                if (m_WheelEffects[i].PlayingAudio)
                {
                    m_WheelEffects[i].StopAudio();
                }
                // end the trail generation
                m_WheelEffects[i].EndSkidTrail();
            }
        }

        public void PlaySkidAudio()
        {
            if (pModel.GameModeID != "winter")
            {
                m_WheelEffects[0].PlayAudio();
            }
        }

        public void StartSkidTrail()
        {
            Debug.Log("StartSkidTrail");

            // loop through all wheels
            for (int i = 0; i < 4; i++)
            {
                // *************** SKID SMOKE
                // is the tire slipping above the given threshhold
                m_WheelEffects[i].EmitTireSmoke();

                // avoiding all four tires screeching at the same time
                // if they do it can lead to some strange audio artefacts
                if (!AnySkidSoundPlaying() && !stop)
                {
                    if (pModel.GameModeID != "winter") m_WheelEffects[i].PlayAudio();
                }
            }
        }

        // crude traction control that reduces the power to wheel if the car is wheel spinning too much
        private void TractionControl()
        {
            WheelHit wheelHit;
            switch (m_CarDriveType)
            {
                case CarDriveType.FourWheelDrive:
                    // loop through all wheels
                    for (int i = 0; i < 4; i++)
                    {
                        m_WheelColliders[i].GetGroundHit(out wheelHit);

                        AdjustTorque(wheelHit.forwardSlip);
                    }
                    break;

                case CarDriveType.RearWheelDrive:
                    m_WheelColliders[2].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    m_WheelColliders[3].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;

                case CarDriveType.FrontWheelDrive:
                    m_WheelColliders[0].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);

                    m_WheelColliders[1].GetGroundHit(out wheelHit);
                    AdjustTorque(wheelHit.forwardSlip);
                    break;
            }
        }
        
        private void AdjustTorque(float forwardSlip)
        {
            if (forwardSlip >= m_SlipLimit && m_CurrentTorque >= 0)
            {
                m_CurrentTorque -= 10 * m_TractionControl;
            }
            else
            {
                m_CurrentTorque += 10 * m_TractionControl;
                if (m_CurrentTorque > m_FullTorqueOverAllWheels)
                {
                    m_CurrentTorque = m_FullTorqueOverAllWheels;
                }
            }
        }

        private bool AnySkidSoundPlaying()
        {
            for (int i = 0; i < 4; i++)
            {
                if (m_WheelEffects[i].PlayingAudio)
                {
                    return true;
                }
            }
            return false;
        }

		void Update()
		{            
			if (transform.position.y < -25) // todo, make this constant configerable 
			{
            	race.Track.PositionVehicleAtCheckpoint (true);
			}
		}
    }
}
