using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AudioClips
{
    [System.Serializable]
    public class EditAudioClip
        : EditObject
    {
        public string name;
        public uint id;

        [JsonIgnore]
		public float duration
        {
            get
            {
                var clipInfo = AudioClipManager.Instance.FindClip(name);
                if (clipInfo != null)
                {
                    return clipInfo.clip.length;
                }
                else
                {
                    Debug.LogError("Cannot find audio clip " + name);
                    return 0.0f;
                }
            }
        }

    }
}
