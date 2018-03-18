using System;

namespace ProductContext.Framework.Tests.Helpers
{
    public class DeferredExecutionTestMessage : Message
    {
        private readonly Action _action;

        public DeferredExecutionTestMessage(Action action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Execute()
        {
            _action();
        }
    }

    public class ExecutableTestMessage : Message
    {
        private readonly Action _action;

        public ExecutableTestMessage(Action action)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public void Execute()
        {
            _action();
        }
    }
}
