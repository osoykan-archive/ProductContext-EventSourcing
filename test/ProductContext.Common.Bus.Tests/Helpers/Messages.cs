using System;

using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace ProductContext.Common.Bus.Tests.Helpers
{
    public class DeferredExecutionTestMessage : Message
    {
        private readonly Action _action;

        public DeferredExecutionTestMessage(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            _action = action;
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
            if (action == null)
                throw new ArgumentNullException("action");
            _action = action;
        }

        public void Execute()
        {
            _action();
        }
    }
}