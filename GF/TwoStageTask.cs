using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GF {
    public class ColoredSpriteCue : Cue {
        public readonly string Name;
        public readonly Sprite Sprite;
        public readonly Color Color;
        public ColoredSpriteCue(string name, Sprite sprite, Color color) => (Name, Sprite, Color) = (name, sprite, color);
    }

    public class TwoCuesDecision<TCueType> : State<TCueType> where TCueType : Cue {
        public IEnumerable<TCueType> Cues => new [] { First, Second };
        public readonly string Name;
        public readonly TCueType First;
        public readonly TCueType Second;
        public TwoCuesDecision(string name, TCueType first, TCueType second) => (Name, First, Second) = (name, first, second);
    }

    public class BinaryReward : Reward {
        public float Value => Win ? 1 : 0;
        public readonly bool Win, HighlightReward;
        public BinaryReward(bool win, bool highlightReward) => (Win, HighlightReward) = (win, highlightReward);
    }

    public class ProbabilisticTransition<TCueType, TState, TReward> : Transition<TCueType, TState, TReward>
        where TCueType : Cue where TState : State<TCueType> where TReward : Reward {
        public TReward Reward { get; }
        public TState NextState { get; }
        public readonly float Probability;

        public ProbabilisticTransition(float probability, TReward reward, TState nextState)
            => (Probability, Reward, NextState) = (probability, reward, nextState);
    }

    public class TwoStageTask : GeneralPurposeExperimentController<
        TwoStageTask.Parameters,
        ColoredSpriteCue,
        TwoCuesDecision<ColoredSpriteCue>,
        BinaryReward,
        ProbabilisticTransition<ColoredSpriteCue, TwoCuesDecision<ColoredSpriteCue>, BinaryReward>
    > {

        #region Unity Settings

        [SerializeField] private int SuccessfulTrialsToCompleteExperiment;

        [Header("Probabilities")] [SerializeField]
        private float CommonTransitionProbability;

        [SerializeField] private float GreatWinProbability;
        [SerializeField] private float NormalWinProbability;

        [Header("Time Constraints")] [SerializeField]
        private float MeanInterTrialTime;

        [SerializeField] private float TimePerDecision;
        [SerializeField] private float DecisionHighlightTime;
        [SerializeField] private float RewardHighlightTime;

        [Header("Cues")] [SerializeField] private Sprite FirstA;
        [SerializeField] private Sprite FirstB;
        [SerializeField] private Color First;
        [SerializeField] private Sprite Second1A, Second1B;
        [SerializeField] private Color Second1;
        [SerializeField] private Sprite Second2A, Second2B;
        [SerializeField] private Color Second2;

        #endregion

        public TwoCuesDecision<ColoredSpriteCue> StartState => _parameters.StartState;
        protected override Parameters ExperimentParameters => _parameters;
        private Parameters _parameters;

        private void Awake() => _parameters = new Parameters(this);

        public class Parameters : ExperimentParameters<
            ColoredSpriteCue,
            TwoCuesDecision<ColoredSpriteCue>,
            BinaryReward,
            ProbabilisticTransition<ColoredSpriteCue, TwoCuesDecision<ColoredSpriteCue>, BinaryReward>
        > {

            #region Private Fields

            private readonly TwoStageTask _twoStageTask;
            private readonly ColoredSpriteCue _firstA;
            private readonly TwoCuesDecision<ColoredSpriteCue> _first, _second1, _second2;
            private readonly bool _commonTransitionIsFirstAToSecond1;
            private readonly Dictionary<ColoredSpriteCue, float> _winProbabilities = new Dictionary<ColoredSpriteCue, float>();

            public Parameters(TwoStageTask twoStageTask) {
                _twoStageTask = twoStageTask;
                _firstA = new ColoredSpriteCue("First A", _twoStageTask.FirstA, _twoStageTask.First);
                var firstB = new ColoredSpriteCue("First B", _twoStageTask.FirstB, _twoStageTask.First);
                _first = new TwoCuesDecision<ColoredSpriteCue>("First", _firstA, firstB);
                var second1A = new ColoredSpriteCue("Second 1 A", _twoStageTask.Second1A, _twoStageTask.Second1);
                var second1B = new ColoredSpriteCue("Second 1 B", _twoStageTask.Second1B, _twoStageTask.Second1);
                _second1 = new TwoCuesDecision<ColoredSpriteCue>("Second 1", second1A, second1B);
                var second2A = new ColoredSpriteCue("Second 2 A", _twoStageTask.Second2A, _twoStageTask.Second2);
                var second2B = new ColoredSpriteCue("Second 2 B", _twoStageTask.Second2B, _twoStageTask.Second2);
                _second2 = new TwoCuesDecision<ColoredSpriteCue>("Second 2", second2A, second2B);
                _commonTransitionIsFirstAToSecond1 = Random.value > .5;
                int bestChoice = Random.Range(0, 4);
                _winProbabilities[second1A] = bestChoice == 0 ? _twoStageTask.GreatWinProbability : _twoStageTask.NormalWinProbability;
                _winProbabilities[second1B] = bestChoice == 1 ? _twoStageTask.GreatWinProbability : _twoStageTask.NormalWinProbability;
                _winProbabilities[second2A] = bestChoice == 2 ? _twoStageTask.GreatWinProbability : _twoStageTask.NormalWinProbability;
                _winProbabilities[second2B] = bestChoice == 3 ? _twoStageTask.GreatWinProbability : _twoStageTask.NormalWinProbability;
            }

            #endregion
            

            public int NumberOfTrials => _twoStageTask.SuccessfulTrialsToCompleteExperiment;
            public TwoCuesDecision<ColoredSpriteCue> StartState => _first;
            public float GetInterTrialTime(int trialNumber) => -(float) Math.Log(Random.value) * _twoStageTask.MeanInterTrialTime;
            public float GetTimeForDecision(TwoCuesDecision<ColoredSpriteCue> state) => _twoStageTask.TimePerDecision;
            public float GetDecisionHighlightDuration(ColoredSpriteCue action) => _twoStageTask.DecisionHighlightTime;
            public float GetRewardHighlightDuration(BinaryReward reward) 
                => reward.HighlightReward ? _twoStageTask.RewardHighlightTime : 0;

            public ProbabilisticTransition<ColoredSpriteCue, TwoCuesDecision<ColoredSpriteCue>, BinaryReward> GetTransition(
                TwoCuesDecision<ColoredSpriteCue> currentState, ColoredSpriteCue action
            ) {
                // after first choice
                if (currentState == _first) {
                    float commonProbability = _twoStageTask.CommonTransitionProbability;
                    bool commonTransition = Random.value <= commonProbability;
                    var nextState = commonTransition ^ _commonTransitionIsFirstAToSecond1 ^ action == _firstA ? _second1 : _second2;
                    return new ProbabilisticTransition<ColoredSpriteCue, TwoCuesDecision<ColoredSpriteCue>, BinaryReward>(
                        commonTransition ? commonProbability : 1 - commonProbability,
                        new BinaryReward(false, false),
                        nextState
                    );
                    
                // after second choice
                } else {
                    float winProbability = _winProbabilities[action];
                    bool win = Random.value <= winProbability;
                    return new ProbabilisticTransition<ColoredSpriteCue, TwoCuesDecision<ColoredSpriteCue>, BinaryReward>(
                        win ? winProbability : 1 - winProbability,
                        new BinaryReward(win, true),
                        _first
                    );
                }
            }

            public bool TryGetNoActionTransition(TwoCuesDecision<ColoredSpriteCue> currentState, out ProbabilisticTransition<ColoredSpriteCue, TwoCuesDecision<ColoredSpriteCue>, BinaryReward> transition) {
                // always abort on "no choice"
                transition = null;
                return false;
            }

        }
    }
}