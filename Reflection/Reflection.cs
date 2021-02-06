using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ParrotLucene
{
    internal class Reflection
    {
        /// <summary>
        /// Read the custom attribute of a type
        /// </summary>
        /// <typeparam name="T">The returned custom attribute</typeparam>
        /// <param name="type">The type with the custom attribute</param>
        /// <returns></returns>
        public static T CustomAttributeInfo<T>(Type pType) where T : Attribute
        {
            var attribute = pType.GetTypeInfo().GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
            return attribute;
        }

        /// <summary>
        /// Read a property custom attribute
        /// </summary>
        /// <typeparam name="T">The returned custom attribute</typeparam>
        /// <param name="prop">The property that has the custom attribute</param>
        /// <returns></returns>
        public static T CustomAttributeInfo<T>(PropertyInfo prop) where T : Attribute
        {
            var attribute = prop.GetCustomAttributes(typeof(T), true).FirstOrDefault() as T;
            return attribute;
        }

        /// <summary>
        /// Returns an array of custom attributes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static T[] CustomAttributeInfos<T>(PropertyInfo prop) where T : Attribute
        {
            var attributes = prop.GetCustomAttributes(typeof(T), true) as T[];
            return attributes.ToArray();
        }

        /// <summary>
        /// Generate the expression prop=>prop.pPropertyName
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="pPropertyName">The property name</param>
        /// <returns>The expression prop=>prop.[pPropertyName] </returns>
        public static Expression<Func<T, object>> PropertyExpression<T>(string pPropertyName)
        {
            var parameter = Expression.Parameter(typeof(T));
#if NETSTANDARD1_5 || NETSTANDARD1_6
            PropertyInfo prop = typeof(T).GetTypeInfo().GetProperty(pPropertyName);
#else
            PropertyInfo prop = typeof(T).GetProperty(pPropertyName);
#endif
            var property = Expression.Property(parameter, prop);
            var conversion = Expression.Convert(property, typeof(object));
            var lambda = Expression.Lambda<Func<T, object>>(conversion, parameter);
            return lambda;
        }

        /// <summary>
        /// Using a member Expression like x=>x.Property get x.Property value of the entity
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="idField">Expression</param>
        /// <param name="entity">Entity that we want to get the value using the expression</param>
        /// <returns></returns>
        public static object ObjectValue<T>(Expression<Func<T, object>> expression, T entity)
        {
            var memberExp = GetMemberInfo(expression);
            var propertyInfo = memberExp.Member as PropertyInfo;
            var item = propertyInfo.GetValue(entity, null);
            return item;
        }
        /// <summary>
        /// Using a known property name returns the value of the property
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="fieldName">Property name</param>
        /// <param name="entity">Entity that we want to get the value using the expression</param>
        /// <returns></returns>
        public static object ObjectValue<T>(string fieldName, T entity)
        {
            if (entity is System.Dynamic.ExpandoObject)
            {
                var props = ((IDictionary<string, object>)entity);
                if (props.ContainsKey(fieldName))
                {
                    return props[fieldName];
                }
                return null;
            }
#if NETSTANDARD1_5 || NETSTANDARD1_6
            var propertyInfo = typeof(T).GetTypeInfo().GetProperty(fieldName);
#else
            var propertyInfo = typeof(T).GetProperty(fieldName);
#endif
            if (propertyInfo == null)
            {
                return null;
            }
            else
            {
                var item = propertyInfo.GetValue(entity, null);
                return item;
            }
        }

        /// <summary>
        /// Returns the Member Expression of a method lamda expression
        /// </summary>
        /// <param name="method">the method expression</param>
        /// <returns>MemberExpression or null if the expression is invalid</returns>
        public static MemberExpression GetMemberInfo(Expression method)
        {
            if (method is LambdaExpression lambda)
            {

                MemberExpression memberExpr = null;

                if (lambda.Body.NodeType == ExpressionType.Convert)
                {
                    memberExpr =
                        ((UnaryExpression)lambda.Body).Operand as MemberExpression;
                }
                else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
                {
                    memberExpr = lambda.Body as MemberExpression;
                }

                return memberExpr;
            }
            return null;
        }
        private static string GetName(Expression e, out Expression parent)
        {
            if (e is MemberExpression m)
            { //property or field           
                parent = m.Expression;
                return m.Member.Name;
            }
            else if (e is MethodCallExpression mc)
            {
                string args = string.Join(",", mc.Arguments.SelectMany(GetExpressionParts));
                if (mc.Method.IsSpecialName)
                { //for indexers, not sure this is a safe check...           
                    return string.Format("{0}[{1}]", GetName(mc.Object, out parent), args);
                }
                else
                { //other method calls      
                    parent = mc.Object;
                    return string.Format("{0}({1})", mc.Method.Name, args);
                }
            }
            else if (e is ConstantExpression c)
            { //constant value
                parent = null;
                return c.Value?.ToString() ?? "null";
            }
            else if (e is UnaryExpression u)
            { //convert
                parent = u.Operand;
                return null;
            }
            else
            {
                parent = null;
                return e.ToString();
            }
        }
        private static string GetCleanName(Expression e, out Expression parent)
        {
            if (e is MemberExpression m)
            { //property or field           
                parent = m.Expression;
                return m.Member.Name;
            }
            else if (e is MethodCallExpression mc)
            {
                if (mc.Method.IsSpecialName)
                { //for indexers, not sure this is a safe check...           
                    return string.Format("{0}", GetCleanName(mc.Object, out parent));
                }
                else
                { //other method calls      
                    parent = mc.Object;
                    return string.Format("{0}", mc.Method.Name);
                }
            }
            else if (e is ConstantExpression c)
            { //constant value
                parent = null;
                return c.Value?.ToString() ?? "null";
            }
            else if (e is UnaryExpression u)
            { //convert
                parent = u.Operand;
                return null;
            }
            else
            {
                parent = null;
                return e.ToString();
            }
        }
        private static IEnumerable<string> GetExpressionParts(Expression e)
        {
            var list = new List<string>();
            while (e != null && !(e is ParameterExpression))
            {
                var name = GetName(e, out e);
                if (name != null) list.Add(name);
            }
            list.Reverse();
            return list;
        }
        private static IEnumerable<string> GetCleanExpressionParts(Expression e)
        {
            var list = new List<string>();
            while (e != null && !(e is ParameterExpression))
            {
                var name = GetCleanName(e, out e);
                if (name != null) list.Add(name);
            }
            list.Reverse();
            return list;
        }
        public static string GetExpressionText<T>(Expression<Func<T, object>> expression) => string.Join(".", GetExpressionParts(expression.Body));
        public static string GetExpressionPath<T>(Expression<Func<T, object>> expression) => string.Join(".", GetCleanExpressionParts(expression.Body));
    }

}
