using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using g3;
using System.IO;
using System;

public class RemeshDemo : MonoBehaviour
{
    DMesh3 startMesh;
    DMesh3 curMesh;
    InteractiveRemesher remesh;
    Coroutine active_remesh;

    GameObject meshGO;

    public int RemeshPasses = 20;
    public float EdgeLengthMultiplier = 0.5f;
    public float SmoothSpeed = 0.5f;
    public bool Reproject = true;
    public bool PreserveBoundary = true;
    public bool RemeshBoundary = true;

    public bool Loop = false;
    public float FrameDelayS = 0.1f;

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
            if (active_remesh != null) {
                StopCoroutine(active_remesh);
                active_remesh = null;
            }
        }

        if ( Input.GetKeyUp(KeyCode.R) ) {
            if (active_remesh != null) {
                StopCoroutine(active_remesh);
                active_remesh = null;
            }

            curMesh = new DMesh3(startMesh);
            remesh = make_remesher(curMesh);
            active_remesh = StartCoroutine(remesh_playback());
        }

        // if we are looping, restart
        if (in_loop && active_remesh == null) {
            curMesh = new DMesh3(startMesh);
            remesh = make_remesher(curMesh);
            active_remesh = StartCoroutine(remesh_playback());
        }
    }


    InteractiveRemesher make_remesher(DMesh3 mesh)
    {
        var m = new InteractiveRemesher(mesh);
        m.PreventNormalFlips = true;

        double mine, maxe, avge;
        MeshQueries.EdgeLengthStats(mesh, out mine, out avge, out maxe);
        m.SetTargetEdgeLength(avge * EdgeLengthMultiplier);

        m.SmoothSpeedT = SmoothSpeed;

        if (Reproject)
            m.SetProjectionTarget(MeshProjectionTarget.Auto(mesh));

        if (RemeshBoundary) {
            MeshBoundaryLoops loops = new MeshBoundaryLoops(mesh);
            int k = 1;
            foreach (var loop in loops)
                MeshConstraintUtil.ConstrainVtxLoopTo(m, loop.Vertices, new DCurveProjectionTarget(loop.ToCurve()), k++);
        } else if (PreserveBoundary) {
            MeshConstraintUtil.FixAllBoundaryEdges(m);
        }

        return m;
    }


    IEnumerator remesh_playback()
    {
        g3UnityUtils.SetGOMesh(meshGO, curMesh);
        yield return new WaitForSecondsRealtime(FrameDelayS);

        foreach (int i in InteractiveRemesh(remesh, RemeshPasses) ) {
            g3UnityUtils.SetGOMesh(meshGO, curMesh);
            yield return new WaitForSecondsRealtime(FrameDelayS);
        }
        yield return new WaitForSecondsRealtime(1.0f);
        active_remesh = null;
    }


    IEnumerable<int> InteractiveRemesh(Remesher r, int nPasses) {
        for (int k = 0; k < nPasses; ++k) {
            r.BasicRemeshPass();
            yield return k;
        }
    }

}
