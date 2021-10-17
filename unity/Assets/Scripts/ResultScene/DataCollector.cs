using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using GF;
using UnityEngine;

public class DataCollector : MonoBehaviour
{
    private Trial _currentTrial = null;
    private readonly List<Trial> allTrials = new List<Trial>();
    public IEnumerable<Trial> AllTrials => allTrials;

    private void Awake() {
        var experimentController = FindObjectOfType<TwoStageTask>();
        experimentController.OnShowInterTrialScreen += interTrialTime => {
            if (_currentTrial != null) allTrials.Add(_currentTrial);
            _currentTrial = new Trial();
            _currentTrial.TrialNumber = experimentController.CurrentTrialNumber;
            _currentTrial.TimeBeforeBlackScreen = DateTime.Now;
            _currentTrial.State = Trial.FillState.BeforeFirst;
        };
        experimentController.OnShowState += (state, _) => {
            switch (_currentTrial.State) {
                case Trial.FillState.BeforeFirst:
                    Contract.Assert(state.Equals(experimentController.StartState), "logging state does not match experiment state");
                    _currentTrial.TimeFirstChoiceShown = DateTime.Now;
                    _currentTrial.BlackScreenDuration = _currentTrial.TimeFirstChoiceShown - _currentTrial.TimeBeforeBlackScreen;
                    _currentTrial.State = Trial.FillState.FirstShown;
                    break;
                case Trial.FillState.FirstTransition:
                    _currentTrial.TimeSecondChoiceShown = DateTime.Now;
                    _currentTrial.State = Trial.FillState.SecondShown;
                    break;
                default:
                    Contract.Assert(false, "logging state does not match experiment state");
                    break;
            }
        };
        experimentController.OnHighlightChoice += (cue, _) => {
            switch (_currentTrial.State) {
                case Trial.FillState.FirstShown:
                    _currentTrial.TimeFirstDecision = DateTime.Now;
                    _currentTrial.FirstReactionTime = _currentTrial.TimeFirstDecision - _currentTrial.TimeFirstChoiceShown;
                    _currentTrial.FirstChoice = cue.Name;
                    _currentTrial.State = Trial.FillState.FirstDecision;
                    break;
                case Trial.FillState.SecondShown:
                    _currentTrial.TimeSecondDecision = DateTime.Now;
                    _currentTrial.SecondReactionTime = _currentTrial.TimeSecondDecision - _currentTrial.TimeSecondChoiceShown;
                    _currentTrial.SecondChoice = cue.Name;
                    _currentTrial.State = Trial.FillState.SecondDecision;
                    break;
                default:
                    Contract.Assert(false, "logging state does not match experiment state");
                    break;
            }
        };
        experimentController.OnTransitionSelected += transition => {
            switch (_currentTrial.State) {
                case Trial.FillState.FirstDecision:
                    _currentTrial.CommonTransition = transition.Probability > .5;
                    _currentTrial.SecondStage = transition.NextState.Name;
                    _currentTrial.State = Trial.FillState.FirstTransition;
                    break;
                case Trial.FillState.SecondDecision:
                    bool win = transition.Reward.Win;
                    _currentTrial.Win = win;
                    _currentTrial.RewardProbability = win ? transition.Probability : 1 - transition.Probability;
                    _currentTrial.State = Trial.FillState.SecondTransition;
                    break;
                default:
                    Contract.Assert(false, "logging state does not match experiment state");
                    break;
            }
        };
        experimentController.OnTrialCompleted += () => {
            Contract.Assert(_currentTrial.State == Trial.FillState.SecondTransition, "logging state does not match experiment state");
            _currentTrial.TimeEnd = DateTime.Now;
            _currentTrial.Completed = true;
            _currentTrial.State = Trial.FillState.Done;
            allTrials.Add(_currentTrial);
            _currentTrial = null;
        };
        experimentController.OnTrialAborted += () => {
            _currentTrial.TimeEnd = DateTime.Now;
            _currentTrial.Completed = false;
            allTrials.Add(_currentTrial);
            _currentTrial = null;
        };
    }

    public class Trial {
        public int TrialNumber;
        public bool Completed;
        
        public DateTime TimeBeforeBlackScreen;
        public TimeSpan BlackScreenDuration;
        
        public DateTime TimeFirstChoiceShown;
        public DateTime TimeFirstDecision;
        public TimeSpan FirstReactionTime;
        public string FirstChoice;
        public bool CommonTransition;
        public string SecondStage;

        public DateTime TimeSecondChoiceShown;
        public DateTime TimeSecondDecision;
        public TimeSpan SecondReactionTime;
        public string SecondChoice;
        public float RewardProbability;
        public bool Win;

        public DateTime TimeEnd;

        public FillState State;
        public enum FillState {
            BeforeFirst, FirstShown, FirstDecision, FirstTransition, SecondShown, SecondDecision, SecondTransition, Done
        }
    }
}
