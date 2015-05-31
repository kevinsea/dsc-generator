using System.Collections.Generic;
using System.Text;

namespace DesiredState.Common
{

	/// <summary>
	// Provides base plumbing for desired state classes.
	/// Allow you to add whatever attributes you need
	/// and does code generation of all attributes provided
	/// </summary>
	internal abstract class DesiredStateBase
	{
		protected abstract string DscObjectType { get; }
		public string Key { get; protected set; }

		/// <summary>
		/// The object name.  In IIS this would map to Pool or Site name in the UI.
		/// Map this to the appropriate attribute.
		/// </summary>
		public string Name { get; protected set; }
		public string ObjectDescription { get; protected set; }

		public List<Attribute> Attributes = new List<Attribute>();

		protected DesiredStateBase()
		{
			this.ObjectDescription = "";
			this.Name = "";
		}

		public void AddAttribute(string paramName, string value)
		{
			var comment = "";

			CreateAttribute(paramName, value, comment);
		}

		public void AddAttributeWithOverrideValue(string paramName, string value, string sourceServerValue)
		{
			var comment = CodeGenHelpers.GetCommentOnOverride(value, sourceServerValue);


			CreateAttribute(paramName, value, comment);
		}

		public void AddAttributeWithComment(string paramName, string value, string comment)
		{
			comment = "  # " + comment;

			CreateAttribute(paramName, value, comment);
		}

		private void CreateAttribute(string paramName, string value, string comment)
		{
			string code = CodeGenHelpers.FormatAttributeCode(paramName, value);

			var attrib = new Attribute(paramName, code, comment);

			this.Attributes.Add(attrib);

			if (CodeGenHelpers.AreEqualCI(paramName, "name"))
			{
				this.Name = value;
			}
		}

		public string GetCode(int baseIndentDepth, CodeGenType codeGenType = CodeGenType.Parent)
		{
			var baseIndent = CodeGenHelpers.GetIndentString(baseIndentDepth);

			string scriptPrefix = "";
			string scriptSuffix = "";
			string blockvariableName = "";

			var sb = new StringBuilder();

			if (codeGenType == CodeGenType.Parent)  // The parent needs to have a variable name
				blockvariableName = this.Key;

			string objectDescriptionComment = (this.ObjectDescription.Trim() == "") ? "" : "  # " + this.ObjectDescription;

			sb.AppendLine(baseIndent + scriptPrefix + DscObjectType + " " + blockvariableName + objectDescriptionComment);
			sb.AppendLine(baseIndent + "{");

			foreach (var a in this.Attributes)
			{
				sb.AppendLine(baseIndent + CodeGenHelpers.Indent + a.Code + a.Comment);
			}

			sb.Append(this.GetChildCode(baseIndentDepth + 1));

			sb.AppendLine(baseIndent + "}" + scriptSuffix);
			return sb.ToString();
		}

		public virtual string GetChildCode(int baseIndentDepth)
		{
			return "";
		}

		protected string GetChildListCode(string propertyName, List<DesiredStateBase> childList, int baseIndentDepth, string baseIndent)
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

		public class Attribute
		{
			public Attribute(string name, string code, string comment = "")
			{
				Name = name;
				Code = code;
				Comment = comment;
			}

			public readonly string Name;
			public readonly string Code;
			public readonly string Comment;
		}
	}

	public enum CodeGenType
	{
		Parent = 1,
		SingleChild = 2,
		MultiChild = 3
	}
}
