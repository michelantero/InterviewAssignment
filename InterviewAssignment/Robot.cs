using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Reports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewAssignment
{
    public class Robot
    {
        public enum Triggers
        {
            ExecutionStarts,
            ExecutionEnds,
            InitializationStarts,
            InitializationEnds,
            AxesBusy,
            AxesStop,
            PowerOff,
            PowerOn,
            Recovery,
            Error,
            Deinitialize,
            PowerOffReady,
            PowerOnReady
        }

        public enum State
        {
            /// <summary>
            /// One of the root states. 
            /// Robot is functional i.e. it is not down.
            /// </summary>
            Ready,

            /// <summary>
            /// One of the root states.
            /// Robot is either not initialized or is in error state.
            /// </summary>
            Down,

            /// <summary>
            /// Robot is not processing anything
            /// </summary>
            Idle,

            /// <summary>
            /// Robot is executing a move or an other action.
            /// This doesn't necessary mean that robot is actually moving,
            /// because pretaught moves contains also delays.
            /// </summary>
            Processing,


            /// <summary>
            /// Initial state of robot
            /// </summary>
            NotInitialized,

            /// <summary>
            /// Robot is doing initialization or homing
            /// </summary>
            Initializing,

            /// <summary>
            /// Robot is ready execute the next action
            /// </summary>
            PowerOn,

            /// <summary>
            /// Motor/Driver are powered off
            /// </summary>
            PowerOff,


            /// <summary>
            /// Robot axes are moving
            /// </summary>
            AxesMoving,

            /// <summary>
            /// Robot axes are idle
            /// </summary>
            AxesIdle,

            /// <summary>
            /// Robot encountered an error and must be reinitialized
            /// </summary>
            Error,

            /// <summary>
            /// Robot is powering off
            /// </summary>
            PoweringOff,

            /// <summary>
            /// Robot is powering on
            /// </summary>
            PoweringOn
        }

        public PassiveStateMachine<State, Triggers> StateMachine = new PassiveStateMachine<State, Triggers>();
        CustomStateMachineReporter<State, Triggers> customReporter = new CustomStateMachineReporter<State, Triggers>();

        public string myCurrentState { get; private set; }

        public Robot()
        {
            #region State machine definition
            StateMachine.In(State.NotInitialized)
                .On(Triggers.InitializationStarts).Goto(State.Initializing);

            StateMachine.In(State.Initializing)
                .On(Triggers.InitializationEnds).Goto(State.PowerOn);

            StateMachine.In(State.PowerOn)
                .On(Triggers.ExecutionStarts).Goto(State.Processing);

            StateMachine.In(State.Processing)
                .On(Triggers.ExecutionEnds).Goto(State.PowerOn);

            StateMachine.In(State.AxesIdle)
                .On(Triggers.AxesBusy).Goto(State.AxesMoving);

            StateMachine.In(State.AxesMoving)
                .On(Triggers.AxesStop).Goto(State.AxesIdle);

            StateMachine.In(State.PowerOn)
                .On(Triggers.PowerOff).Goto(State.PoweringOff);

            StateMachine.In(State.PowerOff)
                .On(Triggers.PowerOn).Goto(State.PoweringOn);

            StateMachine.In(State.PoweringOff)
                .On(Triggers.PowerOffReady).Goto(State.PowerOff);

            StateMachine.In(State.PoweringOn)
                .On(Triggers.PowerOnReady).Goto(State.PowerOn);

            StateMachine.In(State.Error)
                .On(Triggers.InitializationStarts).Goto(State.Initializing);

            StateMachine.In(State.Ready)
                .On(Triggers.Error).Goto(State.Error);

            StateMachine.In(State.Down)
                .On(Triggers.Error).Goto(State.Error);

            StateMachine.In(State.Idle)
                .On(Triggers.Deinitialize).Goto(State.NotInitialized);

            StateMachine.DefineHierarchyOn(State.Processing)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(State.AxesMoving)
                .WithSubState(State.AxesIdle);

            StateMachine.DefineHierarchyOn(State.Idle)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(State.PowerOn)
                .WithSubState(State.PowerOff)
                .WithSubState(State.PoweringOff)
                .WithSubState(State.PoweringOn);

            StateMachine.DefineHierarchyOn(State.Ready)
                .WithHistoryType(HistoryType.Deep)
                .WithInitialSubState(State.Idle)
                .WithSubState(State.Processing);

            StateMachine.DefineHierarchyOn(State.Down)
                .WithHistoryType(HistoryType.None)
                .WithInitialSubState(State.NotInitialized)
                .WithSubState(State.Initializing)
                .WithSubState(State.Error);

            StateMachine.Initialize(State.NotInitialized);

            StateMachine.Start();

            StateMachine.TransitionCompleted += MyStateMachine_TransitionCompleted;

            using (Stream stream = File.Open("StateDiagram.graphml", FileMode.Create, FileAccess.Write, FileShare.Read))
            using (TextWriter writer = new StreamWriter(stream))
            {
                var generator = new YEdStateMachineReportGenerator<State, Triggers>(writer);
                StateMachine.Report(generator);
            }
            StateMachine.Report(customReporter);
        }
        #endregion

        private void MyStateMachine_TransitionCompleted(object sender, Appccelerate.StateMachine.Machine.Events.TransitionCompletedEventArgs<State, Triggers> e)
        {
            myCurrentState = customReporter.StateToString(e.NewStateId);
        }
    }
}
