using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ProxyGenerator
{
	public class Endpoint
	{
		public string Filename { get; set; }
		public string Namespace { get; set; }
		public string URL { get; set; }
	}

	public class Program
	{
		public static void Main(string[] args)
		{
			try
			{
				string proxyDir = "Proxies";

				// We need the configuration (Test, Staging, Live) and
				// the path where the Web.config files are stored and where to output the proxy files
				if (args == null || args.Length != 2)
					throw new Exception("Wrong parameters!");

				// Configuration (Test, Staging, Live)
				string configuration = args[0];

				// Project path
				string projectPath = args[1];

				// Full proxy path
				string fullProxyPath = projectPath + "\\" + proxyDir;
				// Web.config file
				string webConfigFile = "Web.config";
				// Web.config transformation file
				string webConfigTransFile = "Web." + configuration + ".config";

				// Check if project path exists
				if (!Directory.Exists(projectPath))
					throw new Exception("Project path doesn't exist");

				// Check if Web.config file exists
				if (!File.Exists(projectPath + "\\" + webConfigFile))
					throw new Exception("Web.config file doesn't exist");

				// Check if Web.config transformation file exists
				if (!File.Exists(projectPath + "\\" + webConfigTransFile))
					throw new Exception("Web.config transformation file doesn't exist");

				// Create proxy directory if it doesn't exist
				if (!Directory.Exists(fullProxyPath))
					Directory.CreateDirectory(fullProxyPath);

				// Dictionary for our endpoints
				Dictionary<string, Endpoint> endpoints = new Dictionary<string, Endpoint>();

				// Open Web.config
				XmlDocument webConfig = new XmlDocument();
				webConfig.Load(projectPath + "\\" + webConfigFile);

				// Open Web.config transform
				XmlDocument webConfigTransform = new XmlDocument();
				webConfigTransform.Load(projectPath + "\\" + webConfigTransFile);

				// Select all endpoints in Web.config file
				XmlNode node_configEndpoints = webConfig.DocumentElement.SelectSingleNode("/configuration/system.serviceModel/client");

				if (node_configEndpoints == null)
					throw new Exception("Couldn't find endpoints in " + webConfigFile);

				// Go through each endpoint node
				foreach (XmlNode node in node_configEndpoints.ChildNodes)
				{
					// Get the full name of the endpoint
					string fullName = node.Attributes["name"].Value;

					// Get the namespace of the endpoint
					string nameSpace = node.Attributes["contract"].Value.Split('.').First();

					// Get the output file name of the endpoint (without Soap postfix)
					string outputName = node.Attributes["name"].Value;
					if (node.Attributes["name"].Value.LastIndexOf("Soap") != -1)
						outputName = node.Attributes["name"].Value.Substring(0, node.Attributes["name"].Value.LastIndexOf("Soap"));

					// Get the endpoint name in the Web.config transform
					XmlNode node_transformEndpoint = webConfigTransform.DocumentElement.SelectSingleNode("/configuration/system.serviceModel/client/endpoint[@name='" + fullName + "']");

					if (node_transformEndpoint == null)
						throw new Exception("Couldn't find endpoint '" + fullName + "' in " + webConfigTransFile);

					// Get the URL of the endpoint from the Web.config transformation file
					string url = node_transformEndpoint.Attributes["address"].Value;

					// Create the endpoint
					Endpoint endpoint = new Endpoint
					{
						Filename = outputName,
						Namespace = nameSpace,
						URL = url
					};

					endpoints.Add(fullName, endpoint);
				}

				// Go through each endpoint and generate proxy file with svcutil
				foreach (var e in endpoints)
				{
					var p = new Process();
					p.StartInfo = new ProcessStartInfo("svcutil.exe") { UseShellExecute = false };
					p.StartInfo.Arguments = string.Format("{0} /serializer:XmlSerializer /noConfig  /tcv:Version35  /n:*,{1} /out:{2}", e.Value.URL, e.Value.Namespace, fullProxyPath + "\\" + e.Value.Filename + ".cs");
					p.Start();
					p.WaitForExit();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("ERROR: " + ex.Message);
				Environment.Exit(1);
			}
		}
	}
}
