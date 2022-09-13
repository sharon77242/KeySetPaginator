using System.Linq.Expressions;
using System.Reflection;

namespace KeySetPaginator
{
    public static class Utils
    {
        public static bool GreaterThan(string s1, string s2)
        {
            return s1.CompareTo(s2) > 0;
        }

        public static bool SmallerThan(string s1, string s2)
        {
            return s1.CompareTo(s2) < 0;
        }

        public static bool Equal(string s1, string s2)
        {
            return s1.CompareTo(s2) == 0;
        }

        public static readonly MethodInfo StringGreaterThanMethod = typeof(Utils).GetMethod("GreaterThan", new[] { typeof(string), typeof(string) });
        public static readonly MethodInfo StringSmallerThanMethod = typeof(Utils).GetMethod("SmallerThan", new[] { typeof(string), typeof(string) });
        public static readonly MethodInfo StringEqualThanMethod = typeof(Utils).GetMethod("Equal", new[] { typeof(string), typeof(string) });

        public static bool IsNullableType(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);


        public static Expression<Func<QueryType, object>> FieldGetterFromProperty<QueryType>(Type type, PropertyInfo prop)
        {
            // x =>
            ParameterExpression parameter = Expression.Parameter(type, "x");
            // x => x.SomeId
            MemberExpression property = Expression.Property(parameter, prop);
            // convert to an object type
            Expression conversion = Expression.Convert(property, typeof(object));

            return Expression.Lambda<Func<QueryType, object>>(conversion, parameter);
        }

        public static MemberExpression GetPropertyMemberExpression(Type queryType, ParameterExpression parameter, string keySetFieldName)
        {
            PropertyInfo prop = queryType.GetProperty(keySetFieldName);

            if (prop == null)
                throw new ArgumentException($"There is no such field {keySetFieldName}");

            // a => a.Property
            MemberExpression property = Expression.Property(parameter, prop);
            return property;
        }

        public static (UnaryExpression unaryExpression, bool valueNull) GetKeySetTokenValueExpression<KeySetTokenType>(KeySetTokenType keySetToken, string keySetField, PropertyInfo keySetProp)
        {
            var value = keySetProp.GetValue(keySetToken, null);

            if (value == null)
                return (Expression.Convert(Expression.Constant(value), typeof(object)), true);

            var type = value.GetType();

            var propertyInfo = type.GetProperty("Value");

            try
            {
                value = propertyInfo.GetValue(value, null);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Field with name {keySetField} must be from type KeySetTokenValue, and not from type {type}", e);
            }

            if (value == null)
                return (null, true);

            if (IsNullableType(value.GetType()))
            {
                throw new ArgumentException($"Token does not support nullable type for field {keySetField}");
            }

            return (Expression.Convert(Expression.Constant(value), value.GetType()), false);
        }

        public static bool EmptyToken<KeySetTokenType>(this KeySetTokenType keySetToken)
            where KeySetTokenType : KeySetToken
        {
            PropertyInfo[] properties = typeof(KeySetTokenType).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.Name != "DefaultFields")
                {
                    // Check if field initialized in keySetToken
                    var field = property.GetValue(keySetToken, null);
                    if (field != null)
                        return false;
                }
            }

            return true;
        }

        public static List<string> GetKeySetFields<KeySetTokenType>(this KeySetTokenType keySetToken)
            where KeySetTokenType : KeySetToken
        {
            PropertyInfo[] properties = typeof(KeySetTokenType).GetProperties();

            var keySetFields = new HashSet<string>();
            var defaultFields = new List<string>();

            foreach (PropertyInfo property in properties)
            {
                if (property.Name != "DefaultFields")
                {
                    // Check if field initialized in keySetToken so we can add it to the list of key set fields
                    var field = property.GetValue(keySetToken, null);
                    if (field != null)
                        keySetFields.Add(property.Name);
                }
                else
                    defaultFields = property.GetValue(keySetToken, null) as List<string>;
            }

            keySetFields.UnionWith(defaultFields);

            return keySetFields.ToList();
        }
    }
}
