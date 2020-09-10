using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Animations;
using Behaviors;
using Presets;
using Dice;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[System.Serializable]
public class PreviewSettings
{
    public DesignAndColor design;
}

public class AppDataSet : SingletonMonoBehaviour<AppDataSet>
{
    [System.Serializable]
    public class Data
    {
        public int jsonVersion = 1;
        public List<EditDie> dice = new List<EditDie>();
        public List<EditPattern> patterns = new List<EditPattern>();
        public List<EditAnimation> animations = new List<EditAnimation>();
        public List<EditBehavior> behaviors = new List<EditBehavior>();
        public List<EditPreset> presets = new List<EditPreset>();

        [JsonIgnore]
        public EditPreset activePreset; // Updated after serializing
        public int activePresetIndex; // Updated before serializing

        public void Clear()
        {
            dice.Clear();
            patterns.Clear();
            animations.Clear();
            behaviors.Clear();
            presets.Clear();
        }
    }

    Data data = new Data();
    public List<EditDie> dice => data.dice;
    public List<EditPattern> patterns => data.patterns;
    public List<EditAnimation> animations => data.animations;
    public List<EditBehavior> behaviors => data.behaviors;
    public List<EditPreset> presets => data.presets;
    public EditPreset activePreset
    {
        get { return data.activePreset; }
        set { data.activePreset = value; }
    }

    JsonSerializer CreateSerializer()
    {
        var serializer = new JsonSerializer();
        serializer.Converters.Add(new EditAnimationConverter());
        serializer.Converters.Add(new EditAnimationGradientPattern.Converter(this));
        serializer.Converters.Add(new EditAnimationKeyframed.Converter(this));
        serializer.Converters.Add(new EditActionConverter());
        serializer.Converters.Add(new EditActionPlayAnimation.Converter(this));
        serializer.Converters.Add(new EditDieAssignmentConverter(this));
        serializer.Converters.Add(new EditConditionConverter());
        return serializer;
    }

    public void ToJson(JsonWriter writer, JsonSerializer serializer)
    {
        data.activePresetIndex = data.presets.IndexOf(data.activePreset);
        serializer.Serialize(writer, data);
    }

