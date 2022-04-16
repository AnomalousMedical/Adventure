using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Engine.Tests
{
    public class IntColorTests
    {
        //This is good for test data
        //https://bgrins.github.io/TinyColor/
        [Theory]
        [InlineData(255, 0, 0, 0, 100, 50)]
        [InlineData(0, 255, 0, 120, 100, 50)]
        [InlineData(0, 0, 255, 240, 100, 50)]
        public void FromHsl(byte r, byte g, byte b, float h, float s, float l)
        {
            var color = new IntColor(255, r, g, b);
            var converted = IntColor.FromHsl(h, s, l);
            Assert.Equal(color.ARGB, converted.ARGB);
        }
    }
}
