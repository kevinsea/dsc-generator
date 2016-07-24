using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesiredState.Common
{
    class ValueFormatter
    {

        internal static string Format(object value)
        {
            string valueString;

            if (value is bool)
                valueString = ValueFormatter.Boolean((bool)value);
            else
                valueString = ValueFormatter.String(value.ToString());

            return valueString;
        }

        private static string Boolean(bool value)
        {
            return $"${value.ToString().ToLower()}";
        }

        private static string String(string value)
        {
            if (value.StartsWith("@")) // don't encapsulate code blocks in quotes
                return value;
            if (value.StartsWith("$")) // don't encapsulate variables in quotes
                return value;
            else
                return $"\"{value}\"";

        }

    }
}
