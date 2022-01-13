using System;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Crime.PoliceAkten;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Computer.Apps.PoliceAktenSearchApp
{
	public class PoliceAddAktenApp : SimpleApp
	{
		public class ResponseAkteJson
		{
			[JsonProperty(PropertyName = "number")]
			public uint AktenId { get; set; }

			[JsonProperty(PropertyName = "title")]
			public string Title { get; set; }

			[JsonProperty(PropertyName = "text")]
			public string Text { get; set; }

			[JsonProperty(PropertyName = "created")]
			public DateTime Created { get; set; }

			[JsonProperty(PropertyName = "closed")]
			public DateTime Closed { get; set; }

			[JsonProperty(PropertyName = "officer")]
			public string Officer { get; set; }

			[JsonProperty(PropertyName = "open")]
			public bool Open { get; set; }
		}

		public PoliceAddAktenApp()
			: base("PoliceAddAktenApp")
		{
		}

		[RemoteEvent]
		public async void savePersonAkte(Client player, string playername, string json)
		{
			DbPlayer player2 = player.GetPlayer();
			if (player2 == null || !player2.IsValid())
			{
				return;
			}
			DbPlayer dbPlayer = GVRP.Module.Players.Players.Instance.FindPlayer(playername);
			if (dbPlayer == null || !dbPlayer.IsValid())
			{
				return;
			}
			ResponseAkteJson responseAkteJson;
			try
			{
				responseAkteJson = JsonConvert.DeserializeObject<ResponseAkteJson>(json);
			}
			catch (Exception ex)
			{
				Logger.Crash(ex);
				return;
			}
			if (responseAkteJson == null)
			{
				return;
			}
			if (responseAkteJson.AktenId != 0)
			{
				if (!player2.CanAktenEdit())
				{
					player2.SendNewNotification("Keine Berechtigung!");
					return;
				}
				Module<PoliceAktenModule>.Instance.SaveServerAkte(dbPlayer, responseAkteJson);
				player2.SendNewNotification("Akte gespeichert.");
			}
			else if (!player2.CanAktenCreate())
			{
				player2.SendNewNotification("Keine Berechtigung!");
			}
			else
			{
				Module<PoliceAktenModule>.Instance.AddServerAkte(dbPlayer, responseAkteJson);
				player2.SendNewNotification("Akte angelegt.");
			}
		}

		[RemoteEvent]
		public async void deletePersonAkte(Client player, int aktenId)
		{
			DbPlayer player2 = player.GetPlayer();
			if (player2 != null && player2.IsValid() && aktenId != 0)
			{
				if (!player2.CanAktenDelete())
				{
					player2.SendNewNotification("Keine Berechtigung!");
					return;
				}
				Module<PoliceAktenModule>.Instance.DeleteServerAkte((uint)aktenId);
				player2.SendNewNotification("Akte gelöscht.");
			}
		}
	}
}
