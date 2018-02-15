using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using g3;
using System.IO;

public class PokeDemo : MonoBehaviour
{
    DMesh3 sphereMesh;
    double SphereRadius = 5.0;

    //DMesh3 curMesh;
    //InteractiveReducer reduce;
    //Coroutine active_reduce;

    GameObject meshGO;

    // Use this for initialization
    void Start() {
        // find path to sample file
        string curPath = Application.dataPath;
        string filePath = Path.Combine(curPath, Path.Combine("..\\sample_files", "bunny_solid.obj"));

        // load sample file, convert to unity coordinate system, translate and scale to origin
        Sphere3Generator_NormalizedCube spheregen = new Sphere3Generator_NormalizedCube() {
            Radius = SphereRadius
        };
        sphereMesh = spheregen.Generate().MakeDMesh();

        // load wireframe shader
        Material wireframeShader = g3UnityUtils.SafeLoadMaterial("wireframe_shader/Wireframe");

        // create initial mesh
        meshGO = g3UnityUtils.CreateMeshGO("sphere", sphereMesh, wireframeShader);
    }
	

	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp(KeyCode.R)) {
            Vector3d[] points = generate_sphere_points(500, SphereRadius*1.0f);

            DMesh3 tmp = new DMesh3(sphereMesh);
            int[] point_to_vid_map = insert_points(points, tmp);
            remove_old_vertices(point_to_vid_map, tmp);
            g3UnityUtils.SetGOMesh(meshGO, tmp);

            StandardMeshWriter.WriteMesh("c:\\scratch\\FULL_COLLAPSE.obj", tmp, WriteOptions.Defaults);
        }
    }



    void remove_old_vertices(int[] MapV, DMesh3 mesh)
    {
        HashSet<int> keepV = new HashSet<int>();
        for (int k = 0; k < MapV.Length; ++k) {
            if (MapV[k] != DMesh3.InvalidID)
                keepV.Add(MapV[k]);
        }

        Remesher r = new Remesher(mesh);
        //r.EnableCollapses = false;
        //r.EnableSplits = false;
        //r.EnableFlips = false;
        r.SmoothSpeedT = 1.0;
        //r.EnableSmoothing = false;
        r.PreventNormalFlips = true;
        r.SetTargetEdgeLength(1.0);
        //r.EnableSmoothing = false;
        MeshConstraints c = new MeshConstraints();
        foreach (int vid in keepV)
            c.SetOrUpdateVertexConstraint(vid, VertexConstraint.Pinned);
        r.SetExternalConstraints(c);

        double minE, maxE, avgE;
        MeshQueries.EdgeLengthStats(mesh, out minE, out maxE, out avgE);
        r.SetTargetEdgeLength(avgE*.3);

        for (int k = 0; k < 10; ++k)
            r.BasicRemeshPass();

        //int iter = 0;
        //while (iter++ < 10) {
        //    r.SetTargetEdgeLength(iter * 1.0);
        //    for (int k = 0; k < 10; ++k)
        //        r.BasicRemeshPass();
        //}
    }




    int[] insert_points(Vector3d[] points, DMesh3 mesh)
    {
        int[] MapV = new int[points.Length];
        HashSet<int> newV = new HashSet<int>();

        for ( int i = 0; i < points.Length; ++i ) {
            MapV[i] = DMesh3.InvalidID;

            Vector3d pt = points[i];
            pt.Normalize();
            Ray3d ray = new Ray3d(Vector3d.Zero, pt);

            int hit_tid =
                MeshQueries.FindHitTriangle_LinearSearch(mesh, ray);
            if (hit_tid == DMesh3.InvalidID)
                continue;
            Index3i hit_tri = mesh.GetTriangle(hit_tid);

            IntrRay3Triangle3 intr =
                MeshQueries.TriangleIntersection(mesh, hit_tid, ray);
            Vector3d bary = intr.TriangleBaryCoords;
            bool done = false;
            for ( int j = 0; j < 3 && done == false; ++j ) {
                if ( bary[j] > 0.9 ) {
                    // hit-vertex case
                    if (newV.Contains(hit_tri[j]) == false) {
                        MapV[i] = hit_tri[j];
                        newV.Add(MapV[i]);
                        done = true;
                    }
                } else if ( bary[j] < 0.1 ) {
                    // hit-edge case
                    DMesh3.EdgeSplitInfo split_info;
                    MeshResult splitResult = mesh.SplitEdge(hit_tri[(j + 1) % 3], hit_tri[(j + 2) % 3], out split_info);
                    if (splitResult == MeshResult.Ok) {
                        MapV[i] = split_info.vNew;
                        newV.Add(MapV[i]);
                        mesh.SetVertex(split_info.vNew, points[i]);
                        done = true;
                    }
                }
            }
            if (done)
                continue;

            DMesh3.PokeTriangleInfo poke_info;
            MeshResult result = mesh.PokeTriangle(hit_tid, out poke_info);
            if ( result == MeshResult.Ok ) {
                MapV[i] = poke_info.new_vid;
                newV.Add(MapV[i]);
                mesh.SetVertex(poke_info.new_vid, points[i]);
            }
        }

        return MapV;
    }



    Vector3d[] generate_sphere_points(int N, double radius)
    {
        System.Random r = new System.Random(1717117);

        Vector3d[] points = new Vector3d[N];
        for (int i = 0; i < N; ++i ) {
            Vector3d v = new Vector3d(r.NextDouble(), r.NextDouble(), r.NextDouble());
            v = 2.0 * (v - 0.5);
            v.Normalize();
            v = radius * v;
            points[i] = v;
        }
        return points;
    }


}
