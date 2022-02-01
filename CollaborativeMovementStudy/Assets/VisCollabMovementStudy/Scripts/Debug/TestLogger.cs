using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TestLogger : MonoBehaviour
{

    public Transform Head;
    public string filenameCsv = "thisisAtestlog.csv";
    public string filenameTxt = "thisisAtestlog.txt";
    public string filenameJson = "thisisAtestlog.json";

    // Start is called before the first frame update
    void Awake()
    {
        if (Head == null)
            Debug.LogWarning("Head component not set.");
    }

    void Start()
    {
        WriteFile(filenameJson, "textinhalt json file");
        WriteFile(filenameTxt, "textinhalt txt file");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        string datetimeStr = DateTime.UtcNow.ToString("o");

        if (Head != null)
            WriteFile(filenameCsv, $"HeadTransform; {datetimeStr}; {Head.position.x}; {Head.position.y}; {Head.position.z}");
    }

    void WriteFile(string a_filename, string a_loggingText)
    {
        string path = Application.persistentDataPath + "/" + a_filename;
        bool doesFileExist = System.IO.File.Exists(path);

        if (doesFileExist)
        {
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(a_loggingText);
            }
        }
        else
        {
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(a_loggingText);
            }
        }
    }
}
