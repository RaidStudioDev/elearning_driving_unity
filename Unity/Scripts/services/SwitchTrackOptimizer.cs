using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * SwitchTrackOptimizer iterates through the track and combines selected meshes into one
 * 
 * This optimizes the level and brings down the batch and pass calls
 * It will also remove any unessassary 
 * 
 */

public class SwitchTrackOptimizer {

    public bool IsEnabled = true;
    public Material roadMaterial;

    private List<MeshFilter> trackMeshesToCombine;
    private CombineInstance[] combinedMeshes;
    private Material sharedTrackMaterial;

    private List<MeshFilter> trackSegmentMeshesToCombine;
    private CombineInstance[] combinedSegmentMeshes;
    private Material sharedSegmentMaterial;

    private List<int> trackSubMeshesIds;
    
    private Transform transform;
    
    public SwitchTrackOptimizer(Transform t)
    {
        transform = t;
    }

    public void InitMeshCombiner()
    {
        if (!ValidateTrack() || !IsEnabled) return;

        // process track scenery
        trackMeshesToCombine = new List<MeshFilter>();
        sharedTrackMaterial = null;
        ProcessTrack();
        CombineMeshesFromTrack();


        // process segments / road
        trackSegmentMeshesToCombine = new List<MeshFilter>();
        trackSubMeshesIds = new List<int>();
        sharedSegmentMaterial = null;
        ProcessSegment();
        CombineSegmentMeshesFromTrack();

        // check if we have any submeshes from the segment meshes
        // if so, we will need to update the background track combined mesh
        if (trackSubMeshesIds.Count > 0)
        {
            // Debug.Log("Segment SubMeshes found, combining them with main background mesh");
            CombineMeshesFromTrack();
        }

        // check for dedicate sub mesh procession
        if (isWindinwRoadSegmentsAvailable)
        {
            CombineWindingSubMeshes();
        }

        if (isOffRoadSegmentsAvailable)
        {
            CombineIslandOffroadSubMeshes();
        }

        trackMeshesToCombine.Clear();
        trackSegmentMeshesToCombine.Clear();
        trackSubMeshesIds.Clear();
    }

    private bool ValidateTrack()
    {
        // Debug.Log("ValidateTrack: " + PersistentModel.Instance.GameTrack);

        switch (PersistentModel.Instance.GameTrack)
        {
            case "Island1":
            case "Island2":
            case "Island3":
            case "IslandOffroad1":
            case "IslandOffroad2":
            case "IslandOffroad3":
            case "IslandSnow1":
            case "IslandSnow2":
            case "IslandSnow3":
            case "Highway1":
            case "Highway2":
            case "Highway3":
            case "HighwaySnow1":
            case "HighwaySnow2":
            case "HighwaySnow3":
            case "WindingRoad1":
            case "WindingRoad2":
            case "WindingRoad3":
            case "WindingSnow1":
            case "WindingSnow2":
            case "WindingSnow3":
            case "City1":
            case "City2":
            case "City3":
            case "CitySnow1":
            case "CitySnow2":
            case "CitySnow3":
            case "Racing1":
            case "Racing2":
            case "Racing3":
                return true;

            default:
                return false;
        }
    }

