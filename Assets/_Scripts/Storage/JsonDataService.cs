using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class JsonDataService
{
    private JsonSerializerSettings settings;

    // :::::::::: METHODS ::::::::::
    public JsonDataService()
    {
        settings = new JsonSerializerSettings
        {
            Converters = {
            new Vector2Converter(),
            new Vector2IntConverter(),
            new Vector3Converter(),
            new Vector3IntConverter()
        },
            Formatting = Formatting.None
        };
    }
    
    // ::::: Serialize (Save)
    public bool SaveData<T>(string RelativePath, T Data, bool Encrypted)
    {
        string path = Application.persistentDataPath + RelativePath;

        try
        {
            if (File.Exists(path)) File.Delete(path);

            using FileStream stream = File.Create(path);
            stream.Close();
            string json = JsonConvert.SerializeObject(Data, settings);
            File.WriteAllText(path, json);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error serializing data: {e.Message}");
            return false;
        }
    }

    // ::::: Deserialize (Load)
    public T LoadData<T>(string RelativePath, bool Encrypted)
    {
        string path = Application.persistentDataPath + RelativePath;

        if (!File.Exists(path))
            throw new FileNotFoundException($"{path} does not exist!");

        try
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(json, settings);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error deserializing data: {e.Message}");
            throw;
        }
    }
}
