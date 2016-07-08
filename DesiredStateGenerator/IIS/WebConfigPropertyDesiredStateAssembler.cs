using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace DesiredState.IIS
{
	internal class WebConfigPropertyDesiredStateAssembler
	{

		public List<WebConfigPropertyDesiredState> GetWebConfigPropertyDesiredStates()
		{
			var masterList = new List<WebConfigPropertyDesiredState>(0);

			var locationElements = LoadLocationElements();

			foreach (XElement locationElement in locationElements)
			{
				var attributeList = new List<WebConfigPropertyDesiredState>();

				if (locationElement.Attribute("path") != null)
				{
					Traverse(locationElement, "", locationElement.Attribute("path").Value, attributeList);

					masterList.AddRange(attributeList);
				}

			}
			return masterList;
		}

		/// <summary>
		/// Recursive method that goes that traverses the tree until it finds configuration attributes
		/// </summary>
		public void Traverse(XElement element, string nodePath, string sitelocation, List<WebConfigPropertyDesiredState> resultList)
		{
			if (element.HasElements)
			{
				foreach (var childElement in element.Elements())
				{
					string newNodePath = nodePath + "/" + childElement.Name.LocalName;
					Traverse(childElement, newNodePath, sitelocation, resultList);
				}
			}
			else
			{
				var attribute = BuildEntry(element, nodePath, sitelocation);
				resultList.AddRange(attribute);
			}
		}

		public List<WebConfigPropertyDesiredState> BuildEntry(XElement element, string nodePath, string sitelocation)
		{
			var attributes = new List<WebConfigPropertyDesiredState>();

			foreach (var property in element.Attributes())
			{
				var desiredState = new WebConfigPropertyDesiredState(nodePath, property.Name.LocalName
															, property.Value, sitelocation);

				attributes.Add(desiredState);
			}

			return attributes;
		}

		private static IEnumerable<XElement> LoadLocationElements()
		{
			string filePath = Environment.GetFolderPath(Environment.SpecialFolder.System) + @"\inetsrv\config\applicationHost.config";

			//TODO if this file does not exist, either skip attribute generation or throw an exception...or something else smart
			XElement root = XElement.Load(filePath);

			return root.Elements("location");
		}

	}
}
