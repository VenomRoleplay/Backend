using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Crime;
using GVRP.Module.Crime.PoliceAkten;
using GVRP.Module.Houses;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Computer.Apps.PoliceAktenSearchApp
{
	public class PoliceEditPersonApp : SimpleApp
	{
		public PoliceEditPersonApp()
			: base("PoliceEditPersonApp")
		{
		}

		[RemoteEvent]
		public async void requestPersonData(Client p_Client, string p_Name)
		{
			DbPlayer player = p_Client.GetPlayer();
			if (player == null || !player.IsValid())
			{
				return;
			}
			DbPlayer dbPlayer = GVRP.Module.Players.Players.Instance.FindPlayer(p_Name);
			if (dbPlayer == null || !dbPlayer.IsValid())
			{
				return;
			}
			dbPlayer.CustomData.CanAktenView = player.CanAktenView();
			string note = "";
			if ((player.Team.IsCops() && player.TeamRank >= 10) || dbPlayer.GovLevel.Length > 0)
			{
				note = "Sicherheitsstufe " + dbPlayer.GovLevel;
			}
			if (dbPlayer.ownHouse[0] != 0)
			{
				dbPlayer.CustomData.Address = "Haus " + dbPlayer.ownHouse[0];
			}
			else if (dbPlayer.IsTenant())
			{
				HouseRent tenant = dbPlayer.GetTenant();
				if (tenant != null)
				{
					dbPlayer.CustomData.Address = "Mieter " + tenant.HouseId;
				}
			}
			TriggerEvent(p_Client, "responsePersonData", NAPI.Util.ToJson((object)new CustomDataJson(dbPlayer.CustomData, note)));
		}

		[RemoteEvent]
		public async void requestAktenList(Client player, string searchQuery)
		{
			DbPlayer player2 = player.GetPlayer();
			if (player2 != null && player2.IsValid())
			{
				DbPlayer dbPlayer = GVRP.Module.Players.Players.Instance.FindPlayer(searchQuery);
				if (dbPlayer != null && dbPlayer.IsValid() && player2.CanAktenView())
				{
					TriggerEvent(player, "responseAktenList", NAPI.Util.ToJson((object)Module<PoliceAktenModule>.Instance.GetPlayerClientJsonAkten(dbPlayer)));
				}
			}
		}

		[RemoteEvent]
		public async void requestLicenses(Client player, string searchQuery)
		{
			DbPlayer player2 = player.GetPlayer();
			if (player2 != null && player2.IsValid())
			{
				DbPlayer dbPlayer = GVRP.Module.Players.Players.Instance.FindPlayer(searchQuery);
				if (dbPlayer != null && dbPlayer.IsValid())
				{
					List<LicenseJson> list = new List<LicenseJson>();
					list.Add(new LicenseJson
					{
						Name = "Motorradschein",
						Value = dbPlayer.Lic_Bike[0]
					});
					list.Add(new LicenseJson
					{
						Name = "Führerschein",
						Value = dbPlayer.Lic_Car[0]
					});
					list.Add(new LicenseJson
					{
						Name = "Bootsschein",
						Value = dbPlayer.Lic_Boot[0]
					});
					list.Add(new LicenseJson
					{
						Name = "LKW-Schein",
						Value = dbPlayer.Lic_LKW[0]
					});
					list.Add(new LicenseJson
					{
						Name = "Flugschein A",
						Value = dbPlayer.Lic_PlaneA[0]
					});
					list.Add(new LicenseJson
					{
						Name = "Flugschein B",
						Value = dbPlayer.Lic_PlaneB[0]
					});
					list.Add(new LicenseJson
					{
						Name = "Pers. Beförderungsschein",
						Value = dbPlayer.Lic_Transfer[0]
					});
					list.Add(new LicenseJson
					{
						Name = "Waffenschein",
						Value = dbPlayer.Lic_Gun[0]
					});
					list.Add(new LicenseJson
					{
						Name = "Erstehilfekurs",
						Value = dbPlayer.Lic_FirstAID[0]
					});
					TriggerEvent(player, "responseLicenses", NAPI.Util.ToJson((object)list));
				}
			}
		}

		[RemoteEvent]
		public async void requestAkte(Client player, string playername)
		{
			DbPlayer player2 = player.GetPlayer();
			if (player2 != null && player2.IsValid())
			{
				DbPlayer dbPlayer = GVRP.Module.Players.Players.Instance.FindPlayer(playername);
				if (dbPlayer != null && dbPlayer.IsValid() && player2.CanAktenView())
				{
					TriggerEvent(player, "responseAkte", NAPI.Util.ToJson((object)Module<PoliceAktenModule>.Instance.GetOpenAkteOrNew(dbPlayer)));
				}
			}
		}

		[RemoteEvent]
		public async void savePersonData(Client player, string playername, string address, string membership, string phone, string info)
		{
			DbPlayer player2 = player.GetPlayer();
			if (player2 == null || !player2.IsValid())
			{
				return;
			}
			if (!player2.CanEditData())
			{
				player2.SendNewNotification("Keine Berechtigung!");
				return;
			}
			DbPlayer dbPlayer = GVRP.Module.Players.Players.Instance.FindPlayer(playername);
			if (dbPlayer != null && dbPlayer.IsValid())
			{
				dbPlayer.UpdateCustomData(address, membership, phone, info);
			}
		}

		[RemoteEvent]
		public async void requestOpenCrimes(Client p_Client, string p_Name)
		{
			DbPlayer dbPlayer = GVRP.Module.Players.Players.Instance.FindPlayer(p_Name);
			if (dbPlayer == null || !dbPlayer.IsValid())
			{
				return;
			}
			List<CrimePlayerReason> crimes = dbPlayer.Crimes;
			List<CrimeJsonObject> list = new List<CrimeJsonObject>();
			try
			{
				foreach (CrimePlayerReason item in crimes.ToList())
				{
					list.Add(new CrimeJsonObject
					{
						id = (int)item.Id,
						name = item.Name,
						description = item.Notice
					});
				}
				string text = NAPI.Util.ToJson((object)list);
				TriggerEvent(p_Client, "responseOpenCrimes", text);
			}
			catch (Exception ex)
			{
				Logger.Crash(ex);
			}
		}

		[RemoteEvent]
		public async void requestJailCosts(Client p_Client, string p_Name)
		{
			DbPlayer dbPlayer = GVRP.Module.Players.Players.Instance.FindPlayer(p_Name);
			if (dbPlayer != null && dbPlayer.IsValid())
			{
				TriggerEvent(p_Client, "responseJailCosts", Module<CrimeModule>.Instance.CalcJailCosts(dbPlayer.Crimes));
			}
		}

		[RemoteEvent]
		public async void requestJailTime(Client p_Client, string p_Name)
		{
			try
			{
				DbPlayer dbPlayer = GVRP.Module.Players.Players.Instance.FindPlayer(p_Name);
				if (dbPlayer != null && dbPlayer.IsValid())
				{
					TriggerEvent(p_Client, "responseJailTime", Module<CrimeModule>.Instance.CalcJailTime(dbPlayer.Crimes));
				}
			}
			catch (Exception ex)
			{
				Logger.Crash(ex);
			}
		}
	}
}
