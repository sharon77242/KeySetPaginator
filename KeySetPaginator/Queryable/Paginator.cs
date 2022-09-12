using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace KeySetPaginator.Queryable
{
    /// <summary>
    /// Extension class for doing paging actions using expressions
    /// </summary>
    public static class Paginator
    {
        //TODO: add a validation that every field in the token expect of default fields are from type KeySetTokenValue



        /// <summary>
        /// On a Query build key set skipping according to keySetToken and sortDirection
        /// Inside the method we build an expression that represents
        /// </summary>
        /// <typeparam name="QueryType"></typeparam>
        /// <typeparam name="KeySetTokenType"></typeparam>
        /// <param name="query"></param>
        /// <param name="keySetToken"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static IQueryable<QueryType> KeySetSkip<QueryType, KeySetTokenType>(
        this IQueryable<QueryType> query,
        KeySetTokenType keySetToken,
        SortDirectionDTO sortDirection)
        where KeySetTokenType : KeySetToken
        {
            List<string> keySetFields = Utils.GetKeySetFields(keySetToken, false);
            if (keySetFields.Count == 0)
                return query;

            Type keySetType = typeof(KeySetTokenType);
            Type queryType = typeof(QueryType);

            Expression<Func<QueryType, bool>> previousEqualExpression = _ => true;
            Expression<Func<QueryType, bool>> expressions = _ => false;
            // e0 =>
            ParameterExpression parameter = Expression.Parameter(queryType, "e0");

            foreach (var keySetFieldName in keySetFields)
            {
                PropertyInfo keySetProp = keySetType.GetProperty(keySetFieldName);

                if (keySetProp == null)
                    throw new ArgumentException($"There is no such field {keySetFieldName}");

                // This is a expression represnts the current keyset initalize field from the request
                UnaryExpression valueExpression = Utils.GetKeySetTokenValueExpression(keySetToken, keySetFieldName, keySetProp);

                // This is a expression represents the current member from queryble according to keySet current field name
                MemberExpression property = Utils.GetPropertyMemberExpression(queryType, parameter, keySetFieldName);
                BinaryExpression equalExpression;

                // string has customized GreaterThan, SmallerThan, Equal methods above
                if (typeof(KeySetTokenValue<string>).IsAssignableFrom(keySetProp.PropertyType))
                {
                    // SortDirectionDTO.asc ? e0.Property > value : e0.Property < value
                    BinaryExpression greaterOrSmallerExpressionBinary = sortDirection == SortDirectionDTO.asc ?
                        Expression.GreaterThan(property, valueExpression, false, Utils.StringGreaterThanMethod) :
                        Expression.LessThan(property, valueExpression, false, Utils.StringSmallerThanMethod);
                    var greaterOrSmallerExpression = Expression.Lambda<Func<QueryType, bool>>(greaterOrSmallerExpressionBinary, parameter);

                    var currentExpression = Expression.OrElse(expressions.Body,
                                Expression.AndAlso(previousEqualExpression.Body, greaterOrSmallerExpression.Body));

                    // old params expression (if exists, first param will have non) || (previousParam equal to token (true if first param ) && currentParam is greater/smaller depends on sortDirection)
                    expressions = Expression.Lambda<Func<QueryType, bool>>(currentExpression, parameter);

                    equalExpression =
                        Expression.Equal(property, valueExpression, false, Utils.StringEqualThanMethod);
                }
                else
                {
                    BinaryExpression greaterOrSmallerExpressionBinary;
                    if (Utils.IsNullableType(property.Type))
                    {
                        var nullCheck = Expression.NotEqual(property, Expression.Constant(null, ((PropertyInfo)property.Member).PropertyType));

                        PropertyInfo valueProp = ((PropertyInfo)property.Member).PropertyType.GetProperty("Value");

                        // Get value of nullable for property
                        property = Expression.Property(property, valueProp);

                        greaterOrSmallerExpressionBinary = Expression.AndAlso(nullCheck,
                                sortDirection == SortDirectionDTO.asc ?
                                    Expression.GreaterThan(property, valueExpression) :
                                    Expression.LessThan(property, valueExpression));
                    }
                    else
                    {
                        // SortDirectionDTO.asc ? a.Property > value : a.Property < value
                        greaterOrSmallerExpressionBinary = sortDirection == SortDirectionDTO.asc ?
                            Expression.GreaterThan(property, valueExpression) :
                            Expression.LessThan(property, valueExpression);
                    }

                    var greaterOrSmallerExpression = Expression.Lambda<Func<QueryType, bool>>(greaterOrSmallerExpressionBinary, parameter);

                    var currentExpression = Expression.OrElse(expressions.Body,
                                Expression.AndAlso(previousEqualExpression.Body, greaterOrSmallerExpression.Body));

                    // old params expression (if exists, first param will have non) || (previousParam equal to token (true if first param ) && currentParam is greater/smaller depends on sortDirection)
                    expressions = Expression.Lambda<Func<QueryType, bool>>(currentExpression, parameter);

                    equalExpression = Expression.Equal(property, valueExpression);
                }

                // All previous field must be equal to DB fields, and the current one also
                previousEqualExpression = Expression.Lambda<Func<QueryType, bool>>(Expression.AndAlso(previousEqualExpression.Body, equalExpression), parameter);
            }

            // Example for expression: 
            //x => x.PositionId.CompareTo(lastRow.PositionId) < 0 || (x.PositionId.CompareTo(lastRow.PositionId) == 0 && x.TaxRate < lastRow.TaxRate)
            // Description: if position id is smaller its true, if not:
            // Check if Position id is equal and tax rate is smaller
            return query.Where(expressions);
        }

        /// <summary>
        /// on a #Query sort it by #keySetToken in #sortDirection order
        /// </summary>
        /// <typeparam name="QueryType">Type of the query</typeparam>
        /// <typeparam name="KeySetTokenType"></typeparam>
        /// <param name="query">>query to sort</param>
        /// <param name="keySetToken">Token to sort by, it must inherit from KeySetToken, and each field in it should be KeySetTokenValue</param>
        /// <param name="sortDirection">direction to sort (asc, desc)</param>
        /// <returns></returns>
        public static IQueryable<QueryType> AddSorting<QueryType, KeySetTokenType>(this IQueryable<QueryType> query, KeySetTokenType keySetToken, string sortDirection)
            where KeySetTokenType : KeySetToken
        {
            List<string> keySetFields = Utils.GetKeySetFields(keySetToken, true);

            return query.AddSorting(sortDirection, keySetFields);
        }

        /// <summary>
        /// on a #Query sort it by #keySetToken in #sortDirection order
        /// </summary>
        /// <typeparam name="QueryType">Type of the query</typeparam>
        /// <typeparam name="KeySetTokenType"></typeparam>
        /// <param name="query">>query to sort</param>
        /// <param name="keySetToken">Token to sort by, it must inherit from KeySetToken, and each field in it should be KeySetTokenValue</param>
        /// <param name="sortDirection">direction to sort (asc, desc)</param>
        /// <returns></returns>
        public static IQueryable<QueryType> AddSorting<QueryType, KeySetTokenType>(this IQueryable<QueryType> query, KeySetTokenType keySetToken, SortDirectionDTO sortDirection)
            where KeySetTokenType : KeySetToken
        {
            return query.AddSorting(keySetToken, sortDirection.ToString());
        }

        /// <summary>
        /// on a #Query sort it by #sortingFields in #sortDirection order
        /// </summary>
        /// <typeparam name="QueryType">Type of the query</typeparam>
        /// <param name="query">query to sort</param>
        /// <param name="sortDirection">direction to sort (asc, desc)</param>
        /// <param name="sortingFields">sorting fileds (in order)</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">No Such field to sort by</exception>
        public static IQueryable<QueryType> AddSorting<QueryType>(this IQueryable<QueryType> query, SortDirectionDTO sortDirection, List<string> sortingFields)
        {
            return query.AddSorting(sortDirection.ToString(), sortingFields);
        }

        /// <summary>
        /// on a #Query sort it by #sortingFields in #sortDirection order
        /// </summary>
        /// <typeparam name="QueryType">Type of the query</typeparam>
        /// <param name="query">query to sort</param>
        /// <param name="sortDirection">direction to sort (asc, desc)</param>
        /// <param name="sortingFields">sorting fileds (in order)</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">No Such field to sort by</exception>
        public static IQueryable<QueryType> AddSorting<QueryType>(this IQueryable<QueryType> query, string sortDirection, List<string> sortingFields)
        {
            Type type = typeof(QueryType);
            bool firstTime = true;
            IOrderedQueryable<QueryType> orderedQuery = null;

            foreach (var sortingField in sortingFields)
            {
                PropertyInfo prop = type.GetProperty(sortingField);

                if (prop == null)
                    throw new ArgumentException($"There is no such field {sortingField}");

                var propType = prop.PropertyType;

                var fieldGetter = Utils.FieldGetterFromProperty<QueryType>(type, prop);

                orderedQuery = firstTime ?  // for first time use regular order by
                                    sortDirection == SortDirectionDTO.asc.ToString() ?
                                        query.OrderBy(fieldGetter) :
                                        query.OrderByDescending(fieldGetter)
                                        : // after first time use then by
                                    sortDirection == SortDirectionDTO.asc.ToString() ?
                                        orderedQuery.ThenBy(fieldGetter) :
                                        orderedQuery.ThenByDescending(fieldGetter);
                firstTime = false;

            }

            return orderedQuery;
        }
    }
}
