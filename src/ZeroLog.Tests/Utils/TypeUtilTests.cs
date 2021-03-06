﻿using System;
using NFluent;
using NUnit.Framework;
using ZeroLog.Utils;

namespace ZeroLog.Tests.Utils
{
    public class TypeUtilTests
    {
        [Test]
        public void should_round_trip_enum()
        {
            var typeHandle = TypeUtil<DayOfWeek>.TypeHandle;
            var type = TypeUtil.GetTypeFromHandle(typeHandle);

            Check.That(type).IsEqualTo(typeof(DayOfWeek));
        }
    }
}
