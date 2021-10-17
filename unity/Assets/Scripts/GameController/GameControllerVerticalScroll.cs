using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using GF;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameControllerVerticalScroll : MonoBehaviour {
    public Button LeftClickArea, RightClickArea;
    public AudioSource DecisionSound, AbortedSound;
    public RectTransform FallArea;
    public Image CueTemplate;
    [Scene] public string ResultsScene;

    private TwoStageTask _experimentController;
    private float _lastDecisionTime = 2, _lastRewardTime = 1.5f;
    private ColoredSpriteCue _left, _right;
    private int _currentChoiceCount;
    private Coroutine _leftFall, _rightFall, _savedFirstChoiceFall;
    private Image _leftImage, _rightImage, _savedFirstChoice, _savedSecondChoice;
    private bool _goFastAgain;
    private readonly List<Image> _allActive = new List<Image>();

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

    private void Start() {
        CueTemplate.gameObject.SetActive(false);
        _experimentController.StartExperiment();
    }

    private void OnDisable() => _experimentController.PauseExperiment();

    private void OnShowInterTrialScreen(float _) {
        StopAllCoroutines();
        HideAllCues();
        _currentChoiceCount = 0;
    }

    private new void StopCoroutine(Coroutine coroutine) {
        if (coroutine != null) base.StopCoroutine(coroutine);
    }
    
    private new void StopAllCoroutines() {
        base.StopAllCoroutines();
        _leftFall = _rightFall = _savedFirstChoiceFall = null;
    }
    
    private void HideAllCues() {
        foreach (var image in _allActive) if (image != null) Destroy(image.gameObject);
        _allActive.Clear();
        _leftImage = _rightImage = _savedFirstChoice = _savedSecondChoice = null;
    }
    
    private void OnShowState(TwoCuesDecision<ColoredSpriteCue> state, float maxDecisionTime) {
        _lastDecisionTime = maxDecisionTime;
        _currentChoiceCount++;

        bool flip = Random.value < 0.5;
        _left = flip ? state.First : state.Second;
        _right = flip ? state.Second : state.First;
        
        _leftImage = CreateCueImage(_left, 0.25f);
        _rightImage = CreateCueImage(_right, 0.75f);

        // falling animation
        _leftFall = StartCoroutine(Fall(_leftImage));
        _rightFall = StartCoroutine(Fall(_rightImage));
        if (DecisionSound != null) DecisionSound.Play();
    }

    private IEnumerator Fall(Image image, float percentage = 1, float start = 1, float speedMultiplier = 1) => 
        AnimationUtil.LinearAction(_lastDecisionTime * percentage / speedMultiplier, 
        value => SetAnchorY(image.rectTransform, start - percentage * value));

    private Image CreateCueImage(ColoredSpriteCue cue, float startX) {
        Contract.Assert(cue != null);

        var image = Instantiate(CueTemplate.gameObject, FallArea).GetComponent<Image>();
        image.gameObject.SetActive(true);
        _allActive.Add(image);

        image.sprite = cue.Sprite;
        image.color = cue.Color;
        
        image.rectTransform.anchorMax = image.rectTransform.anchorMin = new Vector2(startX, 1);
        
        return image;
    }

    private static void SetAnchorY(RectTransform image, float y) {
        var max = image.anchorMax;
        max.y = y;
        image.anchorMin = image.anchorMax = max;
    }
    
    private static void SetAnchorX(RectTransform image, float x) {
        var max = image.anchorMax;
        max.x = x;
        image.anchorMin = image.anchorMax = max;
    }

    private void OnHighlightChoice(Cue choice, float time) {
        if (DecisionSound != null) DecisionSound.Stop();

        bool leftChosen = choice == _left;
        _left = _right = null;
        
        var fadeAway = leftChosen ? _rightImage : _leftImage;
        var focus = leftChosen ? _leftImage : _rightImage;
        var fall = leftChosen ? _leftFall : _rightFall;
        
        StartCoroutine(FadeAway(time, fadeAway));

        if (_currentChoiceCount == 1) {

            StopCoroutine(fall);
            float currentFallHeight = focus.rectTransform.anchorMax.y;

            _savedFirstChoice = focus;
            _savedFirstChoiceFall = StartCoroutine(Fall(focus, Math.Max(0, currentFallHeight - .25f), currentFallHeight, .2f));
            StartCoroutine(MoveCenter(focus, time));
            
        } else if (_currentChoiceCount == 2) {
//            Contract.Assert(_savedFirstChoice != null && _savedFirstChoiceFall != null);
            
            StopCoroutine(fall);
            StopCoroutine(_savedFirstChoiceFall);

            _savedSecondChoice = focus;
            StartCoroutine(CombineThenFall(time));
        }
        else throw new Exception("illegal state");
    }

    private static IEnumerator FadeAway(float time, Graphic toFade) => AnimationUtil.LinearAction(time, value => {
        var color = toFade.color;
        color.a = 1 - value;
        toFade.color = color;
    });

    private static IEnumerator MoveCenter(Graphic image, float time) {
        float startX = image.rectTransform.anchorMax.x;
        return AnimationUtil.LinearAction(time, value => SetAnchorX(image.rectTransform, Mathf.Lerp(startX, .5f, value)));
    }

    private IEnumerator CombineThenFall(float time) {
        _goFastAgain = false;
        
        float fallDistanceLeft = _savedFirstChoice.rectTransform.anchorMax.y;
        float fallSpeed = Math.Min(.1f, fallDistanceLeft / (time + _lastRewardTime));
        float combineSpeed = 1;
        float lastTime = Time.time;
        while (_savedFirstChoice.rectTransform.anchorMax.y > 0) {
            yield return null;

            float deltaTime = Time.time - lastTime;
            lastTime = Time.time;

            var anchorFirst = _savedFirstChoice.rectTransform.anchorMax;
            anchorFirst.y -= (_goFastAgain ? .5f : fallSpeed) * deltaTime;
            _savedFirstChoice.rectTransform.anchorMax = _savedFirstChoice.rectTransform.anchorMin = anchorFirst;

            var anchorSecond = _savedSecondChoice.rectTransform.anchorMax;
            var delta = anchorFirst - anchorSecond;
            anchorSecond += limitToLength(delta, combineSpeed * deltaTime);
            _savedSecondChoice.rectTransform.anchorMax = _savedSecondChoice.rectTransform.anchorMin = anchorSecond;
        }

        static Vector2 limitToLength(Vector2 vector, float maxLength) {
            float length = vector.magnitude;
            return length <= maxLength ? vector : vector / length * maxLength;
        }
    }

    private void OnShowReward(Reward reward, float time) {
        _lastRewardTime = time;
        if (reward.Value > 0) _goFastAgain = true;
        StartCoroutine(FadeAway(time, _savedFirstChoice));
        StartCoroutine(FadeAway(time, _savedSecondChoice));
    }
    
    private void OnTrialAborted() {
        if (AbortedSound != null) AbortedSound.Play();
    }

    private void OnExperimentCompleted() => SceneManager.LoadScene(ResultsScene);
}
