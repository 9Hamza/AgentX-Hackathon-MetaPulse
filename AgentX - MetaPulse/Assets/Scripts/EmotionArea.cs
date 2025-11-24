using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EmotionArea : MonoBehaviour
{
    [SerializeField] private Transform lightSelectedTransform;
    [SerializeField] private List<GameObject> areaVfx;
    
    public Vector3 LightSelectedPosition => lightSelectedTransform.position;

    public EmotionState EmotionState;
    
    public void EnableEmotionArea(bool state, out Vector3 lightSelectedPosition)
    {
        lightSelectedPosition = LightSelectedPosition;
        foreach (var go in areaVfx)
        {
            go.SetActive(state);
        }
    }
}
