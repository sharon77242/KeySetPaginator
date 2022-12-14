using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

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
        /// This field defines the minimum unique index to sort/skip by.
        /// If needed to Append to it use AddPriorToken method
        public abstract List<string> DefaultFields { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DefaultFields"></param>
        /// <param name="appendToken">Decide if to force init default fields or just append to the type default fields, 
        /// false means force init default fields, true means add to current </param>
        public KeySetToken(List<string> DefaultFields)
        {
            this.DefaultFields = DefaultFields;

            ValidateTokenProperties();
        }

        public void AddPriorToken(List<string> tokenNames)
        {
            if (DefaultFields == null)
            {
                DefaultFields = tokenNames;
                return;
            }

            if (tokenNames == null)
                throw new ArgumentNullException("Token names cannot be null");

            DefaultFields = tokenNames.Concat(DefaultFields).ToList();
        }

        public KeySetType AddAndGetPriorToken<KeySetType>(List<string> tokenNames)
            where KeySetType : KeySetToken
        {
            AddPriorToken(tokenNames);
            return (KeySetType)this;
        }

        public KeySetType AddAndGetPriorToken<KeySetType>(string tokenName)
            where KeySetType : KeySetToken
        {
            return AddAndGetPriorToken<KeySetType>(new List<string> { tokenName });
        }

        public void AddPriorToken(string tokenName)
        {
            AddPriorToken(new List<string> { tokenName });
        }

        protected void ValidateTokenProperties()
        {
            PropertyInfo[] properties = GetType().GetProperties();

            bool hasAtLeastOneTokenValue = false;

            foreach (PropertyInfo property in properties)
            {
                if (property.Name != "DefaultFields")
                {
                    if (!property.PropertyType.Name.Contains("KeySetTokenValue"))
                        throw new ArgumentException($"Property of type: {property.Name} must be of type KeySetTokenValue");
                    else
                        hasAtLeastOneTokenValue = true;
                }
            }

            if (!hasAtLeastOneTokenValue)
                throw new ArgumentException($"Token from type {GetType()} must have at least one property from type KeySetTokenValue");
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
