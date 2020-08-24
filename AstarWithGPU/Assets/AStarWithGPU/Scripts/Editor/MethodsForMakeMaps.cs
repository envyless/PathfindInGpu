using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MethodsForMakeMaps : MonoBehaviour
{

    //map을 대충 생성합니다.
    //좀더 정교하게 생성할 수 도 있겠지만, 프로젝트의 목표와 어긋나기에 쉽게 갑니다.
    [MenuItem("PathFinderWithGPU/MakeMap")]
    public static void MakeMap()
    {        
        //get map max size for make map
        var goGround = GameObject.Find("Ground");
        var size = goGround.transform.localScale * 0.5f;

        //parent
        var goObstacles = GameObject.Find("Obstacles");

        //obstacle count from size
        for(int i = 0; i< (int)size.x*0.3f; ++i)
        {
            var originObstacle = Resources.Load<GameObject>("Obstacle");
            var instancedObstacle = GameObject.Instantiate(originObstacle, goObstacles.transform);
			


            //set random position, scale in map,            
            var randomPosition = new Vector3(Random.Range(-size.x, size.x), 2, Random.Range(-size.x, size.x));
            var randomSize = new Vector3(Random.Range(-size.x, size.x), Random.Range(3f, 10f), Random.Range(-size.x, size.x)) * 0.3f;
            

            instancedObstacle.transform.localScale = randomSize;
			instancedObstacle.transform.localScale += Vector3.up * 10;
			instancedObstacle.transform.position = randomPosition;                              
        }
    }

	[MenuItem("PathFinderWithGPU/BakeSetting")]
	public static void BakeMeshForCustom()
	{
		Debug.Log("!! Ground will be walkable");
		var ground = GameObject.Find("Ground");
		GameObjectUtility.SetStaticEditorFlags(ground, StaticEditorFlags.NavigationStatic);
		GameObjectUtility.SetNavMeshArea(ground, 0);

		Debug.Log("!! Obstacle Tag will be not walkable");
		var obstacles = GameObject.FindObjectsOfType<MeshRenderer>();
		foreach(var obstacle in obstacles)
        {
			if(obstacle.name.Contains("Obstacle"))
            {
				GameObjectUtility.SetStaticEditorFlags(obstacle.gameObject, StaticEditorFlags.NavigationStatic);
				GameObjectUtility.SetNavMeshArea(obstacle.gameObject, 1);
			}			
		}		
		return;
	}

	[MenuItem("PathFinderWithGPU/NavMeshToCustomMesh")]
	public static void NavMeshToCustomMesh()
    {
		NavMeshTriangulation triangles = NavMesh.CalculateTriangulation();
		Mesh mesh = new Mesh();
		mesh.vertices = triangles.vertices;
		mesh.triangles = triangles.indices;

		//CacheItem("../NavCustomMesh.mesh", mesh);
		Debug.Log("!! Set To CustomNavMesh for mesh save");
		GameObject.Find("CustomNavMesh").GetComponent<MeshFilter>().mesh = mesh;
	}


    public static void CacheItem(string url, Mesh mesh)
    {
        string path = Path.Combine(Application.streamingAssetsPath, url);
        byte[] bytes = MeshSerializer.WriteMesh(mesh, true);
        File.WriteAllBytes(path, bytes);
    }


    public static Mesh GetCacheItem(string url)
    {
        string path = Path.Combine(Application.streamingAssetsPath, url);
        if (File.Exists(path) == true)
        {
            byte[] bytes = File.ReadAllBytes(path);
            return MeshSerializer.ReadMesh(bytes);
        }
        return null;
    }
}

 
public class MeshSerializer
{
	// A simple mesh saving/loading functionality.
	// This is a utility script, you don't need to add it to any objects.
	// See SaveMeshForWeb and LoadMeshFromWeb for example of use.
	//
	// Uses a custom binary format:
	//
	//    2 bytes vertex count
	//    2 bytes triangle count
	//    1 bytes vertex format (bits: 0=vertices, 1=normals, 2=tangents, 3=uvs)
	//
	//    After that come vertex component arrays, each optional except for positions.
	//    Which ones are present depends on vertex format:
	//        Positions
	//            Bounding box is before the array (xmin,xmax,ymin,ymax,zmin,zmax)
	//            Then each vertex component is 2 byte unsigned short, interpolated between the bound axis
	//        Normals
	//            One byte per component
	//        Tangents
	//            One byte per component
	//        UVs (8 bytes/vertex - 2 floats)
	//            Bounding box is before the array (xmin,xmax,ymin,ymax)
	//            Then each UV component is 2 byte unsigned short, interpolated between the bound axis
	//
	//    Finally the triangle indices array: 6 bytes per triangle (3 unsigned short indices)
	// Reads mesh from an array of bytes. [old: Can return null if the bytes seem invalid.]
	public static Mesh ReadMesh(byte[] bytes)
	{
		if (bytes == null || bytes.Length < 5)
			throw new Exception("Invalid mesh file!");

		var buf = new BinaryReader(new MemoryStream(bytes));

		// read header
		var vertCount = buf.ReadUInt16();
		var triCount = buf.ReadUInt16();
		var format = buf.ReadByte();

		// sanity check
		if (vertCount < 0 || vertCount > 64000)
			throw new Exception("Invalid vertex count in the mesh data!");
		if (triCount < 0 || triCount > 64000)
			throw new Exception("Invalid triangle count in the mesh data!");
		if (format < 1 || (format & 1) == 0 || format > 15)
			throw new Exception("Invalid vertex format in the mesh data!");

		var mesh = new Mesh();
		int i;

		// positions
		var verts = new Vector3[vertCount];
		ReadVector3Array16Bit(verts, buf);
		mesh.vertices = verts;

		if ((format & 2) != 0) // have normals
		{
			var normals = new Vector3[vertCount];
			ReadVector3ArrayBytes(normals, buf);
			mesh.normals = normals;
		}

		if ((format & 4) != 0) // have tangents
		{
			var tangents = new Vector4[vertCount];
			ReadVector4ArrayBytes(tangents, buf);
			mesh.tangents = tangents;
		}

		if ((format & 8) != 0) // have UVs
		{
			var uvs = new Vector2[vertCount];
			ReadVector2Array16Bit(uvs, buf);
			mesh.uv = uvs;
		}

		// triangle indices
		var tris = new int[triCount * 3];
		for (i = 0; i < triCount; ++i)
		{
			tris[i * 3 + 0] = buf.ReadUInt16();
			tris[i * 3 + 1] = buf.ReadUInt16();
			tris[i * 3 + 2] = buf.ReadUInt16();
		}
		mesh.triangles = tris;

		buf.Close();

		return mesh;
	}

