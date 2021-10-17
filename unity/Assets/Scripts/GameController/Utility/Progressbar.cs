using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class Progressbar : MonoBehaviour {
    private const float MinValue = 0, MaxValue = 1;
    private Slider _slider;
    private Image _background, _fill;

    public float Progress {
        get => _slider.value;
        set => _slider.value = Mathf.Clamp(value, MinValue,  MaxValue);
    }

    public Color BackgroundColor {
        get => _background.color;
        set => _background.color = value;
    }
    public Color FillColor {
        get => _fill.color;
        set => _fill.color = value;
    }

    private void Awake() {
        _slider = GetComponent<Slider>();
        _background = _slider.GetComponentsInChildren<Image>().First();
        _fill = _slider.fillRect.GetComponent<Image>();
    }
}