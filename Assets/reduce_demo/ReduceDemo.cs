using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using g3;
using System.IO;
using System;

public class ReduceDemo : MonoBehaviour
{
    DMesh3 startMesh;
    DMesh3 curMesh;
    InteractiveReducer reduce;
    Coroutine active_reduce;

    GameObject meshGO;


    public int ReduceToCount = 500;
    public int StepsPerFrame = 100;
    public bool Loop = true;

    public bool LoadSampleMesh = false;
    public bool FlipLeftRight = false;
    public string SampleFileName = "bunny_solid.obj";


    static byte[] FloatToByte(float[] array)
    {
        var byteArray = new byte[array.Length * sizeof(float)];
        Buffer.BlockCopy(array, 0, byteArray, 0, byteArray.Length);
        return byteArray;
    }

	// Use this for initialization
	void Start () {
        
        meshGO = GameObject.Find("sample_mesh");
        Mesh unityMesh = meshGO.GetComponent<MeshFilter>().mesh;
        startMesh = g3UnityUtils.UnityMeshToDMesh(unityMesh);
        double height = startMesh.CachedBounds.Height;

        // find path to sample file
        if (LoadSampleMesh) {
            string curPath = Application.dataPath;
            string filePath = Path.Combine(curPath, Path.Combine("..\\sample_files", SampleFileName));

            // load sample file, convert to unity coordinate system, translate and scale to origin
            startMesh = StandardMeshReader.ReadMesh(filePath);
            if (startMesh == null)
                startMesh = new Sphere3Generator_NormalizedCube().Generate().MakeDMesh();

            if (FlipLeftRight)
                MeshTransforms.FlipLeftRightCoordSystems(startMesh);
            MeshTransforms.Scale(startMesh, height / startMesh.CachedBounds.Height);
            MeshTransforms.Translate(startMesh, -startMesh.CachedBounds.Center);
            MeshNormals.QuickCompute(startMesh);
            g3UnityUtils.SetGOMesh(meshGO, startMesh);
        }

    }


    // Update is called once per frame
    bool in_loop = false;
    void Update () {
        in_loop = Loop;

        if (Input.GetKeyUp(KeyCode.L))
            Loop = !Loop;

        if (Input.GetKeyUp(KeyCode.S)) {
            if (active_reduce != null) {
                StopCoroutine(active_reduce);
                active_reduce = null;
            }
        }

        if ( Input.GetKeyUp(KeyCode.R) ) {
            if (active_reduce != null) {
                StopCoroutine(active_reduce);
                active_reduce = null;
            }

            curMesh = new DMesh3(startMesh);
            reduce = new InteractiveReducer(curMesh);
            active_reduce = StartCoroutine(reduce_playback());
        }

        // if we are looping, restart
        if (in_loop && active_reduce == null) {
            curMesh = new DMesh3(startMesh);
            reduce = new InteractiveReducer(curMesh);
            active_reduce = StartCoroutine(reduce_playback());
        }
    }


    IEnumerator reduce_playback()
    {
        int iter = 0;
        foreach (int i in reduce.ReduceToTriangleCount_Interactive(ReduceToCount) ) {
            if (iter++ % StepsPerFrame == 0) {
                g3UnityUtils.SetGOMesh(meshGO, curMesh);
                yield return new WaitForSecondsRealtime(0.001f);
            }
        }
        yield return new WaitForSecondsRealtime(1.0f);
        active_reduce = null;
    }

}
