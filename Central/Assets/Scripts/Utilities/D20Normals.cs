using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class D20Normals : MonoBehaviour
{
    public GameObject[] D20s;
    public GameObject Acc;

    // Start is called before the first frame update
    void Start()
    {
        //PositionAccelerometer();
        ////        GenerateD20FaceMapping();
        //TransformNormals();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Vector3[] normals =
    {
        new Vector3(0.187f, -0.7947f, 0.5774f),
        new Vector3(0.6071f, -0.7947f, 0.0000f),
        new Vector3(-0.4911f, -0.7947f, 0.3568f),
        new Vector3(-0.4911f, -0.7947f, -0.3568f),
        new Vector3(0.1876f, -0.7947f, -0.5774f),
        new Vector3(0.9822f, -0.1876f, 0.0000f),
        new Vector3(0.3035f, -0.1876f, 0.9342f),
        new Vector3(-0.7946f, -0.1876f, 0.5774f),
        new Vector3(-0.7946f, -0.1876f, -0.5774f),
        new Vector3(0.3035f, -0.1876f, -0.9342f),
        new Vector3(0.7946f, 0.1876f, 0.5774f),
        new Vector3(-0.3035f, 0.1876f, 0.9342f),
        new Vector3(-0.9822f, 0.1876f, 0.0000f),
        new Vector3(-0.3035f, 0.1876f, -0.9342f),
        new Vector3(0.7946f, 0.1876f, -0.5774f),
        new Vector3(0.4911f, 0.7947f, 0.3568f),
        new Vector3(-0.1876f, 0.7947f, 0.5774f),
        new Vector3(-0.6071f, 0.7947f, 0.0000f),
        new Vector3(-0.1876f, 0.7947f, -0.5774f),
        new Vector3(0.4911f, 0.7947f, -0.3568f),
    };


    Vector3[] d20normals =
    {
        new Vector3(-0.7946f, -0.1876f, 0.5774f),
        new Vector3(0.1876f, -0.7947f, -0.5774f),
        new Vector3(-0.3035f, 0.1876f, -0.9342f),
        new Vector3(0.9822f, -0.1876f, 0.0000f),
        new Vector3(0.6071f, -0.7947f, 0.0000f),
        new Vector3(-0.3035f, 0.1876f, 0.9342f),
        new Vector3(-0.1876f, 0.7947f, -0.5774f),
        new Vector3(0.4911f, 0.7947f, 0.3568f),
        new Vector3(-0.7946f, -0.1876f, -0.5774f),
        new Vector3(-0.4911f, -0.7947f, 0.3568f),
        new Vector3(0.4911f, 0.7947f, -0.3568f),
        new Vector3(0.7946f, 0.1876f, 0.5774f),
        new Vector3(-0.4911f, -0.7947f, -0.3568f),
        new Vector3(0.187f, -0.7947f, 0.5774f),
        new Vector3(0.3035f, -0.1876f, -0.9342f),
        new Vector3(-0.6071f, 0.7947f, 0.0000f),
        new Vector3(-0.9822f, 0.1876f, 0.0000f),
        new Vector3(0.3035f, -0.1876f, 0.9342f),
        new Vector3(-0.1876f, 0.7947f, 0.5774f),
        new Vector3(0.7946f, 0.1876f, -0.5774f),
    };

    byte[] mappings =
    {
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
        1, 11, 10, 5, 17, 12, 6, 16, 19, 4, 15, 0, 3, 13, 7, 2, 14, 9, 8, 18,
        2, 3, 7, 13, 4, 8, 18, 0, 10, 14, 5, 9, 19, 1, 11, 15, 6, 12, 16, 17,
        3, 9, 5, 8, 12, 19, 18, 6, 17, 4, 15, 2, 13, 1, 0, 7, 11, 14, 10, 16,
        4, 8, 0, 6, 2, 3, 5, 7, 9, 1, 18, 10, 12, 14, 16, 17, 13, 19, 11, 15,
        5, 4, 12, 19, 3, 18, 8, 6, 9, 17, 2, 10, 13, 11, 1, 16, 0, 7, 15, 14,
        6, 1, 3, 9, 12, 15, 11, 5, 19, 2, 17, 0, 14, 8, 4, 7, 10, 16, 18, 13,
        7, 13, 0, 1, 4, 10, 16, 2, 5, 11, 8, 14, 17, 3, 9, 15, 18, 19, 6, 12,
        8, 4, 19, 17, 13, 16, 10, 18, 14, 12, 7, 5, 1, 9, 3, 6, 2, 0, 15, 11,
        9, 2, 15, 19, 14, 13, 18, 11, 16, 12, 7, 3, 8, 1, 6, 5, 0, 4, 17, 10,
        10, 4, 17, 12, 1, 6, 5, 16, 11, 19, 0, 8, 3, 14, 13, 18, 7, 2, 15, 9,
        11, 1, 9, 2, 14, 7, 0, 15, 13, 3, 16, 6, 4, 19, 12, 5, 17, 10, 18, 8,
        17, 12, 16, 14, 1, 11, 15, 10, 0, 13, 6, 19, 9, 4, 8, 18, 5, 3, 7, 2,
        13, 14, 8, 10, 19, 17, 16, 18, 12, 4, 15, 7, 1, 3, 2, 0, 9, 11, 5, 6,
        14, 13, 11, 0, 9, 2, 7, 15, 3, 1, 18, 16, 4, 12, 17, 10, 19, 8, 6, 5,
        15, 12, 14, 13, 9, 18, 19, 11, 2, 16, 3, 17, 8, 0, 1, 10, 6, 5, 7, 4,
        16, 14, 10, 4, 1, 0, 7, 17, 6, 8, 11, 13, 2, 12, 19, 18, 15, 9, 5, 3,
        17, 12, 16, 14, 1, 11, 15, 10, 0, 13, 6, 19, 9, 4, 8, 18, 5, 3, 7, 2,
        18, 9, 8, 4, 13, 7, 2, 19, 16, 5, 14, 3, 0, 17, 12, 6, 15, 11, 10, 1,
        19, 17, 18, 9, 13, 14, 15, 8, 7, 3, 16, 12, 11, 4, 5, 6, 10, 1, 2, 0,
    };

    Vector3 AccEuler = new Vector3(-52.622f, -90.0f, 120.0f);
    // real acc Z => AccEuler Y
    // real acc X => AccEuler -Z
    // real acc Y => AccEuler X

    Vector3[] rotatedD20Normals =
    {
        new Vector3(-0.9341605f, -0.1273862f,  0.3333025f),
        new Vector3( 0.0000000f,  0.6667246f, -0.7453931f),
        new Vector3( 0.3568645f,  0.8726854f,  0.3333218f),
        new Vector3( 0.5773069f, -0.3333083f, -0.7453408f),
        new Vector3( 0.0000000f,  0.0000000f, -1.0000000f),
        new Vector3(-0.5773357f, -0.7453963f,  0.3333219f),
        new Vector3( 0.5774010f,  0.3333614f,  0.7453930f),
        new Vector3( 0.5773722f, -0.7453431f,  0.3333741f),
        new Vector3(-0.3567604f,  0.8726999f,  0.3333025f),
        new Vector3(-0.9341723f,  0.1273475f, -0.3333741f),
        new Vector3( 0.9341723f, -0.1273475f,  0.3333741f),
        new Vector3( 0.3567604f, -0.8726999f, -0.3333025f),
        new Vector3(-0.5773722f,  0.7453431f, -0.3333741f),
        new Vector3(-0.5778139f, -0.3331230f, -0.7450288f),
        new Vector3( 0.5773357f,  0.7453963f, -0.3333219f),
        new Vector3( 0.0000000f,  0.0000000f,  1.0000000f),
        new Vector3(-0.5773069f,  0.3333083f,  0.7453408f),
        new Vector3(-0.3568645f, -0.8726854f, -0.3333218f),
        new Vector3( 0.0000000f, -0.6667246f,  0.7453931f),
        new Vector3( 0.9341605f,  0.1273862f, -0.3333025f),
    };

    Vector3[] rotatedD20NormalsAccRF =
    {
        new Vector3(-0.1273862f,  0.3333025f,  0.9341605f),
        new Vector3( 0.6667246f, -0.7453931f, -0.0000000f),
        new Vector3( 0.8726854f,  0.3333218f, -0.3568645f),
        new Vector3(-0.3333083f, -0.7453408f, -0.5773069f),
        new Vector3( 0.0000000f, -1.0000000f, -0.0000000f),
        new Vector3(-0.7453963f,  0.3333219f,  0.5773357f),
        new Vector3( 0.3333614f,  0.7453930f, -0.5774010f),
        new Vector3(-0.7453431f,  0.3333741f, -0.5773722f),
        new Vector3( 0.8726999f,  0.3333025f,  0.3567604f),
        new Vector3( 0.1273475f, -0.3333741f,  0.9341723f),
        new Vector3(-0.1273475f,  0.3333741f, -0.9341723f),
        new Vector3(-0.8726999f, -0.3333025f, -0.3567604f),
        new Vector3( 0.7453431f, -0.3333741f,  0.5773722f),
        new Vector3(-0.3331230f, -0.7450288f,  0.5778139f),
        new Vector3( 0.7453963f, -0.3333219f, -0.5773357f),
        new Vector3( 0.0000000f,  1.0000000f, -0.0000000f),
        new Vector3( 0.3333083f,  0.7453408f,  0.5773069f),
        new Vector3(-0.8726854f, -0.3333218f,  0.3568645f),
        new Vector3(-0.6667246f,  0.7453931f, -0.0000000f),
        new Vector3( 0.1273862f, -0.3333025f, -0.9341605f),
    };

    Vector3[] rotatedD20NormalsV5 =
    {
        new Vector3(-0.9341605f, -0.1273862f,  0.3333025f),
        new Vector3( 0.5773357f,  0.7453963f, -0.3333219f),
        new Vector3(-0.3568645f, -0.8726854f, -0.3333218f),
        new Vector3( 0.5774010f,  0.3333614f,  0.7453930f),
        new Vector3(-0.3567604f,  0.8726999f,  0.3333025f),
        new Vector3( 0.5773722f, -0.7453431f,  0.3333741f),
        new Vector3(-0.9341723f,  0.1273475f, -0.3333741f),
        new Vector3( 0.5773069f, -0.3333083f, -0.7453408f),
        new Vector3( 0.0000000f, -0.6667246f,  0.7453931f),
        new Vector3( 0.0000000f,  0.0000000f, -1.0000000f),
        new Vector3( 0.0000000f,  0.0000000f,  1.0000000f),
        new Vector3( 0.0000000f,  0.6667246f, -0.7453931f),
        new Vector3(-0.5773069f,  0.3333083f,  0.7453408f),
        new Vector3( 0.9341723f, -0.1273475f,  0.3333741f),
        new Vector3(-0.5773722f,  0.7453431f, -0.3333741f),
        new Vector3( 0.3567604f, -0.8726999f, -0.3333025f),
        new Vector3(-0.5778139f, -0.3331230f, -0.7450288f),
        new Vector3( 0.3568645f,  0.8726854f,  0.3333218f),
        new Vector3(-0.5773357f, -0.7453963f,  0.3333219f),
        new Vector3( 0.9341605f,  0.1273862f, -0.3333025f),
    };

    Vector3 D20V5Rotation = new Vector3(19.398f, 158.043f, 82.284f); // Degrees

    void TransformNormals()
    {
        var rotAcc = Quaternion.Inverse(Quaternion.Euler(-52.622f, -90.0f, 120.0f));
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("{");
        for (int i = 0; i < 20; ++i)
        {
            Vector3 rotated = rotAcc * d20normals[i];
            builder.Append("new Vector3(");
            builder.Append(rotated.x);
            builder.Append("f, ");
            builder.Append(rotated.y);
            builder.Append("f, ");
            builder.Append(rotated.z);
            builder.AppendLine("f),");
        }
        builder.AppendLine("}");
        Debug.Log(builder.ToString());
    }

    void PositionAccelerometer()
    {
        var face5n = d20normals[4];
        var face2n = d20normals[1];

        Vector3 forward = -face5n;
        Vector3 right = Vector3.Cross(face5n, face2n).normalized;
        Vector3 up = Vector3.Cross(forward, right);
        Acc.transform.rotation = Quaternion.LookRotation(forward, up);
    }

    void GenerateD20FaceMapping()  
    {
        byte[] mappings = new byte[20 * 20];

        for (int i = 0; i < 20; ++i)
        {
            // Create a rotation from face 1 to face i+1
            var xform = D20s[i].transform.localToWorldMatrix * D20s[0].transform.worldToLocalMatrix;

            // Apply the rotation to all faces
            Vector3[] rotatedNormals = new Vector3[20];
            for (int j = 0; j < 20; ++j)
            {
                rotatedNormals[j] = xform.MultiplyVector(d20normals[j]);
            }

            // Now for each rotated normal, figure out what original normal
            // it is closest to, this will tell us what face that rotated face matches
            for (int j = 0; j < 20; ++j)
            {
                Vector3 rotatedNormal = rotatedNormals[j];
                float bestDot = -1000.0f;
                int bestFace = -1;
                for (int h = 0; h < 20; ++h)
                {
                    float dot = Vector3.Dot(rotatedNormal, d20normals[h]);
                    if (dot > bestDot)
                    {
                        bestDot = dot;
                        bestFace = h;
                    }
                }

                mappings[i * 20 + bestFace] = (byte)j;
            }
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("{");
        for (int i = 0; i < 20; ++i)
        {
            for (int j = 0; j < 20; ++j)
            {
                builder.Append(mappings[i*20 +j]);
                builder.Append(", ");
            }
            builder.AppendLine();
        }
        builder.AppendLine("}");
        Debug.Log(builder.ToString());

    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Create a rotation from face 1 to face i+1
        //var xform = D20s[1].transform.localToWorldMatrix * D20s[0].transform.worldToLocalMatrix;
        //var rotAcc = Quaternion.Inverse(Quaternion.LookRotation(rotatedD20NormalsV5[19]));

        for (int i = 0; i < 20; ++i)
        {
            Vector3 rotated = transform.localToWorldMatrix.MultiplyVector(rotatedD20NormalsV5[i]);
            var n = rotated * 0.015f;
            //n = xform.MultiplyVector(n);
            Gizmos.DrawLine(transform.position, transform.position + n);
            UnityEditor.Handles.Label(transform.position + n, (i + 1).ToString());
        }
    }
#endif
}
