// MicCaptureToNeMo.cs
using System;
using System.Collections;
using Scripts.EventBus.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Networking;

public class MicCaptureToNeMo : MonoBehaviour
{
    [Header("Mic")]
    public string deviceName = null;          // null = default device
    public int sampleRate = 16000;            // good for speech ML (matches your NeMo setup)
    public int clipLengthSeconds = 10;        // ring buffer length
    public bool forceMono = true;
    [SerializeField] private float recordingDurationSeconds = 3f;

    public UnityEvent OnMicRecordingStarted = new UnityEvent();
    public UnityEvent OnMicRecordingFinishedAndSaved = new UnityEvent();

    [Header("API")]
    // ⬇️ Point this to your FastAPI predict_emotion endpoint
    public string nemoEndpoint = "http://127.0.0.1:8000/predict_emotion";
    public string authBearer = "";             // if you need Authorization

    private AudioClip micClip;
    private bool isRecording;

    // Optional: expose the predicted emotion to UnityEvents / other scripts
    [Serializable]
    public class StringEvent : UnityEvent<string> { }
    public StringEvent OnEmotionPredicted = new StringEvent();

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryStartRecording();
        }
    }

    public void TryStartRecording()
    {
        if (!isRecording) 
        {
            StartCoroutine(CaptureAndSendOnce(recordingDurationSeconds));
        }
        else
        {
            HelperFunctions.LogFeedbackText("Currently recording... Can't start new recording session.");
        }
    }

    IEnumerator CaptureAndSendOnce(float seconds)
    {
        OnMicRecordingStarted?.Invoke();
        HelperFunctions.LogFeedbackText("Started recording...");
        isRecording = true;

        // Start mic (looping ring buffer)
        micClip = Microphone.Start(deviceName, true, clipLengthSeconds, sampleRate);
        while (Microphone.GetPosition(deviceName) <= 0)
            yield return null; // wait for mic to actually start

        // Record for `seconds`
        yield return new WaitForSeconds(seconds);

        // Stop mic and freeze samples in the clip
        Microphone.End(deviceName);
        HelperFunctions.LogFeedbackText("Stopped recording...");

        // Turn clip into WAV bytes (PCM16 LE)
        byte[] wav = WavWriter.FromAudioClip(micClip, forceMono);

        // debug save file
        string fileName = $"audio-gemini-test.wav";
        string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);
        System.IO.File.WriteAllBytes(path, wav);
        Debug.Log("Saved WAV to: " + path);

        // OnMicRecordingFinishedAndSaved.Invoke();

        // ✅ POST to NeMo FastAPI as multipart/form-data
        WWWForm form = new WWWForm();
        // "file" must match: file: UploadFile = File(...)
        form.AddBinaryData("file", wav, "audio.wav", "audio/wav");

        using (UnityWebRequest req = UnityWebRequest.Post(nemoEndpoint, form))
        {
            if (!string.IsNullOrEmpty(authBearer))
                req.SetRequestHeader("Authorization", "Bearer " + authBearer);

            Debug.Log("Sending request to Emotion API...");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"NeMo request failed: {req.responseCode} {req.error}\n{req.downloadHandler.text}");
            }
            else
            {
                var json = req.downloadHandler.text;
                Debug.Log("NeMo emotion result (raw JSON): " + json);

                // Parse JSON -> emotion.label
                EmotionResponse response = null;
                try
                {
                    response = JsonUtility.FromJson<EmotionResponse>(json);
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to parse emotion response: " + e);
                }

                if (response != null && response.emotion != null && !string.IsNullOrEmpty(response.emotion.label))
                {
                    string label = response.emotion.label;
                    Debug.Log("Predicted emotion: " + label);

                    // fire event so other scripts / UI can use it
                    OnEmotionPredicted?.Invoke(label);

                    HelperFunctions.LogFeedbackText($"Emotion detected: {label}");
                    
                    var emotion = ParseEmotion(label);
                    EventBus.Publish(new EmotionStateChangedEvent()
                    {
                        NewEmotionState =  emotion,
                    });
                }
                else
                {
                    Debug.LogWarning("Could not parse emotion label from response.");
                }
            }
        }

        isRecording = false;
    }
    
    public EmotionState ParseEmotion(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return EmotionState.None;

        text = text.Trim().ToLower();

        if (text.Contains("happy"))
            return EmotionState.Happy;

        if (text.Contains("sad"))
            return EmotionState.Sad;

        if (text.Contains("angry"))
            return EmotionState.Angry;

        return EmotionState.None;
    }
}

// Match the JSON structure from FastAPI
// {
//   "emotion": {
//     "label": "happy",
//     "scores": { ... }
//   },
//   "raw_result": { ... }
// }

[Serializable]
public class EmotionInner
{
    public string label;
    // scores dictionary not parsed here (JsonUtility doesn't support Dictionary easily)
}

[Serializable]
public class EmotionResponse
{
    public EmotionInner emotion;
}

public static class HelperFunctions
{
    public static void LogFeedbackText(string feedbackText)
    {
        Debug.Log(feedbackText);
        EventBus.Publish(new FeedbackTextChangedEvent()
        {
            NewFeedbackText = feedbackText,
        });
    }
}
