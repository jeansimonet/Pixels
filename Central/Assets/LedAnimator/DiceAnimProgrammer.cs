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

        Debug.Log(animationSet.ToString());

        string jsonText = JsonUtility.ToJson(animationSet.ToAnimationSet(), true);
        File.WriteAllText(JsonFilePath, jsonText);
    }

    public void LoadFromJsonFile()
    {
        string jsonText = File.ReadAllText(JsonFilePath);
        animationSet.FromAnimationSet(JsonUtility.FromJson<Animations.AnimationSet>(jsonText));

        Debug.Log(animationSet.ToString());

        if (animationSet.animations.Count > 0)
        {
            timeline.ChangeCurrentAnimation(animationSet.animations[0]);
        }
    }

    public void UploadAnimationSet()
    {
        if (die != null)
        {
            die.UploadAnimationSet(animationSet.ToAnimationSet());
        }
    }

    public void PlayAnim()
    {
    }
}
