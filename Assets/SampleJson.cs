using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Newtonsoft.Json;
using System.IO;

public class Student
{
    public string Name;
    public int Index;
}

public class SampleJson : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Student sampleStudent = new Student();
        sampleStudent.Name = "zhangsan";
        sampleStudent.Index = 1;

        SaveJson(sampleStudent);
        LoadJson();
    }
    void SaveJson(object targgetObject)
    {
        string jsonString = JsonMapper.ToJson(targgetObject);
        string jsonpath = Path.Combine(Application.dataPath, "LitJson.Sample");
        File.WriteAllText(jsonpath, jsonString);

        jsonString = JsonConvert.SerializeObject(targgetObject);
        jsonpath = Path.Combine(Application.dataPath, "NewtonJson.Sample");
        File.WriteAllText(jsonpath, jsonString);

        jsonString = JsonUtility.ToJson(targgetObject);
        jsonpath = Path.Combine(Application.dataPath, "JsonUtility.Sample");
        File.WriteAllText(jsonpath, jsonString);
    }

    void LoadJson()
    {
        string jsonpath = Path.Combine(Application.dataPath, "LitJson.Sample");
        string jsonString = File.ReadAllText(jsonpath);
        Student sampleStudent = JsonMapper.ToObject<Student>(jsonString);
        Debug.Log(sampleStudent.Name);

        jsonpath = Path.Combine(Application.dataPath, "NewtonJson.Sample");
        jsonString = File.ReadAllText(jsonpath);
        sampleStudent = JsonConvert.DeserializeObject<Student>(jsonString);
        Debug.Log(sampleStudent.Name);

        jsonpath = Path.Combine(Application.dataPath, "JsonUtility.Sample");
        jsonString = File.ReadAllText(jsonpath);
        sampleStudent = JsonUtility.FromJson<Student>(jsonString);
        Debug.Log(sampleStudent.Name);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
