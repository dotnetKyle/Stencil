using NUnit.Framework;
using Stencil;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Stencil.Tests
{
    public class TemplateEngineTests
    {
        StencilEngineService<Person> engine;
        
        [SetUp]
        public void Setup()
        {
            engine = StencilEngineService
                .For<Person>()
                .AddStoredTemplateProperty(p => p.StoredTemplate)
                .AddDefaultTemplate("{LastName-WithComma} {FirstName} {MiddleName} {Business}")
                .AddBackupTemplate("No Name:({Id})")
                .Build();

            // Standard Rules
            engine.NewRuleFor(p => p.Id).Add();
            engine.NewRuleFor(p => p.FirstName).Add();
            engine.NewRuleFor(p => p.MiddleName).Add();
            engine.NewRuleFor(p => p.LastName).Add();

            // Additional Default Rules
            engine.NewRuleFor(p => p.Business)
                .AddStencil("({})")
                .Add();
            engine.NewRuleFor(p => p.LastName)
                .AddRuleName("LastName-WithComma")
                .AddStencil("{},")
                .Add();

            // Initials
            engine.NewRuleFor(p => p.FirstName)
                .AddRuleName("FirstInitial")
                .AddSetup(p => p.Substring(0, 1))
                .Add();
            engine.NewRuleFor(p => p.MiddleName)
                .AddRuleName("MiddleInitial")
                .AddSetup(p => p.Substring(0, 1))
                .Add();
            engine.NewRuleFor(p => p.MiddleName)
                .AddRuleName("MiddleInitial-WithPeriod")
                .AddSetup(p => p.Substring(0, 1))
                .AddStencil("{}.")
                .Add();

            // Other
            engine.NewRuleFor(p => p.Business)
                .AddRuleName("business-no-parenthesis")
                .Add();
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
            //var ms = new List<long>();
            var loopThisManyTimes = 100;

            // Act
            for (int i = 0; i < loopThisManyTimes; i++)
            {
                // First Name
                stopWatch.Start();
                engine.GetResult(new Person(firstname:"Bob"));
                stopWatch.Stop();
                ticks.Add(stopWatch.ElapsedTicks);
                //ms.Add(stopWatch.ElapsedMilliseconds);
                stopWatch.Reset();

                // Middle Name
                stopWatch.Start();
                engine.GetResult(new Person(middlename: "Howard"));
                stopWatch.Stop();
                ticks.Add(stopWatch.ElapsedTicks);
                //ms.Add(stopWatch.ElapsedMilliseconds);
                stopWatch.Reset();

                // Last Name
                stopWatch.Start();
                engine.GetResult(new Person(lastname: "Smith"));
                stopWatch.Stop();
                ticks.Add(stopWatch.ElapsedTicks);
                //ms.Add(stopWatch.ElapsedMilliseconds);
                stopWatch.Reset();

                // Business
                stopWatch.Start();
                engine.GetResult(new Person(business: "ABC Enterprises LLC"));
                stopWatch.Stop();
                ticks.Add(stopWatch.ElapsedTicks);
                //ms.Add(stopWatch.ElapsedMilliseconds);
                stopWatch.Reset();
            }

            var average = ticks.Average();
            Assert.Less(average, 100, $"Average time to run the template engine is too high: {average} ticks");
        }

        [Test]
        public void LFMB_ButWithNullNames()
        {
            // Arrange
            var person = new Person(
                firstname: "John",
                middlename: "Robert",
                lastname: "Smith",
                business: "XYZ Plumbing",
                storedTemplate:"{LastName-WithComma} {FirstName} {MiddleInitial-WithPeriod} {business}"
            );

            // Act
            var result = engine.GetResult(person);

            // Assert
            Assert.AreEqual("Smith, John R. (XYZ Plumbing)", result);
        }

        [Test]
        public void LFMB()
        {
            // Arrange
            var person = new Person(
                firstname: "John",
                middlename: "Robert",
                lastname: "Smith",
                business: "XYZ Plumbing",
                storedTemplate: "{LastName-WithComma} {firstName} {MiddleInitial-WithPeriod} {business}"
            );

            // Act
            var result = engine.GetResult(person);

            Assert.AreEqual("Smith, John R. (XYZ Plumbing)", result);
        }

        [Test]
        public void FirstName()
        {
            // Arrange
            var person = new Person(
                firstname: "Bob"
            );

            // Act
            var result = engine.GetResult(person);

            Assert.AreEqual("Bob", result);
        }
        [Test]
        public void MiddleName()
        {
            // Arrange
            var person = new Person(
                middlename: "Ronald"
            );

            // Act
            var result = engine.GetResult(person);

            Assert.AreEqual("Ronald", result);
        }
        [Test]
        public void LastName()
        {
            // Arrange
            var person = new Person(
                lastname: "Smith"
            );

            // Act
            var result = engine.GetResult(person);

            Assert.AreEqual("Smith", result);
        }
        [Test]
        public void Business()
        {
            // Arrange
            var person = new Person(
                business: "ABC Construction LLC"
            );

            // Act
            var result = engine.GetResult(person);

            Assert.AreEqual("(ABC Construction LLC)", result);
        }

        [Test]
        public void MiddleInitial()
        {
            // Arrange
            var person = new Person(
                middlename: "Howard",
                storedTemplate: "{MiddleInitial}"
            );

            // Act
            var result = engine.GetResult(person);

            Assert.AreEqual("H", result);
        }

        [Test]
        public void Test_BackupTemplate()
        {
            // Arrange
            var id = System.Guid.NewGuid();
            var person = new Person(id:id);

            // Act
            var result = engine.GetResult(person);

            Assert.AreEqual($"No Name:({id})", result, "The backup template was not used.");
        }
        [Test]
        public void IfRuleNotFound_RemoveText()
        {
            // Arrange
            var person = new Person(
                firstname:"John",
                middlename: "Howard",
                lastname: "Smith",
                storedTemplate:"{lastName} {ruleNotFound}"
            );

            // Act
            var result = engine.GetResult(person);

            Assert.IsFalse(result.Contains("ruleNotFound"), "The not found rule was not removed");
        }

        [Test]
        public void NullObject()
        {
            // Act
            var result = engine.GetResult(null);

            Assert.AreEqual("", result, "The result should have been an empty string");
        }
    }
}