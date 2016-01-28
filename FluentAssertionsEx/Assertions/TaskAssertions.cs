using System;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;

namespace HivePeople.FluentAssertionsEx.Assertions
{
    public class TaskAssertions
    {
        public Task Subject { get; private set; }

        internal TaskAssertions(Task actualTask)
        {
            this.Subject = actualTask;
        }

        public AndConstraint<TaskAssertions> BeCompleted(string because = "", params object[] reasonArgs)
        {
            // Freeze actual value, since the status of a task may change during execution
            var actualStatus = Subject.Status;

            Execute.Assertion
                .ForCondition(actualStatus == TaskStatus.RanToCompletion || actualStatus == TaskStatus.Faulted || actualStatus == TaskStatus.Canceled)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected {context:task} to be completed{reason}, but its actual status was {0:G}.", actualStatus);

            return new AndConstraint<TaskAssertions>(this);
        }

        public AndConstraint<TaskAssertions> NotBeCompleted(string because = "", params object[] reasonArgs)
        {
            // Freeze actual value, since the status of a task may change during execution
            var actualStatus = Subject.Status;

            Execute.Assertion
                .ForCondition(actualStatus != TaskStatus.RanToCompletion && actualStatus != TaskStatus.Faulted && actualStatus != TaskStatus.Canceled)
                .BecauseOf(because, reasonArgs)
                .FailWith("Did not expect {context:task} to be completed{reason}, but its actual status was {0:G}.", actualStatus);

            return new AndConstraint<TaskAssertions>(this);
        }

        public AndConstraint<TaskAssertions> BeCompletedWithSuccess(string because = "", params object[] reasonArgs)
        {
            // Freeze actual value, since the status of a task may change during execution
            var actualStatus = Subject.Status;

            Execute.Assertion
                .ForCondition(actualStatus == TaskStatus.RanToCompletion)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected {context:task} to have completed successfully{reason}, but its actual status was {0:G}.", actualStatus);

            return new AndConstraint<TaskAssertions>(this);
        }

        public AndConstraint<TaskAssertions> BeCancelled(string because = "", params object[] reasonArgs)
        {
            // Freeze actual value, since the status of a task may change during execution
            var actualStatus = Subject.Status;

            Execute.Assertion
                .ForCondition(actualStatus == TaskStatus.Canceled)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected {context:task} to be canceled{reason}, but its actual status was {0:G}.", actualStatus);

            return new AndConstraint<TaskAssertions>(this);
        }

        public AndConstraint<TaskAssertions> BeFaulted(string because = "", params object[] reasonArgs)
        {
            return HaveStatus(TaskStatus.Faulted, because, reasonArgs);
        }

        public AndWhichConstraint<TaskAssertions, Exception> BeFaultedWithException<TException>(string because = "", params object[] reasonArgs)
        {
            return BeFaulted(because, reasonArgs).And.HaveException<TException>(because, reasonArgs);
        }

        public AndConstraint<TaskAssertions> HaveStatus(TaskStatus expectedStatus, string because = "", params object[] reasonArgs)
        {
            // Freeze actual value, since the status of a task may change during execution
            var actualStatus = Subject.Status;

            Execute.Assertion
                .ForCondition(actualStatus == expectedStatus)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected {context:task} to have status {0:G}{reason}, but its actual status was {1:G}.", expectedStatus, actualStatus);

            return new AndConstraint<TaskAssertions>(this);
        }

        public AndWhichConstraint<TaskAssertions, Exception> HaveException<TException>(string because = "", params object[] reasonArgs)
        {
            // Freeze actual value, since the exception can be set during execution (we don't know if the task is running or completed)
            var actualException = Subject.Exception;

            Execute.Assertion
                .ForCondition(actualException is TException)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected {context:task} to have exception {0}{reason}, but found {1}.", typeof(TException), actualException);

            return new AndWhichConstraint<TaskAssertions, Exception>(this, actualException);
        }
    }
}
