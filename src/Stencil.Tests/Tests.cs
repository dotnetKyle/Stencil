using NUnit.Framework;
using Stencil;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Stencil.Tests
{
    public class Tests
    {
        TemplateEngine engine;

        [SetUp]
        public void Setup()
        {
            engine = new TemplateEngine()
                .AddRule("FirstName")
                .AddRule("FirstInitial", setup:(o) => ((string)o).Substring(0, 1))
                .AddRule("MiddleName")
                .AddRule("LastName")
                .AddRule("LastName-WithComma", propertyName:"LastName", stencil:"{},")
                .AddRule("Business", stencil:"({})")
                .AddRule("MiddleInitial-WithPeriod", propertyName:"middleName", setup:(o) => ((string)o).Substring(0,1), stencil:"{}.")
                .AddRule("MiddleInitial", propertyName:"middleName", setup: (o) => ((string)o).Substring(0, 1))
                ;
        }

        /*
         * Support Rules:
         * Rule Name: {LastName-WithComma} or {Business-WithParenthesis} or {MiddleInitial-WithPeriod}
         * Support a Required option
         * Replace
         * Replace and Add Text
         */

        [Test]
        public void AverageTime()
        {
            // Arrange
            var stopWatch = new Stopwatch();
            var ticks = new List<long>();
            var loopThisManyTimes = 100;

            // Act
            for (int i = 0; i < loopThisManyTimes; i++)
            {
                // First Name
                stopWatch.Start();
                engine.GetResult("{firstName}", new { firstName = "Bob" });
                stopWatch.Stop();
                ticks.Add((long)stopWatch.ElapsedTicks);
                stopWatch.Reset();

                // Middle Name
                stopWatch.Start();
                engine.GetResult("{middleName}", new { middleName = "Howard" });
                stopWatch.Stop();
                ticks.Add((long)stopWatch.ElapsedTicks);
                stopWatch.Reset();

                // Last Name
                stopWatch.Start();
                engine.GetResult("{lastName}", new { lastName = "Smith" });
                stopWatch.Stop();
                ticks.Add((long)stopWatch.ElapsedTicks);
                stopWatch.Reset();

                // Business
                stopWatch.Start();
                engine.GetResult("{business}", new { business = "ABC Enterprises LLC" });
                stopWatch.Stop();
                ticks.Add((long)stopWatch.ElapsedTicks);
                stopWatch.Reset();
            }

            var average = ticks.Average();
            Assert.Less(average, 50, $"Average time to run the template engine is too high: {average} ticks");
        }

        [Test]
        public void FirstName()
        {

            // Act
            var result = engine.GetResult("{firstName}",
                new { firstName = "Bob" });

            Assert.AreEqual("Bob", result);
        }
        [Test]
        public void MiddleName()
        {
            // Act
            var result = engine.GetResult("{middleName}",
                new { MiddleName = "Ronald" });

            Assert.AreEqual("Ronald", result);
        }
        [Test]
        public void LastName()
        {
            // Act
            var result = engine.GetResult("{lastName}",
                new { lastName = "Bob" });

            Assert.AreEqual("Bob", result);
        }
        [Test]
        public void Business()
        {
            // Act
            var result = engine.GetResult("{business}",
                new { business = "ABC Construction LLC" });

            Assert.AreEqual("(ABC Construction LLC)", result);
        }

        [Test]
        public void MiddleInitial()
        {
            // Act
            var result = engine.GetResult("{middleInitial}", 
                new { middleName = "Howard"});

            Assert.AreEqual("H", result);
        }

        [Test]
        public void IfRuleNotFound_RemoveText()
        {
            // Act
            var result = engine.GetResult("{lastName} {ruleNotFound}",
                new { firstName = "John", middleName = "Howard", lastName="Smith" });

            Assert.IsFalse(result.Contains("ruleNotFound"), "The not found rule was not removed");
        }
    }
}