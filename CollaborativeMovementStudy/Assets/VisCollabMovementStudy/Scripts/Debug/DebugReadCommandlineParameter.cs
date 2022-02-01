using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugReadCommandlineParameter : MonoBehaviour
{

    public string param = "-outputDir";
    public string param2 = "-projectpath";
    // Start is called before the first frame update
    void Start()
    {
        var arg = GetArg(param);
        Debug.Log("Argument: " + arg);

        var arg2 = GetArg(param2);
        Debug.Log("Argument: " + arg2);

        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            Debug.Log("a: " + args[i]);

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //https://forum.unity.com/threads/pass-custom-parameters-to-standalone-on-launch.429144/
    private static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}
