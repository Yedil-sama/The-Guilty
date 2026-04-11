using UnityEngine;
using UnityEngine.AI;

namespace TheGuilty.Game
{
    public class IdleStrategy : IMannequinStrategy
    {
        public void Execute(Mannequin mannequin)
        {
            if (mannequin.NavMeshAgent == null || mannequin.Animator == null) return;

            // Stop NavMesh movement
            mannequin.NavMeshAgent.ResetPath();
            mannequin.NavMeshAgent.speed = 0;

            // Set animator - all movement bools are false
            mannequin.SetMovementState(false, false, false, false);
        }
    }
}