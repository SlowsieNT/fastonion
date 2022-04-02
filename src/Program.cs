using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TorFlow;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace fastonion
{
    public class JSON
    { // System.Web.Extensions.dll
        public static System.Web.Script.Serialization.JavaScriptSerializer m_JSS = new System.Web.Script.Serialization.JavaScriptSerializer();
        public static T Parse<T>(string aString)
        {
            try { return m_JSS.Deserialize<T>(aString); } catch { return default(T); }
        }
        public static string ToString<T>(T aObject)
        {
            try { return m_JSS.Serialize(aObject); } catch { return ""; }
        }
    }
    public class JSettings
    {
        //emptyDir dataDir onionPort localserverPort torrcFilePath torExePath
        public string EmptyDir, DataDir, TorrcFilePath, TorExePath;
        public int OnionPort, ServerPort;
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            new Program(args);
            Application.Run();
        }
        TorProcess m_TorProcess;
        NotifyIcon m_NI = new NotifyIcon();
        StringBuilder m_Logs = new StringBuilder();
        ContextMenuStrip m_CMS1 = new ContextMenuStrip();
        Form m_DummyForm = new Form();
        public Program(string[] aArgs)
        {
            var vSettings = JSON.Parse<JSettings>(File.ReadAllText(aArgs[0]));
            m_NI = new NotifyIcon();
            var vItem = m_CMS1.Items.Add("Show Logs");
            var vItem2 = m_CMS1.Items.Add("Kill tor");
            var vItem3 = m_CMS1.Items.Add("Restart tor");
            var vItem4 = m_CMS1.Items.Add("Quit + Kill tor");
            var vItem5 = m_CMS1.Items.Add("Quit (no tor kill)");
            vItem.Click += m_CMS1_ShowLogsItemClick;
            vItem2.Click += delegate (object sender, EventArgs e) { m_TorProcess.Kill(false); };
            vItem3.Click += delegate (object sender, EventArgs e) { m_TorProcess.Kill(false); Thread.Sleep(50); m_TorProcess.Run(); };
            vItem4.Click += delegate (object sender, EventArgs e) { m_TorProcess.Kill(false); Application.Exit(); };
            vItem5.Click += delegate (object sender, EventArgs e) { Application.Exit(); };
            m_CMS1.Items.AddRange(new ToolStripItem[] { vItem, vItem2, vItem3, vItem4, vItem5 });
            m_NI.ContextMenuStrip = m_CMS1;
            m_NI.Text = "fastonion";
            m_NI.Visible = true;
            m_NI.Icon = new Icon(m_DummyForm.Icon, 40, 40);
            if (!File.Exists(vSettings.TorExePath)) {
                MessageBox.Show("Tor.exe not found: " + vSettings.TorExePath);
                Application.Exit();
            }
            // Very simple instancing
            m_TorProcess = new TorProcess {
                // (Both paths are full filenames)
                ExecutablePath = vSettings.TorExePath,
                TorrcFilePath = vSettings.TorrcFilePath
            };
            // Once you call AddHiddenService, it will return HiddenService
            // HiddenService.AddPort will return HiddenService
            // Meaning you can call AddPort over and over again
            m_TorProcess.Torrc.AddHiddenService(vSettings.EmptyDir).AddPort(vSettings.OnionPort, vSettings.ServerPort);
            // Handle events
            m_TorProcess.OnLine += VTorProcess_OnLine;
            m_TorProcess.OnReady += VTorProcess_OnReady;
            m_TorProcess.OnState += VTorProcess_OnState;
            m_TorProcess.OnHiddenServiceCreated += VTorProcess_OnHiddenServiceCreated;
            // Set data directory where the tor is supposed to write files
            m_TorProcess.Torrc.DataDirectory = vSettings.DataDir;
            // Run the Thread
            m_TorProcess.Run();
        }
        

       

        private void m_CMS1_ShowLogsItemClick(object sender, EventArgs e) {
            var vForm1 = new Form1("" + m_Logs);
            vForm1.ShowDialog();
        }

        private void VTorProcess_OnHiddenServiceCreated(TorProcess aTorProc, TorProcess.HiddenService aValue) {
            m_Logs.AppendLine("--- Onions ---");
            m_Logs.AppendLine(aValue.Hostname.Trim() + ":[" + aValue.ToString(1) + "]");
        }

        private void VTorProcess_OnState(TorProcess aTorProc, int aValue) {
            
        }

        private void VTorProcess_OnReady(TorProcess aTorProc) {
            
        }

        private void VTorProcess_OnLine(TorProcess aTorProc, string aValue) {
            
        }
    }
}
