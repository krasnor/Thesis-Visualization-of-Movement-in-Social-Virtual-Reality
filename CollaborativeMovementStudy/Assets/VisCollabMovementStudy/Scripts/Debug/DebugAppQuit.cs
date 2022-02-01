using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugAppQuit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.KeepAliveInBackground = 10;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DoDebugAppQuit()
    {
        Debug.Log("DoDebugAppQuit");
        Debug.Log("Photon KeepAliveInBackground: " + PhotonNetwork.KeepAliveInBackground);
        Debug.Log("Photon IsConnected: " + PhotonNetwork.IsConnected);
        Debug.Log("Photon runInBackground: " + Application.runInBackground);

#if UNITY_EDITOR
        Debug.Log("- UnityEditor.EditorApplication.isPlaying = false;");
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Debug.Log("-  Application.Quit();");
        Application.Quit();
#endif
    }

    private void OnApplicationQuit()
    {
        if (enabled)
        {
            Debug.Log("OnApplicationQuit was called");
            PhotonNetwork.Disconnect();
            if (!Application.isEditor)
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }
    }
}
