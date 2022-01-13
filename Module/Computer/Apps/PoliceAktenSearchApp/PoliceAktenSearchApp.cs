using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using System.Linq;

namespace GVRP.Module.Computer.Apps.PoliceAktenSearchApp
{
	public class PoliceAktenSearchApp : SimpleApp
	{
		public PoliceAktenSearchApp()
			: base("PoliceAktenSearchApp")
		{
		}

		[RemoteEvent]
		public async void requestPlayerResults(Client player, string searchQuery)
		{
			DbPlayer dbPlayer = player.GetPlayer();
			if (dbPlayer == null || !dbPlayer.IsValid())
			{
				return;
			}
			List<string> list = new List<string>();
			int num = 0;
			foreach (DbPlayer item in from p in GVRP.Module.Players.Players.Instance.GetValidPlayers()
									  where p.GetName().ToLower().Contains(searchQuery.ToLower()) || p.CustomData.Phone.Trim() == searchQuery.Trim() || (dbPlayer.TeamId == 5 && p.CustomData.Address.ToLower().Contains(searchQuery.ToLower())) || p.CustomData.Info.Contains(searchQuery.ToLower()) || p.CustomData.Membership.ToLower().Contains(searchQuery.ToLower())
									  select p)
			{
				if (num < 10)
				{
					list.Add(item.GetName());
					num++;
					continue;
				}
				break;
			}
			TriggerEvent(player, "responsePlayerResults", NAPI.Util.ToJson((object)list));
		}
	}
}
