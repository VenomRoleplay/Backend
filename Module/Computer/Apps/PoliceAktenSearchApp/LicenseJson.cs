using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;


namespace GVRP.Module.Computer.Apps.PoliceAktenSearchApp
{
	public class LicenseJson
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "value")]
		public int Value { get; set; }
	}
}
