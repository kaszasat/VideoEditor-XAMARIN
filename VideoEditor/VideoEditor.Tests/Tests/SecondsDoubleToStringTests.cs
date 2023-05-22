using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoEditor.Tests.Tests
{
    internal class SecondsDoubleToStringTests
    {
        [Test]
        public void ConvertSeconds()
        {
            VideoEditor.View.Converter.SecondsDoubleToString converter = new();
            Assert.That(converter, Is.Not.Null);

            var result = converter.Convert(1.4, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture);
            Assert.That(result.ToString(), Is.EqualTo("00:00:01.4000"));
        }

        [Test]
        public void ConvertMinutes()
        {
            VideoEditor.View.Converter.SecondsDoubleToString converter = new();
            Assert.That(converter, Is.Not.Null);

            var result = converter.Convert(71.4, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture);
            Assert.That(result.ToString(), Is.EqualTo("00:01:11.4000"));
        }

        [Test]
        public void ConvertHours()
        {
            VideoEditor.View.Converter.SecondsDoubleToString converter = new();
            Assert.That(converter, Is.Not.Null);

            var result = converter.Convert(3671.4, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
            Assert.That(result.ToString(), Is.EqualTo("01:01:11.4000"));
        }

        [Test]
        public void ConvertHoursLong()
        {
            VideoEditor.View.Converter.SecondsDoubleToString converter = new();
            Assert.That(converter, Is.Not.Null);

            var result = converter.Convert(33671.4, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture);
            Assert.That(result.ToString(), Is.EqualTo("09:21:11.4000"));
        }

        [Test]
        public void ConvertHoursLonger()
        {
            VideoEditor.View.Converter.SecondsDoubleToString converter = new();
            Assert.That(converter, Is.Not.Null);

            var result = converter.Convert(3333671.4, typeof(string), null, System.Globalization.CultureInfo.InvariantCulture);
            Assert.That(result.ToString(), Is.EqualTo("926:01:11.4000"));
        }

        [Test]
        public void ConvertBackSeconds()
        {
            VideoEditor.View.Converter.SecondsDoubleToString converter = new();
            Assert.That(converter, Is.Not.Null);

            var result = converter.ConvertBack("00:00:01.4000", typeof(double), null, System.Globalization.CultureInfo.InvariantCulture);
            Assert.That(result,Is.EqualTo((double)1.4));
        }

        [Test]
        public void ConvertBackMinutes()
        {
            VideoEditor.View.Converter.SecondsDoubleToString converter = new();
            Assert.That(converter, Is.Not.Null);

            var result = converter.ConvertBack("00:10:01.4000", typeof(double), null, System.Globalization.CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo((double)601.4));
        }

        [Test]
        public void ConvertBackHours()
        {
            VideoEditor.View.Converter.SecondsDoubleToString converter = new();
            Assert.That(converter, Is.Not.Null);

            var result = converter.ConvertBack("05:10:01.4000", typeof(double), null, System.Globalization.CultureInfo.InvariantCulture);
            Assert.That(result, Is.EqualTo((double)18601.4));
        }
    }
}
