// This program set up a state machine system for projectile based on Dictionary
// In this mod, this state machine is used to maintain the state list and execute the current state of any sword projectiles
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MeleeRevamp.Content.Core
{
    // ProjectileStateMachine is an abstract class inherited from ModProjectile, which is the base class for all projectiles that have a state machine
    // This class requires Projectile.ai[0] and Projectile.ai[1] to be used as Timer and State respectively
    // This class allows registration of states and switching between those states, using currentState to track the current state of this projectile
    // All projectiles inherited from this class must implement Initialize() to register all the states used by that projectile
    // While running AI() for the first time, state = 0, so the projectile will run Initialize() to register all states and switch to state 1
    // All projectiles inherited from this class have their AI sealed and running the AI corresponding to currentState
    // AIBefore() and AIAfter() are provided for executing code before and after the AI executed
    public abstract class ProjectileStateMachine : ModProjectile
    {
        // ProjectileState is an abstract class that defines the components of a state of projectile
        // As the state of a projectile is only for state machine, it does not inherit from ModProjectile
        // And the state of a projectile is only for state machine, which means it is only defined for projectiles inherited from ProjectileStateMachine
        public abstract class ProjectileState
        {
            // TriggerAI is used to trigger the AI of this state, it can be used to set up variables or do other things before the AI is executed
            public abstract void TriggerAI(ProjectileStateMachine projectile, params object[] args);
            // AI, ugh, is literally AI for this state
            public abstract void AI(ProjectileStateMachine projectile);
            // SwitchState is used to switch to other ProjectileState using SetState while setting up variables for that state
            public abstract void SwitchState(ProjectileStateMachine projectile);
        }
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
        // Set the state for projectile. It only set projectile state, so it should only be called in SwitchState
        public void SetState<T>(params object[] args) where T : ProjectileState 
        {
            var name = typeof(T).FullName; // Get the type of template
            if (!stateDict.ContainsKey(name))
                throw new ArgumentException("This state does not exist"); // throw exception if the required state does not exist
            State = stateDict[name]; // Set state to the required state
            Timer = 0; // Reset timer to 0 whenever the state is switched
            projStates[State - 1].TriggerAI(this, args);
        }
        // Register the state for projectile
        protected void RegisterState<T>(T state) where T : ProjectileState 
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

            AIBefore(); // Execute this function before AI of this state is executed, not affected by currentState
            currentState.AI(this); // Execute currentState.AI()
            AIAfter(); // Execute this function after AI of this state is executed, not affected by currentState
        }

        public virtual void AIBefore() { } // AI executed before currentState.AI() is executed

        public virtual void AIAfter() { } // AI executed after currentState.AI() is executed
    }
}
