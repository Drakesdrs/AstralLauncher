using System;
using System.Text;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Net.Http;
using DiscordRPC;
using System.Linq;
using System.Xml;
using System.Drawing;

namespace Astral_Launcher
{
    public partial class Astral : Form
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess,
            IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        // privileges
        const int PROCESS_CREATE_THREAD = 0x0002;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_READ = 0x0010;

        // used for memory allocation
        const uint MEM_COMMIT = 0x00001000;
        const uint MEM_RESERVE = 0x00002000;
        const uint PAGE_READWRITE = 4;
        public DiscordRpcClient client;
        string discordTime = "";


        public Astral()
        {
            InitializeComponent();
            versionFinderForLabel("Get-AppPackage -name Microsoft.MinecraftUWP | select -expandproperty Version", VersionDisplay);
            ComboBoxText();
            NewsfeedLoader();
            InitializeDiscordHome("In Launcher");
            if (Settings.Default.AfterLaunch == "Hide")
            {
                HideLauncherButton.Checked = true;

            }
            else if (Settings.Default.AfterLaunch == "Close")
            {
                CloseLauncherButton.Checked = true;
            }
            else if (Settings.Default.AfterLaunch == "Minimize")
            {
                KeepOpenButton.Checked = true;
            }
            guna2ComboBox2.SelectedItem = Settings.Default.Priority;
            guna2ToggleSwitch1.Checked = Settings.Default.Animations;
            guna2ComboBox1.SelectedItem = Settings.Default.Resolution;
            AnimationsOff();
            IDCheck();
        }
        
        public void IDCheck()
        { //https://raw.githubusercontent.com/xarson/Astral_Client.DLL/main/betausers
            WebClient webclient = new WebClient();
            string BetaIDs = webclient.DownloadString("https://raw.githubusercontent.com/xarson/Astral_Client.DLL/main/Newsfeed%20Data/News3.md");
            if (BetaIDs.Contains(Settings.Default.UserId.ToString()))
            {
                DiscordPanel.Visible = false;
            }
            else
            {
                DiscordPanel.Visible = true;
            }
            if (Settings.Default.UserId == 0)
            {
                DiscordPanel.Visible = true;
                DiscordPanel.BringToFront();
            }
        }

        public void NewsfeedLoader()
        {
            WebRequest req = WebRequest.Create("https://github.com/xarson/Astral_Client.DLL/raw/main/Newsfeed%20Data/picture1.png");
            Stream stream = req.GetResponse().GetResponseStream();
            System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
            this.guna2PictureBox2.Image = img;

            WebClient webClient = new WebClient();
            string NewsFeedPanel1 = webClient.DownloadString("https://raw.githubusercontent.com/xarson/Astral_Client.DLL/main/Newsfeed%20Data/News1.md");
            richTextBox1.Text = NewsFeedPanel1;

            string NewsFeedPanel2 = webClient.DownloadString("https://raw.githubusercontent.com/xarson/Astral_Client.DLL/main/Newsfeed%20Data/News2.md");
            richTextBox2.Text = NewsFeedPanel2;

            string NewsFeedPanel3 = webClient.DownloadString("https://raw.githubusercontent.com/xarson/Astral_Client.DLL/main/Newsfeed%20Data/News3.md");
            richTextBox3.Text = NewsFeedPanel3;

            WebRequest request2 = WebRequest.Create("https://github.com/xarson/Astral_Client.DLL/raw/main/Newsfeed%20Data/picture2.jpg");
            Stream stream2 = request2.GetResponse().GetResponseStream();
            System.Drawing.Image img2 = System.Drawing.Image.FromStream(stream2);
            this.guna2PictureBox3.Image = img2;
        }

        public static void versionFinderForLabel(string script, Label version)
        {
            using (PowerShell powerShell = PowerShell.Create())
            {
                powerShell.AddScript(script);
                powerShell.AddCommand("Out-String");
                Collection<PSObject> PSOutput = powerShell.Invoke();
                StringBuilder stringBuilder = new StringBuilder();
                foreach (PSObject pSObject in PSOutput)
                    stringBuilder.AppendLine(pSObject.ToString());
                version.Text = stringBuilder.ToString();
            }
        }

        public void InitializeDiscordHome(string status)
        {
            int TimestampStart = 0;
            int TimestampEnd = 0;
            dynamic DateTimestampEnd = null;

            if (discordTime != "" && Int32.TryParse(discordTime, out TimestampEnd))
                DateTimestampEnd = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(TimestampEnd);

            client = new DiscordRpcClient("722349140180205611");
            client.Initialize();
            client.SetPresence(new RichPresence()
            {
                Details = status,

                Assets = new Assets()
                {

                    LargeImageKey = "logonewdiscord",
                    LargeImageText = "Astral Launcher",
                },
                Timestamps = new Timestamps()
                {
                    Start = discordTime != "" && Int32.TryParse(discordTime, out TimestampStart) ? new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(TimestampStart) : DateTime.UtcNow,
                    End = DateTimestampEnd
                }

            });
        }

