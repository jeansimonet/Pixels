using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

public class AppSettings : SingletonMonoBehaviour<AppSettings>
{
    [System.Serializable]
    public class Data
    {
        public bool displayWhatsNew = true;
        public bool mainTutorialEnabled = true;
        public bool homeTutorialEnabled = true;
        public bool presetsTutorialEnabled = true;
        public bool presetTutorialEnabled = true;
        public bool behaviorsTutorialEnabled = true;
        public bool behaviorTutorialEnabled = true;
        public bool ruleTutorialEnabled = true;
        public bool animationsTutorialEnabled = true;
        public bool animationTutorialEnabled = true;
    }
    Data data = new Data();

    public bool displayWhatsNew => data.displayWhatsNew;
    public bool mainTutorialEnabled => data.mainTutorialEnabled;
    public bool homeTutorialEnabled => data.homeTutorialEnabled;
    public bool presetsTutorialEnabled => data.presetsTutorialEnabled;
    public bool presetTutorialEnabled => data.presetTutorialEnabled;
    public bool behaviorsTutorialEnabled => data.behaviorsTutorialEnabled;
    public bool behaviorTutorialEnabled => data.behaviorTutorialEnabled;
    public bool ruleTutorialEnabled => data.ruleTutorialEnabled;
    public bool animationsTutorialEnabled => data.animationsTutorialEnabled;
    public bool animationTutorialEnabled => data.animationTutorialEnabled;

    public void SetDisplayWhatsNew(bool value)
    {
        data.displayWhatsNew = value;
        SaveData();
    }

    public void SetMainTutorialEnabled(bool value)
    {
        data.mainTutorialEnabled = value;
        SaveData();
    }

    public void SetHomeTutorialEnabled(bool value)
    {
        data.homeTutorialEnabled = value;
        SaveData();
    }

    public void SetPresetsTutorialEnabled(bool value)
    {
        data.presetsTutorialEnabled = value;
        SaveData();
    }

    public void SetPresetTutorialEnabled(bool value)
    {
        data.presetTutorialEnabled = value;
        SaveData();
    }

    public void SetBehaviorsTutorialEnabled(bool value)
    {
        data.behaviorsTutorialEnabled = value;
        SaveData();
    }

    public void SetBehaviorTutorialEnabled(bool value)
    {
        data.behaviorTutorialEnabled = value;
        SaveData();
    }

    public void SetRuleTutorialEnabled(bool value)
    {
        data.ruleTutorialEnabled = value;
        SaveData();
    }

    public void SetAnimationsTutorialEnabled (bool value)
    {
        data.animationsTutorialEnabled = value;
        SaveData();
    }

    public void SetAnimationTutorialEnabled(bool value)
    {
        data.animationTutorialEnabled = value;
        SaveData();
    }

    public void EnableAllTutorials()
    {
        SetMainTutorialEnabled(true);
        SetHomeTutorialEnabled(true);
        SetPresetsTutorialEnabled(true);
        SetPresetTutorialEnabled(true);
        SetBehaviorsTutorialEnabled(true);
        SetBehaviorTutorialEnabled(true);
        SetRuleTutorialEnabled(true);
        SetAnimationsTutorialEnabled(true);
        SetAnimationTutorialEnabled(true);
    }

    public void DisableAllTutorials()
    {
        SetMainTutorialEnabled(false);
        SetHomeTutorialEnabled(false);
        SetPresetsTutorialEnabled(false);
        SetPresetTutorialEnabled(false);
        SetBehaviorsTutorialEnabled(false);
        SetBehaviorTutorialEnabled(false);
        SetRuleTutorialEnabled(false);
        SetAnimationsTutorialEnabled(false);
        SetAnimationTutorialEnabled(false);
    }

    JsonSerializer CreateSerializer()
    {
        var serializer = new JsonSerializer();
        return serializer;
    }

    public void ToJson(JsonWriter writer, JsonSerializer serializer)
    {
        serializer.Serialize(writer, data);
    }

    public void FromJson(JsonReader reader, JsonSerializer serializer)
    {
        serializer.Populate(reader, data); 
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
        var path = System.IO.Path.Combine(Application.persistentDataPath, AppConstants.Instance.SettingsFilename);
        if (System.IO.File.Exists(path))
        {
            var serializer = CreateSerializer();
            using (StreamReader sw = new StreamReader(path))
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
        var path = System.IO.Path.Combine(Application.persistentDataPath, AppConstants.Instance.SettingsFilename);
        var serializer = CreateSerializer();
        using (StreamWriter sw = new StreamWriter(path))
        using (JsonWriter writer = new JsonTextWriter(sw))
        {
            writer.Formatting = Formatting.Indented;
            ToJson(writer, serializer);
        }
    }

}
