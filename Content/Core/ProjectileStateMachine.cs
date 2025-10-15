// This program set up a state machine system for projectile based on Dictionary
// In this mod, this state machine is used to maintain the state list and execute the current state of any sword projectiles
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MeleeRevamp.Content.Core
{
    public abstract class ProjectileState // An abstract class, the state of Projectile
    {
        public abstract void TriggerAI(ProjectileStateMachine projectile, params object[] args);
        public abstract void AI(ProjectileStateMachine projectile); // AI for ProjectileStateMachine
    }
    public abstract class ProjectileStateMachine : ModProjectile
    {
        private List<ProjectileState> projStates = new List<ProjectileState>(); // The list of all projectile states
        private Dictionary<string, int> stateDict = new Dictionary<string, int>(); // A dictionary for matching name of states and their index
        public ProjectileState currentState => projStates[State - 1]; // Current state located in the state list

        public int Timer // The timer of projectile, get and set by ai[0] provided by TModloader API
        {
            get
            {
                return (int)Projectile.ai[0];
            }
            set
            {
                Projectile.ai[0] = value;
            }
        }
        private int State // The int index of state, get and set by ai[1] provided by TModloader API
        {
            get
            {
                return (int)Projectile.ai[1];
            }
            set
            {
                Projectile.ai[1] = value;
            }
        }

        public void SetState<T>() where T : ProjectileState // Set the state for projectile
        {
            var name = typeof(T).FullName; // Get the type of template
            if (!stateDict.ContainsKey(name))
                throw new ArgumentException("This state does not exist"); // throw exception if the required state does not exist
            State = stateDict[name]; // Set state to the required state
        }
        protected void RegisterState<T>(T state) where T : ProjectileState // Register the state for projectile
        {
            var name = typeof(T).FullName; // Get the type of template
            if (stateDict.ContainsKey(name))
                throw new ArgumentException("This state is already registered"); // throw exception if the state is already registered
            projStates.Add(state); // Add the new state to the state list
            stateDict.Add(name, projStates.Count); // Add the new state to the projectile
        }
        public abstract void Initialize(); //Initialize all the ProjectileState

        public sealed override void AI()
        {
            if (State == 0) // Initial state, all projectiles start at state 0
            {
                Initialize(); // Initialize and set the projectile state to the state with index 1
                State = 1;
            }

            AIBefore(); // Execute this function before AI of this state is executed
            currentState.AI(this); // Execute currentState.AI()
            AIAfter(); // Execute this function after AI of this state is executed
        }

        public virtual void AIBefore() { } // AI executed before currentState.AI() is executed

        public virtual void AIAfter() { } // AI executed after currentState.AI() is executed
    }
}
