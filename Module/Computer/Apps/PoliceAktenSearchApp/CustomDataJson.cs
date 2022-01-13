using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using GVRP.Module.Players;

namespace GVRP.Module.Computer.Apps.PoliceAktenSearchApp
{
	public class CustomDataJson
	{
		[JsonProperty(PropertyName = "address")]
		public string Address { get; set; }

		[JsonProperty(PropertyName = "membership")]
		public string Membership { get; set; }

		[JsonProperty(PropertyName = "phone")]
		public string Phone { get; set; }

		[JsonProperty(PropertyName = "info")]
		public string Info { get; set; }

		[JsonProperty(PropertyName = "canAktenView")]
		public bool CanAktenView { get; set; }

		[JsonProperty(PropertyName = "note")]
		public string Note { get; set; }

		public CustomDataJson(CustomData customData, string note = "")
		{
			Address = customData.Address;
			Membership = customData.Membership;
			Phone = customData.Phone;
			Info = customData.Info;
			CanAktenView = customData.CanAktenView;
			Note = note;
		}
	}
}
