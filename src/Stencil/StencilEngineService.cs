using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Stencil
{
    public class StencilEngineService<T> where T : class
    {
        StencilWriter stencilWriter = new StencilWriter();

        StencilEngineService()
        {
            _rules = new List<Rule2<T>>();
            _properties = typeof(T).GetProperties();
        }

        internal static StencilEngineService<TClass> New<TClass>(
            PropertyInfo StoredTemplateProperty,
            string defaultTemplate,
            string backupTemplate = null
            ) 
            where TClass : class
        {
            return new StencilEngineService<TClass>
            {
                DefaultTemplate = defaultTemplate,
                BackupTemplate = backupTemplate,
                StoredTemplateProperty = StoredTemplateProperty
            };
        }

        List<Rule2<T>> _rules;
        PropertyInfo[] _properties;

        /// <summary>
        /// The default template to use if none is available
        /// </summary>
        public string DefaultTemplate { get; private set; }
        /// <summary>
        /// The backup template to use if the default template results in an empty string
        /// <para>For example: If it's a person's name, if the name ends up being blank, you could say "Name Unknown" or even provide an Id</para>
        /// </summary>
        public string BackupTemplate { get; private set; }
        /// <summary>
        /// This represents the property on the class where a stored template is if the user has chosen to not use the default template for their object
        /// </summary>
        public PropertyInfo StoredTemplateProperty { get; private set; }

        public string GetResult(T obj)
        {
            var template = (string)StoredTemplateProperty.GetValue(obj);
            if (string.IsNullOrWhiteSpace(template))
                template = DefaultTemplate;

            var result = getResultFromTemplate(obj, template);

            if(string.IsNullOrWhiteSpace(result))
                result = getResultFromTemplate(obj, BackupTemplate);

            result = result.TrimEnd(',');

            return result;
        }

        string getResultFromTemplate(T obj, string template)
        {
            stencilWriter.Reset();
            using (var reader = new StringReader(template))
            //using (var writer = new StringWriter())
            {
                while (true)
                {
                    var c = reader.Read();

                    if (c == -1)
                        break;

                    if (c == '{')
                    {
                        // in property
                        var ruleName = getRuleName(reader);

                        var rule = _rules.FirstOrDefault(r => r.RuleNameLowerCase == ruleName);

                        if (rule != null)
                        {
                            var prop = _properties.FirstOrDefault(p => p.Name.ToLower() == rule.PropertyName);

                            rule.ExecuteRule(obj, stencilWriter);
                        }
                    }
                    else
                    {
                        // add characters to the template
                        stencilWriter.Write((char)c);
                    }
                }

                return stencilWriter.ToString();
            }
        }

        string getRuleName(StringReader reader)
        {
            string ruleName = "";
            while (true) // read until done
            {
                var c = reader.Read();

                if (c == '}' || c == -1)
                    break;

                //if(char.IsLower((char)c) == false)

                ruleName += char.ToLower((char)c);
            }
            return ruleName;
        }
        /// <summary>
        /// Add a rule to the engine
        /// </summary>
        internal void AddRule(Rule2<T> rule)
        {
            if (rule == null)
                throw new ArgumentException("Rule cannot be null",  nameof(rule));

            _rules.Add(rule);
        }

        /// <summary>
        /// Test if the engine has a rule with a certain name
        /// </summary>
        /// <param name="rulename">The rule name without brackets</param>
        internal bool HasRule(string rulename)
            => _rules.Any(r => r.RuleNameLowerCase == rulename.ToLower());
    }
}
