using RainbowDraw.LOGIC;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RainbowDraw
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ApplicationSetting Setting { get; private set; }
        private System.Threading.Mutex mutex = new System.Threading.Mutex(false, "RainbowDraw");

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Setting = ApplicationSetting.Load();
            Setting.Save();

            if (!mutex.WaitOne(0, false))
            {
                mutex.Close();
                mutex = null;
                this.Shutdown();
            }
            else
            {
                if (Update())
                {
                    var result = MessageBox.Show("A new version has been registered.\nWould you like to confirm?", "Update", MessageBoxButton.YesNo);
                    if(result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(App.Setting.HelpUrl);
                    }
                    else
                    {
                        RainbowDraw.MainWindow.GetInstance().Show();
                    }
                }
                else
                {
                    RainbowDraw.MainWindow.GetInstance().Show();
                }

            }

        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex.Close();
            }
        }

        public bool Update()
        {
            return false;
            WebRequest wrGETURL = WebRequest.Create(App.Setting.UpdateUrl);
            string sLine = String.Empty;
            StringBuilder htmlSb = new StringBuilder();
            using (StreamReader reader = new StreamReader(wrGETURL.GetResponse().GetResponseStream()))
            {
                while (sLine != null)
                {
                    sLine = reader.ReadLine();

                    if (sLine != null)
                    {
                        htmlSb.Append(sLine);
                    }
                }
            }

            string html = htmlSb.ToString();
            HtmlParser hp = new HtmlParser(html);
            while(true)
            {
                hp.ParseNext("div", out HtmlTag ht);
                if(ht == null)
                {
                    break;
                }
                var attrList = ht.Attributes;
                if (attrList.ContainsKey("id"))
                {
                    if (attrList["id"] == "RainbowDraw")
                    {
                        if (attrList.ContainsKey("data-v"))
                        {
                            Version newVer = new Version(attrList["data-v"]);
                            Version thisVer = Assembly.GetExecutingAssembly().GetName().Version;

                            var result = newVer.CompareTo(thisVer);
                            if (result > 0)
                                return true;
                            else
                                return false;

                        }
                        return false;
                    }
                }
            }
            return false;
        }
    }
}
