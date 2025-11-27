using System;
using System.Collections.Generic;
using DG.Tweening;
using Scripts.EventBus.Events;
using Sirenix.OdinInspector;
using UnityEngine;

public class EmotionManager : MonoBehaviour
{
    [SerializeField] private List<EmotionArea> emotionAreas = new List<EmotionArea>();
    [SerializeField] private GameObject selectionLightGameObject;
    [SerializeField] private GameObject selectionVFXGameObject;

    [SerializeField] private TextAnimator emotionStatusText;
    [SerializeField] private TextAnimator feedbackText;
    
    private void OnEnable()
    {
        EventBus.Subscribe<EmotionStateChangedEvent>(OnEmotionStateChanged);
        EventBus.Subscribe<FeedbackTextChangedEvent>(OnFeedbackTextChanged);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<EmotionStateChangedEvent>(OnEmotionStateChanged);
        EventBus.Unsubscribe<FeedbackTextChangedEvent>(OnFeedbackTextChanged);
    }

    private void Start()
    {
        DOTween.Init();
        
        selectionLightGameObject.SetActive(false);
        selectionVFXGameObject.SetActive(false);
        
        DisableAllEmotionAreas();
    }

    private void OnEmotionStateChanged(EmotionStateChangedEvent e)
    {
        DisableAllEmotionAreas();
        EnableEmotionArea(e.NewEmotionState, true);
        emotionStatusText.AnimateTextUpdate($"Last Emotion State: {e.NewEmotionState}");
    }

    private void OnFeedbackTextChanged(FeedbackTextChangedEvent e)
    {
        feedbackText.AnimateTextUpdate($"{e.NewFeedbackText}");
    }

    private void DisableAllEmotionAreas()
    {
        for (int i = 0; i < emotionAreas.Count; i++)
        {
            emotionAreas[i].EnableEmotionArea(false, out Vector3 position);
        }
    }

    [Button("Debug Sad Area")]
    public void DebugSadEmotion()
    {
        DisableAllEmotionAreas();
        EnableEmotionArea(EmotionState.Sad, true);
    }
    
    [Button("Debug Happy Area")]
    public void DebugHappyEmotion()
    {
        DisableAllEmotionAreas();
        EnableEmotionArea(EmotionState.Happy, true);
    }
    
    [Button("Debug Angry Area")]
    public void DebugAngryEmotion()
    {
        DisableAllEmotionAreas();
        EnableEmotionArea(EmotionState.Angry, true);
    }

    private void EnableEmotionArea(EmotionState emotionState, bool activeState)
    {
        var emotionArea = emotionAreas.Find(emotionArea => emotionArea.EmotionState == emotionState);
        if (emotionArea != null)
        {
            emotionArea.EnableEmotionArea(activeState, out Vector3 lightSelectedPosition);
            selectionLightGameObject.SetActive(activeState);
            selectionVFXGameObject.SetActive(activeState);
            Vector3 selectedLightPosNoY = new Vector3(lightSelectedPosition.x, 0, lightSelectedPosition.z);
            selectionVFXGameObject.transform.DOMove(selectedLightPosNoY, 0.3f).SetEase(Ease.OutBack).Play();
            selectionLightGameObject.transform.position = lightSelectedPosition;
        }
    }
}
