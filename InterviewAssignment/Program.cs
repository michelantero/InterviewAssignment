using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InterviewAssignment
{
    class Program
    {
        static void Main(string[] args)
        {
            var r = new Robot();
            r.StateMachine.Fire(Robot.Triggers.InitializationStarts);

            if (r.myCurrentState == "Down.Initializing")
            {
                r.StateMachine.Fire(Robot.Triggers.InitializationEnds);
                if (r.myCurrentState == "Ready.Idle.PowerOn")
                {

                    Console.WriteLine("Assignment done!");
                    Console.ReadKey();
                    return;
                }
            }

            Console.WriteLine("Assignment not ready...");
            Console.ReadKey();
        }
    }
}
