using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace KeySetPaginator
{
    public static class PaginatorAPI
    {
        /// <summary>
        /// According to the first token build key set fields list and that and the last row build a key set token.
        /// Note - firstToken is not current token it must be the first one (as current token can have one of the field init to null)
        /// </summary>
        /// <typeparam name="RowType"></typeparam>
        /// <typeparam name="KeySetTokenType"></typeparam>
        /// <param name="lastRow"></param>
        /// <param name="firstToken"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static KeySetTokenType LastReponseToToken<RowType, KeySetTokenType>(this KeySetTokenType firstToken, RowType lastRow)
            where KeySetTokenType : KeySetToken
        {
            if (firstToken == null)
                throw new ArgumentException("Token can't be null");

            if (lastRow == null)
                return null;

            var keySetFields = Utils.GetKeySetFields(firstToken);
            Type keySetType = typeof(KeySetTokenType);
            Type rowType = typeof(RowType);
            KeySetTokenType keySet = (KeySetTokenType)Activator.CreateInstance(keySetType);

            foreach (var keySetFieldName in keySetFields)
            {
                PropertyInfo rowProp = rowType.GetProperty(keySetFieldName);

                var rowValue = rowProp.GetValue(lastRow, null);

                if (rowValue != null)
                {
                    GetValueForNullableType(rowType, keySetFieldName, ref rowProp, ref rowValue);

                    PropertyInfo keySetProp = keySetType.GetProperty(keySetFieldName);

                    // Get constructor of generic type KeySetTokenValue according to rowValue type
                    ConstructorInfo ctor = keySetProp.PropertyType.GetConstructor(new[] { rowValue.GetType() });
                    var initializedValue = ctor.Invoke(new[] { rowValue });
                    keySetProp.SetValue(keySet, initializedValue);
                }
            }

            return keySet;
        }

        private static void GetValueForNullableType(Type rowType, string keySetFieldName, ref PropertyInfo rowProp, ref object rowValue)
        {
            if (Utils.IsNullableType(rowValue.GetType()))
            {
                rowProp = rowType.GetProperty(keySetFieldName);
                rowValue = (PropertyInfo)rowProp.GetValue("Value");
            }
        }

        /// <summary>
        /// According to the request and keySetPagingRequest call search action to get paged results
        /// for each response build a token to find next page
        /// Do the above untill getting all the results
        /// </summary>
        /// <typeparam name="RequestType"></typeparam>
        /// <typeparam name="KeySetTokenType"></typeparam>
        /// <typeparam name="ReturnType"></typeparam>
        /// <param name="searchAction">Key set search action that get a request and a KeySetPagingRequest and returns a list of ReturnType</param>
        /// <param name="request"></param>
        /// <param name="keySetPagingRequest"></param>
        public static async Task<List<ReturnType>> GetAllResults<RequestType, KeySetTokenType, ReturnType>(
            this Func<RequestType, KeySetPagingRequest<KeySetTokenType>, Task<List<ReturnType>>> searchAction,
            RequestType request,
            KeySetPagingRequest<KeySetTokenType> keySetPagingRequest)
                where KeySetTokenType : KeySetToken, new()
        {
            var firstToken = keySetPagingRequest.KeySetToken;

            List<ReturnType> response;
            var result = new List<ReturnType>();

            do
            {
                response = await searchAction(request, keySetPagingRequest);
                keySetPagingRequest.KeySetToken = firstToken.LastReponseToToken(response.LastOrDefault());

                result.AddRange(response);

            } while (response.Count == keySetPagingRequest.PageSize);

            return result;
        }

        /// <summary>
        /// According to the request and keySetPagingRequest call search action to get paged results
        /// for each response build a token to find next page
        /// Do the above untill getting all the results
        /// </summary>
        /// <typeparam name="RequestType"></typeparam>
        /// <typeparam name="KeySetTokenType"></typeparam>
        /// <typeparam name="ReturnType"></typeparam>
        /// <param name="searchAction">Key set search action that get a request and a KeySetPagingRequest and returns a list of ReturnType</param>
        /// <param name="AfterSearchAction">After getting results from search action get them and do something, the result of this action will not change the loop</param>
        /// <param name="request"></param>
        /// <param name="keySetPagingRequest"></param>
        public static async Task<List<ReturnType>> GetAllResults<RequestType, KeySetTokenType, ReturnType>(
            Func<RequestType, KeySetPagingRequest<KeySetTokenType>, Task<List<ReturnType>>> searchAction,
            Func<List<ReturnType>, Task> AfterSearchAction,
            RequestType request,
            KeySetPagingRequest<KeySetTokenType> keySetPagingRequest)
                where KeySetTokenType : KeySetToken, new()
        {
            var firstToken = keySetPagingRequest.KeySetToken;

            List<ReturnType> response;
            var result = new List<ReturnType>();

            do
            {
                response = await searchAction(request, keySetPagingRequest);
                if (response.Count != 0)
                    await AfterSearchAction(response);

                keySetPagingRequest.KeySetToken = firstToken.LastReponseToToken(response.LastOrDefault());

                result.AddRange(response);

            } while (response.Count == keySetPagingRequest.PageSize);

            return result;
        }

        /// <summary>
        /// According to the request and keySetPagingRequest call search action to get paged results
        /// for each response build a token to find next page
        /// Do the above untill getting all the results
        /// </summary>
        /// <typeparam name="RequestType"></typeparam>
        /// <typeparam name="KeySetTokenType"></typeparam>
        /// <typeparam name="ReturnType"></typeparam>
        /// <param name="searchAction">Key set search action that get a request and a KeySetPagingRequest and returns a list of ReturnType</param>
        /// <param name="AfterSearchAction">After getting results from search action get them and do something, 
        ///                                 if the result of this action is true - it means we end the loop without reading the next results</param>
        /// <param name="request"></param>
        /// <param name="keySetPagingRequest"></param>
        public static async Task<List<ReturnType>> GetAllResults<RequestType, KeySetTokenType, ReturnType>(
            Func<RequestType, KeySetPagingRequest<KeySetTokenType>, Task<List<ReturnType>>> searchAction,
            Func<List<ReturnType>, Task<bool>> AfterSearchAction,
            RequestType request,
            KeySetPagingRequest<KeySetTokenType> keySetPagingRequest)
                where KeySetTokenType : KeySetToken, new()
        {
            var firstToken = keySetPagingRequest.KeySetToken;

            List<ReturnType> response;
            var result = new List<ReturnType>();
            bool afterSearchResponse = false;

            do
            {
                response = await searchAction(request, keySetPagingRequest);
                if (response.Count != 0)
                    afterSearchResponse = await AfterSearchAction(response);
                
                keySetPagingRequest.KeySetToken = firstToken.LastReponseToToken(response.LastOrDefault());

                result.AddRange(response);

            } while (response.Count == keySetPagingRequest.PageSize && !afterSearchResponse);

            return result;
        }
    }
}
