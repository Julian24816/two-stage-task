using System.Linq;
using UnityEngine;

namespace GF {
    public interface ExperimentParameters<in TCue, TState, in TReward, TTransition>
        where TCue : Cue where TState : State<TCue> where TReward : Reward
        where TTransition : Transition<TCue, TState, TReward> {

        int NumberOfTrials { get; }
        TState StartState { get; }
        float GetInterTrialTime(int trialNumber);
        float GetTimeForDecision(TState state);
        float GetDecisionHighlightDuration(TCue action);
        float GetRewardHighlightDuration(TReward reward);
        TTransition GetTransition(TState currentState, TCue action);
        bool TryGetNoActionTransition(TState currentState, out TTransition transition);
    }
    
    public abstract class GeneralPurposeExperimentController<TExperimentParameters, TCue, TState, TReward, TTransition>
        : MonoBehaviour
        where TCue : class, Cue where TState : class, State<TCue> where TReward : Reward 
        where TTransition : Transition<TCue, TState, TReward> 
        where TExperimentParameters : ExperimentParameters<TCue, TState, TReward, TTransition> {
        
        protected abstract TExperimentParameters ExperimentParameters { get; }

        #region public properties

        public int CurrentTrialNumber { get; private set; } = 0;
        public int SuccessfulTrialCount { get; private set; } = 0;
        public int TargetSuccessfulTrialCount => ExperimentParameters.NumberOfTrials;
        public float CumulativeRewards { get; private set; } = 0;
        public float Countdown => _state == ExperimentState.Wait ? 0 : _deadline - Time.time;

        #endregion

        #region events

        public delegate void ShowInterTrailScreen(float duration);
        public event ShowInterTrailScreen OnShowInterTrialScreen;

        public delegate void ShowState(TState state, float maxDecisionTime);
        public event ShowState OnShowState;

        public delegate void HighlightChoice(TCue choice, float duration);
        public event HighlightChoice OnHighlightChoice;

        public delegate void TransitionWasSelected(TTransition transition);
        public event TransitionWasSelected OnTransitionSelected;

        public delegate void RewardWasEarned(TReward reward);
        public event RewardWasEarned OnReward;

        public delegate void ShowReward(TReward reward, float duration);
        public event ShowReward OnShowReward;

        public delegate void RewardWasNotShown(TReward reward);
        public event RewardWasNotShown OnSkippedReward;

        public delegate void TrialWasAborted();
        public event TrialWasAborted OnTrialAborted;

        public delegate void TrialWasCompleted();
        public event TrialWasCompleted OnTrialCompleted;

        public delegate void ExperimentWasCompleted();
        public event ExperimentWasCompleted OnExperimentCompleted;

        #endregion

        #region public methods

        public void StartExperiment() => StartExperimentInternal();

        public void PauseExperiment() => PauseExperimentInternal();
        
        public void OnCueSelected(TCue action) => OnCueSelectedInternal(action);

        #endregion

        #region private state and methods

        private enum ExperimentState { Wait, BeforeTrial, Decision, HighlightChoice, HighlightReward }

        private ExperimentState _state = ExperimentState.Wait;
        private float _deadline;
        private TState _currentState;
        private TCue _currentChoice;

        private void StartExperimentInternal() => PrepareNextTrial();

        private void PrepareNextTrial() {
            if (SuccessfulTrialCount >= ExperimentParameters.NumberOfTrials) {
                OnExperimentCompleted?.Invoke();
                _state = ExperimentState.Wait;
                return;
            }

            CurrentTrialNumber++;
            _currentState = ExperimentParameters.StartState;
            _currentChoice = null;

            _state = ExperimentState.BeforeTrial;
            _deadline = Time.time + ExperimentParameters.GetInterTrialTime(CurrentTrialNumber);
            OnShowInterTrialScreen?.Invoke(Countdown);
        }

        private void Update() {
            if (_state == ExperimentState.Wait || Time.time < _deadline) /* return */;
            
            else if (_state == ExperimentState.BeforeTrial) StartTrial();
            else if (_state == ExperimentState.Decision) DecisionDeadlineMissed();
            else if (_state == ExperimentState.HighlightChoice) AfterHighlightChoice();
            else if (_state == ExperimentState.HighlightReward) AfterHighlightReward();
        }
        
        private void PauseExperimentInternal() {
            if (_state != ExperimentState.Wait && _state != ExperimentState.BeforeTrial) AbortCurrentTrial();
            _state = ExperimentState.Wait;
        }

        private void StartTrial() => ShowCurrentState();

        private void ShowCurrentState() {
            _currentChoice = null;
            _state = ExperimentState.Decision;
            _deadline = Time.time + ExperimentParameters.GetTimeForDecision(_currentState);
            OnShowState?.Invoke(_currentState, Countdown);
        }
        
        private void OnCueSelectedInternal(TCue action) {
            if (_state != ExperimentState.Decision) {
                Debug.LogError("called OnCueSelected even though no State is active");
                return;
            } else if (!_currentState.Cues.Contains(action)) {
                Debug.LogError("selected cue that is not one of the current states cues");
                return;
            }

            _currentChoice = action;
            _state = ExperimentState.HighlightChoice;
            _deadline = Time.time + ExperimentParameters.GetDecisionHighlightDuration(_currentChoice);
            if (Countdown > 0) OnHighlightChoice?.Invoke(action, Countdown);
            else AfterHighlightChoice();
        }

        private void DecisionDeadlineMissed() {
            if (ExperimentParameters.TryGetNoActionTransition(_currentState, out var transition))
                ApplyTransition(transition);
            else AbortCurrentTrial();
        }
        
        private void AbortCurrentTrial() {
            OnTrialAborted?.Invoke();
            PrepareNextTrial();
        }

        private void AfterHighlightChoice() {
            var transition = ExperimentParameters.GetTransition(_currentState, _currentChoice);
            ApplyTransition(transition);
        }

        private void ApplyTransition(TTransition transition) {

            OnTransitionSelected?.Invoke(transition);

            _currentState = transition.NextState;
            _currentChoice = null;
            CumulativeRewards += transition.Reward.Value;
            OnReward?.Invoke(transition.Reward);

            _state = ExperimentState.HighlightReward;
            _deadline = Time.time + ExperimentParameters.GetRewardHighlightDuration(transition.Reward);
            if (Countdown > 0) OnShowReward?.Invoke(transition.Reward, Countdown);
            else {
                OnSkippedReward?.Invoke(transition.Reward);
                AfterHighlightReward();
            }
        }

        private void AfterHighlightReward() {
            if (_currentState == ExperimentParameters.StartState) {
                SuccessfulTrialCount++;
                OnTrialCompleted?.Invoke();
                PrepareNextTrial();
            } else ShowCurrentState();
        }

        #endregion

    }
}