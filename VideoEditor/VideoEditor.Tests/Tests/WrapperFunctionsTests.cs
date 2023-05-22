using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoEditor.Tests
{
    internal class WrapperFunctionsTests
    {
        [Test]
        public void RandomStringLength()
        {
            int length = 8;
            string randomString = Model.WrapperFunctions.RandomString(length);
            Assert.That(randomString, Has.Length.EqualTo(length));
        }

        [Test]
        public void RandomStringLengthBig()
        {
            int length = 20000;
            string randomString = Model.WrapperFunctions.RandomString(length);
            Assert.That(randomString, Has.Length.EqualTo(length));
        }

        [Test]
        public void RandomStringLengthBigger()
        {
            int length = 20000000;
            string randomString = Model.WrapperFunctions.RandomString(length);
            Assert.That(randomString, Has.Length.EqualTo(length));
        }

        [Test]
        public void RandomStringLengthBiggest()
        {
            int length = int.MaxValue;
            Assert.Throws<System.OutOfMemoryException>(() => Model.WrapperFunctions.RandomString(length));
        }

        [Test]
        public void RandomStringLengthSmall()
        {
            int length = -10;
            Assert.Throws<System.ArgumentOutOfRangeException>(() => Model.WrapperFunctions.RandomString(length));
        }

        [Test]
        public void RandomStringLengthSmaller()
        {
            int length = -1000000;
            Assert.Throws<System.ArgumentOutOfRangeException>(() => Model.WrapperFunctions.RandomString(length));
        }

        [Test]
        public void RandomStringLengthSmallest()
        {
            int length = int.MinValue;
            Assert.Throws<System.ArgumentOutOfRangeException>(() => Model.WrapperFunctions.RandomString(length));
        }

        [Test]
        public void HoursMinutesSecondsConverterSecond()
        {
            double lowerValue = 10.2;
            double uppwervalue = 20.8;
            var convertedTimeValues = Model.WrapperFunctions.HoursMinutesSecondsConverter(lowerValue, uppwervalue);
            Assert.Multiple(() =>
            {
                Assert.That(convertedTimeValues[0], Is.EqualTo((int)0));
                Assert.That(convertedTimeValues[1], Is.EqualTo((int)0));
                Assert.That(convertedTimeValues[2], Is.EqualTo((lowerValue).ToString()));
                Assert.That(convertedTimeValues[3], Is.EqualTo((int)0));
                Assert.That(convertedTimeValues[4], Is.EqualTo((int)0));
                Assert.That(convertedTimeValues[5], Is.EqualTo("10,6"));
            });
        }

        [Test]
        public void HoursMinutesSecondsConverterMinute()
        {
            double lowerValue = 61.2;
            double uppwervalue = 140.8;
            var convertedTimeValues = Model.WrapperFunctions.HoursMinutesSecondsConverter(lowerValue, uppwervalue);
            Assert.Multiple(() =>
            {
                Assert.That(convertedTimeValues[0], Is.EqualTo((int)0));
                Assert.That(convertedTimeValues[1], Is.EqualTo((int)1));
                Assert.That(convertedTimeValues[2], Is.EqualTo(("1,20")));
                Assert.That(convertedTimeValues[3], Is.EqualTo((int)0));
                Assert.That(convertedTimeValues[4], Is.EqualTo((int)1));
                Assert.That(convertedTimeValues[5], Is.EqualTo("19,6"));
            });
        }

        [Test]
        public void HoursMinutesSecondsConverterHour()
        {
            double lowerValue = 3666;
            double uppwervalue = 7740.1;
            var convertedTimeValues = Model.WrapperFunctions.HoursMinutesSecondsConverter(lowerValue, uppwervalue);
            Assert.Multiple(() =>
            {
                Assert.That(convertedTimeValues[0], Is.EqualTo((int)1));
                Assert.That(convertedTimeValues[1], Is.EqualTo((int)1));
                Assert.That(convertedTimeValues[2], Is.EqualTo(("6,00")));
                Assert.That(convertedTimeValues[3], Is.EqualTo((int)1));
                Assert.That(convertedTimeValues[4], Is.EqualTo((int)7));
                Assert.That(convertedTimeValues[5], Is.EqualTo("54,1"));
            });
        }
    }
}
