using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

public class Track : MonoBehaviour
{
    private Vector3 checkpointPosition;
    private Quaternion checkpointRotation;
    private uint lastPassedCheckpointIndex = 0;

    private bool moveToCheckpoint = false;
    private float timeUntilMoveToCheckpoint = 0f;

    private Vehicle vehicle;

    private Dictionary<Checkpoint, bool> requiredCheckpoints = new Dictionary<Checkpoint, bool>();

    public delegate void LapCompletedEventHandler();
    public event LapCompletedEventHandler OnLapCompleted;

    private Material emissionMat;
    private AudioSource vehicleAudioSource;
    private CarController carController;
    private ExplosionEffect vehicleExplosionEffect;
    private GameObject vehiclePlaceholder;

    [HideInInspector] public TrackSettings trackSettings;

    void Start()
    {
        name = "Track";

        vehicleAudioSource = vehicle.GetComponent<AudioSource>();
        carController = vehicle.GetComponent<CarController>();
        vehicleExplosionEffect = vehicle.GetComponent<ExplosionEffect>();
        trackSettings = GetComponent<TrackSettings>();

        InitEmissionMaterial();
        InitTrackSegments();
        InitBrandingSigns();
        InitBackgroundEffects();
        InitWeather();
        InitVehicleStartBox();
        InitLapEndBox();
        InitTimeBoosts();
        InitObstacles();
        InitCarProps();
        InitBackground();
        InitWall();
        InitExcludeMobile();
        
        InitMeshCombiner();
    }

    private SwitchTrackOptimizer trackOptimizer;
    private void InitMeshCombiner()
    {
        Material roadMat = null;
        if (trackSettings != null)
        {
            roadMat = trackSettings.roadMaterial;

        }

        trackOptimizer = new SwitchTrackOptimizer(transform)
        {
            IsEnabled = true,
            roadMaterial = roadMat
        };
        trackOptimizer.InitMeshCombiner();
    }

    private void InitEmissionMaterial()
    {
        // Emission at run time works in Editor, but not in build
        // You don't need to use the material on anything because as 
        // long as it's in the Resources folder Unity will presume it's going to be needed.
        emissionMat = Instantiate(Resources.Load("EmissionMat")) as Material;
    }
    
    private void InitTrackSegments()
    {
        List<GameObject> trackSegments = transform.Find("track").gameObject.FindGameObjectChildrenWithPartialName("segment_");
        
        foreach (GameObject trackSegment in trackSegments)
        {
            //Debug.Log("trackSegment: " + trackSegment.GetComponent<MeshFilter>().mesh.subMeshCount);

            trackSegment.AddComponent<DetectVehicleStuck>();
            trackSegment.AddComponent<MeshCollider>();

            // suppress warning CS0219: The variable `checkpoint' is assigned but its value is never used
            #pragma warning disable 219

            //checkpoints
            List<GameObject> checkpointGameObjects = trackSegment.FindGameObjectChildrenWithPartialName("checkpoint_");
            foreach (GameObject checkpointPlaceholder in checkpointGameObjects)
            {
                Checkpoint checkpoint = checkpointPlaceholder.gameObject.AddComponent<Checkpoint>();
                if (!PersistentModel.Instance.DEBUG) checkpointPlaceholder.GetComponent<MeshRenderer>().enabled = false;
                checkpointPlaceholder.AddComponent<BoxCollider>().isTrigger = true;

                // separating the checkpoints from its segment
                // to help support mesh combining for the segments
                // submeshes are not entirely supported. 
                // The segment mesh needs to be clean/ without submeshes(elements in max) and share the same material
                checkpoint.transform.SetParent(transform);
                
                //TODO uncomment this once the demo is done
                //if (checkpoint.name.Contains("_required"))
                //{
                //    requiredCheckpoints.Add(checkpoint, false);
                //    checkpoint.OnTriggered += OnRequiredCheckpointTriggered;
                //}
            }
            #pragma warning restore 219
            
            //boosts
            List<GameObject> boostGameObjects = ExtensionMethods.FindGameObjectChildrenWithPartialName(trackSegment, "boost_");
            foreach (GameObject boostPlaceholder in boostGameObjects)
            {
                if (!PersistentModel.Instance.DEBUG) boostPlaceholder.GetComponent<MeshRenderer>().enabled = false;
                Destroy(boostPlaceholder);
                
                //boostPlaceholder.AddComponent<BoxCollider>().isTrigger = true;
                //boostPlaceholder.AddComponent<Boost>();
            }
        }
    }
   
