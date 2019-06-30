using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

namespace Animations
{
    /// <summary>
    /// Simple anition keyframe, time in seconds and color!
    /// </summary>
    public class EditKeyframe
    {
        public float time = -1;
        public Color color;
    }

    /// <summary>
    /// Simple list of keyframes for a led
    /// </summary>
    public class EditTrack
    {
        public int ledIndex = -1;
        public float duration { get { return keyframes.Max(k => k.time); } }
        public float firstTime { get { return keyframes.First().time; } }
        public float lastTime { get { return keyframes.Last().time; } }
        public List<EditKeyframe> keyframes = new List<EditKeyframe>();
    }

    /// <summary>
    /// An animation is a list of tracks!
    /// </summary>
    public class EditAnimation
    {
        public float duration
        {
            get
            {
                if (tracks == null || tracks.Count == 0)
                    return 0.0f;
                else
                    return tracks.Max(t => t.duration);
            }
        }
        public List<EditTrack> tracks = new List<EditTrack>();
    }

    /// <summary>
    /// The animation set is a list of multiple animations
    /// This class knows how to convert to/from the runtime data used by the dice
    /// </summary>
    public class EditAnimationSet
    {
        public List<EditAnimation> animations = new List<EditAnimation>();

        public void FromAnimationSet(AnimationSet set)
        {
            // Reset the animations, and read them in!
            animations = new List<EditAnimation>();
            for (int i = 0; i < set.animations.Length; ++i)
            {
                var anim = set.getAnimation((ushort)i);
                var editAnim = new EditAnimation();
                for (int j = 0; j < anim.trackCount; ++j)
                {
                    var track = anim.GetTrack(set, (ushort)j);
                    var editTrack = new EditTrack();
                    editTrack.ledIndex = track.ledIndex;
                    var rgbTrack = track.GetTrack(set);
                    for (int k = 0; k < rgbTrack.keyFrameCount; ++k)
                    {
                        var kf = rgbTrack.GetKeyframe(set, (ushort)k);
                        var editKf = new EditKeyframe();
                        editKf.time = (float)kf.time() / 1000.0f;
                        editKf.color = set.getColor(kf.colorIndex());
                        editTrack.keyframes.Add(editKf);
                    }
                    editAnim.tracks.Add(editTrack);
                }
                animations.Add(editAnim);
            }
        }

        public AnimationSet ToAnimationSet()
        {
            AnimationSet set = new AnimationSet();

            // Collect all colors used and stuff them in the palette
            var colors = new Dictionary<Color, int>();
            int index = 0;
            foreach (var anim in animations)
            {
                foreach (var animTrack in anim.tracks)
                {
                    foreach (var keyframe in animTrack.keyframes)
                    {
                        int ignore = 0;
                        if (!colors.TryGetValue(keyframe.color, out ignore))
                        {
                            colors.Add(keyframe.color, index);
                            index++;
                        }
                    }
                }
            }

            // Add the colors to the palette
            set.palette = new byte[colors.Count * 3];
            foreach (var ci in colors)
            {
                Color color = ci.Key;
                int i = ci.Value;
                set.palette[i * 3 + 0] = (byte)(color.r * 255.0f);
                set.palette[i * 3 + 1] = (byte)(color.g * 255.0f);
                set.palette[i * 3 + 2] = (byte)(color.b * 255.0f);
            }

            int currentTrackOffset = 0;
            int currentKeyframeOffset = 0;

            var anims = new List<Animation>();
            var tracks = new List<AnimationTrack>();
            var rgbTracks = new List<RGBTrack>();
            var keyframes = new List<RGBKeyframe>();

            // Add animations
            for (int i = 0; i < animations.Count; ++i)
            {
                var editAnim = animations[i];
                var anim = new Animation();
                anim.duration = (ushort)(editAnim.duration * 1000.0f);
                anim.tracksOffset = (ushort)currentTrackOffset;
                anim.trackCount = (ushort)editAnim.tracks.Count;
                anims.Add(anim);

                // Now add tracks
                for (int j = 0; j < editAnim.tracks.Count; ++j)
                {
                    var editTrack = editAnim.tracks[j];
                    var track = new AnimationTrack();
                    track.ledIndex = (byte)editTrack.ledIndex;

                    var rgbTrack = new RGBTrack();
                    rgbTrack.keyframesOffset = (ushort)currentKeyframeOffset;
                    rgbTrack.keyFrameCount = (byte)editTrack.keyframes.Count;
                    track.trackOffset = (ushort)rgbTracks.Count;
                    rgbTracks.Add(rgbTrack);

                    tracks.Add(track);

                    // Now add keyframes
                    for (int k = 0; k < editTrack.keyframes.Count; ++k)
                    {
                        var editKeyframe = editTrack.keyframes[k];
                        int colorIndex = colors[editKeyframe.color];
                        var keyframe = new RGBKeyframe();
                        keyframe.setTimeAndColorIndex((ushort)(editKeyframe.time * 1000.0f), (ushort)colorIndex);
                        keyframes.Add(keyframe);
                    }
                    currentKeyframeOffset += editTrack.keyframes.Count;
                }
                currentTrackOffset += editAnim.tracks.Count;
            }

            set.keyframes = keyframes.ToArray();
            set.rgbTracks = rgbTracks.ToArray();
            set.tracks = tracks.ToArray();
            set.animations = anims.ToArray();

            return set;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("Animations: ");
            builder.Append(animations.Count);
            builder.AppendLine();
            for (int i = 0; i < animations.Count; ++i)
            {
                var anim = animations[i];
                builder.Append("\tAnim ");
                builder.Append(i);
                builder.Append(" contains ");
                builder.Append(anim.tracks.Count);
                builder.Append(" tracks");
                builder.AppendLine();
                for (int j = 0; j < anim.tracks.Count; ++j)
                {
                    var track = anim.tracks[j];
                    builder.Append("\t\tTrack ");
                    builder.Append(j);
                    builder.Append(" contains ");
                    builder.Append(track.keyframes.Count);
                    builder.Append(" keyframes");
                    builder.AppendLine();
                    for (int k = 0; k < track.keyframes.Count; ++k)
                    {
                        var keyframe = track.keyframes[k];
                        builder.Append("\t\t\tTime ");
                        builder.Append(keyframe.time);
                        builder.Append(" : ");
                        builder.Append(keyframe.color);
                        builder.AppendLine();
                    }
                }
            }
            return builder.ToString();
        }

        public static EditAnimationSet CreateTestSet()
        {
            EditAnimationSet set = new EditAnimationSet();
            for (int a = 0; a < 5; ++a)
            {
                EditAnimation anim = new EditAnimation();
                for (int i = 0; i < a + 1; ++i)
                {
                    var track = new EditTrack();
                    track.ledIndex = i;
                    for (int j = 0; j < 3; ++j)
                    {
                        var kf = new EditKeyframe();
                        kf.time = j;
                        kf.color = Random.ColorHSV();
                        track.keyframes.Add(kf);
                    }
                    anim.tracks.Add(track);
                }
                set.animations.Add(anim);
            }

            return set;
        }
    }
}
