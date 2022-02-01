using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NLPCalcRequester
{
    private const string DefaultUrl = "localhost:8001";

    public string RequestUrl { get; private set; }

    public NLPCalcRequester(string a_requestUrl = DefaultUrl)
    {
        RequestUrl = a_requestUrl;
    }


    /// <summary>
    /// https://forum.unity.com/threads/posting-json-through-unitywebrequest.476254/
    /// </summary>
    /// <param name="a_audioEventDataPoints"></param>
    /// <returns></returns>
    public IEnumerator Post(SpatialAudioEventDataPoint[] a_audioEventDataPoints, TextFileWriter a_textFileWriter)
    {
        var bodyJsonString = JsonArrayHelper.ToJson(a_audioEventDataPoints);
        UnityWebRequest request = CreatePostRequest(bodyJsonString);

        yield return request.SendWebRequest();

        Debug.Log("Status Code: " + request.responseCode);
        ProcessRequestResult(request, a_textFileWriter);
    }

    private void ProcessRequestResult(UnityWebRequest request, TextFileWriter a_textFileWriter)
    {
        try
        {
            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("Error: " + request.error);
                    break;
                case UnityWebRequest.Result.Success:
                    var responseText = request.downloadHandler.text;
                    Debug.Log("Received: " + responseText);
                    var response = JsonArrayHelper.FromJson<NLPCalcResponse>(responseText);

                    a_textFileWriter.WriteToNLPCalc(response);
                    break;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    /// <summary>
    /// https://forum.unity.com/threads/posting-json-through-unitywebrequest.476254/
    /// </summary>
    /// <param name="a_url"></param>
    /// <param name="a_bodyJsonString"></param>
    /// <returns></returns>
    private UnityWebRequest CreatePostRequest(string a_bodyJsonString)
    {
        var request = new UnityWebRequest(RequestUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(a_bodyJsonString);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }
}
