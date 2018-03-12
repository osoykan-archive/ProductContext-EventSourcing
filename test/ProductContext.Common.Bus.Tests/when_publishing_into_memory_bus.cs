using System;
using System.Threading.Tasks;

using ProductContext.Common.Bus.Tests.Helpers;
using ProductContext.Framework;

using Xunit;

namespace ProductContext.Common.Bus.Tests
{
    public class when_publishing_into_memory_bus
    {
        private readonly InMemoryBus _bus;

        public when_publishing_into_memory_bus()
        {
            _bus = new InMemoryBus("test_bus");
        }

        [Fact(Skip = "We do not check each message for null for performance reasons.")]
        public async Task null_message_app_should_throw()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _bus.PublishAsync(null));
        }

        [Fact]
        public async Task unsubscribed_messages_noone_should_handle_it()
        {
            var handler1 = new TestHandler<TestMessage>();
            var handler2 = new TestHandler<TestMessage2>();
            var handler3 = new TestHandler<TestMessage3>();

            await _bus.PublishAsync(new TestMessage());
            await _bus.PublishAsync(new TestMessage2());
            await _bus.PublishAsync(new TestMessage3());

            Assert.True(handler1.HandledMessages.Count == 0
                        && handler2.HandledMessages.Count == 0
                        && handler3.HandledMessages.Count == 0);
        }

        [Fact]
        public async Task any_message_no_other_messages_should_be_published()
        {
            var handler1 = new TestHandler<TestMessage>();
            var handler2 = new TestHandler<TestMessage2>();

            _bus.Subscribe(handler1);
            _bus.Subscribe(handler2);

            await _bus.PublishAsync(new TestMessage());

            Assert.True(handler1.HandledMessages.ContainsSingle<TestMessage>() && handler2.HandledMessages.Count == 0);
        }

        [Fact]
        public async Task same_message_n_times_it_should_be_handled_n_times()
        {
            var handler = new TestHandler<TestMessageWithId>();
            var message = new TestMessageWithId(11);

            _bus.Subscribe(handler);

            await _bus.PublishAsync(message);
            await _bus.PublishAsync(message);
            await _bus.PublishAsync(message);

            Assert.True(handler.HandledMessages.ContainsN<TestMessageWithId>(3, mes => mes.Id == 11));
        }

        [Fact]
        public async Task multiple_messages_of_same_type_they_all_should_be_delivered()
        {
            var handler = new TestHandler<TestMessageWithId>();
            var message1 = new TestMessageWithId(1);
            var message2 = new TestMessageWithId(2);
            var message3 = new TestMessageWithId(3);

            _bus.Subscribe(handler);

            await _bus.PublishAsync(message1);
            await _bus.PublishAsync(message2);
            await _bus.PublishAsync(message3);

            Assert.True(handler.HandledMessages.ContainsSingle<TestMessageWithId>(mes => mes.Id == 1));
            Assert.True(handler.HandledMessages.ContainsSingle<TestMessageWithId>(mes => mes.Id == 2));
            Assert.True(handler.HandledMessages.ContainsSingle<TestMessageWithId>(mes => mes.Id == 3));
        }

        [Fact]
        public async Task message_of_child_type_then_all_subscribed_handlers_of_parent_type_should_handle_message()
        {
            var parentHandler = new TestHandler<ParentTestMessage>();
            _bus.Subscribe(parentHandler);

            await _bus.PublishAsync(new ChildTestMessage());

            Assert.True(parentHandler.HandledMessages.ContainsSingle<ChildTestMessage>());
        }

        [Fact]
        public async Task message_of_parent_type_then_no_subscribed_handlers_of_child_type_should_handle_message()
        {
            var childHandler = new TestHandler<ChildTestMessage>();
            _bus.Subscribe(childHandler);

            await _bus.PublishAsync(new ParentTestMessage());

            Assert.True(childHandler.HandledMessages.ContainsNo<ParentTestMessage>());
        }

        [Fact]
        public async Task message_of_grand_child_type_then_all_subscribed_handlers_of_base_types_should_handle_message()
        {
            var parentHandler = new TestHandler<ParentTestMessage>();
            var childHandler = new TestHandler<ChildTestMessage>();

            _bus.Subscribe(parentHandler);
            _bus.Subscribe(childHandler);

            await _bus.PublishAsync(new GrandChildTestMessage());

            Assert.True(parentHandler.HandledMessages.ContainsSingle<GrandChildTestMessage>() &&
                        childHandler.HandledMessages.ContainsSingle<GrandChildTestMessage>());
        }

        [Fact]
        public async Task message_of_grand_child_type_then_all_subscribed_handlers_of_parent_types_including_grand_child_handler_should_handle_message()
        {
            var parentHandler = new TestHandler<ParentTestMessage>();
            var childHandler = new TestHandler<ChildTestMessage>();
            var grandChildHandler = new TestHandler<GrandChildTestMessage>();

            _bus.Subscribe(parentHandler);
            _bus.Subscribe(childHandler);
            _bus.Subscribe(grandChildHandler);

            await _bus.PublishAsync(new GrandChildTestMessage());

            Assert.True(parentHandler.HandledMessages.ContainsSingle<GrandChildTestMessage>() &&
                        childHandler.HandledMessages.ContainsSingle<GrandChildTestMessage>() &&
                        grandChildHandler.HandledMessages.ContainsSingle<GrandChildTestMessage>());
        }
    }
}
