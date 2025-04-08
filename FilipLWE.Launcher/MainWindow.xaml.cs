using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using Microsoft.Win32;

namespace FilipLWE.Launcher
{
    public partial class MainWindow : Window
    {
        private Process? serverProcess;

        public MainWindow()
        {
            InitializeComponent();
            ShowLocalIp();
        }

        private void ShowLocalIp()
        {
            string localIP = "Neznáma IP";
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            catch { }

            IpAddressText.Text = $"🖥 IP adresa: {localIP}";
        }

        private void BtnNastav_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tu budú nastavenia (DB, port...)", "Nastavenia");
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (serverProcess != null && !serverProcess.HasExited)
            {
                MessageBox.Show("Server už beží.");
                return;
            }

            try
            {
                using var regKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\FilipLWE");
                if (regKey == null)
                {
                    MessageBox.Show("❌ Server nie je nainštalovaný. Chýba kľúč v registri.", "Chyba");
                    return;
                }

                string? installPath = regKey.GetValue("InstallPath") as string;
                if (string.IsNullOrEmpty(installPath))
                {
                    MessageBox.Show("❌ Server nie je správne nainštalovaný. Chýba cesta v registri.", "Chyba");
                    return;
                }

                string serverExe = Path.Combine(installPath, "FilipLWE.Server.exe");
                if (!File.Exists(serverExe))
                {
                    MessageBox.Show("❌ Server nie je správne nainštalovaný. Súbor FilipLWE.Server.exe sa nenašiel.", "Chyba");
                    return;
                }

                serverProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = serverExe,
                        Arguments = "--urls=http://localhost:5000",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = installPath
                    }
                };

                serverProcess.Start();
                MessageBox.Show("✅ Server bol spustený.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Chyba pri spúšťaní servera:\n{ex.Message}", "Chyba");
            }
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            if (serverProcess == null || serverProcess.HasExited)
            {
                MessageBox.Show("Server nie je spustený.");
                return;
            }

            try
            {
                serverProcess.Kill(true);
                serverProcess.WaitForExit();
                serverProcess = null;
                MessageBox.Show("🛑 Server bol zastavený.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Chyba pri zastavovaní servera:\n{ex.Message}", "Chyba");
            }
        }

        private void BtnStatus_Click(object sender, RoutedEventArgs e)
        {
            if (serverProcess != null && !serverProcess.HasExited)
                MessageBox.Show("✅ Server je spustený.");
            else
                MessageBox.Show("❌ Server nie je spustený.");
        }

        private void BtnKoniec_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (serverProcess != null && !serverProcess.HasExited)
                {
                    serverProcess.Kill(true);
                    serverProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Server sa nepodarilo korektne ukončiť:\n{ex.Message}", "Chyba");
                return;
            }

            Application.Current.Shutdown();
        }
    }
}
