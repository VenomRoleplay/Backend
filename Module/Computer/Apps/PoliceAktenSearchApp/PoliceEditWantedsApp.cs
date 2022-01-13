using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Crime;
using GVRP.Module.Injury;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams;
using static GVRP.Module.Players.PlayerNotification;

namespace GVRP.Module.Computer.Apps.PoliceAktenSearchApp
{
	public class PoliceEditWantedsApp : SimpleApp
	{
		public PoliceEditWantedsApp()
			: base("PoliceEditWantedsApp")
		{
		}

		[RemoteEvent]
		public void requestWantedCategories(Client p_Client)
		{
			Dictionary<uint, CrimeCategory> all = Module<CrimeCategoryModule>.Instance.GetAll();
			List<CategoryObject> list = new List<CategoryObject>();
			foreach (KeyValuePair<uint, CrimeCategory> item in all)
			{
				list.Add(new CategoryObject
				{
					id = (int)item.Value.Id,
					name = item.Value.Name
				});
			}
			string text = NAPI.Util.ToJson((object)list);
			TriggerEvent(p_Client, "responseCategories", text);
		}

		[RemoteEvent]
		public void requestCategoryReasons(Client p_Client, int p_ID)
		{
			Dictionary<uint, CrimeReason> all = Module<CrimeReasonModule>.Instance.GetAll();
			List<ReasonObject> list = new List<ReasonObject>();
			foreach (KeyValuePair<uint, CrimeReason> item in all)
			{
				if (item.Value.Category.Id == p_ID)
				{
					list.Add(new ReasonObject
					{
						id = (int)item.Value.Id,
						name = item.Value.Name
					});
				}
			}
			string text = NAPI.Util.ToJson((object)list);
			TriggerEvent(p_Client, "responseCategoryReasons", text);
		}

		[RemoteEvent]
		public void requestPlayerWanteds(Client p_Client, string p_Name)
		{
			Main.m_AsyncThread.AddToAsyncThread(new Task(delegate
			{
				DbPlayer dbPlayer = GVRP.Module.Players.Players.Instance.FindPlayer(p_Name);
				if (dbPlayer != null && dbPlayer.IsValid())
				{
					List<CrimePlayerReason> crimes = dbPlayer.Crimes;
					List<CrimeJsonObject> list = new List<CrimeJsonObject>();
					foreach (CrimePlayerReason item in crimes)
					{
						list.Add(new CrimeJsonObject
						{
							id = (int)item.Id,
							name = item.Name,
							description = item.Description
						});
					}
					string text = NAPI.Util.ToJson((object)list);
					TriggerEvent(p_Client, "responsePlayerWanteds", text);
				}
			}));
		}

		[RemoteEvent]
		public void removeAllCrimes(Client p_Client, string name)
		{
			Main.m_AsyncThread.AddToAsyncThread(new Task(delegate
			{
				GVRP.Module.Players.Players.Instance.FindPlayer(name)?.RemoveAllCrimes(p_Client.Name);
			}));
		}

		[RemoteEvent]
		public void removePlayerCrime(Client p_Client, string name, int crime)
		{
			Main.m_AsyncThread.AddToAsyncThread(new Task(delegate
			{
				DbPlayer dbPlayer = GVRP.Module.Players.Players.Instance.FindPlayer(name);
				if (dbPlayer != null)
				{
					CrimePlayerReason crimePlayerReason = dbPlayer.Crimes.Where((CrimePlayerReason cpr) => cpr.Id == (uint)crime).FirstOrDefault();
					if (crimePlayerReason != null)
					{
						dbPlayer.RemoveCrime(crimePlayerReason, p_Client.Name);
					}
				}
			}));
		}

		[RemoteEvent]
		public void addPlayerWanteds(Client player, string name, string crimes)
		{
			DbPlayer player2 = player.GetPlayer();
			if (player2 == null || !player2.IsValid() || !player2.IsACop() || player2.TeamId == 21)
			{
				return;
			}
			List<uint> list = JsonConvert.DeserializeObject<List<uint>>(crimes);
			DbPlayer dbPlayer = GVRP.Module.Players.Players.Instance.FindPlayer(name);
			if (dbPlayer == null || dbPlayer.IsACop() || list == null || dbPlayer.isInjured())
			{
				return;
			}
			foreach (uint item in list)
			{
				dbPlayer.AddCrime(player2, Module<CrimeReasonModule>.Instance.Get(item));
			}
			Module<TeamModule>.Instance.SendChatMessageToDepartments(player2.GetName() + " hat die Akte von " + dbPlayer.GetName() + " bearbeitet!");
			dbPlayer.SendNewNotification("Du hast eine E-Mail erhalten!", NotificationType.INFO, "Aktensystem");
		}
	}
}
