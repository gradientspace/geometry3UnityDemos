using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using g3;
using System.IO;

public class ReduceDemo : MonoBehaviour
{
    DMesh3 startMesh;
    DMesh3 curMesh;
    InteractiveReducer reduce;
    Coroutine active_reduce;

    GameObject meshGO;

	// Use this for initialization
	void Start () {
        // find path to sample file
        string curPath = Application.dataPath;
        string filePath = Path.Combine(curPath, Path.Combine("..\\sample_files", "bunny_solid.obj"));

        // load sample file, convert to unity coordinate system, translate and scale to origin
        startMesh = StandardMeshReader.ReadMesh(filePath);
        if (startMesh == null)
            startMesh = new Sphere3Generator_NormalizedCube().Generate().MakeDMesh();
        MeshTransforms.FlipLeftRightCoordSystems(startMesh);
        MeshTransforms.Translate(startMesh, -startMesh.CachedBounds.Center);
        MeshTransforms.Scale(startMesh, 8.0 / startMesh.CachedBounds.MaxDim);

        // load wireframe shader
        Material wireframeShader = g3UnityUtils.SafeLoadMaterial("wireframe_shader/Wireframe");

        // create initial mesh
        meshGO = g3UnityUtils.CreateMeshGO("start_mesh", startMesh, wireframeShader);
    }
	

	// Update is called once per frame
	void Update () {
        if ( Input.GetKeyUp(KeyCode.R) ) {
            if (active_reduce != null)
                StopCoroutine(active_reduce);

            curMesh = new DMesh3(startMesh);
            reduce = new InteractiveReducer(curMesh);
            active_reduce = StartCoroutine(reduce_playback());
        }
	}


    IEnumerator reduce_playback()
    {
        int iter = 0;
        int N = 100;
        foreach (int i in reduce.ReduceToTriangleCount_Interactive(500) ) {
            if (iter++ % N == 0) {
                g3UnityUtils.SetGOMesh(meshGO, curMesh);
                yield return new WaitForSecondsRealtime(0.001f);
            }
        }
    }

}