	static void ReadVector3Array16Bit(Vector3[] arr, BinaryReader buf)
	{
		var n = arr.Length;
		if (n == 0)
			return;

		// read bounding box
		Vector3 bmin;
		Vector3 bmax;
		bmin.x = buf.ReadSingle();
		bmax.x = buf.ReadSingle();
		bmin.y = buf.ReadSingle();
		bmax.y = buf.ReadSingle();
		bmin.z = buf.ReadSingle();
		bmax.z = buf.ReadSingle();

		// decode vectors as 16 bit integer components between the bounds
		for (var i = 0; i < n; ++i)
		{
			ushort ix = buf.ReadUInt16();
			ushort iy = buf.ReadUInt16();
			ushort iz = buf.ReadUInt16();
			float xx = ix / 65535.0f * (bmax.x - bmin.x) + bmin.x;
			float yy = iy / 65535.0f * (bmax.y - bmin.y) + bmin.y;
			float zz = iz / 65535.0f * (bmax.z - bmin.z) + bmin.z;
			arr[i] = new Vector3(xx, yy, zz);
		}
	}
	static void WriteVector3Array16Bit(Vector3[] arr, BinaryWriter buf)
	{
		if (arr.Length == 0)
			return;

		// calculate bounding box of the array
		var bounds = new Bounds(arr[0], new Vector3(0.001f, 0.001f, 0.001f));
		foreach (var v in arr)
			bounds.Encapsulate(v);

		// write bounds to stream
		var bmin = bounds.min;
		var bmax = bounds.max;
		buf.Write(bmin.x);
		buf.Write(bmax.x);
		buf.Write(bmin.y);
		buf.Write(bmax.y);
		buf.Write(bmin.z);
		buf.Write(bmax.z);

		// encode vectors as 16 bit integer components between the bounds
		foreach (var v in arr)
		{
			var xx = Mathf.Clamp((v.x - bmin.x) / (bmax.x - bmin.x) * 65535.0f, 0.0f, 65535.0f);
			var yy = Mathf.Clamp((v.y - bmin.y) / (bmax.y - bmin.y) * 65535.0f, 0.0f, 65535.0f);
			var zz = Mathf.Clamp((v.z - bmin.z) / (bmax.z - bmin.z) * 65535.0f, 0.0f, 65535.0f);
			var ix = (ushort)xx;
			var iy = (ushort)yy;
			var iz = (ushort)zz;
			buf.Write(ix);
			buf.Write(iy);
			buf.Write(iz);
		}
	}
	static void ReadVector2Array16Bit(Vector2[] arr, BinaryReader buf)
	{
		var n = arr.Length;
		if (n == 0)
			return;

		// Read bounding box
		Vector2 bmin;
		Vector2 bmax;
		bmin.x = buf.ReadSingle();
		bmax.x = buf.ReadSingle();
		bmin.y = buf.ReadSingle();
		bmax.y = buf.ReadSingle();

		// Decode vectors as 16 bit integer components between the bounds
		for (var i = 0; i < n; ++i)
		{
			ushort ix = buf.ReadUInt16();
			ushort iy = buf.ReadUInt16();
			float xx = ix / 65535.0f * (bmax.x - bmin.x) + bmin.x;
			float yy = iy / 65535.0f * (bmax.y - bmin.y) + bmin.y;
			arr[i] = new Vector2(xx, yy);
		}
	}
	static void WriteVector2Array16Bit(Vector2[] arr, BinaryWriter buf)
	{
		if (arr.Length == 0)
			return;

		// Calculate bounding box of the array
		Vector2 bmin = arr[0] - new Vector2(0.001f, 0.001f);
		Vector2 bmax = arr[0] + new Vector2(0.001f, 0.001f);
		foreach (var v in arr)
		{
			bmin.x = Mathf.Min(bmin.x, v.x);
			bmin.y = Mathf.Min(bmin.y, v.y);
			bmax.x = Mathf.Max(bmax.x, v.x);
			bmax.y = Mathf.Max(bmax.y, v.y);
		}

		// Write bounds to stream
		buf.Write(bmin.x);
		buf.Write(bmax.x);
		buf.Write(bmin.y);
		buf.Write(bmax.y);

		// Encode vectors as 16 bit integer components between the bounds
		foreach (var v in arr)
		{
			var xx = (v.x - bmin.x) / (bmax.x - bmin.x) * 65535.0f;
			var yy = (v.y - bmin.y) / (bmax.y - bmin.y) * 65535.0f;
			var ix = (ushort)xx;
			var iy = (ushort)yy;
			buf.Write(ix);
			buf.Write(iy);
		}
	}

