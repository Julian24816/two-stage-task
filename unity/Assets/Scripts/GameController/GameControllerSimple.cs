using System;
using System.Collections;
using GF;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameControllerSimple : MonoBehaviour {
    public Button LeftClickArea, RightClickArea;
    public Image LeftImage, RightImage, ImageForSavedCue1, ImageForSavedCue2;
    public Progressbar DecisionProgress;
    public AudioSource AbortedSound;
    [Scene] public string ResultsScene;

    private TwoStageTask _experimentController;
    private ColoredSpriteCue _left, _right;
    private bool _currentlyFirstChoice = true;

    private void Awake() {
        _experimentController = FindObjectOfType<TwoStageTask>();
        _experimentController.OnShowInterTrialScreen += OnShowInterTrialScreen;
        _experimentController.OnShowState += OnShowState;
        _experimentController.OnHighlightChoice += OnHighlightChoice;
        _experimentController.OnShowReward += OnShowReward;
        _experimentController.OnTrialAborted += OnTrialAborted;
        _experimentController.OnExperimentCompleted += OnExperimentCompleted;
        
        AddButtonListener(LeftClickArea, true);
        AddButtonListener(RightClickArea, false);
    }

    private void AddButtonListener(Button button, bool left) => button.onClick.AddListener(() => {
        var associatedCue = left ? _left : _right;
        if (associatedCue == null) return;

        _experimentController.OnCueSelected(associatedCue);
        button.GetComponent<AudioSource>()?.Play();
    });

    private void Start() => _experimentController.StartExperiment();
    private void OnDisable() => _experimentController.PauseExperiment();

    private void OnShowInterTrialScreen(float _) {
        StopAllCoroutines();
        HideAllCues();
        HideDecisionProgress();
        _currentlyFirstChoice = false;
    }

    private void HideAllCues() {
        foreach (var image in new [] {ImageForSavedCue1, ImageForSavedCue2, LeftImage, RightImage}) image.gameObject.SetActive(false);
    }

    private void HideDecisionProgress() => DecisionProgress.gameObject.SetActive(false);

    private void OnShowState(TwoCuesDecision<ColoredSpriteCue> state, float maxDecisionTime) {
        StopAllCoroutines();

        bool flip = Random.value < 0.5;
        ShowCue(LeftImage, _left = flip ? state.First : state.Second);
        ShowCue(RightImage, _right = flip ? state.Second : state.First);
        
        DecisionProgress.gameObject.SetActive(true);
        StartCoroutine(AnimationUtil.LinearAction(maxDecisionTime, value => DecisionProgress.Progress = value));
        DecisionProgress.GetComponent<AudioSource>()?.Play();

        _currentlyFirstChoice = !_currentlyFirstChoice;
    }

    private void ShowCue(Image image, ColoredSpriteCue cue) {
        if (cue != null) {
            image.gameObject.SetActive(true);
            image.rectTransform.localScale = Vector3.one;
            image.sprite = cue.Sprite;
            image.color = cue.Color;
        } else image.gameObject.SetActive(false);
    }

    private void OnHighlightChoice(ColoredSpriteCue choice, float time) {
        StopAllCoroutines();
        HideDecisionProgress();
        
        var fadeAway = choice == _left ? RightImage : LeftImage;
        var shrink = choice == _left ? LeftImage : RightImage;
        var highlight = _currentlyFirstChoice ? ImageForSavedCue1 : ImageForSavedCue2;
        ShowCue(highlight, choice);

        float transferTime = Mathf.Min(.25f, time); // use maximally a quarter second for this animation
        StartCoroutine(FadeAway(fadeAway, time));
        StartCoroutine(Shrink(shrink, transferTime));
        StartCoroutine(Grow(highlight, transferTime));

        _left = _right = null;
    }

    private static IEnumerator FadeAway(Graphic toFade, float time) => AnimationUtil.LinearAction(time, value => {
        var color = toFade.color;
        color.a = 1 - value;
        toFade.color = color;
    });

    private static IEnumerator Shrink(Graphic toShrink, float time) =>
        AnimationUtil.LinearAction(time, value => toShrink.rectTransform.localScale = Vector3.one * (1 - value));

    private static IEnumerator Grow(Graphic toGrow, float time) =>
        AnimationUtil.LinearAction(time, value => toGrow.rectTransform.localScale = Vector3.one * value);

    private void OnShowReward(Reward _, float time) {
        StopAllCoroutines();
        HideDecisionProgress();
        LeftImage.gameObject.SetActive(false);
        RightImage.gameObject.SetActive(false);
        float fadeTime = Math.Min(.5f, time);
        StartCoroutine(FadeAway(ImageForSavedCue1, fadeTime));
        StartCoroutine(FadeAway(ImageForSavedCue2, fadeTime));
    }

    private void OnTrialAborted() {
        if (AbortedSound != null) AbortedSound.Play();
    }

    private void OnExperimentCompleted() => SceneManager.LoadScene(ResultsScene);
}