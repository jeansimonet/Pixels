using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Animations
{
    /// <summary>
    /// Procedural rainbow animation
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [System.Serializable]
    public class AnimationFadeCandy
        : Animation
    {
        public AnimationType type { get; set; } = AnimationType.Gradient;
        public byte padding_type { get; set; }
        public ushort duration { get; set; }
        public float radiusInner;
        public float radiusOuter;

        public AnimationInstance CreateInstance(DataSet.AnimationBits bits)
        {
            return new AnimationInstanceFadeCandy(this, bits);
        }
    };

    /// <summary>
    /// Procedural on off animation instance data
    /// </summary>
    public class AnimationInstanceFadeCandy
        : AnimationInstance
    {
        static Vector3[] rotatedD20Normals =
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

        public class MovingSphere
        {
            public Vector3 startPosition;
            public Vector3 velocity;
            public float radiusInner;
            public float radiusOuter;
            public int startTime;

            public Vector3 position(int ms)
            {
                int time = ms - startTime;
                return startPosition + velocity * (float)time / 1000.0f;
            }
        }
        public MovingSphere[] spheres;
        public int sphereCount;

        public AnimationInstanceFadeCandy(Animation animation, DataSet.AnimationBits bits)
            : base(animation, bits)
        {
        }

        void AddSphere(int ms)
        {
            if (sphereCount < spheres.Length)
            {
                var preset = getPreset();
                var sphere = spheres[sphereCount];
                Vector3 direction = Random.insideUnitSphere;
                Vector3 offset = Random.insideUnitSphere;

                sphere.startPosition = direction * 2.0f;
                sphere.velocity = -direction * 2.0f + offset;
                sphere.radiusInner = preset.radiusInner;
                sphere.radiusOuter = preset.radiusOuter;
                sphere.startTime = ms;
                sphereCount += 1;
            }
        }

        public override void start(int _startTime, byte _remapFace, bool _loop)
        {
            base.start(_startTime, _remapFace, _loop);
            AddSphere(_startTime);
        }

        public override int updateLEDs(int ms, int[] retIndices, uint[] retColors)
        {
            int time = ms - startTime;
            var preset = getPreset();

            //// Figure out the color from the gradient
            //var gradient = animationBits.getRGBTrack(preset.gradientTrackOffset);
            //int gradientTime = time * 1000 / preset.duration;
            //uint color = gradient.evaluateColor(animationBits, gradientTime);

            // Fill the indices and colors for the anim controller to know how to update leds
            int retCount = 20;
            for (int i = 0; i < 20; ++i)
            {
                retIndices[i] = i;
                retColors[i] = 0;
                Vector3 ledPosition = rotatedD20Normals[i];
                for (int j = 0; j < sphereCount; ++j)
                {
                    var sphere = spheres[j];
                    Vector3 sphereCenter = sphere.position(ms);
                    float distance = Vector3.Distance(sphereCenter, ledPosition);
                    float intensity = 0.0f;
                    if (distance < sphere.radiusOuter)
                    {
                        if (distance < sphere.radiusInner)
                        {
                            intensity = 1.0f;
                        }
                        else
                        {
                            intensity = Mathf.Lerp(1.0f, 0.0f, (distance - sphere.radiusInner) / (sphere.radiusOuter = sphere.radiusInner));
                        }
                    }
                    var color = ColorUtils.toColor((byte)(intensity * 255.0f), 0, 0);
                    retColors[i] = ColorUtils.addColors(retColors[i], color);
                }
            }
            return retCount;
        }

        public override int stop(int[] retIndices)
        {
            int retCount = 0;
            for (int i = 0; i < 20; ++i)
            {
                retIndices[retCount] = i;
                retCount++;
            }
            return retCount;
        }

        public AnimationFadeCandy getPreset()
        {
            return (AnimationFadeCandy)animationPreset;
        }
    };
}