    public void FromJson(JsonReader reader, JsonSerializer serializer)
    {
        data.Clear();
        serializer.Populate(reader, data); 
        data.activePreset = data.activePresetIndex != -1 ? data.presets[data.activePresetIndex] : null;
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

    public EditDataSet ExtractEditSetForAnimation(EditAnimation animation)
    {
        EditDataSet ret = new EditDataSet();
        ret.animations.Add(animation);
        return ret;
    }

    public EditDie AddNewDie(Die die)
    {
        var ret = new EditDie()
        {
            name = die.name,
            deviceId = die.deviceId,
            faceCount = die.faceCount,
            designAndColor = die.designAndColor,
            dataSetHash = die.dataSetHash
        };
        dice.Add(ret);
        return ret;
    }

    public EditAnimation AddNewDefaultAnimation()
    {
        var newAnim = new Animations.EditAnimationSimple();
        newAnim.duration = 3.0f;
        newAnim.color = new Color32(0xFF, 0x30, 0x00, 0xFF);
        newAnim.faces = 0b11111111111111111111;
        newAnim.name = "New Lighting Pattern";
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

    public void DeleteAnimation(EditAnimation animation)
    {
        foreach (var behavior in behaviors)
        {
            behavior.DeleteAnimation(animation);
        }
        animations.Remove(animation);
    }

    public EditPattern AddNewDefaultPattern()
    {
        var newPattern = new Animations.EditPattern();
        newPattern.name = "New Pattern";
        for (int i = 0; i < 20; ++i)
        {
            var grad = new EditRGBGradient();
            grad.keyframes.Add(new EditRGBKeyframe() { time = 0.0f, color = Color.black });
            grad.keyframes.Add(new EditRGBKeyframe() { time = 0.5f, color = Color.white });
            grad.keyframes.Add(new EditRGBKeyframe() { time = 1.0f, color = Color.black });
            newPattern.gradients.Add(grad);
        }
        patterns.Add(newPattern);
        return newPattern;
    }

    public void ReplacePattern(EditPattern oldPattern, EditPattern newPattern)
    {
        foreach (var animation in animations)
        {
            animation.ReplacePattern(oldPattern, newPattern);
        }
        int oldPatternIndex = patterns.IndexOf(oldPattern);
        patterns[oldPatternIndex] = newPattern;
    }

    public void DeletePattern(EditPattern pattern)
    {
        foreach (var animation in animations)
        {
            animation.DeletePattern(pattern);
        }
        patterns.Remove(pattern);
    }

    public IEnumerable<Animations.EditAnimation> CollectAnimationsForPattern(Animations.EditPattern pattern)
    {
        return animations.Where(b => b.DependsOnPattern(pattern));
    }

    public IEnumerable<Behaviors.EditBehavior> CollectBehaviorsForAnimation(Animations.EditAnimation anim)
    {
        return behaviors.Where(b => b.DependsOnAnimation(anim));
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
            actions = new List<Behaviors.EditAction> () {
                new Behaviors.EditActionPlayAnimation()
                {
                    animation = null,
                    faceIndex = 0,
                    loopCount = 1
                }
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

    public void DeleteBehavior(EditBehavior behavior)
    {
        foreach (var preset in presets)
        {
            preset.DeleteBehavior(behavior);
        }
        behaviors.Remove(behavior);
    }

    public IEnumerable<Presets.EditPreset> CollectPresetsForBehavior(Behaviors.EditBehavior behavior)
    {
        return presets.Where(b => b.DependsOnBehavior(behavior));
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
        newPreset.name = "New Profile";
        newPreset.dieAssignments.Add(new EditDieAssignment()
        {
            die = null,
            behavior = null
        });
        presets.Add(newPreset);
        return newPreset;
    }

    public EditPreset DuplicatePreset(EditPreset editPreset)
    {
        var newPreset = editPreset.Duplicate();
        presets.Add(newPreset);
        return newPreset;
    }

    public void DeletePreset(EditPreset editPreset)
    {
        presets.Remove(editPreset);
    }

    public IEnumerable<Presets.EditPreset> CollectPresetsForDie(Dice.EditDie die)
    {
        return presets.Where(b => b.DependsOnDie(die));
    }

    public void DeleteDie(EditDie editDie)
    {
        foreach (var preset in presets)
        {
            preset.DeleteDie(editDie);
        }
        dice.Remove(editDie);
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
        if (System.IO.File.Exists(path))
        {
            var serializer = CreateSerializer();
            using (StreamReader sw = new StreamReader(path))
            using (JsonReader reader = new JsonTextReader(sw))
            {
                FromJson(reader, serializer);
            }
        }
        else
        {
            var serializer = CreateSerializer();
            using (StringReader sw = new StringReader(AppConstants.Instance.defaultDiceJson.text))
            using (JsonReader reader = new JsonTextReader(sw))
            {
                FromJson(reader, serializer);
            }
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
        var die0 = new EditDie()
        {
            name = "Die 000",
            deviceId = 0x123456789ABCDEF0,
            faceCount = 20,
            designAndColor = DesignAndColor.V3_Orange
        };
        ret.dice.Add(die0);
        var die1 = new EditDie()
        {
            name = "Die 001",
            deviceId = 0xABCDEF0123456789,
            faceCount = 20,
            designAndColor = DesignAndColor.V5_Black
        };
        ret.dice.Add(die1);
        var die2 = new EditDie()
        {
            name = "Die 002",
            deviceId = 0xCDEF0123456789AB,
            faceCount = 20,
            designAndColor = DesignAndColor.V5_Grey
        };
        ret.dice.Add(die2);
        var die3 = new EditDie()
        {
            name = "Die 003",
            deviceId = 0xEF0123456789ABCD,
            faceCount = 20,
            designAndColor = DesignAndColor.V5_Gold
        };
        ret.dice.Add(die3);
        
        EditAnimationSimple simpleAnim = new EditAnimationSimple();
        simpleAnim.duration = 1.0f;
        simpleAnim.color = Color.blue;
        simpleAnim.faces = 0b11111111111111111111;
        simpleAnim.name = "Simple Anim 1";
        ret.animations.Add(simpleAnim);


        EditBehavior behavior = new EditBehavior();
        behavior.rules.Add(new EditRule() {
            condition = new EditConditionRolling(),
            actions = new List<EditAction> () { new EditActionPlayAnimation() { animation = simpleAnim, faceIndex = 0, loopCount = 1 }}
        });
        behavior.rules.Add(new EditRule() {
            condition = new EditConditionFaceCompare()
            {
                faceIndex = 19,
                flags = ConditionFaceCompare_Flags.Equal
            },
            actions = new List<EditAction> () { new EditActionPlayAnimation() { animation = simpleAnim, faceIndex = 19, loopCount = 1 }}
        });
        behavior.rules.Add(new EditRule() {
            condition = new EditConditionFaceCompare()
            {
                faceIndex = 0,
                flags = ConditionFaceCompare_Flags.Less | ConditionFaceCompare_Flags.Equal | ConditionFaceCompare_Flags.Greater
            },
            actions = new List<EditAction> () { new EditActionPlayAnimation() { animation = simpleAnim, faceIndex = 2, loopCount = 1 }}
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
