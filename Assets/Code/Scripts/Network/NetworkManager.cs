using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    public static IEnumerator GetJsonData<T>(string url, Action<T> onSuccess, Action<string> onError)
    {
        while (true)
        {

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke(webRequest.error);
                }
                else
                {
                    try
                    {
                        string json = webRequest.downloadHandler.text;
                        T data = JsonUtility.FromJson<T>(json);
                        onSuccess?.Invoke(data);
                    }
                    catch (Exception e)
                    {
                        onError?.Invoke("JSON Dönüştürme Hatası: " + e.Message);
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }


}