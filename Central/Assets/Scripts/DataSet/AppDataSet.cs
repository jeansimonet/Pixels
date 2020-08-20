using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Animations;
using Behaviors;
using Presets;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[System.Serializable]
public class PreviewSettings
{
    public DiceVariants.DesignAndColor design;
}

public class AppDataSet : SingletonMonoBehaviour<AppDataSet>
{
    [System.Serializable]
    public class Data
    {
        public List<Presets.EditDie> dice = new List<Presets.EditDie>();
        public List<EditAnimation> animations = new List<EditAnimation>();
        public List<EditBehavior> behaviors = new List<EditBehavior>();
        public List<EditPreset> presets = new List<EditPreset>();

        public void Clear()
        {
            dice.Clear();
            animations.Clear();
            behaviors.Clear();
            presets.Clear();
        }
    }

    Data data = new Data();
    public List<Presets.EditDie> dice => data.dice;
    public List<EditAnimation> animations => data.animations;
    public List<EditBehavior> behaviors => data.behaviors;
    public List<EditPreset> presets => data.presets;

    JsonSerializer CreateSerializer()
    {
        var serializer = new JsonSerializer();
        serializer.Converters.Add(new EditAnimationConverter());
        serializer.Converters.Add(new EditActionConverter());
        serializer.Converters.Add(new EditActionPlayAnimation.Converter(this));
        serializer.Converters.Add(new EditDieAssignmentConverter(this));
        serializer.Converters.Add(new EditConditionConverter());
        return serializer;
    }

    public void ToJson(JsonWriter writer, JsonSerializer serializer)
    {
        serializer.Serialize(writer, data);
    }

    public void FromJson(JsonReader reader, JsonSerializer serializer)
    {
        data.Clear();
        serializer.Populate(reader, data); 
    }

    public EditDataSet ExtractEditSetForDie(EditDie die)
    {
        EditDataSet ret = new EditDataSet();

        // Start with all the presets this die is a part of
        foreach (var preset in presets.Where(p => p.dieAssignments.Any(a => a.die == die)))
        {
            // Grab the behavior
            var behavior = preset.dieAssignments.First(a => a.die == die).behavior;
            ret.behaviors.Add(behavior);

            // And add the animations that this behavior uses
            ret.animations.AddRange(behavior.CollectAnimations());
        }

        return ret;
    }

    public EditDie AddNewDie(Die die)
    {
        return new EditDie()
        {
            name = die.name,
            deviceId = die.deviceId,
            faceCount = die.faceCount,
            designAndColor = die.designAndColor,
            dataSetHash = die.dataSetHash
        };
    }

    public EditDie FindDie(Die die)
    {
        return dice.FirstOrDefault(d =>
        {
            // We should only use device Id
            if (d.deviceId == 0 || die.deviceId == 0)
            {
                return d.name == die.name;
            }
            else
            {
                return d.deviceId == die.deviceId;
            }
        });
    }

    public EditAnimation AddNewDefaultAnimation()
    {
        var newAnim = new Animations.EditAnimationSimple();
        newAnim.duration = 3.0f;
        newAnim.color = new Color32(0xFF, 0x30, 0x00, 0xFF);
        newAnim.faces = 0b11111111111111111111;
        newAnim.name = "New Animation";
        animations.Add(newAnim);
        return newAnim;
    }

    public EditAnimation DuplicateAnimation(EditAnimation animation)
    {
        var newAnim = animation.Duplicate();
        animations.Add(newAnim);
        return newAnim;
    }

    public void ReplaceAnimation(EditAnimation oldAnimation, EditAnimation newAnimation)
    {
        foreach (var behavior in behaviors)
        {
            behavior.ReplaceAnimation(oldAnimation, newAnimation);
        }
        int oldAnimIndex = animations.IndexOf(oldAnimation);
        animations[oldAnimIndex] = newAnimation;
    }

    public EditBehavior AddNewDefaultBehavior()
    {
        var newBehavior = new Behaviors.EditBehavior();
        newBehavior.name = "New Behavior";
        newBehavior.description = "New Behavior Description";
        newBehavior.rules.Add(new Behaviors.EditRule()
        {
            condition = new Behaviors.EditConditionFaceCompare()
            {
                flags = ConditionFaceCompare_Flags.Equal,
                faceIndex = 19
            },
            action = new Behaviors.EditActionPlayAnimation()
            {
                animation = null,
                faceIndex = 0,
                loopCount = 1
            }
        });
        behaviors.Add(newBehavior);
        return newBehavior;
    }

    public EditBehavior DuplicateBehavior(EditBehavior behavior)
    {
        var newBehavior = behavior.Duplicate();
        behaviors.Add(newBehavior);
        return newBehavior;
    }

    public bool CheckDependency(EditDie die)
    {
        bool dependencyFound = false;
        foreach (var preset in presets)
        {
            dependencyFound = dependencyFound | preset.CheckDependency(die);
        }
        return dependencyFound;
    }

    public EditPreset AddNewDefaultPreset()
    {
        var newPreset = new EditPreset();
        newPreset.name = "New Preset";
        newPreset.dieAssignments.Add(new EditDieAssignment()
        {
            die = null,
            behavior = null
        });
        presets.Add(newPreset);
        return newPreset;
    }

    void OnEnable()
    {
        LoadData();
    }

