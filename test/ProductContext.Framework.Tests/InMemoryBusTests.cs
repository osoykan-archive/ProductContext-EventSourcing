using System;
using System.Threading.Tasks;
using FluentAssertions;
using ProductContext.Framework.Tests.Helpers;
using Xunit;

namespace ProductContext.Framework.Tests
{
    public class InMemoryBusTests
    {
        private readonly InMemoryBus _bus;

        public InMemoryBusTests()
        {
            _bus = new InMemoryBus();
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
        public async Task
            message_of_grand_child_type_then_all_subscribed_handlers_of_parent_types_including_grand_child_handler_should_handle_message()
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

        [Fact]
        public void null_as_handler_app_should_throw_arg_null_exception()
        {
            Assert.Throws<ArgumentNullException>(() => _bus.Subscribe((IHandle<TestMessage>) null));
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
        public async Task
            multiple_handlers_to_multiple_messages_then_each_handler_should_handle_only_subscribed_messages()
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

        [Fact]
        public void null_as_handler_app_should_throw()
        {
            Assert.Throws<ArgumentNullException>(() => _bus.Unsubscribe((IHandle<TestMessage>) null));
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
        public async Task
            handler_from_message_it_should_not_handle_this_message_anymore_and_still_handle_other_messages()
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