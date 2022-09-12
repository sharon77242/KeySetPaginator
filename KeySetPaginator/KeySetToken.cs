using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KeySetPaginator
{
    /// <summary>
    /// Base class for KeySetToken
    /// Those deriving from it can add fields (the fields must be of type KeySetTokenValue<FieldType>)
    /// <FieldType> is the type of the field which is in db (non-nullable)
    /// Each field defined and initialized will be sorted and skipped only if its defined and initialized (according to the order in the class)
    /// </summary>
    public abstract class KeySetToken
    {
        [Required]
        public abstract List<string> DefaultFields { get; set; }
        public KeySetToken (List<string> DefaultFields)
        {
            this.DefaultFields = DefaultFields;
        }

        public static KeySetTokenValue<T> InitField<T>(T a)
        {
            return new KeySetTokenValue<T>(a);
        }
    }

    /// <summary>
    /// We need this type so that it will be initialized only we want to sort and ket set skip by it.
    /// We cant use primitive types instead because we need to check if they are initialized or not - with this type we do.
    /// We cant use Nullable type because it does not work with all types.
    /// </summary>
    /// <typeparam name="ValueType"></typeparam>
    public class KeySetTokenValue<ValueType>
    {
        public ValueType Value { get; set; }
        public KeySetTokenValue() { }
        public KeySetTokenValue(ValueType value)
        {
            Value = value;
        }
    }
}
