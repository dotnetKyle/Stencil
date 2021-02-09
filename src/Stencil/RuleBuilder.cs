using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Stencil
{
    public static class RuleBuildingExtensions
    {
        public static RuleBuilder<T, TValue> NewRuleFor<T, TValue>(this StencilEngineService<T> engine, Expression<Func<T, TValue>> propertyExpression)
            where T : class
        {
            return new RuleBuilder<T, TValue>(engine, propertyExpression);
        }
    }

    public class RuleBuilder<T, TValue> where T : class
    {
        private StencilEngineService<T> _service;

        public RuleBuilder(StencilEngineService<T> service, Expression<Func<T, TValue>> propertyExpression)
        {
            _service = service;

            PropertyExpression = propertyExpression;

            var memberExpr = propertyExpression.Body as MemberExpression
                ?? throw new ArgumentException("The property expression must be a Member");

            propInfo = memberExpr.Member as PropertyInfo
                ?? throw new ArgumentException("The property expression must be a Property");
        }

        private PropertyInfo propInfo;

        public Expression<Func<T, TValue>> PropertyExpression { get; private set; }

        public string RuleName { get; private set; }
        public string Stencil { get; set; } = "{}";
        public Func<TValue, string> Setup { get; set; }

        public RuleBuilder<T, TValue> AddRuleName(string ruleName)
        {
            if (string.IsNullOrWhiteSpace(ruleName))
                throw new ArgumentException($"{nameof(ruleName)} cannot be blank.");

            RuleName = ruleName;
            return this;
        }

        public RuleBuilder<T, TValue> AddStencil(string stencil)
        {
            if (stencil.Contains("{}") == false)
                throw new ArgumentException("stencil must contain brackets \"{}\" to indicate where the final value goes in the stencil.");

            Stencil = stencil;
            return this;
        }

        public RuleBuilder<T, TValue> AddSetup(Func<TValue, string> setup)
        {
            Setup = setup;
            return this;
        }

        public void Add()
        {
            if (RuleName == null)
                RuleName = propInfo.Name;

            if (_service.HasRule(RuleName))
                throw new Exception($"Duplicate rule name: {{{RuleName}}}");

            _service.AddRule(
                new Rule2<T, TValue>
                {
                    PropertyExpression = PropertyExpression,
                    PropertyInfo = propInfo,
                    RuleName = RuleName,
                    RuleNameLowerCase = RuleName.ToLower(),
                    RuleNameLowerCaseWithBrackets = '{' + RuleName + '}', 
                    Setup = Setup, 
                    Stencil = Stencil
                }
            );
        }
    }
}
