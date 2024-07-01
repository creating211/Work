using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using UnityEngine;

public class SampleCSV : MonoBehaviour
{
    public string CarriageReturn = "\r";
    public string LineFeed = "\n";
    string SeparataSymbol = ",";
    string LineFeedSymbol = "\r\n";
   
    // Start is called before the first frame update
    void Start()
    {
        SaveCSV();
        LoadCSV();
    }
    void LoadCSV()
    {
        string csvPath = Path.Combine(Application.dataPath, "Resources/PlayerPropertyData");
        string csvString = File.ReadAllText(csvPath);

        string[] csvRowDatas = csvString.Split(LineFeedSymbol);

        //因为第一行是表头，所以不需要输出，所以这里不读取表头
        for(int i=1;i<csvRowDatas.Length;i++)
        {
            string[] csvColunmDatas = csvRowDatas[i].Split(SeparataSymbol);

            for(int j=0;j<csvColunmDatas.Length;j++ )
            {
                Debug.Log(csvColunmDatas[j]);
            }
        }
    }

    void SaveCSV()
    {
        System.Data.DataTable csvSample = new System.Data.DataTable("示例CSV");
        csvSample.Columns.Add("等级");
        csvSample.Columns.Add("力量");
        csvSample.Columns.Add("移动速度");
        /*
        DataRow dataRow = csvSample.NewRow();
        dataRow["序号"] = 0;
        dataRow["姓名"] = "张三";
        dataRow["成绩"] = Random.Range(60, 100);
        csvSample.Rows.Add(dataRow);

        dataRow = csvSample.NewRow();
        dataRow["序号"] = 1;
        dataRow["姓名"] = "李四";
        dataRow["成绩"] = Random.Range(60, 100);
        csvSample.Rows.Add(dataRow);
        */

        DataRow dataRow = csvSample.NewRow();
        for (int i = 1; i < 10; i++)
        {
            dataRow = csvSample.NewRow();
            dataRow[0] = i;
            dataRow[1] = Random.Range(10,100);
            dataRow[2] = 5;
            if (dataRow[2] != System.DBNull.Value)
            {
                dataRow[2] = (System.Convert.ToInt32(dataRow[2]) * i + 1)*1.13;
            }
            else
            {
                // 给一个默认值或者处理方式
                dataRow[2] = 3; // 或者其他处理方式
            }
            csvSample.Rows.Add(dataRow);
        }

        StringBuilder csvString = new StringBuilder();
        
        //单独添加表头
        for (int j = 0; j < csvSample.Columns.Count; j++)
        {
            csvString.Append(csvSample.Columns[j].ColumnName);
            if (j < csvSample.Columns.Count - 1)
            {
                csvString.Append(SeparataSymbol);
            }
        }
        //遍历每一行，并将某一行某一列的数据传入字符串中
        for (int i = 0; i < csvSample.Rows.Count; i++)
        {
            csvString.Append(LineFeedSymbol);

            for (int j = 0; j < csvSample.Columns.Count; j++)
            {
                csvString.Append(csvSample.Rows[i][j].ToString());
                if (j < csvSample.Columns.Count - 1)
                {
                    csvString.Append(SeparataSymbol);
                }
            }
        }

        string csvPath = Path.Combine(Application.dataPath, "Resources/PlayerPropertyData");
        File.WriteAllText(csvPath,csvString.ToString());
    }

    void Update()
    {
        
    }
}
