using System;
using GVRP.Module.Players.Db;
using Newtonsoft.Json;
using GTANetworkAPI;
using GVRP.Module.Chat;
using GVRP.Module.ClientUI.Windows;
using GVRP.Module.Logging;
using GVRP.Module.Players.Events;
using System.Threading.Tasks;
using GVRP.Module.Configurations;
using GVRP.Module.Customization;
using System.Net;
using GVRP.Module.Helper;
using GVRP.Module.Time;
using System.Text;
using System.Security.Cryptography;

namespace GVRP.Module.Players.Windows
{
    public class LoginWindow : Window<Func<DbPlayer, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "name")] private string Name { get; }
            [JsonProperty(PropertyName = "rank")] private uint Rank { get; }

            public ShowEvent(DbPlayer dbPlayer, string name, uint rank) : base(dbPlayer)
            {
                Name = name;
                Rank = rank;
            }
        }

        public LoginWindow() : base("Login")
        {
        }

        public override Func<DbPlayer, bool> Show()
        {
            return player => OnShow(new ShowEvent(player, player.GetName(), player.RankId));
        }
        
        [RemoteEvent]
        public void PlayerLogin(Client player, string password)
        {
            try { 
            Console.WriteLine("----------------");
            Console.WriteLine(player.Name);
            Console.WriteLine(password);

            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null) return;

                

                if (dbPlayer.AccountStatus != AccountStatus.Registered)
                  
                {
                    if (dbPlayer == null) return;
                    dbPlayer.SendNewNotification("Sie sind bereits eingeloggt!");
                    TriggerEvent(player, "status", "successfully");
                    
                    return;
                }

                if (player.HasData("KeinPW"))
                {
                    dbPlayer.SendNewNotification("Passwort erfolgreich erstellt.", title: "SERVER", notificationType: PlayerNotification.NotificationType.SERVER);
                    //var Sha = HashThis.CreateSHA256Hash(password);

                    #region sha256
                    SHA256 sha256Hash = SHA256.Create();
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                    StringBuilder stringbuilder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        stringbuilder.Append(bytes[i].ToString("x2"));
                    }
                    #endregion

                    #region md5
                    MD5 md5hash = MD5.Create();
                    byte[] bytes2 = md5hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                    StringBuilder stringbuilder2 = new StringBuilder();
                    for (int i2 = 0; i2 < bytes2.Length; i2++)
                    {
                        stringbuilder2.Append(bytes2[i2].ToString("x2"));
                    }
                    #endregion

                    //var MDNigger = HashThis.CreateMD5Hash(password);
                    var password2 = HashThis.GetSha256Hash(dbPlayer.Salt + password);


                    MySQLHandler.ExecuteAsync($"UPDATE player SET Pass = '{password}' WHERE id = '{dbPlayer.Id}';");
                    MySQLHandler.ExecuteAsync($"UPDATE player SET Salt = '{password2}' WHERE id = '{dbPlayer.Id}';");
                    MySQLHandler.ExecuteAsync($"UPDATE player SET Pw = '1' WHERE id = '{dbPlayer.Id}';");

                    try
                    {
                        // Set Data that Player is Connected
                        dbPlayer.Player.SetData("Connected", true);


                        dbPlayer.AccountStatus = AccountStatus.LoggedIn;

                        //Set online
                        //      Console.WriteLine(DateTime.Now.GetTimestamp());
                        var query =
                            $"UPDATE `player` SET `Online` = '{1}' WHERE `id` = '{dbPlayer.Id}';";
                        MySQLHandler.ExecuteAsync(query);

                        dbPlayer.Player.ResetData("loginStatusCheck");

                        TriggerEvent(player, "status", "successfully");

                        player.SetSharedData("AC_Status", true);

                        // send
                        // data
                        GVRP.Phone.SetPlayerPhoneData(dbPlayer);

                        var duplicates = NAPI.Pools.GetAllPlayers().FindAll(p => p.Name == player.Name && p != player);

                        try
                        {
                            var duplicatesRemoved = Players.Instance.players.RemoveAll(p => p.GetName() == player.Name && p.Player != player);
                        }
                        catch (Exception e)
                        {
                            Logger.Crash(e);
                        }

                        if (duplicates.Count > 0)
                        {
                            try
                            {
                                foreach (var duplicate in duplicates)
                                {
                                    Logger.Debug($"Duplicated Player {duplicate.Name} deleted");

                                    duplicate.Delete();

                                    duplicate.SendNotification("Duplicated Player");
                                    duplicate.Kick();
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Crash(ex);
                                // ignored
                            }
                        }

                        dbPlayer.Firstspawn = true;
                        // Character Sync
                        NAPI.Task.Run(() =>
                        {
                            dbPlayer.ApplyCharacter();
                            dbPlayer.ApplyPlayerHealth();
                            dbPlayer.Player.TriggerEvent("setPlayerHealthRechargeMultiplier");
                        }, 3000);
                        PlayerSpawn.OnPlayerSpawn(player);
                        //dbPlayer.SendNewNotification($"Bitte verbinde auf folgenden Teamspeak für das Ingame-Voice: NEXUS.Rp", PlayerNotification.NotificationType.ADMIN, "Unser Voice-Server ist umgezogen!", 60000);
                        //dbPlayer.SendNewNotification($"Du hast 2 Minuten Zeit dich im anderen Voice einzufinden. Sonst wirst du vom Server gekickt!", PlayerNotification.NotificationType.ADMIN, "ACHTUNG!", 120000);
                        dbPlayer.SetData("login_time", DateTime.Now);
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }


                }

                var pass = password;
                var pass2 = dbPlayer.Password;
          //      Console.WriteLine(pass + " | " + pass2);
                if (pass == pass2)
                {
                    //Console.WriteLine(dbPlayer.Id, dbPlayer.Player.SocialClubName, dbPlayer.Player.Address, 1);

                    try
                    {
                        // Set Data that Player is Connected
                        dbPlayer.Player.SetData("Connected", true);


                        dbPlayer.AccountStatus = AccountStatus.LoggedIn;

                        //Set online
                  //      Console.WriteLine(DateTime.Now.GetTimestamp());
                        var query =
                            $"UPDATE `player` SET `Online` = '{1}' WHERE `id` = '{dbPlayer.Id}';";
                        MySQLHandler.ExecuteAsync(query);

                        dbPlayer.Player.ResetData("loginStatusCheck");

                        TriggerEvent(player, "status", "successfully");

                        player.SetSharedData("AC_Status", true);

                        // send
                        // data
                        GVRP.Phone.SetPlayerPhoneData(dbPlayer);

                        var duplicates = NAPI.Pools.GetAllPlayers().FindAll(p => p.Name == player.Name && p != player);

                        try
                        {
                            var duplicatesRemoved = Players.Instance.players.RemoveAll(p => p.GetName() == player.Name && p.Player != player);
                        }
                        catch (Exception e)
                        {
                            Logger.Crash(e);
                        }

                        if (duplicates.Count > 0)
                        {
                            try
                            {
                                foreach (var duplicate in duplicates)
                                {
                                    Logger.Debug($"Duplicated Player {duplicate.Name} deleted");

                                    duplicate.Delete();

                                    duplicate.SendNotification("Duplicated Player");
                                    duplicate.Kick();
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Crash(ex);
                                // ignored
                            }
                        }

                        dbPlayer.Firstspawn = true;
                        // Character Sync
                        NAPI.Task.Run(() =>
                        {
                            dbPlayer.ApplyCharacter();
                            dbPlayer.ApplyPlayerHealth();
                            dbPlayer.Player.TriggerEvent("setPlayerHealthRechargeMultiplier");
                        }, 3000);
                        PlayerSpawn.OnPlayerSpawn(player);
                        //dbPlayer.SendNewNotification($"Bitte verbinde auf folgenden Teamspeak für das Ingame-Voice: NEXUS.Rp", PlayerNotification.NotificationType.ADMIN, "Unser Voice-Server ist umgezogen!", 60000);
                        //dbPlayer.SendNewNotification($"Du hast 2 Minuten Zeit dich im anderen Voice einzufinden. Sonst wirst du vom Server gekickt!", PlayerNotification.NotificationType.ADMIN, "ACHTUNG!", 120000);
                        dbPlayer.SetData("login_time", DateTime.Now);
                    }
                    catch (Exception e)
                    {
                        Logger.Crash(e);
                    }
                }
                else
                {
               //     Logger.SaveLoginAttempt(dbPlayer.Id, dbPlayer.Player.SocialClubName, dbPlayer.Player.Address, 0);
                    dbPlayer.PassAttempts += 1;

                    if (dbPlayer.PassAttempts >= 3)
                    {
                        //dbPlayer.SendNewNotification("Sie haben ein falsches Passwort 3x eingegeben, Sicherheitskick.", title:"SERVER", notificationType:PlayerNotification.NotificationType.SERVER);
                        TriggerEvent(player, "status", "Passwort wurde 3x falsch eingegeben. Sicherheitskick");
                        player.Kick("Falsches Passwort (3x)");
                        return;
                    }

                    string message = string.Format(

                        "Falsches Passwort ({0}/3)",
                        dbPlayer.PassAttempts);
                    //dbPlayer.SendNewNotification(message, title:"SERVER", notificationType:PlayerNotification.NotificationType.SERVER);
                    TriggerEvent(player, "status", message);
                }
            }));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}