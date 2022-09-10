﻿using System;
using Xunit;
using Engine;
using Xunit.Abstractions;

namespace RpgMath.Tests
{
    public class LimitTests
    {

        private readonly ITestOutputHelper output;

        public LimitTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData(20)]
        [InlineData(100)]
        public void PhysicalDamageEqualLevel(long damage)
        {
            var calc = new DamageCalculator();
            var result = calc.LimitGain(Characters.level10, damage);
            output.WriteLine(result.ToString());
        }
    }
}