	static void ReadVector3ArrayBytes(Vector3[] arr, BinaryReader buf)
	{
		// decode vectors as 8 bit integers components in -1.0f .. 1.0f range
		var n = arr.Length;
		for (var i = 0; i < n; ++i)
		{
			byte ix = buf.ReadByte();
			byte iy = buf.ReadByte();
			byte iz = buf.ReadByte();
			float xx = (ix - 128.0f) / 127.0f;
			float yy = (iy - 128.0f) / 127.0f;
			float zz = (iz - 128.0f) / 127.0f;
			arr[i] = new Vector3(xx, yy, zz);
		}
	}
	static void WriteVector3ArrayBytes(Vector3[] arr, BinaryWriter buf)
	{
		// encode vectors as 8 bit integers components in -1.0f .. 1.0f range
		foreach (var v in arr)
		{
			var ix = (byte)Mathf.Clamp(v.x * 127.0f + 128.0f, 0.0f, 255.0f);
			var iy = (byte)Mathf.Clamp(v.y * 127.0f + 128.0f, 0.0f, 255.0f);
			var iz = (byte)Mathf.Clamp(v.z * 127.0f + 128.0f, 0.0f, 255.0f);
			buf.Write(ix);
			buf.Write(iy);
			buf.Write(iz);
		}
	}

