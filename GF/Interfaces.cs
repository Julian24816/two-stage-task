using System.Collections.Generic;

namespace GF {
    public interface Cue {
        // Marker interface that a certain class is meant to be used as a cue
    }

    public interface State<out TCueType> where TCueType : Cue {
        IEnumerable<TCueType> Cues { get; }
    }

    public interface Reward {
        float Value { get; }
    }

    public interface Transition<out TCueType, out TState, out TReward>
        where TCueType : Cue where TState : State<TCueType> where TReward : Reward {

        TReward Reward { get; }
        TState NextState { get; }
    }

}