    /// <summary>
    /// Load our pool from file
    /// </sumary>
    public void LoadData()
    {
        var path = System.IO.Path.Combine(Application.persistentDataPath, AppConstants.Instance.DataSetFilename);
        //var path = System.IO.Path.Combine(Application.persistentDataPath, $"test_dataset3.json");
        var serializer = CreateSerializer();
        using (StreamReader sw = new StreamReader(path))
        using (JsonReader reader = new JsonTextReader(sw))
        {
            FromJson(reader, serializer);
        }
    }

    /// <summary>
    /// Save our pool to file
    /// </sumary>
    public void SaveData()
    {
        var path = System.IO.Path.Combine(Application.persistentDataPath, AppConstants.Instance.DataSetFilename);
        var serializer = CreateSerializer();
        using (StreamWriter sw = new StreamWriter(path))
        using (JsonWriter writer = new JsonTextWriter(sw))
        {
            writer.Formatting = Formatting.Indented;
            ToJson(writer, serializer);
        }
    }

    public static AppDataSet CreateTestDataSet()
    {
        AppDataSet ret = new AppDataSet();

        // We only save the dice that we have indicated to be in the pool
        // (i.e. ignore dice that are 'new' and we didn't connect to)
        var die0 = new Presets.EditDie()
        {
            name = "Die 000",
            deviceId = 0x123456789ABCDEF0,
            faceCount = 20,
            designAndColor = DiceVariants.DesignAndColor.V3_Orange
        };
        ret.dice.Add(die0);
        var die1 = new Presets.EditDie()
        {
            name = "Die 001",
            deviceId = 0xABCDEF0123456789,
            faceCount = 20,
            designAndColor = DiceVariants.DesignAndColor.V5_Black
        };
        ret.dice.Add(die1);
        var die2 = new Presets.EditDie()
        {
            name = "Die 002",
            deviceId = 0xCDEF0123456789AB,
            faceCount = 20,
            designAndColor = DiceVariants.DesignAndColor.V5_Grey
        };
        ret.dice.Add(die2);
        var die3 = new Presets.EditDie()
        {
            name = "Die 003",
            deviceId = 0xEF0123456789ABCD,
            faceCount = 20,
            designAndColor = DiceVariants.DesignAndColor.V5_Gold
        };
        ret.dice.Add(die3);
        
        EditAnimationSimple simpleAnim = new EditAnimationSimple();
        simpleAnim.duration = 1.0f;
        simpleAnim.color = Color.blue;
        simpleAnim.faces = 0b11111111111111111111;
        simpleAnim.name = "Simple Anim 1";
        ret.animations.Add(simpleAnim);

        EditAnimationKeyframed keyAnim = new EditAnimationKeyframed();
        keyAnim.duration = 3.0f;
        keyAnim.name = "Keyframed Anim 2";
        keyAnim.tracks.Add(new EditRGBTrack()
        {
            ledIndices = new List<int>() { 1, 5, 9 },
            gradient = new EditRGBGradient() {
                keyframes = new List<EditRGBKeyframe>() {
                    new EditRGBKeyframe() { time = 0.0f, color = Color.black },
                    new EditRGBKeyframe() { time = 1.5f, color = Color.red },
                    new EditRGBKeyframe() { time = 3.0f, color = Color.black },
                }
            }
        });
        keyAnim.tracks.Add(new EditRGBTrack()
        {
            ledIndices = new List<int>() { 0, 2, 3, 4 },
            gradient = new EditRGBGradient() {
                keyframes = new List<EditRGBKeyframe>() {
                    new EditRGBKeyframe() { time = 0.0f, color = Color.black },
                    new EditRGBKeyframe() { time = 1.0f, color = Color.cyan },
                    new EditRGBKeyframe() { time = 2.0f, color = Color.cyan },
                    new EditRGBKeyframe() { time = 3.0f, color = Color.black },
                }
            }
        });
        ret.animations.Add(keyAnim);

        EditBehavior behavior = new EditBehavior();
        behavior.rules.Add(new EditRule() {
            condition = new EditConditionRolling(),
            action = new EditActionPlayAnimation() { animation = simpleAnim, faceIndex = 0, loopCount = 1 }
        });
        behavior.rules.Add(new EditRule() {
            condition = new EditConditionFaceCompare()
            {
                faceIndex = 19,
                flags = ConditionFaceCompare_Flags.Equal
            },
            action = new EditActionPlayAnimation() { animation = keyAnim, faceIndex = 19, loopCount = 1 }
        });
        behavior.rules.Add(new EditRule() {
            condition = new EditConditionFaceCompare()
            {
                faceIndex = 0,
                flags = ConditionFaceCompare_Flags.Less | ConditionFaceCompare_Flags.Equal | ConditionFaceCompare_Flags.Greater
            },
            action = new EditActionPlayAnimation() { animation = keyAnim, faceIndex = 2, loopCount = 1 }
        });
        ret.behaviors.Add(behavior);

        ret.presets.Add(new EditPreset()
        {
            name = "Preset 0",
            dieAssignments = new List<EditDieAssignment>()
            {
                new EditDieAssignment()
                {
                    die = die0,
                    behavior = behavior
                },
                new EditDieAssignment()
                {
                    die = die1,
                    behavior = behavior
                }
            }
        });

        return ret;
    }
}
