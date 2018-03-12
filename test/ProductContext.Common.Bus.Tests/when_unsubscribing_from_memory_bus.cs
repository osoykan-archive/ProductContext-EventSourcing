using System;
using System.Threading.Tasks;

using FluentAssertions;

using ProductContext.Common.Bus.Tests.Helpers;
using ProductContext.Framework;

using Xunit;

namespace ProductContext.Common.Bus.Tests
{
    public class when_unsubscribing_from_memory_bus
    {
        private readonly InMemoryBus _bus;

        public when_unsubscribing_from_memory_bus()
        {
            _bus = new InMemoryBus("test_bus");
        }

        [Fact]
        public void null_as_handler_app_should_throw()
        {
            Assert.Throws<ArgumentNullException>(() => _bus.Unsubscribe((IHandle<TestMessage>)null));
        }

        [Fact]
        public void not_subscribed_handler_app_doesnt_throw()
        {
            var handler = new TestHandler<TestMessage>();

            Action act = () => _bus.Unsubscribe(handler);

            act.Should().NotThrow<Exception>();
        }

        [Fact]
        public void same_handler_from_same_message_multiple_times_app_doesnt_throw()
        {
            var handler = new TestHandler<TestMessage>();

            Action act = () =>
            {
                _bus.Unsubscribe(handler);
                _bus.Unsubscribe(handler);
                _bus.Unsubscribe(handler);
            };

            act.Should().NotThrow<Exception>();
        }

        [Fact]
        public void multihandler_from_single_message_app_doesnt_throw()
        {
            var handler = new TestMultiHandler();
            _bus.Subscribe<TestMessage>(handler);
            _bus.Subscribe<TestMessage2>(handler);
            _bus.Subscribe<TestMessage3>(handler);

            Action act = () => _bus.Unsubscribe<TestMessage>(handler);

            act.Should().NotThrow<Exception>();
        }

        [Fact]
        public async Task handler_from_message_it_should_not_handle_this_message_anymore()
        {
            var handler = new TestHandler<TestMessage>();
            _bus.Subscribe(handler);

            _bus.Unsubscribe(handler);
            await _bus.PublishAsync(new TestMessage());

            Assert.True(handler.HandledMessages.IsEmpty());
        }

        [Fact]
        public async Task handler_from_multiple_messages_they_all_should_not_be_handled_anymore()
        {
            var handler = new TestMultiHandler();
            _bus.Subscribe<TestMessage>(handler);
            _bus.Subscribe<TestMessage2>(handler);
            _bus.Subscribe<TestMessage3>(handler);

            _bus.Unsubscribe<TestMessage>(handler);
            _bus.Unsubscribe<TestMessage2>(handler);
            _bus.Unsubscribe<TestMessage3>(handler);

            await _bus.PublishAsync(new TestMessage());
            await _bus.PublishAsync(new TestMessage2());
            await _bus.PublishAsync(new TestMessage3());

            Assert.True(handler.HandledMessages.ContainsNo<TestMessage>() &&
                        handler.HandledMessages.ContainsNo<TestMessage2>() &&
                        handler.HandledMessages.ContainsNo<TestMessage3>());
        }

        [Fact]
        public async Task handler_from_message_it_should_not_handle_this_message_anymore_and_still_handle_other_messages()
        {
            var handler = new TestMultiHandler();
            _bus.Subscribe<TestMessage>(handler);
            _bus.Subscribe<TestMessage2>(handler);
            _bus.Subscribe<TestMessage3>(handler);

            _bus.Unsubscribe<TestMessage>(handler);

            await _bus.PublishAsync(new TestMessage());
            await _bus.PublishAsync(new TestMessage2());
            await _bus.PublishAsync(new TestMessage3());

            Assert.True(handler.HandledMessages.ContainsNo<TestMessage>() &&
                        handler.HandledMessages.ContainsSingle<TestMessage2>() &&
                        handler.HandledMessages.ContainsSingle<TestMessage3>());
        }

        [Fact]
        public async Task one_handler_and_leaving_others_subscribed_only_others_should_handle_message()
        {
            var handler1 = new TestHandler<TestMessage>();
            var handler2 = new TestHandler<TestMessage>();
            var handler3 = new TestHandler<TestMessage>();

            _bus.Subscribe(handler1);
            _bus.Subscribe(handler2);
            _bus.Subscribe(handler3);

            _bus.Unsubscribe(handler1);
            await _bus.PublishAsync(new TestMessage());

            Assert.True(handler1.HandledMessages.ContainsNo<TestMessage>() &&
                        handler2.HandledMessages.ContainsSingle<TestMessage>() &&
                        handler3.HandledMessages.ContainsSingle<TestMessage>());
        }

        [Fact]
        public async Task all_handlers_from_message_noone_should_handle_message()
        {
            var handler1 = new TestHandler<TestMessage>();
            var handler2 = new TestHandler<TestMessage>();
            var handler3 = new TestHandler<TestMessage>();

            _bus.Subscribe(handler1);
            _bus.Subscribe(handler2);
            _bus.Subscribe(handler3);

            _bus.Unsubscribe(handler1);
            _bus.Unsubscribe(handler2);
            _bus.Unsubscribe(handler3);
            await _bus.PublishAsync(new TestMessage());

            Assert.True(handler1.HandledMessages.ContainsNo<TestMessage>() &&
                        handler2.HandledMessages.ContainsNo<TestMessage>() &&
                        handler3.HandledMessages.ContainsNo<TestMessage>());
        }

        [Fact]
        public async Task handlers_after_publishing_message_all_is_still_done_correctly()
        {
            var handler1 = new TestHandler<TestMessage>();
            var handler2 = new TestHandler<TestMessage>();
            var handler3 = new TestHandler<TestMessage>();

            _bus.Subscribe(handler1);
            _bus.Subscribe(handler2);
            _bus.Subscribe(handler3);

            await _bus.PublishAsync(new TestMessage());
            handler1.HandledMessages.Clear();
            handler2.HandledMessages.Clear();
            handler3.HandledMessages.Clear();

            //just to ensure
            Assert.True(handler1.HandledMessages.ContainsNo<TestMessage>() &&
                        handler2.HandledMessages.ContainsNo<TestMessage>() &&
                        handler3.HandledMessages.ContainsNo<TestMessage>());

            _bus.Unsubscribe(handler1);
            _bus.Unsubscribe(handler2);
            _bus.Unsubscribe(handler3);
            await _bus.PublishAsync(new TestMessage());

            Assert.True(handler1.HandledMessages.ContainsNo<TestMessage>() &&
                        handler2.HandledMessages.ContainsNo<TestMessage>() &&
                        handler3.HandledMessages.ContainsNo<TestMessage>());
        }
    }
}
