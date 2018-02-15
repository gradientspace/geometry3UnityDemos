using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using g3;

public class g3UnityUtils
{


    public static GameObject CreateMeshGO(string name, DMesh3 mesh, Material setMaterial = null, bool bCollider = true)
    {
        var gameObj = new GameObject(name);
        gameObj.AddComponent<MeshFilter>();
        SetGOMesh(gameObj, mesh);
        if (bCollider) {
            gameObj.AddComponent(typeof(MeshCollider));
            gameObj.GetComponent<MeshCollider>().enabled = false;
        }
        if (setMaterial) {
            gameObj.AddComponent<MeshRenderer>().material = setMaterial;
        } else {
            gameObj.AddComponent<MeshRenderer>().material = StandardMaterial(Color.red);
        }
        return gameObj;
    }
    public static GameObject CreateMeshGO(string name, DMesh3 mesh, Colorf color, bool bCollider = true)
    {
        return CreateMeshGO(name, mesh, StandardMaterial(color), bCollider);
    }


    public static void SetGOMesh(GameObject go, DMesh3 mesh)
    {
        DMesh3 useMesh = mesh;
        if ( ! mesh.IsCompact ) {
            useMesh = new DMesh3(mesh, true);
        }


        MeshFilter filter = go.GetComponent<MeshFilter>();
        if (filter == null)
            throw new Exception("g3UnityUtil.SetGOMesh: go " + go.name + " has no MeshFilter");
        Mesh unityMesh = DMeshToUnityMesh(useMesh);
        filter.sharedMesh = unityMesh;
    }




    /// <summary>
    /// Convert DMesh3 to unity Mesh
    /// </summary>
    public static Mesh DMeshToUnityMesh(DMesh3 m, bool bLimitTo64k = false)
    {
        if (bLimitTo64k && (m.MaxVertexID > 65535 || m.MaxTriangleID > 65535) ) {
            Debug.Log("g3UnityUtils.DMeshToUnityMesh: attempted to convert DMesh larger than 65535 verts/tris, not supported by Unity!");
            return null;
        }

        Mesh unityMesh = new Mesh();
        unityMesh.vertices = dvector_to_vector3(m.VerticesBuffer);
        if (m.HasVertexNormals)
            unityMesh.normals = (m.HasVertexNormals) ? dvector_to_vector3(m.NormalsBuffer) : null;
        if (m.HasVertexColors)
            unityMesh.colors = dvector_to_color(m.ColorsBuffer);
        if (m.HasVertexUVs)
            unityMesh.uv = dvector_to_vector2(m.UVBuffer);
        unityMesh.triangles = dvector_to_int(m.TrianglesBuffer);

        if (m.HasVertexNormals == false)
            unityMesh.RecalculateNormals();

        return unityMesh;
    }


    /// <summary>
    /// Convert unity Mesh to a g3.DMesh3. Ignores UV's.
    /// </summary>
    public static DMesh3 UnityMeshToDMesh(Mesh mesh)
    {
        Vector3[] mesh_vertices = mesh.vertices;
        Vector3f[] dmesh_vertices = new Vector3f[mesh_vertices.Length];
        for (int i = 0; i < mesh.vertexCount; ++i)
            dmesh_vertices[i] = mesh_vertices[i];

        Vector3[] mesh_normals = mesh.normals;
        if (mesh_normals != null) {
            Vector3f[] dmesh_normals = new Vector3f[mesh_vertices.Length];
            for (int i = 0; i < mesh.vertexCount; ++i)
                dmesh_normals[i] = mesh_normals[i];

            return DMesh3Builder.Build(dmesh_vertices, mesh.triangles, dmesh_normals);

        } else {
            return DMesh3Builder.Build<Vector3f,int,Vector3f>(dmesh_vertices, mesh.triangles, null, null);
        }
    }



    public static Material StandardMaterial(Colorf color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        return mat;
    }


    public static Material SafeLoadMaterial(string sPath)
    {
        Material mat = null;
        try {
            Material loaded = Resources.Load<Material>(sPath);
            mat = new Material(loaded);
        } catch (Exception e) {
            Debug.Log("g3UnityUtil.SafeLoadMaterial: exception: " + e.Message);
            mat = new Material(Shader.Find("Standard"));
            mat.color = Color.red;
        }
        return mat;
    }






    // per-type conversion functions
    public static Vector3[] dvector_to_vector3(DVector<double> vec)
    {
        int nLen = vec.Length / 3;
        Vector3[] result = new Vector3[nLen];
        for (int i = 0; i < nLen; ++i) {
            result[i].x = (float)vec[3 * i];
            result[i].y = (float)vec[3 * i + 1];
            result[i].z = (float)vec[3 * i + 2];
        }
        return result;
    }
    public static Vector3[] dvector_to_vector3(DVector<float> vec)
    {
        int nLen = vec.Length / 3;
        Vector3[] result = new Vector3[nLen];
        for (int i = 0; i < nLen; ++i) {
            result[i].x = vec[3 * i];
            result[i].y = vec[3 * i + 1];
            result[i].z = vec[3 * i + 2];
        }
        return result;
    }
    public static Vector2[] dvector_to_vector2(DVector<float> vec)
    {
        int nLen = vec.Length / 2;
        Vector2[] result = new Vector2[nLen];
        for (int i = 0; i < nLen; ++i) {
            result[i].x = vec[2 * i];
            result[i].y = vec[2 * i + 1];
        }
        return result;
    }
    public static Color[] dvector_to_color(DVector<float> vec)
    {
        int nLen = vec.Length / 3;
        Color[] result = new Color[nLen];
        for (int i = 0; i < nLen; ++i) {
            result[i].r = vec[3 * i];
            result[i].g = vec[3 * i + 1];
            result[i].b = vec[3 * i + 2];
        }
        return result;
    }
    public static int[] dvector_to_int(DVector<int> vec)
    {
        // todo this could be faster because we can directly copy chunks...
        int nLen = vec.Length;
        int[] result = new int[nLen];
        for (int i = 0; i < nLen; ++i)
            result[i] = vec[i];
        return result;
    }


}
