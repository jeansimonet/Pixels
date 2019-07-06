using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DiceAnimProgrammer
    : MonoBehaviour
    , IClient
{
    public Central central;
    public TimelineView timeline;
    public string JsonFilePath = "animation_set.json";

    Die die = null;
    Animations.EditAnimationSet animationSet;
    
    // We use this to compare against the last version uploaded on the dice
    // and avoid re-uploading the set every time.
    // This is a hack to not have to implement change detection in the animation editor
    byte[] animationSetByteArray; 

    void Awake()
    {
    }

    // Use this for initialization
    IEnumerator Start()
    {
        // Until we can properly record data, disable
        yield return new WaitUntil(() => central.state == CentralState.Idle);

        // Register to be notified of new dice getting connected
        central.RegisterClient(this);

        // Create empty anim if needed
        animationSet = new Animations.EditAnimationSet();
        animationSet.animations = new List<Animations.EditAnimation>();
        animationSet.animations.Add(new Animations.EditAnimation());
        timeline.ChangeCurrentAnimation(animationSet.animations[0]);
    }

    public void OnNewDie(Die die)
    {
        if (this.die == null)
        {
            this.die = die;
            Debug.Log("Led animator connected to " + die.name);
        }
    }


    // Update is called once per frame
    void Update ()
    {
		
	}

    public void SaveToJsonFile()
    {
        timeline.ApplyChanges();
        string jsonText = JsonUtility.ToJson(animationSet, true);
        File.WriteAllText(JsonFilePath, jsonText);
    }

    public void LoadFromJsonFile()
    {
        string jsonText = File.ReadAllText(JsonFilePath);
        animationSet = JsonUtility.FromJson<Animations.EditAnimationSet>(jsonText);
        if (animationSet.animations.Count > 0)
        {
            timeline.ChangeCurrentAnimation(animationSet.animations[0]);
        }
    }

    bool ByteArraysEquals(byte[] array1, byte[] array2)
    {
        if (array1.Length != array2.Length)
            return false;

        for (int i = 0; i < array1.Length; ++i)
        {
            if (array1[i] != array2[i])
                return false;
        }

        return true;
    }

    public void UploadAnimationSet()
    {
        StartCoroutine(UploadAnimationSetCr());
    }

    IEnumerator UploadAnimationSetCr()
    {
        timeline.ApplyChanges();
        var rawAnim = animationSet.ToAnimationSet();
        var newByteArray = rawAnim.ToByteArray();
        if (animationSetByteArray == null || !ByteArraysEquals(newByteArray, animationSetByteArray))
        {
            // Update stored array
            animationSetByteArray = newByteArray;

            // Write to file (why not?)
            string jsonText = JsonUtility.ToJson(animationSet, true);
            File.WriteAllText(JsonFilePath, jsonText);

            // Upload!
            if (die != null)
            {
                yield return die.UploadAnimationSet(rawAnim);
            }
        }
    }

    public void DownloadAnimationSet()
    {
        if (die != null)
        {
            //die.DownloadAnimationSet(animationSet.ToAnimationSet());
        }
    }

    public void PlayAnim()
    {
        StartCoroutine(PlayAnimCr());
    }

    IEnumerator PlayAnimCr()
    {
        if (die != null)
        {
            yield return StartCoroutine(UploadAnimationSetCr());
            die.PlayAnimation(0);
        }
    }
}
