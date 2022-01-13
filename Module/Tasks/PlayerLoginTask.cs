using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.ClientUI.Windows;
using GVRP.Module.Clothes;
using GVRP.Module.Configurations;
using GVRP.Module.Helper;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Time;

namespace GVRP.Module.Tasks
{
    public class PlayerLoginTask : SqlResultTask
    {
        private readonly Client player;

        public PlayerLoginTask(Client player)
        {
            this.player = player;
        }

        public override string GetQuery()
        {
            return $"SELECT * FROM `player` WHERE `Name` = '{MySqlHelper.EscapeString(player.Name)}' LIMIT 1;";
        }

        public override async void OnFinished(MySqlDataReader reader)
        {
            try
            {


                if (reader.HasRows)
                {
                    //DbPlayer iPlayer = player.GetPlayer();
                    DbPlayer iPlayer = null;
                    DbPlayer dPlayer = player.GetPlayer();
                    while (reader.Read())
                    {
                        //Kayano
                        //if (player.SocialClubName == null)

                        /*if (reader.GetInt32("Pw") == 0)
                        {
                            ComponentManager.Get<TextInputBoxWindow>().Show()(dPlayer, new TextInputBoxWindowObject() { Title = "Passwort Eingabe", Callback = "addpw", Message = "Gebe dein Passwort ein! " });
                            return;
                            //addpw(iPlayer);
                        }*/


                        //if (reader.GetInt32("pw") == 0)
                            //player.SetData("KeinPW", true);

                        if (player == null) return;
                        //Bei Warn hau wech
                        if (reader.GetInt32("warns") >= 3)
                        {
                            player.TriggerEvent("freezePlayer", true);
                            //player.Freeze(true);
                            player.CreateUserDialog(Dialogs.menu_register, "banwindow");

                            PlayerLoginDataValidationModule.SyncUserBanToForum(reader.GetInt32("forumid"));

                            player.SendNotification($"Dein Venom (IC-)Account wurde gesperrt. Melde dich im Teamspeak!");
                            player.Kick();
                            return;
                        }

                        if (!PlayerLoginDataValidationModule.HasValidForumAccount(reader.GetInt32("forumid")))
                        {
                            player.TriggerEvent("freezePlayer", true);

                            player.CreateUserDialog(Dialogs.menu_register, "banwindow");

                            player.Kick("Dein Forumaccount ist nicht für das Spiel freigeschaltet!");
                            return;
                        }


                        // Check Timeban
                        if (reader.GetInt32("timeban") != 0 && reader.GetInt32("timeban") > DateTime.Now.GetTimestamp())
                        {
                            player.SendNotification("Ban aktiv");
                            player.Kick("Ban aktiv");
                            return;
                        }

                        

                        /*if (reader.GetString("Pass") == null && reader.GetString("Salt") == null)
                        {
                            ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Passwort Eingabe", Callback = "addpw", Message = "Gebe dein Passwort ein! " });
                            return;
                            //addpw(iPlayer);
                        }*/

                        iPlayer = Players.Players.Instance.Load(reader, player);


                        //     iPlayer.Freezed = false;
                        iPlayer.watchDialog = 0;
                        iPlayer.Firstspawn = false;
                        iPlayer.PassAttempts = 0;
                        iPlayer.TempWanteds = 0;
                        iPlayer.PlayerPet = null;

                        iPlayer.adminObject = null;
                        iPlayer.adminObjectSpeed = 0.5f;

                        iPlayer.AccountStatus = AccountStatus.Registered;

                        iPlayer.Character = ClothModule.Instance.LoadCharacter(iPlayer);

                        await VehicleKeyHandler.Instance.LoadPlayerVehicleKeys(iPlayer);

                        //           iPlayer.SetPlayerCurrentJobSkill();
                        //iPlayer.ClearChat();

                        // Check Socialban
                        if (SocialBanHandler.Instance.IsPlayerSocialBanned(iPlayer.Player))
                        {
                            player.SendNotification("Bitte melde dich beim Support im Teamspeak (Social-Ban)");
                            player.Kick();
                            return;
                        }

                        // Check Social Name
                        if (!Configurations.Configuration.Instance.Ptr && iPlayer.SocialClubName != "" && iPlayer.SocialClubName != iPlayer.Player.SocialClubName)
                        {
                            //DBLogging.LogAcpAdminAction("System", player.Name, adminLogTypes.perm, $"Social-Club-Name-Changed DB - {iPlayer.SocialClubName} - CLIENT - {iPlayer.Player.SocialClubName}");
                            iPlayer.Player.SendNotification("Bitte melde dich beim Support im Teamspeak (Social-Name-Changed)");
                            iPlayer.Player.Kick("Bitte melde dich beim Support im Teamspeak (Social-Name-Changed)");
                              return;
                        }


                        if (Players.Players.Instance.players.ToList().Count >= Configuration.Instance.MaxPlayers)
                        {
                            iPlayer.Player.SendNotification($"Server voll! ({Configuration.Instance.MaxPlayers.ToString()})");
                            iPlayer.Player.Kick("Server voll");
                        }

                        if (reader.GetInt32("Pw") == 0)
                        {
                            iPlayer.Player.SetData("KeinPW", true);
                            //ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Passwort Eingabe", Callback = "addpw", Message = "Gebe dein Passwort ein! " });
                            //return;
                            //addpw(iPlayer);
                        }

                        //            player.FreezePosition = true;

                        if (iPlayer == null) return;
                        player.TriggerEvent("setPlayerHealthRechargeMultiplier");
                        ComponentManager.Get<LoginWindow>().Show()(iPlayer);

                        if (Configuration.Instance.IsUpdateModeOn)
                        {
                            new LoginWindow().TriggerEvent(iPlayer.Player, "status", "Der Server befindet sich derzeit im Update Modus!");
                            if (iPlayer.Rank.Id < 1) iPlayer.Kick();
                        }

                        if (!Configuration.Instance.IsUpdateModeOn && iPlayer.HasData("KeinPW"))
                        {
                            new LoginWindow().TriggerEvent(iPlayer.Player, "status", "Bitte gebe dein Wunsch Passwort ein!");
                        }

                    }
                }
                else
                {
                    player.SendNotification("Sie benoetigen einen Account (Beta Zugang)! Name richtig gesetzt? Vorname_Nachname");
                    player.Kick(
                        "Sie benoetigen einen Account (Beta Zugang)! Name richtig gesetzt? Vorname_Nachname " + player.SocialClubName + " " + player.Name);
                    Logger.Debug($"Player was kicked, no Account found for {player.Name}");
                }

             }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


        }

        [RemoteEvent]
        public void addpw(Client player, string returnstring)
        {
            DbPlayer iPlayer = player.GetPlayer();

            //ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Passwort Eingabe", Callback = "addpw", Message = "Gebe dein Passwort ein! " });

            if (returnstring == null)
                player.Kick();

            //var Sha = HashThis.CreateSHA256Hash(returnstring);
            //var MDNigger = HashThis.CreateMD5Hash(returnstring);

            //MySQLHandler.ExecuteAsync($"UPDATE player SET Password = '{Sha}' WHERE id = '{iPlayer.Id}';");
            //MySQLHandler.ExecuteAsync($"UPDATE player SET Password = '{MDNigger}' WHERE id = '{iPlayer.Id}';");
            player.Kick("Passwort erfolgreich erstellt!"); 

        }
    }
}