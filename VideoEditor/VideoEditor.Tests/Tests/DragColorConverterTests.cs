using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoEditor.Tests
{
    internal class DragColorConverterTests
    {
        [Test]
        public void ConvertTrue()
        {
            VideoEditor.View.Converter.DragColorConverter converter = new();
            Assert.That(converter, Is.Not.Null);
            
            var result = converter.Convert(true, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);
      
            Assert.That(Xamarin.Forms.Color.LightGray, Is.EqualTo(result));
        }

        [Test]
        public void ConvertFalse()
        {
            VideoEditor.View.Converter.DragColorConverter converter = new();
            Assert.That(converter, Is.Not.Null);

            var result = converter.Convert(false, typeof(bool), null, System.Globalization.CultureInfo.InvariantCulture);

            Assert.That(Xamarin.Forms.Color.Azure, Is.EqualTo(result));
        }

        [Test]
        public void ConvertNull()
        {
            VideoEditor.View.Converter.DragColorConverter converter = new();
            Assert.That(converter, Is.Not.Null);

            var result = converter.Convert(null, typeof(bool?), null, System.Globalization.CultureInfo.InvariantCulture);
            Assert.That(Xamarin.Forms.Color.Azure, Is.EqualTo(result));
        }

        [Test]
        public void ConvertBackLightGray()
        {
            VideoEditor.View.Converter.DragColorConverter converter = new();
            Assert.That(converter, Is.Not.Null);
            converter.ConvertBack(Xamarin.Forms.Color.LightGray,
                typeof(Xamarin.Forms.Color), null, System.Globalization.CultureInfo.InvariantCulture);
        }

        [Test]
        public void ConvertBackAzure()
        {
            VideoEditor.View.Converter.DragColorConverter converter = new();
            Assert.That(converter, Is.Not.Null);
            converter.ConvertBack(Xamarin.Forms.Color.Azure,
                typeof(Xamarin.Forms.Color), null, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