        public void InitializeDiscordHomeButton(string status)
        {
            int TimestampStart = 0;
            int TimestampEnd = 0;
            dynamic DateTimestampEnd = null;

            if (discordTime != "" && Int32.TryParse(discordTime, out TimestampEnd))
                DateTimestampEnd = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(TimestampEnd);

            client.SetPresence(new RichPresence()
            {
                Details = status,
                Assets = new Assets()
                {
                    LargeImageKey = "logonewdiscord",
                    LargeImageText = "Astral Launcher",
                },
                Timestamps = new Timestamps()
                {
                    Start = discordTime != "" && Int32.TryParse(discordTime, out TimestampStart) ? new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(TimestampStart) : DateTime.UtcNow,
                    End = DateTimestampEnd
                }

            });
        }


        public void InitializeDiscordSettings(string status)
        {
            int TimestampStart = 0;
            int TimestampEnd = 0;
            dynamic DateTimestampEnd = null;

            if (discordTime != "" && Int32.TryParse(discordTime, out TimestampEnd))
                DateTimestampEnd = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(TimestampEnd);

            client.SetPresence(new RichPresence()
            {
                Details = status,
                Assets = new Assets()
                {
                    LargeImageKey = "logonewdiscord",
                    LargeImageText = "Astral Launcher",
                },
                Timestamps = new Timestamps()
                {
                    Start = discordTime != "" && Int32.TryParse(discordTime, out TimestampStart) ? new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(TimestampStart) : DateTime.UtcNow,
                    End = DateTimestampEnd
                }

            });
        }

        public static void IGNFinder(Label ign)
        {
            string userpath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string fileName = Path.Combine(userpath, @"Packages\Microsoft.XboxApp_8wekyb3d8bbwe\LocalState\XboxLiveGamer.xml");
            XmlDocument XmlInfo = new XmlDocument();
            XmlInfo.Load(fileName);
            XmlElement _XmlElement;
            _XmlElement = XmlInfo.GetElementsByTagName("Gamertag")[0] as XmlElement;
            string InGameName = _XmlElement.InnerText;
            ign.Text = InGameName;
        }
        public void InitializeDiscordInGame(string status)
        {
            int TimestampStart = 0;
            int TimestampEnd = 0;
            dynamic DateTimestampEnd = null;

            if (discordTime != "" && Int32.TryParse(discordTime, out TimestampEnd))
                DateTimestampEnd = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(TimestampEnd);

            client.SetPresence(new RichPresence()
            {
                Details = status,
                State = "on " + VersionDisplay.Text,
                Assets = new Assets()
                {
                    LargeImageKey = "logonewdiscord",
                    LargeImageText = "Astral Launcher",
                    SmallImageKey = "minecraft",
                    SmallImageText = "Minecraft Bedrock Edition"
                },
                Timestamps = new Timestamps()
                {
                    Start = discordTime != "" && Int32.TryParse(discordTime, out TimestampStart) ? new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(TimestampStart) : DateTime.UtcNow,
                    End = DateTimestampEnd
                }

            });
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (!DownloadProgress.Visible) DownloadProgress.Visible = true;
            DownloadProgress.Maximum = int.Parse((e.BytesReceived / 100).ToString());
            DownloadProgress.Value += e.ProgressPercentage;
            LaunchStatus.Text = ("Status: Downloading Latest Updates...");
            Thread.Sleep(100);
        }

        private void OpenMC()
        {
            LaunchStatus.Text = ("Status: Opening Minecraft");
            Thread.Sleep(100);
            Process.Start("minecraft://");

            Thread.Sleep(5000);
            Inject();

            DownloadProgress.Value = 0;
            DownloadProgress.Visible = false;
        }

        private void Inject()
        {
            InjectDLL(Directory.GetCurrentDirectory().ToString() + "/AstralClient.dll");
        }

        private void InjectMC()
        {
            LaunchStatus.Text = ("Status: Injecting into Minecraft...");
            Thread.Sleep(100);
            check();
            Thread.Sleep(5000);
            Inject();
            DownloadProgress.Value = 0;
            DownloadProgress.Visible = false;
        }

        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            DownloadProgress.Visible = false;
            Process[] pname = Process.GetProcessesByName("Minecraft.Windows");
            if (pname.Length == 0)
                OpenMC();
            else
                InjectMC();

