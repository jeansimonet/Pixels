using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DiceAnimProgrammer
    : MonoBehaviour
{
    public Central central;
    public TimelineView timeline;
    public PleaseWaitDialogBox pleaseWait;
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
        yield return new WaitUntil(() => central.state == Central.State.Idle);

        // Register to be notified of new dice getting connected
        central.onDieReady += OnNewDie;

        // Create empty anim if needed
        if (!LoadFromJson())
        {
            animationSet = new Animations.EditAnimationSet();
            animationSet.animations = new List<Animations.EditAnimation>();
            var anim = new Animations.EditAnimation();
            anim.Reset();
            animationSet.animations.Add(anim);
            timeline.SetAnimations(animationSet);
        }
    }

    private void OnDisable()
    {
        central.onDieReady -= OnNewDie;
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
        File.WriteAllText(System.IO.Path.Combine(Application.persistentDataPath, JsonFilePath), jsonText);
    }

    public void LoadFromJsonFile()
    {
        LoadFromJson();
    }

    public bool LoadFromJson()
    {
        var path = System.IO.Path.Combine(Application.persistentDataPath, JsonFilePath);
        bool ret = File.Exists(path);
        if (ret)
        {
            string jsonText = File.ReadAllText(path);
            animationSet = JsonUtility.FromJson<Animations.EditAnimationSet>(jsonText);
            timeline.SetAnimations(animationSet);
        }
        return ret;
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
        pleaseWait.Show("Uploading Animation Set to Dice");
        timeline.ApplyChanges();
        var rawAnim = animationSet.ToAnimationSet();
        var newByteArray = rawAnim.ToByteArray();
        if (animationSetByteArray == null || !ByteArraysEquals(newByteArray, animationSetByteArray))
        {
            // Update stored array
            animationSetByteArray = newByteArray;

            // Write to file (why not?)
            string jsonText = JsonUtility.ToJson(animationSet, true);
            File.WriteAllText(System.IO.Path.Combine(Application.persistentDataPath, JsonFilePath), jsonText);

            // Upload!
            if (die != null)
            {
                yield return die.UploadAnimationSet(rawAnim, null);
            }
        }
        pleaseWait.Hide();
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
            die.PlayAnimation(timeline.CurrentAnimationIndex);
            pleaseWait.Show("Playing Animation on Dice");
            yield return new WaitForSeconds(timeline.CurrentAnimation.duration);
            pleaseWait.Hide();

        }
    }
}
