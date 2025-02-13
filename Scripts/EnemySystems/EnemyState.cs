using EnemyLogic;
using EnemyStateMachineLogic;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyStateLogic
{
    public class EnemyState
    {
        protected Enemy _enemyBase;
        protected EnemyStateMachine _stateMachine;
        protected string _animationBoolName;
        protected float _stateTimer;
        protected bool _triggerCalled;
        
        public EnemyState(Enemy enemyBase, EnemyStateMachine stateMachine, string animationBoolName)
        {
            _enemyBase = enemyBase;
            _stateMachine = stateMachine;
            _animationBoolName = animationBoolName;
        }
        
        public virtual void Enter()
        {
            _enemyBase._animator.SetBool(_animationBoolName, true);
            
            _triggerCalled = false;
        }
        
        public virtual void Update()
        {
            _stateTimer -= Time.deltaTime;
        }
        
        public virtual void Exit()
        {
            _enemyBase._animator.SetBool(_animationBoolName, false);
        }
        
        public virtual void SpecialAbilityTrigger()
        {
            
        }

        public void AnimationTrigger()
        {
            _triggerCalled = true;
        }
        
        protected Vector3 GetNextPatrolCorner()
        {
            NavMeshAgent agent = _enemyBase._navMeshAgent;
            NavMeshPath path = agent.path;
            
            if (path.corners.Length < 2)
                return agent.destination;
            
            for (int i = 0; i < path.corners.Length; i++)
            {
                if (Vector3.Distance(agent.transform.position, path.corners[i]) < 1f)
                    return path.corners[i + 1];
            }
            
            return agent.destination;
        }
    }
}