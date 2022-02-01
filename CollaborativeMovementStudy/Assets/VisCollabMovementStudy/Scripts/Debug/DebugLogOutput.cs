using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLogOutput : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }


    public void LogString(string a_stringToLog)
    {
        Debug.Log(a_stringToLog);
    }
}
