
using System.Collections.Generic;

namespace KeySetPaginator.Tests
{
    public class ExampleToken : KeySetToken
    {
        public ExampleToken()
            : base(new List<string>() { nameof(StringName), nameof(NullableName)})
        { }
        public ExampleToken(List<string> DefaultFields)
            : base(DefaultFields ?? new List<string>() { nameof(StringName), nameof(NullableName) })
        { }

        public KeySetTokenValue<string> StringName { get; set; }
        public KeySetTokenValue<decimal> DecimalName { get; set; }
        public KeySetTokenValue<int> IntName { get; set; }
        public KeySetTokenValue<long> LongName { get; set; }
        public KeySetTokenValue<decimal> NullableName { get; set; }
        public override List<string> DefaultFields { get; set; }
    }
}
