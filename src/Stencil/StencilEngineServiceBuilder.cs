using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Stencil
{
    public static class StencilEngineService
    { 
        public static StencilEngineServiceBuilder<TClass> For<TClass>()
            where TClass : class
        {
            return new StencilEngineServiceBuilder<TClass>();
        }
    }

    public class StencilEngineServiceBuilder<T> where T : class
    {
        internal StencilEngineServiceBuilder() { }

        string defaultTemplate;
        string backupTemplate;
        PropertyInfo storedTemplateProperty;

        public StencilEngineServiceBuilder<T> AddStoredTemplateProperty(Expression<Func<T, string>> propertyExpression) 
        {
            var memberExpr = propertyExpression.Body as MemberExpression
                ?? throw new ArgumentException("The property expression must be a Member");

            storedTemplateProperty = memberExpr.Member as PropertyInfo
                ?? throw new ArgumentException("The property expression must be a Property");

            return this;
        }
        public StencilEngineServiceBuilder<T> AddDefaultTemplate(string template)
        {
            if (string.IsNullOrWhiteSpace(template))
                throw new ArgumentException("Default template cannot be empty.", nameof(template));

            defaultTemplate = template;
            return this;
        }
        public StencilEngineServiceBuilder<T> AddBackupTemplate(string template)
        {
            if (string.IsNullOrWhiteSpace(template))
                throw new ArgumentException("Backup template cannot be empty.", nameof(template));

            backupTemplate = template;
            return this;
        }

        public StencilEngineService<T> Build()
        {
            if (storedTemplateProperty == null)
                throw new ArgumentNullException("The Stored Template property is required.");

            if (defaultTemplate == null)
                throw new ArgumentNullException("The Default Template property is required.");

            return StencilEngineService<T>.New<T>(
                storedTemplateProperty, 
                defaultTemplate, 
                backupTemplate
            );
        }
    }
}
