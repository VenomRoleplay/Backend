using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace GVRP.Module.Computer.Apps.PoliceAktenSearchApp
{
	public class CrimeJsonObject
	{
		[JsonProperty(PropertyName = "id")]
		public int id { get; set; }

		[JsonProperty(PropertyName = "name")]
		public string name { get; set; }

		[JsonProperty(PropertyName = "description")]
		public string description { get; set; }
	}
}