    /*
       Iterates Scene Meshes, adds them to one combined mesh 
   */
    private void ProcessTrack()
    {
        // Debug.Log("ProcessTrack");

        Transform[] backgroundTransforms = transform.Find("background").GetAllChildren();
        foreach (Transform backgroundTranform in backgroundTransforms)
        {
            if (backgroundTranform.gameObject.name.ToLower() == "scenario")
            {
                for (int matSceneIndex = 0; matSceneIndex < backgroundTranform.GetComponent<Renderer>().sharedMaterials.Length; matSceneIndex++)
                {
                    Material sharedSceneMat = backgroundTranform.GetComponent<Renderer>().sharedMaterials[matSceneIndex];

                    // for Island Only Tracks
                    if (sharedSceneMat.name == "MAIN")
                    {
                        backgroundTranform.GetComponent<Renderer>().sharedMaterials = new Material[] { sharedSceneMat };
                        backgroundTranform.GetComponent<Renderer>().sharedMaterial = sharedSceneMat;
                    }
                }

                sharedTrackMaterial = backgroundTranform.GetComponent<Renderer>().sharedMaterial;
                trackMeshesToCombine.Add(backgroundTranform.GetComponent<MeshFilter>());
            }

            if (backgroundTranform.gameObject.name == "ground"
                || backgroundTranform.gameObject.name == "startgate")                
            {
                backgroundTranform.GetComponent<Renderer>().sharedMaterial = sharedTrackMaterial;
                trackMeshesToCombine.Add(backgroundTranform.GetComponent<MeshFilter>());
            }
        }

        Transform[] otherTransforms = (transform.Find("other") != null) ? transform.Find("other").GetAllChildren() : new Transform[] { };

        foreach (Transform otherTranform in otherTransforms)
        {
            if (otherTranform.gameObject.name.ToLower() == "scenario")
            {
                sharedTrackMaterial = otherTranform.GetComponent<Renderer>().sharedMaterial;
            }

            // Debug.Log("otherTranform.gameObject.name:" + otherTranform.gameObject.name);
            if (otherTranform.gameObject.name.ToLower() == "scenario"
                || otherTranform.gameObject.name == "startgate"
                //|| otherTranform.gameObject.name == "finish"
                || otherTranform.gameObject.name.Contains("train")
                || otherTranform.gameObject.name.Contains("gate")
                || otherTranform.gameObject.name == "lake"
                || otherTranform.gameObject.name == "signs")
            {
                otherTranform.GetComponent<Renderer>().sharedMaterial = sharedTrackMaterial;
                trackMeshesToCombine.Add(otherTranform.GetComponent<MeshFilter>());

                if (PersistentModel.Instance.GameNight)
                {
                    otherTranform.GetComponent<Renderer>().receiveShadows = false;
                    otherTranform.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }

            }
            else if (otherTranform.gameObject.name == "Traffic_signals"
                || otherTranform.gameObject.name == "solar pannels")
            {
                otherTranform.GetComponent<Renderer>().receiveShadows = false;
                otherTranform.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
        
        // Winding Windmills
        if (otherTransforms.Length > 0)
        {
            List<GameObject> windmills = transform.Find("other").gameObject.FindGameObjectChildrenWithPartialName("windmill");
            foreach (GameObject windmillObj in windmills)
            {
                windmillObj.GetComponent<MeshRenderer>().sharedMaterial = sharedTrackMaterial;
                windmillObj.GetComponent<Renderer>().receiveShadows = false;
                windmillObj.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            windmills = transform.Find("other").gameObject.FindGameObjectChildrenWithPartialName("fence_");
            foreach (GameObject fenceObj in windmills)
            {
                fenceObj.GetComponent<Renderer>().receiveShadows = false;
                fenceObj.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
       
        // Walls
        Transform wallTransform = transform.Find("wall");
        if (wallTransform != null)
        {
            trackMeshesToCombine.Add(wallTransform.GetComponent<MeshFilter>());

            wallTransform.GetComponent<Renderer>().sharedMaterial = sharedTrackMaterial;

            if (PersistentModel.Instance.GameNight)
            {
                wallTransform.GetComponent<Renderer>().receiveShadows = false;
                wallTransform.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        Transform wallTransform2 = transform.Find("wall_collider");
        if (wallTransform2 != null)
        {
            // trackMeshesToCombine.Add(wallTransform2.GetComponent<MeshFilter>());
        }

        // mobile 
        Transform excludeTransform = transform.Find("exclude_mobile");
        if (excludeTransform != null)
        {

            List<GameObject> fences = excludeTransform.gameObject.FindGameObjectChildrenWithPartialName("fence");
            foreach (GameObject fance in fences)
            {
                fance.GetComponent<Renderer>().receiveShadows = false;
                fance.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
    }

    private GameObject combinedBackground;
    private void CombineMeshesFromTrack()
    {
        if (trackMeshesToCombine.Count == 0) return;

        CombineInstance[] combine = new CombineInstance[trackMeshesToCombine.Count];
        int i = 0;
        while (i < trackMeshesToCombine.Count)
        {
            combine[i].mesh = trackMeshesToCombine[i].sharedMesh;
            combine[i].transform = trackMeshesToCombine[i].transform.localToWorldMatrix;

            if (trackMeshesToCombine[i].gameObject.name != "wall")
            {
                trackMeshesToCombine[i].gameObject.SetActive(false);
            }
            else
            {
                trackMeshesToCombine[i].gameObject.GetComponent<MeshRenderer>().enabled = false;
            }

            i++;
        }

        if (!combinedBackground)
        {
            // sharedTrackMaterial.enableInstancing = true;
            combinedBackground = new GameObject("combinedBackground");
            combinedBackground.AddComponent<MeshFilter>().mesh = new Mesh();
            combinedBackground.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
            combinedBackground.AddComponent<MeshRenderer>().sharedMaterial = sharedTrackMaterial;
        }
        else
        {
            combinedBackground.GetComponent<MeshFilter>().mesh.Clear();
            combinedBackground.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
            combinedBackground.GetComponent<MeshRenderer>().sharedMaterial = sharedTrackMaterial;
        }
    }

    /*
        Iterates Segment Meshes, adds them to one combined mesh 
        Extracts any SubMeshes inside the Segment Meshes
        Any SubMeshes found will be combined with the Background Combined Mesh
    */
    private void ProcessSegment()
    {
        // Debug.Log("ProcessSegment");

        List<GameObject> trackSegments = transform.Find("track").gameObject.FindGameObjectChildrenWithPartialName("segment_");
        foreach (GameObject trackSegmentItem in trackSegments)
        {
            // cache sharedSegmentMaterial we will apply to the combined mesh
            if (!sharedSegmentMaterial) sharedSegmentMaterial = trackSegmentItem.GetComponent<Renderer>().sharedMaterial;

            trackSegmentMeshesToCombine.Add(trackSegmentItem.GetComponent<MeshFilter>());
        }
    }

    private void CombineSegmentMeshesFromTrack()
    {
        if (trackSegmentMeshesToCombine.Count == 0) return;

        CombineInstance[] combine = new CombineInstance[trackSegmentMeshesToCombine.Count];

        bool doNotCombineSegmentTrack = false;

        // check for submeshes and look for potholes, etc...
        // we will then remove them from the segment into
        // its own mesh, then combining all submeshes into one mesh
        // and set shared material
        if (trackSegmentMeshesToCombine.Count > 0)
        {
            // when we combine all the segments, we can't have submeshes with different materials
            // the submesh will lose the reference to the assigned material when combined.
            // iterate though the submeshes from the sement mesh
            int i = 0;
            while (i < trackSegmentMeshesToCombine.Count)
            {
                //Debug.Log("trackSegmentMeshesToCombine[i].mesh.name: " + trackSegmentMeshesToCombine[i].gameObject.name);
                //Debug.Log("trackSegmentMeshesToCombine[i].mesh.subMeshCount: " + trackSegmentMeshesToCombine[i].mesh.subMeshCount);

                // check for specific tracks to process differently
                if (PersistentModel.Instance.GameTrack.Contains("Winding")
                    || PersistentModel.Instance.GameTrack.Contains("Racing")
                    || PersistentModel.Instance.GameTrack.Contains("Highway"))
                {
                    doNotCombineSegmentTrack = true;
                    WindingTrackSubMeshProcess(i);
                }
                else if (PersistentModel.Instance.GameTrack.Contains("IslandOffroad")
                    || PersistentModel.Instance.GameTrack.Contains("IslandSnow"))
                {
                    doNotCombineSegmentTrack = true;
                    IslandOffroadTrackSubMeshProcess(i);
                }
                else
                {
                    // submesh processs if any
                    SegmentSubMeshProcess(i);

                    // combine track segment mesh
                    combine[i].mesh = trackSegmentMeshesToCombine[i].sharedMesh;
                    combine[i].transform = trackSegmentMeshesToCombine[i].transform.localToWorldMatrix;

                    trackSegmentMeshesToCombine[i].gameObject.GetComponent<MeshRenderer>().enabled = false;
                }

                i++;
            }
        }

        if (doNotCombineSegmentTrack) return;

        GameObject g = new GameObject("combinedSegments");
        g.AddComponent<MeshFilter>().mesh = new Mesh();
        g.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        g.AddComponent<MeshRenderer>().sharedMaterial = sharedSegmentMaterial;
        
        if (PersistentModel.Instance.GameNight && PersistentModel.Instance.GameTrack.Contains("City"))
        {
            g.GetComponent<MeshRenderer>().receiveShadows = false;
            g.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
            
    }

    // Process SubMeshes in Segment
    private void SegmentSubMeshProcess(int index)
    {
        // before we combine the track segment meshes, we are going to check the mesh for
        // any submeshes that need to be extracted
        // check for the materials in the segment mesh
        // this will let us know if we have a submesh with a different material assigned to it
        // if it does we check for the Material Name ( hard coded ) if we find a match
        // extract it from the segment mesh and create a new game object
        // after this process, will will combine the submeshes to the background track combined mesh
        Material[] materials = trackSegmentMeshesToCombine[index].GetComponent<MeshRenderer>().materials;
        int selectedMaterialIndex = -1;
        for (int matIndex = 0; matIndex < materials.Length; matIndex++)
        {
            // Debug.Log("materials[matIndex].name: " + materials[matIndex].name);
            // Debug.Log("materials[matIndex].mainTexture.name: " + materials[matIndex].mainTexture.name);

            // check if the material names match
            // if so, select/tag it for later extraction
            if (materials[matIndex].name.Contains("Main_Material_"))
            {
                selectedMaterialIndex = matIndex;
                break;
            }
        }

        // Debug.Log("material.selectedMaterialIndex: " + selectedMaterialIndex);

        // if we found a Material match, create a gameobject
        // with the selected submesh
        if (selectedMaterialIndex > 0)
        {
            // GetSubmesh() ref: https://answers.unity.com/questions/1213025/separating-submeshes-into-unique-meshes.html
            Mesh subMesh = trackSegmentMeshesToCombine[index].mesh.GetSubmesh(selectedMaterialIndex);

            // create a combined instance mesh and set its local to world position
            CombineInstance[] subMeshCombine = new CombineInstance[1];
            subMeshCombine[0].mesh = subMesh;
            subMeshCombine[0].transform = trackSegmentMeshesToCombine[index].transform.localToWorldMatrix;

            // create game object sub mesh
            GameObject subMeshObj = new GameObject(trackSegmentMeshesToCombine[index].gameObject.name + "_submesh_" + selectedMaterialIndex);
            subMeshObj.AddComponent<MeshFilter>().mesh = new Mesh();
            subMeshObj.GetComponent<MeshFilter>().mesh.CombineMeshes(subMeshCombine);
            subMeshObj.AddComponent<MeshRenderer>().sharedMaterial = sharedTrackMaterial;

            // add to track meshes, we will need to reupdate the trackCombinedMesh
            trackMeshesToCombine.Add(subMeshObj.GetComponent<MeshFilter>());

            // add to track submeshes
            trackSubMeshesIds.Add(selectedMaterialIndex);
        }

    }

    // WINDING TRACK Processor
    private List<MeshFilter> trackWindingSegmentMat1ToCombine;  // track_vaden_grass_half
    private List<MeshFilter> trackWindingSegmentMat2ToCombine;  // track_vaden
    private List<MeshFilter> trackWindingSegmentMat3ToCombine;  // track_asphalt
    private List<MeshFilter> trackWindingSegmentMat4ToCombine;  // track_piano
    private List<MeshFilter> trackWindingSegmentMat5ToCombine;  // track_vaden_grass_trans
    private List<MeshFilter> trackWindingSegmentMat6ToCombine;  // track_vaden_grass

    private bool isWindinwRoadSegmentsAvailable = false;

    private void WindingTrackSubMeshProcess(int index)
    {
        if (trackWindingSegmentMat1ToCombine == null)
        {
            trackWindingSegmentMat1ToCombine = new List<MeshFilter>();
            trackWindingSegmentMat2ToCombine = new List<MeshFilter>();
            trackWindingSegmentMat3ToCombine = new List<MeshFilter>();
            trackWindingSegmentMat4ToCombine = new List<MeshFilter>();
            trackWindingSegmentMat5ToCombine = new List<MeshFilter>();
            trackWindingSegmentMat6ToCombine = new List<MeshFilter>();
        }

        // Debug.Log("WindingTrackSubMeshProcess()");
        // Debug.Log("trackSegmentMeshesToCombine[i].mesh.name: " + trackSegmentMeshesToCombine[index].gameObject.name);
        // Debug.Log("trackSegmentMeshesToCombine[i].mesh.subMeshCount: " + trackSegmentMeshesToCombine[index].mesh.subMeshCount);

        Material[] materials = trackSegmentMeshesToCombine[index].GetComponent<MeshRenderer>().sharedMaterials;
        for (int matIndex = 0; matIndex < materials.Length; matIndex++)
        {
            // Debug.Log("materials[matIndex].name: " + materials[matIndex].name);

            if (materials[matIndex].name.Contains("track_vaden_grass_half"))
            {
                isWindinwRoadSegmentsAvailable = true;
                CreateSubMeshFromSegment(index, "track_vaden_grass_half", matIndex, materials[matIndex], ref trackWindingSegmentMat1ToCombine);
            }
            else if (materials[matIndex].name.Contains("track_vaden"))
            {
                isWindinwRoadSegmentsAvailable = true;
                CreateSubMeshFromSegment(index, "track_vaden", matIndex, materials[matIndex], ref trackWindingSegmentMat2ToCombine);
            }
            else if (materials[matIndex].name.Contains("track_asphalt"))
            {
                isWindinwRoadSegmentsAvailable = true;
                CreateSubMeshFromSegment(index, "track_asphalt", matIndex, materials[matIndex], ref trackWindingSegmentMat3ToCombine);
            }
            else if (materials[matIndex].name.Contains("track_piano"))
            {
                isWindinwRoadSegmentsAvailable = true;
                CreateSubMeshFromSegment(index, "track_piano", matIndex, materials[matIndex], ref trackWindingSegmentMat4ToCombine);
            }
            else if (materials[matIndex].name.Contains("track_vaden_grass_trans"))
            {
                isWindinwRoadSegmentsAvailable = true;
                CreateSubMeshFromSegment(index, "track_vaden_grass_trans", matIndex, materials[matIndex], ref trackWindingSegmentMat5ToCombine);
            }
            else if (materials[matIndex].name.Contains("track_vaden_grass"))
            {
                isWindinwRoadSegmentsAvailable = true;
                CreateSubMeshFromSegment(index, "track_vaden_grass", matIndex, materials[matIndex], ref trackWindingSegmentMat6ToCombine);
            }
        }
    }

    // ISLAND OFFROAD TRACK Processor
    private List<MeshFilter> trackOffroadSegmentMat1ToCombine;  // MAIN
    private List<MeshFilter> trackOffroadSegmentMat2ToCombine;  // brands_
    private List<MeshFilter> trackOffroadSegmentMat3ToCombine;  // 14 - Default
    private bool isOffRoadSegmentsAvailable = false;

    private void IslandOffroadTrackSubMeshProcess(int index)
    {
        if (trackOffroadSegmentMat1ToCombine == null)
        {
            trackOffroadSegmentMat1ToCombine = new List<MeshFilter>();
            trackOffroadSegmentMat2ToCombine = new List<MeshFilter>();
            trackOffroadSegmentMat3ToCombine = new List<MeshFilter>();
        }

        // Debug.Log("IslandOffroadTrackSubMeshProcess()");
        // Debug.Log("trackSegmentMeshesToCombine[i].mesh.name: " + trackSegmentMeshesToCombine[index].gameObject.name);
        // Debug.Log("trackSegmentMeshesToCombine[i].mesh.subMeshCount: " + trackSegmentMeshesToCombine[index].mesh.subMeshCount);

        Material[] materials = trackSegmentMeshesToCombine[index].GetComponent<MeshRenderer>().sharedMaterials;
        bool isAsphaltTrack = false;
        for (int matIndex = 0; matIndex < materials.Length; matIndex++)
        {
            // Debug.Log("materials[matIndex].name: " + materials[matIndex].name);

            if (materials[matIndex].name.Contains("MAIN"))
            {
                isOffRoadSegmentsAvailable = true;
                CreateSubMeshFromSegment(index, "MAIN", matIndex, materials[matIndex], ref trackOffroadSegmentMat1ToCombine);
            }
            else if (materials[matIndex].name.Contains("brands_"))
            {
                isOffRoadSegmentsAvailable = true;
                CreateSubMeshFromSegment(index, "brands_", matIndex, materials[matIndex], ref trackOffroadSegmentMat2ToCombine);
            }
            else if (materials[matIndex].name.Contains("14 - Default"))
            {
                isOffRoadSegmentsAvailable = true;
                CreateSubMeshFromSegment(index, "14 - Default", matIndex, materials[matIndex], ref trackOffroadSegmentMat3ToCombine);
            }
            else if (materials[matIndex].name.Contains("track_asphalt") && !isAsphaltTrack)
            {
                isOffRoadSegmentsAvailable = true;
                isAsphaltTrack = true;
                CreateSubMeshFromSegment(index, "track_asphalt", matIndex, materials[matIndex], ref trackOffroadSegmentMat3ToCombine);
            }
        }
    }

    private void CreateSubMeshFromSegment(int subMeshIndex, 
        string materialName, 
        int materialIndex, 
        Material material, 
        ref List<MeshFilter> meshFilterCombines)
    {
        // Debug.Log("CreateSubMeshFromSegment().materialName: " + materialName);

        Mesh subMesh = trackSegmentMeshesToCombine[subMeshIndex].mesh.GetSubmesh(materialIndex);

        // create a combined instance mesh and set its local to world position
        CombineInstance[] subMeshCombine = new CombineInstance[1];
        subMeshCombine[0].mesh = subMesh;
        subMeshCombine[0].transform = trackSegmentMeshesToCombine[subMeshIndex].transform.localToWorldMatrix;

        // create game object sub mesh
        GameObject subMeshObj = new GameObject(trackSegmentMeshesToCombine[subMeshIndex].gameObject.name + "_submesh_" + materialName);
        subMeshObj.AddComponent<MeshFilter>().mesh = new Mesh();
        subMeshObj.GetComponent<MeshFilter>().mesh.CombineMeshes(subMeshCombine);
        subMeshObj.AddComponent<MeshRenderer>().sharedMaterial = material;

        // add to track meshes, we will need to reupdate the trackCombinedMesh
        meshFilterCombines.Add(subMeshObj.GetComponent<MeshFilter>());
    }

    // WINDING, RACING, HIGHWAY TRACKS
    private void CombineWindingSubMeshes()
    {
        // Debug.Log("CombineWindingSubMeshes()");

        if (trackWindingSegmentMat1ToCombine.Count > 0)
        {
            CombineSubmeshSegmentSection(trackWindingSegmentMat1ToCombine);
        }

        if (trackWindingSegmentMat2ToCombine.Count > 0)
        {
            CombineSubmeshSegmentSection(trackWindingSegmentMat2ToCombine);
        }

        if (trackWindingSegmentMat3ToCombine.Count > 0)
        {
            CombineSubmeshSegmentSection(trackWindingSegmentMat3ToCombine);
        }

        if (trackWindingSegmentMat4ToCombine.Count > 0)
        {
            CombineSubmeshSegmentSection(trackWindingSegmentMat4ToCombine);
        }

        if (trackWindingSegmentMat5ToCombine.Count > 0)
        {
            CombineSubmeshSegmentSection(trackWindingSegmentMat5ToCombine);
        }

        if (trackWindingSegmentMat6ToCombine.Count > 0)
        {
            CombineSubmeshSegmentSection(trackWindingSegmentMat6ToCombine);
        }

        // Clean up original road/segments
        for (int segIndex = 0; segIndex < trackSegmentMeshesToCombine.Count; segIndex++)
        {
            GameObject.Destroy(trackSegmentMeshesToCombine[segIndex].gameObject);
        }
    }

    // ISLAND OFFROAD TRACKS
    private void CombineIslandOffroadSubMeshes()
    {
        // Debug.Log("CombineIslandOffroadSubMeshes()");

        if (trackOffroadSegmentMat1ToCombine.Count > 0)
        {
            CombineSubmeshSegmentSection(trackOffroadSegmentMat1ToCombine);
        }

        if (trackOffroadSegmentMat2ToCombine.Count > 0)
        {
            CombineSubmeshSegmentSection(trackOffroadSegmentMat2ToCombine);
        }

        if (trackOffroadSegmentMat3ToCombine.Count > 0)
        {
            CombineSubmeshSegmentSection(trackOffroadSegmentMat3ToCombine, (roadMaterial != null));
        }

        // Clean up original road/segments
        for (int segIndex = 0; segIndex < trackSegmentMeshesToCombine.Count; segIndex++)
        {
            GameObject.Destroy(trackSegmentMeshesToCombine[segIndex].gameObject);
        }
    }

    private void CombineSubmeshSegmentSection(List<MeshFilter> meshFilterCombines, bool isRoadMaterial = false)
    {
        if (meshFilterCombines.Count == 0) return;

        // Debug.Log("CombineSubmeshSegmentSection().meshFilterCombines.Count" + meshFilterCombines.Count);

        Material sharedMaterial = meshFilterCombines[0].GetComponent<MeshRenderer>().sharedMaterial;
        CombineInstance[] combine = new CombineInstance[meshFilterCombines.Count];
        int i = 0;
        while (i < meshFilterCombines.Count)
        {
            combine[i].mesh = meshFilterCombines[i].sharedMesh;
            combine[i].transform = meshFilterCombines[i].transform.localToWorldMatrix;

            GameObject.Destroy(meshFilterCombines[i].gameObject);

            i++;
        }
        
        GameObject combinedSubMeshSegmentSection = new GameObject("combinedSubMesh_" + meshFilterCombines[0].gameObject.name);
        combinedSubMeshSegmentSection.AddComponent<MeshFilter>().mesh = new Mesh();
        combinedSubMeshSegmentSection.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        combinedSubMeshSegmentSection.AddComponent<MeshRenderer>();

        if (isRoadMaterial) sharedMaterial = roadMaterial;
        combinedSubMeshSegmentSection.GetComponent<MeshRenderer>().sharedMaterial = sharedMaterial;
      
        combinedSubMeshSegmentSection.AddComponent<MeshCollider>();
        combinedSubMeshSegmentSection.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        combinedSubMeshSegmentSection.AddComponent<DetectVehicleStuck>();
    }
}
