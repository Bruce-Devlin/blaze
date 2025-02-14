﻿using DiscordRPC;
using Newtonsoft.Json;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Blaze.Functions
{
    public class Game
    {
        public ImageSource GameIcon { get; set; }
        public string GIURL { get; set; }
        public string Title { get; set; }
        public uint AppID { get; set; }
        public ImageBrush Background { get; set; }
        public string BGURL { get; set; }
        public string LinkURL { get; set; }
        public string Filename { get; set; }
        public string ServerFilename { get; set; }
        public string PlainName { get; set; }
        public uint ServerAppID { get; set; }
        public bool Running { get; set; }
        public bool BlazingGriffin { get; set; }
    }

    public class PlainGame
    {
        //"{GameIcon},{GameTitle},{AppID},{ImgURL},{LinkURL},{Filename},{ServerFilename},{PlainName},{ServerAppID}"
        public string GameIcon { get; set; }
        public string Title { get; set; }
        public uint AppID { get; set; }
        public string Background { get; set; }
        public string LinkURL { get; set; }
        public string Filename { get; set; }
        public string ServerFilename { get; set; }
        public string Plainname { get; set; }
        public uint ServerAppID { get; set; }
    }

    class Games
    {
        public static async Task<PlainGame> GameToPlain(Game g)
        {
            PlainGame pg = new PlainGame();

            pg.GameIcon = g.GIURL;
            pg.Title = g.Title;
            pg.AppID = g.AppID;
            pg.Background = g.BGURL;
            pg.LinkURL = g.LinkURL;
            pg.Filename = g.Filename;
            pg.ServerFilename = g.ServerFilename;
            pg.Plainname = g.PlainName;
            pg.ServerAppID = g.ServerAppID;

            return pg;
        }

        public static async Task<Game> PlainToGame(PlainGame pg)
        {
            Game g = new Game();

            ImageBrush tmpIMG = await ImgBrushFromURL(pg.GameIcon);
            g.GameIcon = tmpIMG.ImageSource;
            g.GIURL = pg.GameIcon;
            tmpIMG = new ImageBrush();


            g.Title = pg.Title;
            g.AppID = pg.AppID;

            tmpIMG = await ImgBrushFromURL(pg.Background);
            g.Background = tmpIMG;
            g.BGURL = pg.Background;
            tmpIMG = new ImageBrush();

            g.LinkURL = pg.LinkURL;
            g.Filename = pg.Filename;

            if (pg.ServerFilename == null) g.BlazingGriffin = false;
            else
            {
                g.BlazingGriffin = true;
                g.ServerFilename = pg.ServerFilename;
                g.ServerAppID = pg.ServerAppID;
            }
            g.PlainName = pg.Plainname;

            return g;
        }
        public static async Task<ImageBrush> ImgBrushFromURL(string URL)
        {
            WebRequest ImgRequest = WebRequest.Create(URL);
            WebResponse ImgResponse = ImgRequest.GetResponse();

            Stream ImgStream = ImgResponse.GetResponseStream();

            BitmapImage ImgBitmap = new BitmapImage();
            ImgBitmap.BeginInit();
            ImgBitmap.StreamSource = ImgStream;
            ImgBitmap.EndInit();

            ImageBrush ImgBrush = new ImageBrush();
            ImgBrush.ImageSource = ImgBitmap;
            return ImgBrush;
        }

        public static async Task JoinGame(Functions.Server currServer)
        {
            Variables.GameRunning = true;
            try
            {

                SteamClient.Init(Variables.CurrGame.AppID);
                Process game = new Process();

                game.StartInfo.FileName = SteamApps.AppInstallDir(currServer.Game.AppID) + "\\" + Variables.CurrGame.Filename;
                game.StartInfo.Arguments = "-connect=" + currServer.IpandPort();

                string ipAndPort = "";

                if (currServer.Mine) ipAndPort = "192.168.0.1";
                else ipAndPort = currServer.IpandPort();

                game.Start();
                game.WaitForExit();

                //Set status on Discord.
                Functions.Discord.discord.client.ClearPresence();
                List<int> party = new List<int>() { currServer.Info.players + 1, currServer.Info.max_players }; ;

                if (party[0] > party[1]) party[1] = 99;

                string imagekey = "nutural";
                if (Variables.CurrGame.BlazingGriffin) imagekey = Variables.CurrGame.AppID.ToString();
                Functions.Discord.discord.client.SetPresence(new RichPresence()
                {
                    Details = currServer.Info.name,
                    State = "Map: " + currServer.Info.map + " | Players: ",
                    Timestamps = Functions.Discord.startTime,
                    //Buttons = new DiscordRPC.Button[] { new DiscordRPC.Button(){ Label = "Download Blaze!", Url = "https://devlin.gg/Blaze"}},
                    Party = new Party()
                    {
                        ID = currServer.Info.name,
                        Size = party[0],
                        Max = party[1],
                        Privacy = Party.PrivacySetting.Public
                    },
                    Secrets = new Secrets()
                    {
                        JoinSecret = currServer.Game.AppID + "," + currServer.SteamID
                    },
                    Assets = new Assets()
                    {
                        LargeImageKey = imagekey.ToString(),
                        LargeImageText = Variables.CurrGame.Title,
                    }
                });
                game.WaitForExit();
                SteamClient.Shutdown();



                //Set status on Discord.
                Functions.Discord.discord.client.ClearPresence();
                Functions.Discord.discord.client.SetPresence(new RichPresence()
                {
                    Details = "Browsing Servers...",
                    State = "(" + Variables.CurrGame.Title + ")",
                    Timestamps = Functions.Discord.startTime,
                    Buttons = new DiscordRPC.Button[] { new DiscordRPC.Button() { Label = "Download Blaze!", Url = "https://devlin.gg/Blaze" } },
                    Assets = new Assets()
                    {
                        LargeImageKey = "nutural",
                        LargeImageText = "Devlin.gg/Blaze",
                    }
                });
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message.ToString());
            }
        }

        public static async Task<bool> IsGameRunning()
        {
            Process[] processes = Process.GetProcessesByName(Variables.CurrGame.PlainName);
            if (processes.Length > 0) return true;
            else return false;
        }

        public static async Task AddGame(string gameTitle, uint appID, string fileName, string plainName)
        {
            PlainGame pg = new PlainGame();
            Random r = new Random(); 

            pg.GameIcon = "https://devlin.gg/blaze/GameIcons/" + r.Next(0, 8) + ".png";
            pg.Title = gameTitle;
            pg.AppID = appID;
            pg.Background = "https://devlin.gg/blaze/BG.png";
            pg.Filename = fileName;
            pg.LinkURL = "https://devlin.gg/blaze";
            pg.Plainname = plainName;

            Variables.GameList.Add(await PlainToGame(pg));
            await SetLocalGames();

            //"https://devlin.gg/blaze/GI.png," + newGame.Title + "," + newGame.AppID + ",https://devlin.gg/blaze/BG.png,https://devlin.gg/blaze," + newGame.Filename + "," + newGame.PlainName
            //"{GameIcon},{GameTitle},{AppID},{ImgURL},{LinkURL},{filename.exe},{PlainName}"
        }

        public static async Task GetBGGames()
        {

                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync("https://devlin.gg/blaze/games.json");
                var result = await response.Content.ReadAsStringAsync();


                List<PlainGame> pgList = new List<PlainGame>();

                string json = result;
                if (json != "") pgList = JsonConvert.DeserializeObject<List<PlainGame>>(json);

                foreach (PlainGame pg in pgList) Variables.GameList.Add(await PlainToGame(pg));

        }

        public static async Task GetLocalGames()
        {
            if (File.Exists(Variables.ConfigDir + @"\mygames.json"))
            {
                List<PlainGame> pgList = new List<PlainGame>();

                string json = File.ReadAllText(Variables.ConfigDir + @"\mygames.json");
                if (json != "") pgList = JsonConvert.DeserializeObject<List<PlainGame>>(json);

                foreach (PlainGame pg in pgList) Variables.GameList.Add(await PlainToGame(pg));
            }
        }

        public static async Task SetLocalGames()
        {
            List<PlainGame> pgList = new List<PlainGame>();

            foreach (Game g in Variables.GameList.Where(game => game.BlazingGriffin == false)) pgList.Add(await GameToPlain(g));

            if (pgList.Count != 0)
            {
                if (!File.Exists(Variables.ConfigDir + @"\mygames.json")) File.Create(Variables.ConfigDir + @"\mygames.json").Close();
                File.WriteAllText(Variables.ConfigDir + @"\mygames.json", JsonConvert.SerializeObject(pgList, Formatting.Indented));
            }
            else File.Delete(Variables.ConfigDir + @"\mygames.json");
        }

        public static async Task RemoveGames_TMP()
        {
            File.Delete(Variables.ConfigDir + @"\mygames.json");
        }

        public static async Task GetGames()
        {
            await GetBGGames();
            await GetLocalGames();
        }

        private IEnumerable<string> ReadFileLines(string logPath)
        {
            using (StreamReader reader = File.OpenText(logPath))
            {
                string newline = "";
                while ((newline = reader.ReadLine()) != null)
                {
                    yield return newline;
                }
            }
        }
    }
}

