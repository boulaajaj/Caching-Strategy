using Microsoft.Practices.Unity.InterceptionExtension;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Amibou.Infrastructure.Caching;


namespace Amibou.Infrastructure.Utilities
{
    public class ReflectionHelpers
    {
        public static ReflectionHelpers Instance => Containers.Container.Get<ReflectionHelpers>();

        [Cache(CacheType = CacheType.Memory, ChangeTrackingToken = "System.Reflection.Assembly")]
        public virtual Type GetTypeFromAppDomain(string typeName)
        {
            var regex = new Regex(GetTypeNameSearchPattern(typeName));

            var typeResultsList = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(
                    t => t.FullName != null && regex.IsMatch(t.FullName)
                 ).ToList();

            if (typeResultsList.Count == 1) return typeResultsList.First();

            if (typeResultsList.Count > 1)
                throw new Exception(
                    "Ambiguous type Name within current AppDomain. Please provide a Fully Qualified Type Name instead.");

            throw new Exception(
                "Invalid Type Name.");
        }

        public static string GetTypeNameSearchPattern(string typeName)
            => typeName.Contains('.')
                ? @".*" + typeName + "$"
                : @".*\." + typeName + "$";

        public static string GetTypeFullName<T>(Expression<Func<T>> property)
        {
            var expression = GetMemberInfo(property);
            return expression.DeclaringType != null
                ? expression.DeclaringType.FullName
                : null;
        }

        public static MemberInfo GetMemberInfo(Expression method)
        {
            var lambda = method as LambdaExpression;
            if (lambda == null)
                throw new ArgumentNullException(nameof(method));

            MemberExpression memberExpr = null;

            switch (lambda.Body.NodeType)
            {
                case ExpressionType.Convert:
                    memberExpr = ((UnaryExpression)lambda.Body).Operand as MemberExpression;
                    break;
                case ExpressionType.MemberAccess:
                    memberExpr = lambda.Body as MemberExpression;
                    break;
            }

            if (memberExpr == null)
                throw new ArgumentException("Invalid member expression");

            return memberExpr.Member;
        }

        public static IntPtr GetMemberValue(MemberInfo memberInfo)
        {
            var reflectedType = memberInfo.ReflectedType;

            return reflectedType == null
                ? new IntPtr() 
                : reflectedType.TypeHandle.Value;
        }

        public static object GetValueFromMethodParameterExpression(IMethodInvocation method, string parameterExpression)
        {
            var parameterExpressionArray = parameterExpression.Split('.');
            var parameterIndex = method.Inputs.GetParameterInfo(parameterExpressionArray[0]).Position;
            var parameterValue = method.Inputs[parameterIndex];

            if (parameterExpressionArray.Length == 1) return parameterValue;

            for (var i = 1; i < parameterExpressionArray.Length; i++)
            {
                if (parameterValue == null) return null;

                parameterValue = parameterValue
                    .GetType()
                    .GetProperty(parameterExpressionArray[i])
                    ?.GetValue(parameterValue);
            }

            return parameterValue;
        }
    }
}