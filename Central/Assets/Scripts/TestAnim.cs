using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAnim : MonoBehaviour {

	// Use this for initialization
	void Start () {

        Animations.AnimationSet set = new Animations.AnimationSet();
        set.animations = new Animations.RGBAnimation[5];
        for (int a = 0; a < set.animations.Length; ++a)
        {
            Animations.RGBAnimation anim = new Animations.RGBAnimation();
            anim.tracks = new Animations.RGBAnimationTrack[a + 1];
            anim.duration = 2500;
            for (int i = 0; i < anim.tracks.Length; ++i)
            {
                var keyframes = new Animations.RGBKeyframe[Animations.Constants.MAX_KEYFRAMES];
                for (int j = 0; j < 3; ++j)
                {
                    keyframes[j].time = (byte)j;
                    keyframes[j].red = (byte)j;
                    keyframes[j].green = (byte)j;
                    keyframes[j].blue = (byte)j;
                }

                var track = new Animations.RGBAnimationTrack();
                track.keyframes = keyframes;
                track.count = 3;
                track.duration = (short)(2000 - 10 * i);
                track.startTime = (short)(123 + i);
                track.ledIndex = (byte)(i);
                anim.tracks[i] = track;
            }

            set.animations[a] = anim;

            var bytes = Animations.RGBAnimation.ToByteArray(anim);
            Debug.Log(bytes.ToString());
        }

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
