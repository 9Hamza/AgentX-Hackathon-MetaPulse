using System;
using System.Collections;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Networking;

public class EmotionClient : MonoBehaviour
{
    [SerializeField]
    private string apiUrl = "http://127.0.0.1:8000/predict_emotion";

    [Button]
    public void SendAudioFileFromSavedRecording()
    {
        var mediaFilePath = Application.persistentDataPath + "/audio-gemini-test.wav";
        StartCoroutine(SendAudioFileCoroutine(mediaFilePath));
    }

    public IEnumerator SendAudioFileCoroutine(string filePath)
    {
        // Check file exists
        if (!File.Exists(filePath))
        {
            Debug.LogError("Audio file not found: " + filePath);
            yield break;
        }

        // Read file bytes
        byte[] fileBytes = File.ReadAllBytes(filePath);

        // Build the form-data
        WWWForm form = new WWWForm();
        form.AddBinaryData(
            "file",
            fileBytes,
            Path.GetFileName(filePath),
            "audio/wav"
        );

        // Send request
        using (UnityWebRequest www = UnityWebRequest.Post(apiUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Emotion API error: " + www.error);
            }
            else
            {
                string json = www.downloadHandler.text;
                Debug.Log("Emotion API response: " + json);
            }
        }
    }
}