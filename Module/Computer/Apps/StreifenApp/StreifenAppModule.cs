/*using GTANetworkAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Handler;
using GVRP.Module.Computer.Apps.StreifenApp.Apps;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Computer.Apps.StreifenApp
{
    public sealed class StreifenAppModule : Module<StreifenAppModule>
    {
        public override bool Load(bool reload = false)
        {
            return true;
        }

        public Dictionary<uint, List<Streife>> TeamStreifen = new Dictionary<uint, List<Streife>>();
        public ConcurrentDictionary<uint, SxVehicle> StreifenFahrzeuge = new ConcurrentDictionary<uint, SxVehicle>();

        public uint countId = 1;

        public static List<uint> registredStreifenAppAccess = new List<uint>() { 
            (uint)teams.TEAM_POLICE, (uint)teams.TEAM_ARMY, (uint)teams.TEAM_FIB, (uint)teams.TEAM_MEDIC, (uint)teams.TEAM_SWAT, (uint)teams.TEAM_DPOS 
        };


        protected override bool OnLoad()
        {
            TeamStreifen = new Dictionary<uint, List<Streife>>();

            foreach(uint teamId in registredStreifenAppAccess)
            {
                TeamStreifen.Add(teamId, new List<Streife>());
            }

            countId = 1;
            return base.OnLoad();
        }

        public SxVehicle GetVehicleByStreife(Streife streife)
        {
            if (!StreifenFahrzeuge.ContainsKey(streife.Id))
                return null;

            SxVehicle sxVehicle = StreifenFahrzeuge[streife.Id];
            if (sxVehicle == null || !sxVehicle.IsValid())
                return null;

            return sxVehicle;
        }

        public override void OnPlayerDisconnected(DbPlayer iPlayer, string reason)
        {
            if (iPlayer == null || !iPlayer.IsValid())
                return;

            if (!StreifenAppModule.Instance.TeamStreifen.ContainsKey(iPlayer.TeamId)) return;

            Streife streife = StreifenAppModule.Instance.TeamStreifen[iPlayer.TeamId].ToList().Where(s => s.OfficersPlayers.Contains(iPlayer)).FirstOrDefault();
            if (streife == null) return;

            // Remove Old
            StreifenAppModule.Instance.TeamStreifen[iPlayer.TeamId].Remove(streife);

            if (streife.OfficersPlayers.Contains(iPlayer)) streife.OfficersPlayers.Remove(iPlayer);

            // Add new
            StreifenAppModule.Instance.TeamStreifen[iPlayer.TeamId].Add(streife);
        }

        public override void OnTenSecUpdate()
        {
            foreach (uint teamid in registredStreifenAppAccess) 
            {
                LeitstellenPhone.TeamLeitstellenObject teamLeitstellenObject = LeitstellenPhone.LeitstellenPhoneModule.Instance.GetLeitstelle(teamid);

                if (teamLeitstellenObject != null && teamLeitstellenObject.Acceptor != null && teamLeitstellenObject.Acceptor.IsValid())
                {
                    //List<CustomMarkerClientObject> clientSendData = new List<CustomMarkerClientObject>();

                    List<Streife> streifen = StreifenAppModule.Instance.TeamStreifen[teamid].ToList();

                    foreach (Streife streife in streifen)
                    {
                        if (streife.VehicleId > 0)
                        {
                            SxVehicle sxVehicle = GetVehicleByStreife(streife);
                            if (sxVehicle == null || !sxVehicle.IsValid() || sxVehicle.teamid != teamLeitstellenObject.Acceptor.TeamId) continue;

                            int color = 2; // grün
                            if (streife.State == 2) color = 28;
                            else if (streife.State == 3) color = 1;

                            int marker = 56; // vehicle
                            if (sxVehicle.Data.ClassificationId == 2 || sxVehicle.Data.ClassificationId == 7)
                            {
                                marker = 522;
                            }
                            else if (sxVehicle.Data.ClassificationId == 9)
                            {
                                marker = 251;
                            }
                            else if (sxVehicle.Data.ClassificationId == 8)
                            {
                                marker = 759;
                            }
                            else if (sxVehicle.Data.ClassificationId == 3)
                            {
                                marker = 410;
                            }

                            //set client marker
                            //clientSendData.Add(new CustomMarkerClientObject() { Color = color, MarkerId = marker, Name = streife.Name, Position = sxVehicle.entity.Position });
                        }
                    }

                    /*if (clientSendData.Count > 0)
                    {
                        Teams.Team team = Teams.TeamModule.Instance.GetById((int)teamid);
                        if (team == null)
                            return;

                        foreach (DbPlayer dbPlayer in team.Members.Values.Where(x => x.TeamRank >= 10))
                        {
                            if (dbPlayer == teamLeitstellenObject.Acceptor) // Prevent double send to leitstelle
                                continue;

                            //set client side marker
                            //dbPlayer.Player.TriggerEvent("setcustommarks", CustomMarkersKeys.Leitstelle, true, NAPI.Util.ToJson(clientSendData));
                        }

                        //set client side marker
                        //teamLeitstellenObject.Acceptor.Player.TriggerEvent("setcustommarks", CustomMarkersKeys.Leitstelle, true, NAPI.Util.ToJson(clientSendData));
                    }
                }
            }
        }

       
    }
}*/