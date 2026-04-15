using UnityEngine;

namespace FSM.Core
{
        public interface IState
        {
            void Start();

            void Update();

            void Exit();
        }
}