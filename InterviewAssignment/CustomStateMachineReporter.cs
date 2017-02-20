using Appccelerate.StateMachine.Machine;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterviewAssignment
{
    /// <summary>
    /// Provides way to customize state reporting.
    /// 
    /// By default you cannot get container of states from Appccelerate state machine.
    /// This class can be used for that. Also it has StateToString method to enable
    /// printing hierarchical states.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    public class CustomStateMachineReporter<TState, TEvent> : IStateMachineReport<TState, TEvent>
            where TState : IComparable
    where TEvent : IComparable

    {
        IEnumerable<IState<TState, TEvent>> myStates;
        /**/
       class FullStates
        {
            public string State;
            public string SuperState;
            public string InitialState;
        }        
        List<FullStates> StatesPool = new List<FullStates>(); 
               
        /**/
        public IEnumerable<IState<TState, TEvent>> States
        {
            get
            {
                return myStates;
            }
        }

        public void Report(string name, IEnumerable<IState<TState, TEvent>> states, Initializable<TState> initialStateId)
        {   
            myStates = states;            
            string InitialState;
            
            foreach (var item in states)
            {
                //not the most elegant way to check the state but works..
                //could be refactored to method...
                try
                {
                    InitialState = item.SuperState.SuperState.ToString();   
                }
                catch (Exception)
                {
                    InitialState = "";
                }    
                StatesPool.Add(new FullStates { State = item.ToString(), SuperState = Convert.ToString(item.SuperState), InitialState = InitialState });
            }
        }
      
        public string StateToString(TState state, string separator = ".")
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in StatesPool)
            {
                if (item.State.Equals(state.ToString()))
                {
                    if (item.InitialState.ToString().Length > 1)
                    {
                        stringBuilder.Append(item.InitialState.ToString()).Append(separator);
                    }
                    stringBuilder.Append(item.SuperState.ToString()).Append(separator);
                    stringBuilder.Append(item.State);
                    break;
                }
            }
            return stringBuilder.ToString();               
            //Your assignment is here!
            //Tip: You find state machine hierarchy on States property (or myStates field). 
            //You should go through the states and print that on what hierarchy path the current state
            //is found. So if state is Initializing this method should return "Down.Initializing" because
            //"Initializing" is substate of "Down".
            //throw new NotImplementedException();
        }
    }
}
