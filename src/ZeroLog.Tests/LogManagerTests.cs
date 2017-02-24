﻿using System;
using System.Collections.Generic;
using System.Linq;
using NFluent;
using NUnit.Framework;

namespace ZeroLog.Tests
{
    [TestFixture]
    public class LogManagerTests
    {
        private TestAppender _testAppender;

        [SetUp]
        public void SetUpFixture()
        {
            _testAppender = new TestAppender();
            LogManager.Initialize(new List<IAppender> { _testAppender }, 10);
        }

        [TearDown]
        public void Teardown()
        {
            LogManager.Shutdown();
        }

        [Test]
        public void should_create_log()
        {
            var log = LogManager.GetLogger(typeof(LogManagerTests));

            Check.That(log).IsNotNull();
        }

        [Test, ExpectedException]
        public void should_prevent_initialising_already_initialised_log_manager()
        {
            LogManager.Initialize(new IAppender[0]);
        }

        [Test]
        public void should_return_noop_log_event_when_no_more_log_event_are_available()
        {
            var log = LogManager.GetLogger(typeof(LogManagerTests));

            var actualLogEvents = new List<ILogEvent>();
            for (var i = 0; i < 10; i++)
            {
                actualLogEvents.Add(log.Debug());
            }

            var noopEvent = log.Debug();

            Check.That(actualLogEvents.OfType<LogEvent>().Count()).Equals(actualLogEvents.Count);
            Check.That(noopEvent).IsInstanceOf<NoopLogEvent>();

            var signal = _testAppender.SetMessageCountTarget(actualLogEvents.Count);

            for (var i = 0; i < actualLogEvents.Count; i++)
            {
                var actualLogEvent = actualLogEvents[i];
                actualLogEvent.Append(i).Log();
            }

            signal.Wait(TimeSpan.FromMilliseconds(100));

            Check.That(log.Debug()).IsInstanceOf<LogEvent>();
        }

        [Test]
        public void should_log_special_message_when_log_event_pool_is_exhausted()
        {
            var log = LogManager.GetLogger(typeof(LogManagerTests));

            var actualLogEvents = new List<ILogEvent>();
            for (var i = 0; i < 10; i++)
            {
                actualLogEvents.Add(log.Debug());
            }

            var signal = _testAppender.SetMessageCountTarget(actualLogEvents.Count);

            log.Debug().Append("this is not going to happen").Log();

            signal.Wait(TimeSpan.FromMilliseconds(100));

            Check.That(_testAppender.LoggedMessages.Last()).Contains("Log message skipped due to LogEvent pool exhaustion.");
        }

        [Test]
        public void should_not_throw_if_formatting_fails()
        {
            var log = LogManager.GetLogger(typeof(LogManagerTests));
            var signal = _testAppender.SetMessageCountTarget(1);

            var guid = Guid.NewGuid();
            log.InfoFormat("A good format: {0:X4}, A bad format: {1:lol}, Another good format: {2}", (short)-23805, guid, true);

            signal.Wait(TimeSpan.FromMilliseconds(100));

            var logMessage = _testAppender.LoggedMessages.Single();
            Check.That(logMessage).Equals("An error occured during formatting: \"A good format: {0:X4}, A bad format: {1:lol}, Another good format: {2}\", -23805, " + guid + ", True");
        }
    }
}
