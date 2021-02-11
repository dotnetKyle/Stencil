using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace Stencil
{
    public abstract class Rule<T> where T : class
    {
        public string RuleName { get; set; }
        public string RuleNameLowerCase { get; set; }
        public string RuleNameLowerCaseWithBrackets { get; set; }
        public string PropertyName { get; set; }
        public string Stencil { get; set; }
        public PropertyInfo PropertyInfo { get; set; }

        public abstract void ExecuteRule(T obj, StencilWriter writer);

        public override string ToString()
        {
            return $"Rule:{{{RuleName}}}";
        }
    }
    public sealed class Rule<T, TValue> : Rule<T> where T : class
    {
        public Rule() { }

        public Func<TValue, string> Setup { get; set; }
        public Expression<Func<T, TValue>> PropertyExpression { get; set; }

        public static Rule<T, TValue> New(Expression<Func<T, TValue>> propertyExpression)
        {
            var memberExpr = propertyExpression.Body as MemberExpression 
                ?? throw new ArgumentException("The property expression must be a Member");

            var propInfo = memberExpr.Member as PropertyInfo 
                ?? throw new ArgumentException("The property expression must be a Property");

            return new Rule<T, TValue>
            {
                PropertyInfo = propInfo
            };
        }

        public override void ExecuteRule(T obj, StencilWriter writer)
        {
            var propValue = (TValue)PropertyInfo.GetValue(obj);

            if (propValue == null)
                return;

            var processedValue = (Setup != null)
                ? Setup(propValue)
                : propValue.ToString();

            using(var reader = new StringReader(Stencil)) // use the stencil
            {
                while(true)
                {
                    var c = reader.Read();

                    if (c == -1)
                        break;

                    if(c == '{' && reader.Peek() == '}')
                    {
                        // Write the value
                        writer.Write(processedValue);
                        // then skip closing bracket
                        reader.Read();
                    }
                    else
                    {
                        writer.Write((char)c);
                    }
                }
            }
        }
    }
}
