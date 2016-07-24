using System;
using System.Collections.Generic;

namespace DesiredState.Common
{
    internal class CodeGenHelpers
    {
        public const string Indent = "     ";

        public static string GetOverrideComment(object value, object sourceServerValue)
        {
            if (sourceServerValue == null)
                return "";

            string comment = "";
            string valueString = ValueFormatter.Format(value);
            string sourceServerValueString = ValueFormatter.Format(sourceServerValue);

            if (sourceServerValueString != valueString)
            {
                comment = "  # overriding " + sourceServerValueString;
            }

            return comment;
        }

        public static string FormatAttributeCode(string paramName, object value)
        {
            string valueString = ValueFormatter.Format(value);

            return $"{paramName} = {valueString}";
        }

        public static string GetIndentString(int indentDepth)
        {
            string baseIndent = "";
            for (int i = 0; i < indentDepth; i++)
            {
                baseIndent += Indent;
            }
            return baseIndent;
        }

        internal static string FormatKey(string objectName, string type)
        {
            return FormatKey(objectName) + "_" + type;
        }

        private static string FormatKey(string name)
        {
            if (name == null)
            {
                return "";
            }

            if (name == "/")
            {
                return "Root";
            }

            name = name.Replace(" ", "");
            name = name.Replace("/", "");

            return name;
        }

        internal static string FormatKey(string name1, string name2, string type)
        {
            return FormatKey(name1) + "_" + FormatKey(name2) + "_" + type;
        }

        // ReSharper disable once InconsistentNaming
        internal static bool AreEqualCI(string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        internal static string GenerateChildListCode(string propertyName, List<DesiredStateBase> childList, int baseIndentDepth, string baseIndent)
        {
            string code = baseIndent + propertyName + " = \n";

            if (childList.Count == 0)
            {
                return "";
            }
            else
            {
                code += baseIndent + CodeGenHelpers.Indent + CodeGenHelpers.Indent + "@(\n";

                foreach (var child in childList)
                {
                    code += child.GetCode(baseIndentDepth + 3, CodeGenType.MultiChild) + "";
                }
                code += baseIndent + CodeGenHelpers.Indent + CodeGenHelpers.Indent + ")\n";
            }
            return code;
        }

    }
}
