using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KeySetPaginator
{
    public class KeySetPagingRequest<KeySetTokenType>
         where KeySetTokenType : KeySetToken, new()
    {
        [Range(1, 10000)]
        public virtual int PageSize
        {
            get;
            set;
        } = 1000;


        public SortDirectionDTO SortDirection
        {
            get;
            set;
        }

        public KeySetTokenType KeySetToken { get; set; }
        public int? Timeout { get; set; }

        public KeySetPagingRequest()
        {
            KeySetToken = new KeySetTokenType();
        }

        protected KeySetPagingRequest(SortDirectionDTO defaultSortDirection)
        {
            SortDirection = defaultSortDirection;
            KeySetToken = new KeySetTokenType();
        }

        protected KeySetPagingRequest(string defaultSortDirection)
        {
            Enum.TryParse(defaultSortDirection, out SortDirectionDTO sortDirection);

            SortDirection = sortDirection;
            KeySetToken = new KeySetTokenType();
        }

        public IDictionary<string, object> AsParams()
        {
            return new Dictionary<string, object>
            {
                ["PageSize"] = PageSize,
                ["SortDirection"] = SortDirection,
                ["KeySetToken"] = KeySetToken,
                ["Timeout"] = Timeout
            };
        }
    }
}