	static void ReadVector4ArrayBytes(Vector4[] arr, BinaryReader buf)
	{
		// Decode vectors as 8 bit integers components in -1.0f .. 1.0f range
		var n = arr.Length;
		for (var i = 0; i < n; ++i)
		{
			byte ix = buf.ReadByte();
			byte iy = buf.ReadByte();
			byte iz = buf.ReadByte();
			byte iw = buf.ReadByte();
			float xx = (ix - 128.0f) / 127.0f;
			float yy = (iy - 128.0f) / 127.0f;
			float zz = (iz - 128.0f) / 127.0f;
			float ww = (iw - 128.0f) / 127.0f;
			arr[i] = new Vector4(xx, yy, zz, ww);
		}
	}
	static void WriteVector4ArrayBytes(Vector4[] arr, BinaryWriter buf)
	{
		// Encode vectors as 8 bit integers components in -1.0f .. 1.0f range
		foreach (var v in arr)
		{
			var ix = (byte)Mathf.Clamp(v.x * 127.0f + 128.0f, 0.0f, 255.0f);
			var iy = (byte)Mathf.Clamp(v.y * 127.0f + 128.0f, 0.0f, 255.0f);
			var iz = (byte)Mathf.Clamp(v.z * 127.0f + 128.0f, 0.0f, 255.0f);
			var iw = (byte)Mathf.Clamp(v.w * 127.0f + 128.0f, 0.0f, 255.0f);
			buf.Write(ix);
			buf.Write(iy);
			buf.Write(iz);
			buf.Write(iw);
		}
	}

	// Writes mesh to an array of bytes.
	public static byte[] WriteMesh(Mesh mesh, bool saveTangents)
	{
		if (!mesh)
			throw new Exception("No mesh given!");

		var verts = mesh.vertices;
		var normals = mesh.normals;
		var tangents = mesh.tangents;
		var uvs = mesh.uv;
		var tris = mesh.triangles;

		// figure out vertex format
		byte format = 1;
		if (normals.Length > 0)
			format |= 2;
		if (saveTangents && tangents.Length > 0)
			format |= 4;
		if (uvs.Length > 0)
			format |= 8;

		var stream = new MemoryStream();
		var buf = new BinaryWriter(stream);

		// write header
		var vertCount = (ushort)verts.Length;
		var triCount = (ushort)(tris.Length / 3);
		buf.Write(vertCount);
		buf.Write(triCount);
		buf.Write(format);
		// vertex components
		WriteVector3Array16Bit(verts, buf);
		WriteVector3ArrayBytes(normals, buf);
		if (saveTangents)
			WriteVector4ArrayBytes(tangents, buf);
		WriteVector2Array16Bit(uvs, buf);
		// triangle indices
		foreach (var idx in tris)
		{
			var idx16 = (ushort)idx;
			buf.Write(idx16);
		}
		buf.Close();

		return stream.ToArray();
	}
}