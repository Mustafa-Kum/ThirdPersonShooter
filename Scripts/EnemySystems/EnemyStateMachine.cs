using EnemyStateLogic;
using UnityEngine;

namespace EnemyStateMachineLogic
{
    public class EnemyStateMachine
    {
        public EnemyState _currentState { get; private set; }
        
        public void Initialize(EnemyState startState)
        {
            _currentState = startState;
            _currentState.Enter();
        }
        
        public void ChangeState(EnemyState newState)
        {
            _currentState.Exit();
            _currentState = newState;
            _currentState.Enter();
        }
    }
}