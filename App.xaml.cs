﻿using DiskoAIO.MVVM.View;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace DiskoAIO
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string strWorkPath { get; set; }
        public static List<AccountGroup> accountsGroups { get; set; } = new List<AccountGroup>();
        public static List<ProxyGroup> proxyGroups { get; set; } = new List<ProxyGroup>();
        public static MainWindow mainWindow { get; set; }
        public static ProxiesView proxiesView { get; set; } = null;
        public static AccountsView accountsView { get; set; } = null;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            strWorkPath = Path.GetDirectoryName(strExeFilePath);
            SetAccountGroups();

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
            this.Dispatcher.UnhandledException += App_DispatcherUnhandledException;

            mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void SetAccountGroups()
        {
            if (!Directory.Exists(strWorkPath + "/groups"))
            {
                Directory.CreateDirectory(strWorkPath + "/groups");
            }
            foreach(var file in Directory.GetFiles(strWorkPath + "/groups"))
            {
                var name = file.Split('\\').Last().Split('.')[0];
                var tokens = new List<DiscordToken>();
                using (var reader = new StreamReader(file))
                {
                    var line = reader.ReadLine();
                    while (line != null && line != "")
                    {
                        try
                        {
                            line = line.Trim(new char[] { '\n', '\t', '\r', ' ' });
                            var token_array = line.Split(':');
                            var token = DiscordToken.Load(token_array);
                            line = reader.ReadLine();
                            tokens.Add(token);
                        }
                        catch (FormatException ex)
                        {
                            ;
                        }
                    }
                }
                var group = new AccountGroup(tokens, name, false);
                if (group != null && group._name != "")
                    accountsGroups.Add(group);
            }

            if (!Directory.Exists(strWorkPath + "/proxies"))
            {
                Directory.CreateDirectory(strWorkPath + "/proxies");
            }
            foreach (var file in Directory.GetFiles(strWorkPath + "/proxies"))
            {
                var name = file.Split('\\').Last().Split('.')[0];
                var proxies = new List<DiscordProxy>();
                using (var reader = new StreamReader(file))
                {
                    var line = reader.ReadLine();
                    while (line != null && line != "")
                    {
                        try
                        {
                            line = line.Trim(new char[] { '\n', '\t', '\r', ' ' });
                            var proxy_array = line.Split(':');
                            DiscordProxy proxy = null;
                            if (proxy_array.Length > 2)
                            {
                                if (int.TryParse(proxy_array[1], out var port))
                                    proxy = new DiscordProxy(proxy_array[0], port, proxy_array[2], proxy_array[3]);
                                else
                                    proxy = new DiscordProxy(proxy_array[2], int.Parse(proxy_array[3]), proxy_array[1], proxy_array[2]);
                            }
                            else
                            {
                                proxy = new DiscordProxy(proxy_array[0], int.Parse(proxy_array[1]));
                            }
                            line = reader.ReadLine();
                            proxies.Add(proxy);
                        }
                        catch (FormatException ex)
                        {
                            ;
                        }
                    }
                }
                var group = new ProxyGroup(proxies, name, false);
                if(group != null && group._name != "")
                    proxyGroups.Add(group);
            }
        }
        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            if (args.IsTerminating)
                DiscordDriver.CleanUp();
        }
        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception

            // Prevent default unhandled exception processing
            e.Handled = true;
        }
        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);    //we’ve reached the end of the tree
            if (parentObject == null) return null;
            //check if the parent matches the type we’re looking for
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }
    }
    public enum GiveawayType
    {
        Button,
        Reaction
    }
    public enum TaskType
    {
        Join,
        Leave,
        Giveaway,
        CheckWin
    }
}