    private void InitWeather()
    {
        List<GameObject> weatherBoxes = transform.Find("track").gameObject.FindGameObjectChildrenWithPartialName("weather_");
        foreach (GameObject weather in weatherBoxes) weather.AddComponent<Weather>();
    }

    private void InitBrandingSigns()
    {
        StartCoroutine(LoadBrandImage());
    }

    // obstacles->vehicle_prop - is the car - needs collider
    // FINISH LINE - background->sign_finish
    // START LINE - background->sign_start
    // background->monster_eyes
    // background->monster_mouth
    // background->tree_eyes
    IEnumerator LoadBrandImage()
    {
        // *********** Update Brand Logos if any
        string textureId = (PersistentModel.Instance.ChallengeBrandName == "firestone") ? "signs_firestone_logos" : "signs_bridgestone_logos";
        Texture brandTexture = Instantiate(Resources.Load(textureId)) as Texture;
        yield return brandTexture;

        List<GameObject> signs = transform.Find("background").gameObject.FindGameObjectChildrenWithPartialName("signs_branding_");
        bool isCombineSigns = true;// (signs.Count > 1);
        List<MeshFilter> signMeshesToCombine = null;
        if (isCombineSigns) signMeshesToCombine = new List<MeshFilter>();

        Material sharedBrandMat = new Material(emissionMat);
        sharedBrandMat.mainTexture = brandTexture;

        foreach (GameObject sign in signs)
        {
            if (PersistentModel.Instance.GameNight)
            {
                sign.GetComponent<MeshRenderer>().receiveShadows = false;
                sign.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            else
            {
                sign.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            if (isCombineSigns) signMeshesToCombine.Add(sign.GetComponent<MeshFilter>());
        }

        if (isCombineSigns)
        {
            CombineInstance[] combineSigns = new CombineInstance[signMeshesToCombine.Count];
            int i = 0;
            while (i < signMeshesToCombine.Count)
            {
                // combine track segment mesh
                combineSigns[i].mesh = signMeshesToCombine[i].sharedMesh;
                combineSigns[i].transform = signMeshesToCombine[i].transform.localToWorldMatrix;

                GameObject.Destroy(signMeshesToCombine[i].gameObject);
               
                i++;
            }

            GameObject g = new GameObject("combinedBrandSigns");
            g.AddComponent<MeshFilter>().mesh = new Mesh();
            g.GetComponent<MeshFilter>().mesh.CombineMeshes(combineSigns);
            g.AddComponent<MeshRenderer>().sharedMaterial = sharedBrandMat;

            if (PersistentModel.Instance.GameNight)
                SetEmmisionMaterial(sharedBrandMat, new Color(0.5f, 0.5f, 0.5f, 1.0f) * 1.5f);
            else sharedBrandMat.SetColor("_EmissionColor", Color.black);

            g.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }


        // *********** Update Tire Logos if any
        string textureLogoId = (PersistentModel.Instance.ChallengeBrandName == "firestone") ? "signs_firestone_tire_logos" : "signs_bridgestone_tire_logos";
        Texture brandLogoTexture = Instantiate(Resources.Load(textureLogoId)) as Texture;
        yield return brandLogoTexture;

        List<GameObject> logos = transform.Find("background").gameObject.FindGameObjectChildrenWithPartialName("logos_");
        bool isCombineLogos = true;// (logos.Count > 1);
        List<MeshFilter> logoMeshesToCombine = null;

        if (isCombineLogos) logoMeshesToCombine = new List<MeshFilter>();

        Material sharedLogoMat = new Material(emissionMat);
        sharedLogoMat.mainTexture = brandLogoTexture;

        foreach (GameObject logo in logos)
        {
            if (PersistentModel.Instance.GameNight)
            {
                logo.GetComponent<MeshRenderer>().receiveShadows = false;
                logo.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            else
            {
                logo.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            if (isCombineLogos) logoMeshesToCombine.Add(logo.GetComponent<MeshFilter>());
        }

        if (isCombineLogos)
        {
            CombineInstance[] combineLogos = new CombineInstance[logoMeshesToCombine.Count];
            int i = 0;
            while (i < logoMeshesToCombine.Count)
            {
                // combine track segment mesh
                combineLogos[i].mesh = logoMeshesToCombine[i].sharedMesh;
                combineLogos[i].transform = logoMeshesToCombine[i].transform.localToWorldMatrix;

                GameObject.Destroy(logoMeshesToCombine[i].gameObject);

                i++;
            }

            GameObject g = new GameObject("combinedLogoSigns");
            g.AddComponent<MeshFilter>().mesh = new Mesh();
            g.GetComponent<MeshFilter>().mesh.CombineMeshes(combineLogos);
            g.AddComponent<MeshRenderer>().sharedMaterial = sharedLogoMat;

            if (PersistentModel.Instance.GameNight)
                SetEmmisionMaterial(sharedLogoMat, new Color(0.5f, 0.5f, 0.5f, 1.0f) * 1.5f);
            else sharedLogoMat.SetColor("_EmissionColor", Color.black);

            g.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    // Optional scene effects
    private void InitBackgroundEffects()
    {
        Material mat;   // using as emissionMat clone

        // Set emmission for certain objects in background scene
        if (PersistentModel.Instance.GameNight)
        {
            // Emmit Start and Finish Signs at night only
            GameObject startSign = transform.Find("background").gameObject.FindGameObjectChildWithName("sign_start");
            if (startSign)
            {
                mat = new Material(emissionMat);
                mat.mainTexture = startSign.GetComponent<MeshRenderer>().sharedMaterial.mainTexture; ;
                startSign.GetComponent<MeshRenderer>().sharedMaterial = mat;
                SetEmmisionMaterial(mat, new Color(0.5f, 0.5f, 0.5f, 1.0f) * 1.5f);

                startSign.GetComponent<MeshRenderer>().receiveShadows = false;
                startSign.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            GameObject finishedSign = transform.Find("background").gameObject.FindGameObjectChildWithName("sign_finish");
            if (finishedSign)
            {
                mat = new Material(emissionMat);
                mat.mainTexture = finishedSign.GetComponent<MeshRenderer>().sharedMaterial.mainTexture; ;
                finishedSign.GetComponent<MeshRenderer>().sharedMaterial = mat;
                SetEmmisionMaterial(mat, new Color(0.5f, 0.5f, 0.5f, 1.0f) * 1.5f);

                finishedSign.GetComponent<MeshRenderer>().receiveShadows = false;
                finishedSign.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            GameObject treeEyes = transform.Find("background").gameObject.FindGameObjectChildWithName("tree_eyes");
            if (treeEyes)
            {
                mat = new Material(emissionMat);
                mat.mainTexture = treeEyes.GetComponent<MeshRenderer>().material.mainTexture;
                SetEmmisionMaterial(mat, new Color(1.0f, 0.0f, 1.0f) * 25.0f);
                treeEyes.GetComponent<MeshRenderer>().material = mat;
                treeEyes.GetComponent<MeshRenderer>().receiveShadows = false;
                treeEyes.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            GameObject monsterEyes = transform.Find("background").gameObject.FindGameObjectChildWithName("monster_eyes");
            if (monsterEyes)
            {
                mat = new Material(emissionMat);
                mat.mainTexture = monsterEyes.GetComponent<MeshRenderer>().material.mainTexture;
                SetEmmisionMaterial(mat, new Color(5.0f, 0.6f, 0.0f) * 1.0f);
                monsterEyes.GetComponent<MeshRenderer>().material = mat;
                monsterEyes.GetComponent<MeshRenderer>().receiveShadows = false;
                monsterEyes.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            GameObject monsterMouth = transform.Find("background").gameObject.FindGameObjectChildWithName("monster_mouth");
            if (monsterMouth)
            {
                mat = new Material(emissionMat);
                mat.mainTexture = monsterMouth.GetComponent<MeshRenderer>().material.mainTexture;
                SetEmmisionMaterial(mat, new Color(0.6f, 0.0f, 0.0f) * 2.0f);
                monsterMouth.GetComponent<MeshRenderer>().material = mat;
                monsterMouth.GetComponent<MeshRenderer>().receiveShadows = false;
                monsterMouth.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
    }

    private void SetEmmisionMaterial(Material material, Color emissionColor)
    {
        //material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", emissionColor);
        material.SetTexture("_EmissionMap", material.mainTexture);
        material.EnableKeyword("_EMISSION");
    }

    private void InitVehicleStartBox()
    {
        vehiclePlaceholder.GetComponent<MeshRenderer>().enabled = false;
    }
    
    private void InitLapEndBox()
    {
        GameObject lapEndBox = transform.Find("lap_end").gameObject;
        if (!PersistentModel.Instance.DEBUG) lapEndBox.GetComponent<MeshRenderer>().enabled = false;
        lapEndBox.AddComponent<BoxCollider>().isTrigger = true;
        lapEndBox.AddComponent<Endpoint>().OnEnded += OnLapEnded;
    }
    
    private void InitTimeBoosts()
    {
        //stars are just time boosts
        GameObject star;
        Transform[] starTransforms = transform.Find("stars").GetChildren();
        foreach (Transform starPlaceholder in starTransforms)
        {
            star = Instantiate(Resources.Load("star")) as GameObject;
            star.AddComponent<TimeBoost>();
            star.name = starPlaceholder.name;

            star.transform.parent = starPlaceholder.transform.parent;
            star.transform.SetPositionAndRotation(starPlaceholder.transform.position, new Quaternion(0f, 0f, 0f, 0f));
            star.transform.Translate(new Vector3(0f, 0f, .9f));

            Destroy(starPlaceholder.gameObject);
        }
    
        Transform[] transforms = transform.Find("boosts").GetChildren();

		foreach (Transform placeholder in transforms) 
		{
            Destroy(placeholder.gameObject);
			//placeholder.gameObject.AddComponent<TimeBoost>();
		}
    }
    
    private void InitObstacles()
    {
        Transform[] transforms = transform.Find("obstacles").GetChildren();
		
        PhysicMaterial colliderPhysicalMat = new PhysicMaterial();

        Rigidbody rigidbody = null;
        BoxCollider boxCollider = null;
		foreach (Transform obstacleTrasform in transforms)
        {
            rigidbody = obstacleTrasform.gameObject.AddComponent<Rigidbody>();
            obstacleTrasform.gameObject.AddComponent<Obstacle>();

            // fow SlowDown func -> Vehicle.cs
            rigidbody.mass = 30f; 
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rigidbody.drag = 200f; 
            rigidbody.angularDrag = 15f;
            rigidbody.useGravity = false;
            
            // Using BoxCollider to avoid the vehicle from sliding up against the object
            /*var meshCollider = obstacleTrasform.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMaterial = mat;
            meshCollider.sharedMaterial.staticFriction = .001f;
            meshCollider.sharedMaterial.dynamicFriction = .001f;
            meshCollider.convex = true;
            */

            boxCollider = obstacleTrasform.gameObject.AddComponent<BoxCollider>();
            boxCollider.sharedMaterial = colliderPhysicalMat;
            boxCollider.sharedMaterial.staticFriction = .001f;
            boxCollider.sharedMaterial.dynamicFriction = .001f;
        }
    }

    private void InitCarProps()
    {
        Transform carProps = transform.Find("car_props");

        if (!carProps) return;

        Transform[] transforms = carProps.GetChildren();

        PhysicMaterial colliderPhysicalMat = new PhysicMaterial();

        Rigidbody carPropRB;
        BoxCollider boxCollider;
        foreach (Transform carPropTransform in transforms)
        {
            carPropRB = carPropTransform.gameObject.AddComponent<Rigidbody>();
            carPropRB.mass = 1800f;
            carPropRB.drag = 0.1f;
            carPropRB.useGravity = true;

            carPropTransform.gameObject.AddComponent<DetectVehicleStuck>();
            carPropTransform.gameObject.AddComponent<Obstacle>();
            carPropTransform.gameObject.GetComponent<Obstacle>().hasRigidBody = false;

            boxCollider = carPropTransform.gameObject.AddComponent<BoxCollider>();
            boxCollider.sharedMaterial = colliderPhysicalMat;
            boxCollider.sharedMaterial.staticFriction = .001f;
            boxCollider.sharedMaterial.dynamicFriction = .001f;
            
        }
    }

    private void InitBackground()
    {
        Transform[] backgroundTransforms = transform.Find("background").GetAllChildren();
        foreach (Transform backgroundTranform in backgroundTransforms)
        {
            if (backgroundTranform.gameObject.name != "sign_start"
                && backgroundTranform.gameObject.name != "sign_finish"
                && backgroundTranform.gameObject.name != "monster_eyes"
                && backgroundTranform.gameObject.name != "monster_mouth"
                && backgroundTranform.gameObject.name != "Sign_rain_01"
                && backgroundTranform.gameObject.name != "signsd_sun"
                && backgroundTranform.gameObject.name != "signsd_sun001"
                && backgroundTranform.gameObject.name != "signsd_sun002"
                && backgroundTranform.gameObject.name != "Object002"
                && backgroundTranform.gameObject.name != "tree_eyes")
            {
                backgroundTranform.gameObject.AddComponent<MeshCollider>();
                backgroundTranform.gameObject.AddComponent<RestartRaceOnCollide>();
            }
        }
    }

    private void InitWall()
    {
        Transform wallTransform = transform.Find("wall");
        if (wallTransform != null)
        {
            wallTransform.gameObject.AddComponent<MeshCollider>();
            wallTransform.gameObject.AddComponent<DetectVehicleStuck>();
        }

        Transform wallTransform2 = transform.Find("wall_collider");
        if (wallTransform2 != null)
        {
            wallTransform2.gameObject.GetComponent<MeshRenderer>().enabled = false;
            wallTransform2.gameObject.AddComponent<MeshCollider>();
            wallTransform2.gameObject.AddComponent<DetectVehicleStuck>();
        }
    }
    
    private void InitExcludeMobile()
    {
        Transform excludeTransform = transform.Find("exclude_mobile");

        if (excludeTransform == null) return;
        if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.Android) return;
        
        Destroy(excludeTransform.gameObject);
    }

    private void OnRequiredCheckpointTriggered(Checkpoint checkpoint)
    {
        DebugLog.Trace("onRequiredCheckpoint");

        requiredCheckpoints[checkpoint] = true;
    }

    private void OnLapEnded()
    {
        DebugLog.Trace("Track.onLapEnded");

        // abort if any required checkpoints have not been entered
        foreach (KeyValuePair<Checkpoint, bool> keyValuePair in requiredCheckpoints) if (keyValuePair.Value != true) return;

        // reset all required checkpoints
        List<Checkpoint> keys = new List<Checkpoint>(requiredCheckpoints.Keys);
        foreach (Checkpoint key in keys) requiredCheckpoints[key] = false;

        OnLapCompleted();
    }

    public void PositionVehicleAtStartPosition(Vehicle vehicle)
    {
        this.vehicle = vehicle;

        if (vehiclePlaceholder == null) vehiclePlaceholder = transform.Find("vehicle_start_box").gameObject;

        vehicle.transform.SetParent(vehiclePlaceholder.transform.parent);
        vehicle.transform.SetPositionAndRotation(vehiclePlaceholder.transform.position, vehiclePlaceholder.transform.rotation);
        vehicle.transform.Rotate(new Vector3(0f, 90f, 90f));

        checkpointPosition = vehicle.transform.position;
        checkpointRotation = vehicle.transform.rotation;
    }

    public void PositionVehicleAtCheckpoint(bool immediately)
    {
        if (immediately) //do not wait
        {
            vehicle.transform.SetPositionAndRotation(checkpointPosition, checkpointRotation);
            carController.Reset();
        }
        else //do crash sequence
        {
            vehicleAudioSource.PlayOneShot(vehicle.AudioCrashClip);
            vehicleExplosionEffect.Explode();

            moveToCheckpoint = true;
        }
    }

    private Checkpoint checkpoint;
    public void StoreCheckpoint(GameObject checkpointGameObject)
    {
        checkpoint = checkpointGameObject.GetComponent<Checkpoint>();

        //if the checkpoint index they have gone through is lower than the last one they went through, they are going the wrong way
        //if the checpoint index they have gone through is greater than the last index + 2, they probably went in reverse through the starting point
        if (checkpoint.Index < lastPassedCheckpointIndex || checkpoint.Index >= lastPassedCheckpointIndex + 2)
        {
            //TODO, uncomment this once the designers update the tracks so this may work
            PositionVehicleAtCheckpoint(true);
            return;
        }

        checkpoint.Passed = true;
        checkpointPosition = checkpoint.SavedVehiclePosition;
        checkpointRotation = checkpoint.SavedVehicleRotation;
        lastPassedCheckpointIndex = checkpoint.Index;
    }

    void Update()
    {
        //if the move to checkpoint flag is true, it will wait a specified amount of time before it moves the vehicle to the checkpoint
        if (moveToCheckpoint)
        {
            timeUntilMoveToCheckpoint += Time.deltaTime;

            if (!vehicleAudioSource.isPlaying || timeUntilMoveToCheckpoint > 1f)
            {
                vehicle.transform.SetPositionAndRotation(checkpointPosition, checkpointRotation);

                carController.Reset();

                vehicleExplosionEffect.Stop();

                timeUntilMoveToCheckpoint = 0f;

                moveToCheckpoint = false;
            }
        }
    }
}
