using System.Collections;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof (AudioSource))]
    public class WheelEffects : MonoBehaviour
    {
        public Transform SkidTrailPrefab;
        public static Transform skidTrailsDetachedParent;
        public ParticleSystem skidParticles;
        public bool skidding { get; private set; }
        public bool PlayingAudio { get; private set; }

        private AudioSource m_AudioSource;
        private Transform m_SkidTrail;
        private WheelCollider m_WheelCollider;

        private int counter;
        // private bool doSkid = false;

        private void Start()
        {
            if (skidParticles == null)
            {
                Debug.LogWarning(" no particle system found on car to generate smoke particles", gameObject);
            }
            else
            {
                skidParticles.Stop();
            }

            m_WheelCollider = GetComponent<WheelCollider>();
            m_AudioSource = GetComponent<AudioSource>();

            // UPDATE adjust audio sound
            m_AudioSource.dopplerLevel = 0;
            m_AudioSource.spatialBlend = 0.25f;
            m_AudioSource.volume = 0.5f;

            PlayingAudio = false;

            if (skidTrailsDetachedParent == null)
            {
                skidTrailsDetachedParent = new GameObject("Skid Trails - Detached").transform;
            }
        }
        
        public void EmitTireSmoke()
        {
            skidParticles.transform.position = transform.position - transform.up*m_WheelCollider.radius;
            skidParticles.Emit(3);
        }

        public void EmitTireBrakeSmoke(WheelHit wheelHit, Vector3 skidPoint)
        {
            skidParticles.transform.position = transform.position - transform.up * m_WheelCollider.radius;
            skidParticles.Emit(6);
        }

        public void PlayAudio()
        {
            m_AudioSource.Play();
            PlayingAudio = true;
        }

        public void StopAudio()
        {
            m_AudioSource.Stop();
            PlayingAudio = false;
        }

        private IEnumerator StartSkidTrail(WheelHit wheelHit, Vector3 skidPoint)
        {
            // Vector3 normal = wheelHit.normal;
            // Vector3 newPos = skidPoint + normal * 0.1f;

            if (m_SkidTrail != null) EndSkidTrail();
            skidding = true;

            m_SkidTrail = Instantiate(SkidTrailPrefab);
            while (m_SkidTrail == null)
            {
                yield return null;
            }
            
            m_SkidTrail.parent = transform;
            m_SkidTrail.localPosition = -Vector3.up*m_WheelCollider.radius;
        }

        public void EndSkidTrail()
        {
            skidding = false;
            if (m_SkidTrail == null) return;
            Destroy(m_SkidTrail.gameObject, 5);
        }

        private void Update()
        {
            counter++;

            if (counter < 75) return;
            counter = 0;

            // doSkid = false;
        }
    }
}
