﻿using Blaze.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaze
{
    class Variables
    {
        public static bool DiscordConnected = false;
        public static bool FirstRun = true;
        public static bool Updated = false;

        public static string AppVersion = "";
        public static string Username = "";

        public static Game CurrGame = new Game();
        public static List<Server> ServerList = new List<Server>();
        public static List<Game> GameList = new List<Game>();
    }
}