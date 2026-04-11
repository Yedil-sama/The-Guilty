using System;

namespace TheGuilty.Game
{
    public enum MannequinState
    {
        Idle,
        Following,
        Attacking
    }

    public class MannequinStateMachine
    {
        private Mannequin _mannequin;
        private MannequinState _currentState;

        public MannequinStateMachine(Mannequin mannequin)
        {
            _mannequin = mannequin;
            _currentState = MannequinState.Idle;
        }

        public void Update()
        {
            switch (_currentState)
            {
                case MannequinState.Idle:
                    // Idle logic - perhaps set IdleStrategy
                    break;
                case MannequinState.Following:
                    // Following logic - set FollowPlayerStrategy
                    break;
                case MannequinState.Attacking:
                    // Attacking logic
                    break;
            }
        }

        public void ChangeState(MannequinState newState)
        {
            if (_currentState == newState) return;

            _currentState = newState;
            // Handle state transitions and set strategies
            switch (newState)
            {
                case MannequinState.Idle:
                    _mannequin.SetStrategy(new IdleStrategy());
                    break;
                case MannequinState.Following:
                    _mannequin.SetStrategy(new FollowPlayerStrategy());
                    break;
                case MannequinState.Attacking:
                    // Perhaps a different strategy or same as follow
                    _mannequin.SetStrategy(new FollowPlayerStrategy());
                    break;
            }
        }

        public MannequinState CurrentState => _currentState;
    }
}