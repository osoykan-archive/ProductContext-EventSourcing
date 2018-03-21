using System;
using System.Threading.Tasks;

using ProductContext.Framework.Tests.Helpers;

using Xunit;

namespace ProductContext.Framework.Tests
{
    public class when_subscribing_to_memory_bus
    {
        private readonly InMemoryBus _bus;

        public when_subscribing_to_memory_bus()
        {
            _bus = new InMemoryBus();
        }

        [Fact]
        public void null_as_handler_app_should_throw_arg_null_exception()
        {
            Assert.Throws<ArgumentNullException>(() => _bus.Subscribe((IHandle<TestMessage>)null));
        }

        [Fact]
        public async Task but_not_publishing_messages_noone_should_handle_any_messages()
        {
            var multiHandler = new TestMultiHandler();
            _bus.Subscribe<TestMessage>(multiHandler);
            _bus.Subscribe<TestMessage2>(multiHandler);
            _bus.Subscribe<TestMessage3>(multiHandler);

            Assert.True(multiHandler.HandledMessages.Count == 0);
        }

        [Fact]
        public async Task one_handler_to_one_message_it_should_be_handled()
        {
            var handler = new TestHandler<TestMessage>();
            _bus.Subscribe(handler);

            await _bus.PublishAsync(new TestMessage());

            Assert.True(handler.HandledMessages.ContainsSingle<TestMessage>());
        }

        [Fact]
        public async Task one_handler_to_multiple_messages_they_all_should_be_handled()
        {
            var multiHandler = new TestMultiHandler();
            _bus.Subscribe<TestMessage>(multiHandler);
            _bus.Subscribe<TestMessage2>(multiHandler);
            _bus.Subscribe<TestMessage3>(multiHandler);

            await _bus.PublishAsync(new TestMessage());
            await _bus.PublishAsync(new TestMessage2());
            await _bus.PublishAsync(new TestMessage3());

            Assert.True(multiHandler.HandledMessages.ContainsSingle<TestMessage>() &&
                        multiHandler.HandledMessages.ContainsSingle<TestMessage2>() &&
                        multiHandler.HandledMessages.ContainsSingle<TestMessage3>());
        }

        [Fact]
        public async Task one_handler_to_few_messages_then_only_subscribed_should_be_handled()
        {
            var multiHandler = new TestMultiHandler();
            _bus.Subscribe<TestMessage>(multiHandler);
            _bus.Subscribe<TestMessage3>(multiHandler);

            await _bus.PublishAsync(new TestMessage());
            await _bus.PublishAsync(new TestMessage2());
            await _bus.PublishAsync(new TestMessage3());

            Assert.True(multiHandler.HandledMessages.ContainsSingle<TestMessage>() &&
                        multiHandler.HandledMessages.ContainsNo<TestMessage2>() &&
                        multiHandler.HandledMessages.ContainsSingle<TestMessage3>());
        }

        [Fact]
        public async Task multiple_handlers_to_one_message_then_each_handler_should_handle_message_once()
        {
            var handler1 = new TestHandler<TestMessage>();
            var handler2 = new TestHandler<TestMessage>();

            _bus.Subscribe(handler1);
            _bus.Subscribe(handler2);

            await _bus.PublishAsync(new TestMessage());

            Assert.True(handler1.HandledMessages.ContainsSingle<TestMessage>());
            Assert.True(handler2.HandledMessages.ContainsSingle<TestMessage>());
        }

        [Fact]
        public async Task multiple_handlers_to_multiple_messages_then_each_handler_should_handle_subscribed_messages()
        {
            var handler1 = new TestMultiHandler();
            var handler2 = new TestMultiHandler();
            var handler3 = new TestMultiHandler();

            _bus.Subscribe<TestMessage>(handler1);
            _bus.Subscribe<TestMessage3>(handler1);

            _bus.Subscribe<TestMessage>(handler2);
            _bus.Subscribe<TestMessage2>(handler2);

            _bus.Subscribe<TestMessage2>(handler3);
            _bus.Subscribe<TestMessage3>(handler3);

            await _bus.PublishAsync(new TestMessage());
            await _bus.PublishAsync(new TestMessage2());
            await _bus.PublishAsync(new TestMessage3());

            Assert.True(handler1.HandledMessages.ContainsSingle<TestMessage>() &&
                        handler1.HandledMessages.ContainsSingle<TestMessage3>() &&
                        handler2.HandledMessages.ContainsSingle<TestMessage>() &&
                        handler2.HandledMessages.ContainsSingle<TestMessage2>() &&
                        handler3.HandledMessages.ContainsSingle<TestMessage2>() &&
                        handler3.HandledMessages.ContainsSingle<TestMessage3>());
        }

        [Fact]
        public async Task multiple_handlers_to_multiple_messages_then_each_handler_should_handle_only_subscribed_messages()
        {
            var handler1 = new TestMultiHandler();
            var handler2 = new TestMultiHandler();
            var handler3 = new TestMultiHandler();

            _bus.Subscribe<TestMessage>(handler1);
            _bus.Subscribe<TestMessage3>(handler1);

            _bus.Subscribe<TestMessage>(handler2);
            _bus.Subscribe<TestMessage2>(handler2);

            _bus.Subscribe<TestMessage2>(handler3);
            _bus.Subscribe<TestMessage3>(handler3);

            await _bus.PublishAsync(new TestMessage());
            await _bus.PublishAsync(new TestMessage2());
            await _bus.PublishAsync(new TestMessage3());

            Assert.True(handler1.HandledMessages.ContainsSingle<TestMessage>() &&
                        handler1.HandledMessages.ContainsNo<TestMessage2>() &&
                        handler1.HandledMessages.ContainsSingle<TestMessage3>() &&
                        handler2.HandledMessages.ContainsSingle<TestMessage>() &&
                        handler2.HandledMessages.ContainsSingle<TestMessage2>() &&
                        handler2.HandledMessages.ContainsNo<TestMessage3>() &&
                        handler3.HandledMessages.ContainsNo<TestMessage>() &&
                        handler3.HandledMessages.ContainsSingle<TestMessage2>() &&
                        handler3.HandledMessages.ContainsSingle<TestMessage3>());
        }

        [Fact /*, Ignore("This logic is confused when having hierarchy flattening on subscription in InMemoryBus.")*/]
        public async Task same_handler_to_same_message_few_times_then_message_should_be_handled_only_once()
        {
            var handler = new TestHandler<TestMessage>();
            _bus.Subscribe(handler);
            _bus.Subscribe(handler);
            _bus.Subscribe(handler);

            await _bus.PublishAsync(new TestMessage());

            Assert.True(handler.HandledMessages.ContainsSingle<TestMessage>());
        }
    }
}
