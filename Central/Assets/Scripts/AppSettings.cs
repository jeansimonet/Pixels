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
    }
    Data data = new Data();

    public bool displayWhatsNew => data.displayWhatsNew;

    public void SetDisplayWhatsNew(bool value)
    {
        data.displayWhatsNew = value;
        SaveData();
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
