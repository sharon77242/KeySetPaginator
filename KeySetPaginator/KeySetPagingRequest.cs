using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace KeySetPaginator
{
    public class KeySetPagingRequest<KeySetTokenType, RequestType>
         where KeySetTokenType : KeySetToken, new()
        where RequestType : class, new()
    {
        [Range(1, 10000)]
        public virtual int PageSize
        {
            get;
            set;
        } = 1000;


        public SortDirection SortDirection
        {
            get;
            set;
        }

        public KeySetTokenType KeySetToken { get; set; }
        public int? Timeout { get; set; }
        public RequestType Request { get; set; }

        public KeySetPagingRequest()
        {
            KeySetToken = new KeySetTokenType();
            Request = new RequestType();
        }

        protected KeySetPagingRequest(SortDirection defaultSortDirection)
            : base()
        {
            SortDirection = defaultSortDirection;
        }

        protected KeySetPagingRequest(string defaultSortDirection)
            :base()
        {
            Enum.TryParse(defaultSortDirection, out SortDirection sortDirection);

            SortDirection = sortDirection;
        }

        private List<KeyValuePair<string, object>> AddKeySetTokenToParams(List<KeyValuePair<string, object>> dic)
        {
            foreach (PropertyInfo property in KeySetToken.GetType().GetProperties())
            {
                var value = property.GetValue(KeySetToken);
                if (property.Name == "DefaultFields")
                {
                    foreach (var listValue in value as List<string>)
                    {
                        dic.Add(new KeyValuePair<string, object>(nameof(KeySetToken) + "." + property.Name, listValue));
                    }
                }
                else if (value != null)
                {
                    var type = value.GetType();

                    var propertyInfo = type.GetProperty("Value");
                    try
                    {
                        value = propertyInfo.GetValue(value, null);
                    }
                    catch (Exception e)
                    {
                        throw new ArgumentException($"Field with name {property.Name} must be from type KeySetTokenValue, and not from type {type}", e);
                    }

                    dic.Add(new KeyValuePair<string, object>(nameof(KeySetToken) + "." + property.Name + ".Value", value));
                }
            }

            return dic;
        }

        public List<KeyValuePair<string, object>> AsParams()
        {
            var dic = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("PageSize", PageSize),
                new KeyValuePair<string, object>("SortDirection", SortDirection),
                new KeyValuePair<string, object>("Timeout", Timeout)
            };

            return AddKeySetTokenToParams(dic);
        }
    }
}
