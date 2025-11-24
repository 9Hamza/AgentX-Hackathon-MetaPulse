// MicCaptureToNeMo.cs
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
    public int sampleRate = 16000;            // good for speech ML
    public int clipLengthSeconds = 10;        // ring buffer length
    public bool forceMono = true;
    [SerializeField] private float recordingDurationSeconds = 3f;
    
    public UnityEvent OnMicRecordingStarted = new UnityEvent();
    public UnityEvent OnMicRecordingFinishedAndSaved = new UnityEvent();

    [Header("API")]
    public string nemoEndpoint = "https://your-host/analyze"; // replace
    public string authBearer = "";             // if you need Authorization

    private AudioClip micClip;
    private bool isRecording;

    // Example: press Space to capture a short utterance and send it
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
        while (Microphone.GetPosition(deviceName) <= 0) yield return null; // wait for mic

        // Record for `seconds`
        yield return new WaitForSeconds(seconds);

        // Stop mic and freeze samples in the clip
        Microphone.End(deviceName);
        HelperFunctions.LogFeedbackText("Stopped recording...");

        // Turn clip into WAV bytes (PCM16 LE)
        byte[] wav = WavWriter.FromAudioClip(micClip, forceMono);

        // debug save file - after: byte[] wav = WavWriter.FromAudioClip(micClip, forceMono);
        // string fileName = $"capture_{System.DateTime.Now:yyyyMMdd_HHmmss}.wav";
        string fileName = $"audio-gemini-test.wav";
        string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);

        System.IO.File.WriteAllBytes(path, wav); // creates/overwrites the file
        Debug.Log("Saved WAV to: " + path);
        
        OnMicRecordingFinishedAndSaved.Invoke();
        
        // POST to NeMo
        using var req = new UnityWebRequest(nemoEndpoint, UnityWebRequest.kHttpVerbPOST);
        req.uploadHandler = new UploadHandlerRaw(wav);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "audio/wav");
        if (!string.IsNullOrEmpty(authBearer))
            req.SetRequestHeader("Authorization", "Bearer " + authBearer);

        // yield return req.SendWebRequest();
        //
        // if (req.result != UnityWebRequest.Result.Success)
        // {
        //     Debug.LogError($"NeMo request failed: {req.responseCode} {req.error}\n{req.downloadHandler.text}");
        // }
        // else
        // {
        //     var json = req.downloadHandler.text;
        //     Debug.Log("NeMo emotion result: " + json);
        //     // TODO: parse JSON and drive your game state
        // }

        isRecording = false;
    }
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
