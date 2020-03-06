using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testmath : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		float3[] newNormals = new float3[20];
		for (int i = 0; i < 20; ++i)
		{
			newNormals[i] = faceNormals[i];
		}

		float3 face1Normal = new float3(-0.96f, -0.18f, 0);
		float3 face5Normal = new float3(0.25f, 0.43f, -0.84f);
		CalibrateNormals(0, face1Normal, 4, face5Normal, newNormals, 20);
    }

    // Update is called once per frame
    void Update()
    {

	}

	public struct float3
	{
		public float x;
		public float y;
		public float z;

		public float3(float ax, float ay, float az)
		{
			x = ax;
			y = ay;
			z = az;
		}

		public float sqrMagnitude()
		{
			return x* x + y* y + z* z;
		}
		public float magnitude()
		{
			return Mathf.Sqrt(sqrMagnitude());
		}

		public float mag { get { return magnitude(); } }

		public float3 normalize()
		{
			float mag = magnitude();
			x /= mag;
			y /= mag;
			z /= mag;
			return this;
		}
		public float3 normalized()
		{
			float3 ret = this;
			ret.normalize();
			return ret;
		}

		public static float dot(float3 left, float3 right)
		{
			return left.x * right.x + left.y * right.y + left.z * right.z;
		}
		public static float3 cross(float3 left, float3 right)
		{
			return new float3(
				left.y * right.z - left.z * right.y,
				left.z * right.x - left.x * right.z,
				left.x * right.y - left.y * right.x
			);
		}

		public static float3 zero() { return new float3(0, 0, 0); }

		public static float3 operator+(float3 left, float3 right)
		{
			return new float3(left.x + right.x, left.y + right.y, left.z + right.z);
		}
		public static float3 operator-(float3 left, float3 right)
		{
			return new float3(left.x - right.x, left.y - right.y, left.z - right.z);
		}
		public static float3 operator *(float3 left, float right)
		{
			return new float3(left.x * right, left.y * right, left.z * right);
		}
		public static float3 operator *(float left, float3 right)
		{
			return new float3(left * right.x, left * right.y, left * right.z);
		}
		public static float3 operator /(float3 left, float right)
		{
			return new float3(left.x / right, left.y / right, left.z / right);
		}
	};

	public struct matrix3x3
	{
		public float m11; public float m12; public float m13;
		public float m21; public float m22; public float m23;
		public float m31; public float m32; public float m33;

		public matrix3x3(float3 col1, float3 col2, float3 col3) { 
            m11 = col1.x; m12 = col2.x; m13 = col3.x;
            m21 = col1.y; m22 = col2.y; m23 = col3.y;
			m31 = col1.z; m32 = col2.z; m33 = col3.z;
		}
		public float3 col1() { return new float3(m11, m21, m31); }
		public float3 col2() { return new float3(m12, m22, m32); }
		public float3 col3() { return new float3(m13, m23, m33); }
        public float3 row1() { return new float3(m11, m12, m13); }
        public float3 row2() { return new float3(m21, m22, m23); }
        public float3 row3() { return new float3(m31, m32, m33); }

        public static matrix3x3 transpose(matrix3x3 m)
		{
			matrix3x3 ret = new matrix3x3();
			ret.m11 = m.m11; ret.m12 = m.m21; ret.m13 = m.m31;
			ret.m21 = m.m12; ret.m22 = m.m22; ret.m23 = m.m32;
			ret.m31 = m.m13; ret.m32 = m.m23; ret.m33 = m.m33;
			return ret;
		}

		public static float3 mul(matrix3x3 left, float3 right)
		{
			return new float3(
				float3.dot(left.row1(), right),
				float3.dot(left.row2(), right),
				float3.dot(left.row3(), right));
		}

		public static matrix3x3 mul(matrix3x3 left, matrix3x3 right)
		{
			matrix3x3 ret = new matrix3x3();
			ret.m11 = float3.dot(left.row1(), right.col1()); ret.m12 = float3.dot(left.row1(), right.col2()); ret.m13 = float3.dot(left.row1(), right.col3());
			ret.m21 = float3.dot(left.row2(), right.col1()); ret.m22 = float3.dot(left.row2(), right.col2()); ret.m23 = float3.dot(left.row2(), right.col3());
			ret.m31 = float3.dot(left.row3(), right.col1()); ret.m32 = float3.dot(left.row3(), right.col2()); ret.m33 = float3.dot(left.row3(), right.col3());
			return ret;
		}
	};


	float3[] faceNormals = {
			new float3(-0.1273862f,  0.3333025f,  0.9341605f),
			new float3( 0.6667246f, -0.7453931f, -0.0000000f),
			new float3( 0.8726854f,  0.3333218f, -0.3568645f),
			new float3(-0.3333083f, -0.7453408f, -0.5773069f),
			new float3( 0.0000000f, -1.0000000f, -0.0000000f),
			new float3(-0.7453963f,  0.3333219f,  0.5773357f),
			new float3( 0.3333614f,  0.7453930f, -0.5774010f),
			new float3(-0.7453431f,  0.3333741f, -0.5773722f),
			new float3( 0.8726999f,  0.3333025f,  0.3567604f),
			new float3( 0.1273475f, -0.3333741f,  0.9341723f),
			new float3(-0.1273475f,  0.3333741f, -0.9341723f),
			new float3(-0.8726999f, -0.3333025f, -0.3567604f),
			new float3( 0.7453431f, -0.3333741f,  0.5773722f),
			new float3(-0.3331230f, -0.7450288f,  0.5778139f),
			new float3( 0.7453963f, -0.3333219f, -0.5773357f),
			new float3( 0.0000000f,  1.0000000f, -0.0000000f),
			new float3( 0.3333083f,  0.7453408f,  0.5773069f),
			new float3(-0.8726854f, -0.3333218f,  0.3568645f),
			new float3(-0.6667246f,  0.7453931f, -0.0000000f),
			new float3( 0.1273862f, -0.3333025f, -0.9341605f),
		};


	void CalibrateNormals(
		int face1Index, float3 face1Normal,
		int face2Index, float3 face2Normal,
		float3[] inOutNormals, int count) {

		var canonNormals = faceNormals;

		// int closestCanonNormal1 = findClosestNormal(canonNormals, count, face1Normal);
		// int closestCanonNormal2 = findClosestNormal(canonNormals, count, face2Normal);

		// We need to build a rotation matrix that turns canonical face normals into the reference frame
		// of the accelerator, as defined by the measured coordinates of the 2 passed in face normals.
		float3 canonFace1Normal = canonNormals[face1Index];
		float3 canonFace2Normal = canonNormals[face2Index];

		// Create our intermediate reference frame in both spaces
		// Canonical space
		float3 intX_Canon = canonFace1Normal; intX_Canon.normalize();
		float3 intZ_Canon = float3.cross(intX_Canon, canonFace2Normal); intZ_Canon.normalize();
		float3 intY_Canon = float3.cross(intZ_Canon, intX_Canon);
		matrix3x3 int_Canon = new matrix3x3(intX_Canon, intY_Canon, intZ_Canon);

		//BLE_LOG_INFO("intX_Canon: %d, %d, %d", (int)(intX_Canon.x* 100), (int) (intX_Canon.y* 100), (int) (intX_Canon.z* 100));
		//BLE_LOG_INFO("intY_Canon: %d, %d, %d", (int)(intY_Canon.x* 100), (int) (intY_Canon.y* 100), (int) (intY_Canon.z* 100));
		//BLE_LOG_INFO("intZ_Canon: %d, %d, %d", (int)(intY_Canon.x* 100), (int) (intY_Canon.y* 100), (int) (intY_Canon.z* 100));

		// Accelerometer space
		float3 intX_Acc = face1Normal; intX_Acc.normalize();
		float3 intZ_Acc = float3.cross(intX_Acc, face2Normal); intZ_Acc.normalize();
		float3 intY_Acc = float3.cross(intZ_Acc, intX_Acc);
		matrix3x3 int_Acc = new matrix3x3(intX_Acc, intY_Acc, intZ_Acc);

		// This is the matrix that rotates canonical normals into accelerometer reference frame
		matrix3x3 rot = matrix3x3.mul(int_Acc, matrix3x3.transpose(int_Canon));

		// NRF_LOG_INFO("row 1: %d, %d, %d", (int)(rot.row1().x * 100.0f), (int)(rot.row1().y * 100.0f), (int)(rot.row1().z * 100.0f));
		// NRF_LOG_INFO("row 2: %d, %d, %d", (int)(rot.row2().x * 100.0f), (int)(rot.row2().y * 100.0f), (int)(rot.row2().z * 100.0f));
		// NRF_LOG_INFO("row 3: %d, %d, %d", (int)(rot.row3().x * 100.0f), (int)(rot.row3().y * 100.0f), (int)(rot.row3().z * 100.0f));

		// Now transform all the normals
		for (int i = 0; i<count; ++i) {
			float3 canonNormal = inOutNormals[i];
			//NRF_LOG_INFO("canon: %d, %d, %d", (int)(canonNormal.x * 100.0f), (int)(canonNormal.y * 100.0f), (int)(canonNormal.z * 100.0f));
			float3 newNormal = matrix3x3.mul(rot, canonNormal);
			//NRF_LOG_INFO("new: %d, %d, %d", (int)(newNormal.x * 100.0f), (int)(newNormal.y * 100.0f), (int)(newNormal.z * 100.0f));
			inOutNormals[i] = newNormal;
		}
	}

}
