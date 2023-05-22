using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoEditor.Tests
{
    internal class LoggerTests
    {
        [Test]
        public void LoggerSingleton()
        {
            var logger1 = Model.Logger.Instance;
            var logger2 = Model.Logger.Instance;
            Assert.That(logger1, Is.EqualTo(logger2));
        }

        [Test]
        public void LoggerStringIsAsExpected()
        {
            var longtimestr = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            var message = "az élet szép";
            var appendedString = $"\n{longtimestr} - {message}";
            var logger = Model.Logger.Instance;
            Model.Logger.Instance.LogText =new StringBuilder(appendedString);
            logger.OnPropertyChanged(nameof(Model.Logger.Instance.LogText));

            Assert.That(logger.LogText.ToString(), Is.EqualTo(appendedString));
        }
    }
}
