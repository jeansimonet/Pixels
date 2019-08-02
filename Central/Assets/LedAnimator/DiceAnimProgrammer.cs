using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

//TODO use Die.DieType
public enum DiceType
{
	D6, D20,
}

public class DiceAnimProgrammer
    : MonoBehaviour
{
    public TimelineView timeline;
    public PleaseWaitDialogBox pleaseWait;
    public string JsonFileBaseName = "animation_set.json";

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
        yield return new WaitUntil(() => Central.Instance.state == Central.State.Idle);

        // Register to be notified of new dice getting connected
        Central.Instance.onDieReady += OnNewDie;

        // Create empty anim if needed
        LoadFromJson(DiceType.D20);
    }

    private void OnDisable()
    {
        if (Central.HasInstance)
        {
            Central.Instance.onDieReady -= OnNewDie;
        }
    }

    public void OnNewDie(Die die)
    {
        if (this.die == null)
        {
            this.die = die;
            Debug.Log("Led animator connected to " + die.name);
            die.SetLEDAnimatorMode();
        }
    }


    // Update is called once per frame
    void Update ()
    {
		
	}

    public void SaveToJsonFile()
    {
        SaveToJson(timeline.DiceType);
    }

    public void LoadD6FromJsonFile()
    {
        LoadFromJson(DiceType.D6);
    }

    public void LoadD20FromJsonFile()
    {
        LoadFromJson(DiceType.D20);
    }

    public bool LoadFromJson(DiceType diceType)
    {
        string path = GetJsonFilePathname(diceType);
        bool ret = File.Exists(path);
        if (ret)
        {
            string jsonText = File.ReadAllText(path);
            animationSet = JsonUtility.FromJson<Animations.EditAnimationSet>(jsonText);
            timeline.SetAnimations(diceType, animationSet);
            Debug.Log($"Loaded {diceType} animations from {path}");
        }
        else
        {
            animationSet = new Animations.EditAnimationSet();
            animationSet.animations = new List<Animations.EditAnimation>();
            var anim = new Animations.EditAnimation();
            anim.Reset();
            animationSet.animations.Add(anim);
            timeline.SetAnimations(diceType, animationSet);
            Debug.Log($"Loaded empty {diceType} animation");
        }
        return ret;
    }

    public void SaveToJson(DiceType diceType)
    {
        timeline.ApplyChanges();
        string jsonText = JsonUtility.ToJson(animationSet, true);
        string path = GetJsonFilePathname(diceType);
        File.WriteAllText(path, jsonText);
        Debug.Log($"Saved {diceType} animations to {path}");
    }

    string GetJsonFilePathname(DiceType diceType)
    {
        return System.IO.Path.Combine(Application.persistentDataPath, $"{diceType}_{JsonFileBaseName}");
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

        // Update stored array
        animationSetByteArray = newByteArray;

        // Write to file (why not?)
        string jsonText = JsonUtility.ToJson(animationSet, true);
        string path = GetJsonFilePathname(timeline.DiceType);
        File.WriteAllText(path, jsonText);

        // Upload!
        if (die != null)
        {
            yield return die.UploadAnimationSet(rawAnim, null);
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
            timeline.ApplyChanges();
            var rawAnim = animationSet.ToAnimationSet();
            var newByteArray = rawAnim.ToByteArray();
            if (animationSetByteArray == null || !ByteArraysEquals(newByteArray, animationSetByteArray))
            {
                yield return StartCoroutine(UploadAnimationSetCr());
            }

            die.PlayAnimation(timeline.CurrentAnimationIndex);
            pleaseWait.Show("Playing Animation on Dice");
            yield return new WaitForSeconds(timeline.CurrentAnimation.duration);
            pleaseWait.Hide();

        }
    }
}
