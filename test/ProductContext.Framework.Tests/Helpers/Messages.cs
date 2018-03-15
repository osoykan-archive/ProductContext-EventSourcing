using System;

namespace ProductContext.Framework.Tests.Helpers
{
    public class DeferredExecutionTestMessage : Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Message
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

    public class ExecutableTestMessage : Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Message
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