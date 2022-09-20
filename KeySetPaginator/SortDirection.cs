using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KeySetPaginator
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SortDirection
    {
        asc,
        desc
    }
}
