using System;

namespace Stencil
{
    public class Rule
    {
        private Rule() { }

        public string RuleName { get; private set; }
        public string RuleNameLowerCase { get; private set; }
        public string RuleNameLowerCaseWithBrackets { get; private set; }
        public string PropertyName { get; private set; }
        public string Stencil { get; private set; }
        public Func<object, string> Setup { get; private set; }

        public static Rule New(string ruleName, string stencil = "", string propertyName = "", Func<object, string> setup = null)
        {
            if (string.IsNullOrWhiteSpace(ruleName))
                throw new ArgumentException("The name of the rule is required.", nameof(ruleName));

            return new Rule
            {
                RuleName = ruleName,
                RuleNameLowerCase = ruleName.ToLower(),
                RuleNameLowerCaseWithBrackets = '{' + ruleName.ToLower() + '}',

                PropertyName = string.IsNullOrWhiteSpace(propertyName)
                    ? ruleName.ToLower()
                    : propertyName.ToLower(),

                Setup = setup,

                Stencil = string.IsNullOrWhiteSpace(stencil)
                    ? "{}"
                    : stencil
            };
        }

        public override string ToString()
        {
            return $"Rule:{{{RuleName}}}";
        }
    }

}
