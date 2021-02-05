using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Stencil
{
    public class TemplateEngine
    {
        public TemplateEngine()
        {
            _rules = new List<Rule>();
        }

        List<Rule> _rules;

        public string GetResult(string template, object o)
        {
            var sb = new StringBuilder();
            var props = o.GetType().GetProperties();

            var x = new List<char>();

            using(var reader = new StringReader(template))
            {
                while(true)
                {
                    //var charNum = reader.Read();
                    var c = reader.Read();

                    if (c == -1)
                        break;

                    if (c == '{')
                    {
                        // in property
                        var ruleName = "";
                        while(c != '}') // read until done
                        {
                            c = (char)reader.Read();
                            if(c == '}')
                            {
                                c = (char)reader.Read();
                                break;
                            }
                            ruleName += Char.ToLower((char)c);
                        }

                        var rule = _rules.FirstOrDefault(r => r.RuleNameLowerCase == ruleName);

                        if(rule != null)
                        {
                            var prop = props.FirstOrDefault(p => p.Name.ToLower() == rule.PropertyName);

                            var val = (rule.Setup != null)
                                ? rule.Setup(prop.GetValue(o))
                                : prop.GetValue(o).ToString();

                            for (int i = 0; i < rule.Stencil.Length; i++)
                            {
                                if (rule.Stencil[i] == '{')
                                {
                                    sb.Append(val);
                                    i++; // skip end bracket
                                }
                                else
                                {
                                    sb.Append(rule.Stencil[i]);
                                }
                            }
                        }
                    }
                    else
                    {
                        // add characters to the template
                        sb.Append(c);
                    }
                }
            }

            return sb.ToString();
        }

        public TemplateEngine AddRule(Rule rule)
        {
            _rules.Add(rule);

            return this;
        }

        public TemplateEngine AddRule(string name, string stencil = "", string propertyName = "", Func<object, string> setup = null)
            => AddRule(Rule.New(name, 
                stencil:stencil, 
                propertyName:propertyName, 
                setup:setup));
    }
}
