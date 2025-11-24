using UnityEngine;
using TMPro;
using DG.Tweening;

public class TextAnimator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textField;
    
    /// <summary>
    /// Animates the text with a subtle punch scale effect when the value updates
    /// </summary>
    public void AnimateTextUpdate(string newText)
    {
        // Kill any existing tweens on this text to prevent overlap
        textField.transform.DOKill();
        
        // Update the text
        textField.text = newText;
        
        // Subtle punch scale animation
        textField.transform
            .DOPunchScale(Vector3.one * 0.15f, 0.3f, 4, 0.5f)
            .SetEase(Ease.OutQuad);
    }
    
    /// <summary>
    /// Alternative: Bounce effect with slight overshoot
    /// </summary>
    public void AnimateTextUpdateBounce(string newText)
    {
        textField.transform.DOKill();
        textField.text = newText;
        
        // Start slightly smaller, then bounce to normal size
        textField.transform.localScale = Vector3.one * 0.85f;
        textField.transform
            .DOScale(1f, 0.25f)
            .SetEase(Ease.OutBack, 1.5f);
    }
    
    /// <summary>
    /// Alternative: Fade and scale combo for smoother feel
    /// </summary>
    public void AnimateTextUpdateFade(string newText)
    {
        textField.DOKill();
        
        Sequence seq = DOTween.Sequence();
        
        // Quick fade out with slight scale down
        seq.Append(textField.DOFade(0f, 0.1f));
        seq.Join(textField.transform.DOScale(0.9f, 0.1f));
        
        // Update text at the middle
        seq.AppendCallback(() => textField.text = newText);
        
        // Fade in with slight scale up
        seq.Append(textField.DOFade(1f, 0.15f));
        seq.Join(textField.transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack, 1.3f));
    }
    
    /// <summary>
    /// Alternative: Minimal pulse - most subtle option
    /// </summary>
    public void AnimateTextUpdatePulse(string newText)
    {
        textField.transform.DOKill();
        textField.text = newText;
        
        // Very subtle scale pulse
        Sequence seq = DOTween.Sequence();
        seq.Append(textField.transform.DOScale(1.08f, 0.12f).SetEase(Ease.OutQuad));
        seq.Append(textField.transform.DOScale(1f, 0.12f).SetEase(Ease.InQuad));
    }
    
    /// <summary>
    /// Example usage for score updates with number formatting
    /// </summary>
    public void UpdateScore(int newScore)
    {
        AnimateTextUpdate(newScore.ToString("N0")); // Formats with commas
    }
}