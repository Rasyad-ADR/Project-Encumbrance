using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class RotatingSliderHandle : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Total degrees the handle rotates from 0% to 100%. Negative = clockwise rolling.")]
    public float totalRotationDegrees = -720f; 

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        slider.onValueChanged.AddListener(UpdateHandleRotation);
        UpdateHandleRotation(slider.value); 
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(UpdateHandleRotation);
    }

    private void UpdateHandleRotation(float value)
    {
        if (slider.handleRect != null)
        {
            float angle = slider.normalizedValue * totalRotationDegrees;
            slider.handleRect.localEulerAngles = new Vector3(0f, 0f, angle);
        }
    }
}