using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using UnityEngine;
using System;

public class SampleXML : MonoBehaviour
{

    public SampleScriptableObject Sample;
    // Start is called before the first frame update
    void Start()
    {
        SaveXML();
        //LoadXML();

        Sample = ScriptableObject.CreateInstance<SampleScriptableObject>();
        Sample.Index++;
        Debug.Log(Sample.Index);
    }

    void SaveXML()
    {
        XmlDocument xmlDoc = new XmlDocument();
        XmlElement xmlRootElment = xmlDoc.CreateElement("今天上课学生"); 
        xmlRootElment.InnerText = "Unity3D课程";
        xmlRootElment.SetAttribute("时间", DateTime.Now.ToShortDateString());

        XmlElement xmlStudentElement = xmlDoc.CreateElement("男生");
        for (int i = 0210500; i < 0210506; i++)
        {
            xmlStudentElement.SetAttribute("性别", "男");
            XmlElement xmlStudentIndexElement = xmlDoc.CreateElement("学号");
            xmlStudentIndexElement.InnerText = i.ToString();

            xmlStudentElement.AppendChild(xmlStudentIndexElement);
        }

        xmlRootElment.AppendChild(xmlStudentElement);
        XmlElement xmlTeacherElement = xmlDoc.CreateElement("女生");
        for (int i = 0; i < 8; i++)
        {
            xmlTeacherElement.SetAttribute("性别", "女");
            XmlElement xmlTeacherIndexElement = xmlDoc.CreateElement("学号");
            xmlTeacherIndexElement.InnerText = i.ToString();

            xmlTeacherElement.AppendChild(xmlTeacherIndexElement);
        }
        xmlRootElment.AppendChild(xmlTeacherElement);
        xmlDoc.AppendChild(xmlRootElment);

        string xmlPath = Path.Combine(Application.dataPath, "xmlDocSample.Sample");
        xmlDoc.Save(xmlPath);
    }
    void LoadXML()
    {
        string xmlPath = Path.Combine(Application.dataPath, "xmlDocSample.Sample");
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlPath);

        //node是节点读取类型，但部分类型无法设置
        foreach(XmlNode node in xmlDoc.ChildNodes)
        {
            Debug.Log(node.Name);
            XmlElement element = (XmlElement)node;

            element.SetAttribute("动态添加", "运行时");
            foreach(XmlNode childNode in node.ChildNodes)
            {
                Debug.Log(childNode.Name);
            }
        }
        xmlDoc.Save(xmlPath);
    }
    void Update()
    {
        
    }
}
