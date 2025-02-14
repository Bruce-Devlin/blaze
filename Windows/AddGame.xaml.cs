﻿using Blaze.Functions;
using DiscordRPC;
using Microsoft.Win32;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Blaze.Windows
{
    /// <summary>
    /// Interaction logic for AddGame.xaml
    /// </summary>
    public partial class AddGame : Window
    {
        public static Home home;

        public Game newGame = new Game();

        private void CloseBtn_Click(object sender, RoutedEventArgs e) { this.Close(); }
        public AddGame(Home home)
        {
            InitializeComponent();
            this.MouseLeftButtonDown += delegate { DragMove(); };

            Functions.Discord.discord.client.ClearPresence();
            Functions.Discord.discord.client.SetPresence(new RichPresence()
            {
                Details = "Adding a game...",
                Timestamps = Functions.Discord.startTime,
                Assets = new Assets()
                {
                    LargeImageKey = "nutural",
                    LargeImageText = "Devlin.gg/Blaze",
                }
            });
        }

        private void SelectFileBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                InitialDirectory = @"D:\",
                Title = "Find Game Executable",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "exe",
                Filter = "EXE files|*.exe",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if ((bool) fileDialog.ShowDialog())
            {       
                newGame.Filename = fileDialog.SafeFileName;

                SelectFileBtn.Content = System.IO.Path.GetFileName(newGame.Filename);
            }
        }

        private async void AddGameBtn_Click(object sender, RoutedEventArgs e)
        {
            if (GameTitleBox.Text != "" && AppIDBox.Text != "")
            {
                newGame.Title = GameTitleBox.Text;
                uint appID = uint.Parse(AppIDBox.Text);
                newGame.AppID = appID;

                Process game = new Process();
                SteamClient.Init(newGame.AppID);
                game.StartInfo.FileName = SteamApps.AppInstallDir(newGame.AppID) + "\\" + newGame.Filename;
                SteamClient.Shutdown();
                game.Start();
                newGame.PlainName = game.ProcessName;
                game.Kill();

                await Functions.Games.AddGame(GameTitleBox.Text, appID, newGame.Filename, newGame.PlainName);

                await home.UpdateGames();
                this.Close();
            }
            else if (GameTitleBox.Text != "")
            {
                //game title empty
            }
            else
            {
                //app id empty
            }
        }

        

        private void GameTitleBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { GameTitleBox.Text = ""; }

        private void AppIDBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { AppIDBox.Text = ""; }
    }
}