            LaunchStatus.Text = ("Status: Successfully launched Minecraft.");

            if (Settings.Default.AfterLaunch == "Hide")
            {
                Thread.Sleep(200);
                Hide();
                notifyIcon1.Visible = true;
            }

            if (Settings.Default.AfterLaunch == "Close")
            {
                Close();
            }

            if (Settings.Default.AfterLaunch == "Minimize")
            {

                WindowState = FormWindowState.Minimized;
            }

            Process[] MinecraftIndex = Process.GetProcessesByName("Minecraft.Windows");
            if (MinecraftIndex.Length > 0)
            {
                Process Minecraft = Process.GetProcessesByName("Minecraft.Windows")[0];
                if (guna2ComboBox1.SelectedItem == "1920x1080")
                {
                    MoveWindow(Minecraft.MainWindowHandle, 0, 0, 1920, 1080, true);
                }
                else if (guna2ComboBox1.SelectedItem == "1600x900")
                {
                    MoveWindow(Minecraft.MainWindowHandle, 0, 0, 1600, 900, true);
                }
                else if (guna2ComboBox1.SelectedItem == "1280x720")
                {
                    MoveWindow(Minecraft.MainWindowHandle, 0, 0, 1280, 720, true);
                }
            }

            Process[] processes = Process.GetProcessesByName("Minecraft.Windows");
            foreach (Process proc in processes)
            {
                if (guna2ComboBox2.SelectedItem == "RealTime")
                {
                    proc.PriorityClass = ProcessPriorityClass.RealTime;
                }
                else if (guna2ComboBox2.SelectedItem == "High")
                {
                    proc.PriorityClass = ProcessPriorityClass.High;
                }
                else if (guna2ComboBox2.SelectedItem == "High")
                {
                    proc.PriorityClass = ProcessPriorityClass.High;
                }
                else if (guna2ComboBox2.SelectedItem == "AboveNormal")
                {
                    proc.PriorityClass = ProcessPriorityClass.AboveNormal;
                }
                else if (guna2ComboBox2.SelectedItem == "Normal")
                {
                    proc.PriorityClass = ProcessPriorityClass.Normal;
                }
                else if (guna2ComboBox2.SelectedItem == "BelowNormal")
                {
                    proc.PriorityClass = ProcessPriorityClass.BelowNormal;
                }
                else if (guna2ComboBox2.SelectedItem == "Idle")
                {
                    proc.PriorityClass = ProcessPriorityClass.Idle;
                }
            }

        }



        public static void InjectDLL(string DLLPath)
        {
            Process[] targetProcessIndex = Process.GetProcessesByName("Minecraft.Windows");
            if (targetProcessIndex.Length > 0)
            {
                applyAppPackages(DLLPath);

                Process targetProcess = Process.GetProcessesByName("Minecraft.Windows")[0];

                IntPtr procHandle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, targetProcess.Id);

                IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

                IntPtr allocMemAddress = VirtualAllocEx(procHandle, IntPtr.Zero, (uint)((DLLPath.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

                UIntPtr bytesWritten;
                WriteProcessMemory(procHandle, allocMemAddress, Encoding.Default.GetBytes(DLLPath), (uint)((DLLPath.Length + 1) * Marshal.SizeOf(typeof(char))), out bytesWritten);
                CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);


            }
        }

        public static void applyAppPackages(string DLLPath)
        {
            FileInfo InfoFile = new FileInfo(DLLPath);
            FileSecurity fSecurity = InfoFile.GetAccessControl();
            fSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier("S-1-15-2-1"), FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            InfoFile.SetAccessControl(fSecurity);
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click_1(object sender, EventArgs e)
        {
            //Home Button
            InitializeDiscordHomeButton("In Launcher");

            //Button Checks
            HomeButton.Checked = true;
            SettingsButton.Checked = false;
            NewsButton.Checked = false;

            //Changing UI
            LaunchPanel.Visible = true;
            guna2Panel1.Visible = false;
            NewsfeedPanel.Visible = false;
        }

        private void guna2Button2_Click_1(object sender, EventArgs e)
        {
            InitializeDiscordSettings("Configuring Settings");

            //Button Checks
            SettingsButton.Checked = true;
            HomeButton.Checked = false;
            NewsButton.Checked = false;

            //Changing UI According To Button Checked
            guna2Panel1.Visible = true;
            LaunchPanel.Visible = false;
            NewsfeedPanel.Visible = false;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void DownloadProgress_ValueChanged_1(object sender, EventArgs e)
        {


        }

        private void LaunchPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void VersionDisplayTopText_Click(object sender, EventArgs e)
        {

        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        private void guna2Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void VersionDisplay_Click(object sender, EventArgs e)
        {

        }

        private void NewsButton_Click(object sender, EventArgs e)
        {
            InitializeDiscordSettings("Reading Astral News");

            //Button Checks
            SettingsButton.Checked = false;
            HomeButton.Checked = false;
            NewsButton.Checked = true;

            //Changing UI According To Button Checked
            guna2Panel1.Visible = false;
            LaunchPanel.Visible = false;
            NewsfeedPanel.Visible = true;
        }

        private void HideLauncherButton_Click(object sender, EventArgs e)
        {
            HideLauncherButton.Checked = true;
            CloseLauncherButton.Checked = false;
            KeepOpenButton.Checked = false;
            Settings.Default.AfterLaunch = "Hide";
            Settings.Default.Save();
        }

        private void CloseLauncherButton_Click(object sender, EventArgs e)
        {
            HideLauncherButton.Checked = false;
            CloseLauncherButton.Checked = true;
            KeepOpenButton.Checked = false;
            Settings.Default.AfterLaunch = "Close";
            Settings.Default.Save();
        }

        private void MinimizeToTrayButton_Click(object sender, EventArgs e)
        {
            HideLauncherButton.Checked = false;
            CloseLauncherButton.Checked = false;
            KeepOpenButton.Checked = true;
            Settings.Default.AfterLaunch = "Minimize";
            Settings.Default.Save();
        }

        private void LaunchButton_Click_1(object sender, EventArgs e)
        { 
            InitializeDiscordInGame("Playing Minecraft");
            notifyIcon1.Visible = true;

            Cursor.Current = Cursors.WaitCursor;

            DownloadProgress.Visible = true;
            WebClient Client = new WebClient();

            Client.DownloadFileCompleted += Client_DownloadFileCompleted;
            Client.DownloadProgressChanged += Client_DownloadProgressChanged;

            Client.DownloadFileAsync(new Uri("https://github.com/iArsonic/Astral_Client.DLL/releases/latest/download/AstralClient.dll"), "AstralClient.dll");
            Cursor.Current = Cursors.WaitCursor;
            Cursor.Current = Cursors.Default;
        }

        private void guna2ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Default.Resolution = guna2ComboBox1.SelectedItem.ToString();
            Settings.Default.Save();
        }

        private void AnimationsOff()
        {
            if (Settings.Default.Animations == false)
            {
                HideLauncherButton.Animated = false;
                CloseLauncherButton.Animated = false;
                KeepOpenButton.Animated = false;
                guna2ComboBox1.Animated = false;
                guna2ComboBox2.Animated = false;
                guna2ToggleSwitch1.Animated = false;
                HomeButton.Animated = false;
                SettingsButton.Animated = false;
                NewsButton.Animated = false;
                LaunchButton.Animated = false;
                VersionSelector.Animated = false;
            }

            else
            {
                HideLauncherButton.Animated = true;
                CloseLauncherButton.Animated = true;
                KeepOpenButton.Animated = true;
                guna2ComboBox1.Animated = true;
                guna2ComboBox2.Animated = true;
                guna2ToggleSwitch1.Animated = true;
                HomeButton.Animated = true;
                SettingsButton.Animated = true;
                NewsButton.Animated = true;
                LaunchButton.Animated = true;
                VersionSelector.Animated = true;
            }
        }

        private void guna2ToggleSwitch1_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.Animations = guna2ToggleSwitch1.Checked;
            Settings.Default.Save();
            AnimationsOff();
        }

        private void guna2ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Default.Priority = guna2ComboBox2.SelectedItem.ToString();
            Settings.Default.Save();


        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        string VersionPath = @"c:\AstralClient";
        private void guna2ComboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            check();
            DirectoryInfo di = Directory.CreateDirectory(VersionPath);
            VersionInstallerBar.Visible = true;
            LaunchButton.Enabled = false;
        }

        private void client_DownloadFileCompleted1(object sender, AsyncCompletedEventArgs e)
        {
            LaunchButton.Enabled = true;
            VersionInstallerBar.Visible = false;
        }

        private void client_DownloadFileCompleted2(object sender, AsyncCompletedEventArgs e)
        {
            LaunchButton.Enabled = true;
            VersionInstallerBar.Visible = false;
        }

        private void client_DownloadFileCompleted3(object sender, AsyncCompletedEventArgs e)
        {
            LaunchButton.Enabled = true;
            VersionInstallerBar.Visible = false;
        }


        string versionpath1 = @"c:\AstralClient\1.16.100";
        string versionpath2 = @"c:\AstralClient\1.16.20";
        string versionpath3 = @"c:\AstralClient\1.16.40";

        private void check()
        {
            WebClient webClient = new WebClient();

            if (VersionSelector.SelectedItem == "1.16.100")
            {
                if (File.Exists(@"c:\AstralClient\Minecraft-1.16.100.4.Appx"))
                {
                    System.Diagnostics.Process.Start(@"c:\AstralClient\Minecraft-1.16.100.4.Appx");
                }
                
                else
                {
                    DirectoryInfo di = Directory.CreateDirectory(versionpath1);
                    LaunchButton.Enabled = false;
                    webClient.DownloadFileAsync(new Uri("https://github.com/xarson/Astral_Client.DLL/releases/download/1.16.100/Minecraft-1.16.100.4.Appx"),
                        @"c:\AstralClient\Minecraft-1.16.100.4.Appx");
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted1);
                }
            }

            if (VersionSelector.SelectedItem == "1.16.20")
            {
                if (File.Exists(@"c:\AstralClient\Minecraft.1.16.20.Offical.Appx.File.Appx"))
                {
                    System.Diagnostics.Process.Start(@"c:\AstralClient\Minecraft.1.16.20.Offical.Appx.File.Appx");
                }
                else
                {
                    DirectoryInfo di = Directory.CreateDirectory(versionpath2);
                    LaunchButton.Enabled = false;
                    webClient.DownloadFileAsync(new Uri("https://github.com/xarson/Astral_Client.DLL/releases/download/1.16.20/Minecraft.1.16.20.Offical.Appx.File.Appx"),
                        @"c:\AstralClient\Minecraft.1.16.20.Offical.Appx.File.Appx");
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted2);
                }
            }
            //https://github.com/xarson/Astral_Client.DLL/releases/download/1.16.40/Minecraft.1.16.40.Offical.Appx.File.Appx
            if (VersionSelector.SelectedItem == "1.16.40")
            {
                if (File.Exists(@"c:\AstralClient\Minecraft.1.16.40.Offical.Appx.File.Appx"))
                {
                    System.Diagnostics.Process.Start(@"c:\AstralClient\Minecraft.1.16.40.Offical.Appx.File.Appx");
                }
                else
                {
                    DirectoryInfo di = Directory.CreateDirectory(versionpath3);
                    Thread.Sleep(500);
                    LaunchButton.Enabled = false;
                    webClient.DownloadFile(("https://github.com/xarson/Astral_Client.DLL/releases/download/1.16.20/Minecraft.1.16.20.Offical.Appx.File.Appx"),
                        @"c:\AstralClient\Minecraft.1.16.40.Offical.Appx.File.Appx");
                }
            }

            if (VersionSelector.SelectedItem == "1.16.201")
            {
                string message = "This is the latest version of Minecraft. You can download it from the Microsoft Store.";
                string title = "Astral Client";
                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

                LaunchButton.Enabled = true;
                VersionInstallerBar.Visible = false;
            }
        }

        private void ComboBoxText()
        {
            VersionSelector.Text = (VersionDisplay.Text);
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        int r = 255, g = 0, b = 0;

        private void LaunchButton_MouseEnter(object sender, EventArgs e)
        {
            LaunchButton.ImageSize = new Size(485, 60);
        }

        private void LaunchButton_MouseLeave(object sender, EventArgs e)
        {
            LaunchButton.ImageSize = new Size(470, 55);
        }

        private void guna2PictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void DiscordLoginButton_Click(object sender, EventArgs e)
        {
            DiscordLoginButton.Enabled = false;
            User user = client.CurrentUser;
            ulong userID = user.ID;
            Settings.Default.UserId = userID;
            Settings.Default.Save();
            WebClient webClient = new WebClient();
            string betaIDS = webClient.DownloadString("https://raw.githubusercontent.com/xarson/Astral_Client.DLL/main/betausers");
            if (betaIDS.Contains(userID.ToString()))
            {
                DiscordPanel.Visible = false;
            }
            else
            {
                MessageBox.Show("You do not have access to the launcher.", "Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DiscordLogin_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void DiscordPanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2HtmlLabel4_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (r > 255 && b==0) 
            {
                guna2Panel5.BackColor = System.Drawing.Color.FromArgb(153, 50, 204);
            }
            if (g > 0 && r == 255) 
            {
                guna2Panel5.BackColor = System.Drawing.Color.FromArgb(147, 112, 219);
            }
            if (b > 0 && g == 0) 
            {
                guna2Panel5.BackColor = System.Drawing.Color.FromArgb(139, 0, 139);
            }
        }
    }
}