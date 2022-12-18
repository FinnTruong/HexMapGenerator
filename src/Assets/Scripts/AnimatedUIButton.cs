using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

public enum CustomTransitionType
{
    None,
    Color,
    SpriteSwap,
}

public class AnimatedUIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private bool interactableWhenPause;
    [SerializeField] Color highlightedColor = new Color(200 / 255f, 200 / 255f, 200 / 255f, 1);
    [SerializeField] Color disabledColor = new Color(200 / 255f, 200 / 255f, 200 / 255f, 0.5f);
    public UnityEvent onClick;
    public Image image;
    public string sfxClick = "Click_01";
    public Vector3 highlightScale = Vector3.one * 0.9f;
    public Sprite highlightSprite;

    private Color baseColor;
    private Sprite baseSprite;

    Vector3 onPointerDownScale = Vector3.one;
    public float animateDuration = 0.1f;
    public Ease animatedCurve = Ease.InBack;
    public bool enableVibration = false;
    private bool interactebleValue = true;
    public bool interactable {
        get => interactebleValue;
        set {
            interactebleValue = value;
        }
    }

    public float shrinkTime = 0.3f;

    Vector3 startScale;

    public CustomTransitionType transition = CustomTransitionType.Color;

    public bool playSound = true;

    public bool isIgnoreAnimate;
    [SerializeField] private bool oneTimeClick = false;

    private void Awake() 
    {
        image = GetComponent<Image>();
        if (image != null)
        {
            startScale = image.transform.localScale == Vector3.zero ? Vector3.one : image.transform.localScale;
        }
        onPointerDownScale = new Vector3(highlightScale.x * startScale.x, highlightScale.y * startScale.y, highlightScale.z * startScale.z);
        if (image != null) {
            baseColor = image.color;
            baseSprite = image.sprite;
        }

    }


    private void OnPauseUpdated(bool paused)
    {
        interactable = !paused;
    }

    private void OnEnable() {
        if (oneTimeClick) {
            interactable = true;
            image.color = baseColor;
        }
    }
    public void DisableButton() {
        interactable = false;
        image.color = disabledColor;
    }

    public void EnableButton()
    {
        interactable = true;
        image.color = baseColor;
    }    

    public void OnPointerDown(PointerEventData eventData) {
        if (!interactable) return;
        HighlightButton();

        if (isIgnoreAnimate) return;
        transform.DOScale(onPointerDownScale, animateDuration).SetEase(animatedCurve);
        //LeanTween.scale(this.gameObject, onPointerDownScale, duration)
        //    .setIgnoreTimeScale(true)
        //    .setEaseInBack();
    }
    public void OnPointerUp(PointerEventData eventData) {
        if (!interactable || isIgnoreAnimate) return;
        transform.DOScale(startScale, animateDuration)
            .SetEase(animatedCurve)
            .OnComplete(() => ResetButton());
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!interactable || isIgnoreAnimate) return;
        transform.DOScale(startScale, animateDuration)
            .SetEase(animatedCurve)
            .OnComplete(() => ResetButton());
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (!interactable)
            return;
        onClick?.Invoke();
        if (oneTimeClick) DisableButton();
        if (AudioManager.instance != null && sfxClick != null)
            AudioManager.instance.PlaySFX(sfxClick, 0.7f);
        //if (SoundManager.Instance != null && sfxClick != null)
        //    SoundManager.Instance.PlayerSound(sfxClick);
    }

    public void HighlightButton() 
{
        if (image != null) {
            switch (transition) {
                case CustomTransitionType.None:
                    break;
                case CustomTransitionType.Color:
                    image.color = highlightedColor;
                    break;
                case CustomTransitionType.SpriteSwap:
                    if (highlightSprite != null)
                        image.sprite = highlightSprite;
                    break;
                default:
                    break;
            }
        }
    }

    public void ResetButton() {
        if (image == null) return;

        switch (transition) {
            case CustomTransitionType.None:
                break;
            case CustomTransitionType.Color:
                image.color = baseColor;
                break;
            case CustomTransitionType.SpriteSwap:
                image.sprite = baseSprite;
                break;
            default:
                break;
        }
    }

    public void SetColor(Color color)
    {
        image.color = color;
    }

    public void Shrink()
    {
        transform.DOScale(Vector3.zero, shrinkTime).SetEase(Ease.InBack);
        //LeanTween.scale(gameObject, Vector3.zero, shrinkTime).setEaseInBack().setIgnoreTimeScale(true);
    }
}