using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Numerics;
using SKM.V3;
using SKM.V3.Methods;
using SKM.V3.Models;

namespace CWthing
{
    using cw;
    using System.IO;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Threading.Tasks;

    public partial class RennysThing : Form
    {
        // Really inconsistent variable declarations
        public int gamePID = 0;
        public IntPtr hProc;
        public IntPtr baseAddress = IntPtr.Zero;
        public Color defaultColor = Color.Black;
        public bool trainerOn = false;
        public Process gameProc;
        public Single playerSpeed = -1f;
        public int zmTeleportDistance = 150;
        public bool ammoFrozen;
        public int[] ammoVals = new int[6];
        public int[] maxAmmoVals = new int[6];
        public bool freezePlayer;
        public Vector3 frozenPlayerPos = Vector3.Zero;
        public Vector3 lastKnownPlayerPos = Vector3.Zero;
        public Vector3 updatedPlayerPos = Vector3.Zero;
        public Vector3 teleportSpawnPos = Vector3.Zero;

        public Single xpModifier = 1.0f;
        public Single gunXpModifier = 1.0f;


        // Big thanks to JayKoZa2015 on UnKnoWnCheaTs for the following addresses and offsets.
        // Source will have these blanked, be sure to change them to their latest values!

        public IntPtr PlayerBase = (IntPtr)0xFC297B8;
        public IntPtr ZMXPScaleBase = (IntPtr)0xFC517B0;
        public IntPtr TimeScaleBase = (IntPtr)0xECF9C74;
        public IntPtr CMDBufferBase = (IntPtr)0x115DF970;
        public IntPtr XPScaleZM = (IntPtr)0x0;
        public IntPtr GunXPScaleZM = (IntPtr)0x0;
        public IntPtr JumpHeightBase = (IntPtr)0xFD20448; // JumpHeightBase as Pointer + 0x130 (Default 39.0f)



        public IntPtr PlayerCompPtr, PlayerPedPtr, ZMGlobalBase, ZMBotBase, ZMBotListBase, ZMXPScalePtr;

        public const int PC_ArraySize_Offset = 0xB830;

        public const int PC_CurrentUsedWeaponID = 0x28;
        public const int PC_SetWeaponID = 0xB0; // +(1-5 * 0x40 for WP2 to WP6)
        public const int PC_InfraredVision = 0xE66; // (byte) On=0x10|Off=0x0
        public const int PC_GodMode = 0xE67; // (byte) On=0xA0|Off=0x20
        public const int PC_RapidFire1 = 0xE6C;
        public const int PC_Coords = 0xDE8; // Vector3 Writeable
        public const int PC_RapidFire2 = 0xE80;
        public const int PC_MaxAmmo = 0x1360; // +(1-5 * 0x8 for WP1 to WP6)
        public const int PC_Ammo = 0x13D4; // +(1-5 * 0x4 for WP1 to WP6)
        public const int PC_Points = 0x5CE4;
        public const int PC_Name = 0x5BDA;
        public const int PC_RunSpeed = 0x5C30;
        public const int PC_ClanTags = 0x605C;

        public const int PP_ArraySize_Offset = 0x5F8;

        public const int PP_Health = 0x398;
        public const int PP_MaxHealth = 0x39C;
        public const int PP_Coords = 0x2D4;
        public const int PP_Heading_Z = 0x34;
        public const int PP_Heading_XY = 0x38;

        public const int ZM_Global_ZombiesIgnoreAll = 0x14;

        public const int ZM_Global_ZMLeftCount = 0x3C;

        public const int ZM_Bot_List_Offset = 0x8;

        public const int ZM_Bot_ArraySize_Offset = 0x5F8;

        public const int ZM_Bot_Health = 0x398;
        public const int ZM_Bot_MaxHealth = 0x39C;
        public const int ZM_Bot_Coords = 0x2D4;

        public const int XPEP_Offset = 0x20;

        public const int XPUNK01_Offset = 0x24;
        public const int XPUNK02_Offset = 0x28;
        public const int XPUNK03_Offset = 0x2c;
        public const int XPGun_Offset = 0x30;
        public const int XPUNK04_Offset = 0x34;
        public const int XPUNK05_Offset = 0x38;
        public const int XPUNK06_Offset = 0x3c;
        public const int XPUNK07_Offset = 0x40;
        public const int XPUNK08_Offset = 0x44;
        public const int XPUNK09_Offset = 0x48;
        public const int XPUNK10_Offset = 0x4C;

        public const int CMDBB_Exec = -0x1B;






        public Player[] players;
        private bool mouseDown;
        int mouseX = 0, mouseY = 0;

        public Player getPlayerByIndex(int index)
        {
            return players[index];
        }
        public Player getPlayer()
        {
            return players[0];
        }

      

    public class Player{


           public Player(int id)
            {
                playerIndex = id;
            }

            int playerIndex;

            public Boolean godMode = false;
            public Boolean infiniteEss = false;
           public  Boolean freezeAmmo = false;
            public Boolean oneHpZombies = false;
            public Boolean infared = false;
            public Boolean cursorspawn = false;
            public Boolean spawnonpos = false;
            public Single playerSpeed = -1f;
            public Boolean speedToggle = false;
            public Single playerJump = -1f;
            public Boolean jumpToggle = false;
            public Boolean playerxpmod = false;
            public Boolean gunxpmod = false;
            public Single xpModifier = 1.0f;
            public Single gunXpModifier = 1.0f;
            public Boolean setweapon = false;
            public int setweaponid = 0;
            public Boolean unlockcamos = false;
            public Boolean autoweapon = false;
            public Boolean freezeplayer = false;




            void writeToggles()
            {

            }

        }

        private static DialogResult ShowInputDialog(ref string input)
        {

            int xOffset = 300;
            System.Drawing.Size size = new System.Drawing.Size(400, 70);
            Form inputBox = new Form();

            inputBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            inputBox.ClientSize = size;
            inputBox.Text = "Please enter your license key";

            System.Windows.Forms.TextBox textBox = new TextBox();
            textBox.Size = new System.Drawing.Size(size.Width - 10, 23);
            textBox.Location = new System.Drawing.Point(5, 5);
            textBox.Text = input;
            inputBox.Controls.Add(textBox);

            Button okButton = new Button();
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Size = new System.Drawing.Size(75, 23);
            okButton.Text = "&OK";
            okButton.Location = new System.Drawing.Point(size.Width - 80 - 80, 39);
            inputBox.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            cancelButton.Name = "cancelButton";
            cancelButton.Size = new System.Drawing.Size(75, 23);
            cancelButton.Text = "&Cancel";
            cancelButton.Location = new System.Drawing.Point(size.Width - 80, 39);
            inputBox.Controls.Add(cancelButton);

            inputBox.AcceptButton = okButton;
            inputBox.CancelButton = cancelButton;

            inputBox.Location = new System.Drawing.Point(200, 200);

            DialogResult result = inputBox.ShowDialog();
            input = textBox.Text;
            return result;
        }
        public void writeLicense(String licenseKey)
        {
            String dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\ColdWarTrainer\";

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                GrantAccess(dir);
            }
            System.IO.File.Create(dir + "key.txt").Close();
            System.IO.File.WriteAllText(dir+ "key.txt", licenseKey);
        }
        private void GrantAccess(string fullPath)
        {
            DirectoryInfo dInfo = new DirectoryInfo(fullPath);
            DirectorySecurity dSecurity = dInfo.GetAccessControl();
            dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
        }
        public String readLicense()

        {
            String dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\ColdWarTrainer\";
            String key = " ";
            if (System.IO.File.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\ColdWarTrainer\key.txt"))
            {
               key =  System.IO.File.ReadAllText(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + @"\ColdWarTrainer\key.txt");
            }
            return key;
        }
        public Boolean verifyOnStartup()
        {
            if(readLicense().Length > 5)
            {
                if (verifyLicense(readLicense()))
                {
                    return true;
                }
                else
                {
                    System.IO.File.Delete(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/ColdWarTrainer/key.txt");
                    Application.Exit();
                    return false;
                }
            }
            else
            {
                String key = "";
                ShowInputDialog(ref key);
                if (verifyLicense(key))
                {
                   writeLicense(key);
                    return true;
                }
                else
                {
                    Application.Exit();
                    return false;
                }
            }
        }
        public Boolean verifyLicense(String licenseKey)
        {

            var RSAPubKey = "<RSAKeyValue><Modulus>q3CwN3qAqm2ZbFBrO8f+zJA9e2dkDZskw9tEsgdb+gDiKjKjLPM3Po7fRvfYYgVUjCo84qiul3CKIiU6NuOPIXBMKR4G2itneOjBb+P9zseR/A656lpY7umYOh1xMEBIc9N9jUU2Kw7VsG1ELQj4C5HRqzZZvoBKNAosTiUjcpx9Njg42My0Iy/SnKLcTCrMkCduGD2zZp3N4jWvq2l+4mWu2pHUQ4HymGmR0SU336dDFOEZA8pEzvB3ncPz/BGXJDjWpUU5anj0OG5gAp7iS30kDsfX6x077l3/awZ/aRX+bOgXuvFqY9C2KI3RhZnoHR5A88phMiR7IwqL1y4jgw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            var auth = "WyIzNzM4ODkiLCIrK0loWXhHTFB5U3pOT1V6eGFkOVBrMWIyVVM0Mm1BZnZyMzlqZzExIl0=";
            var result = Key.Activate(token: auth, parameters: new ActivateModel()
            {
                Key = licenseKey,
                ProductId = 9498,
                Sign = true,
                MachineCode = Helpers.GetMachineCode()
            });

            if (result == null || result.Result == ResultType.Error ||
                !result.LicenseKey.HasValidSignature(RSAPubKey).IsValid())
            {
                ConsoleOut("The license does not work.");
                return false;
            }
            else
            {
                // everything went fine if we are here!
                ConsoleOut("The license is valid!");
                return true;
            }
            Console.ReadLine();
        }
        public RennysThing()
        {
            InitializeComponent();
            if (verifyOnStartup())
            {
                players = new Player[5];
                players[0] = new Player(0);
            }
            else
            {
                Application.Exit();
                return;
            }
        }
        private void pictureBox5_Click(object sender, EventArgs e)
        {
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.ControlBox = false;
            this.Text = string.Empty;
            int mouseX = 0, mouseY = 0;
            bool mouseDown;
        }
        private void btnOnOff_Click(object sender, EventArgs e)
        {
            // Basic toggle button that enables/disables the tool.
            trainerOn = !trainerOn;

            if (trainerOn)
            {
                btnOnOff.Text = "Online!";
                btnOnOff.ForeColor = Color.Green;
                ConsoleOut("The program has started running...");
            }
            else
            {
                btnOnOff.Text = "Offline!";
                btnOnOff.ForeColor = Color.Red;
                ConsoleOut("The program has stopped...");
            }
        }

        // Don't mind the generic control names and their relevant functions, was too lazy to change them every time.

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            // Updates playerSpeed and syncs the trackBar and numericUpDown, then writes the value to the player speed memory address.

            getPlayer().playerSpeed = (float)numericUpDown1.Value;
            trackBar1.Value = Convert.ToInt32(numericUpDown1.Value);

            cwapi.WriteProcessMemory(hProc, PlayerCompPtr + PC_RunSpeed, Convert.ToSingle(getPlayer().playerSpeed), 4, out _);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            // Updates playerSpeed and syncs the trackBar and numericUpDown, then writes the value to the player speed memory address.

            getPlayer().playerSpeed = trackBar1.Value;
            numericUpDown1.Value = trackBar1.Value;

            cwapi.WriteProcessMemory(hProc, PlayerCompPtr + PC_RunSpeed, Convert.ToSingle(getPlayer().playerSpeed), 4, out _);
        }

       //// // How i use it in C#
       // public async Task CMDBuffer_Exec(string Command)
       // {
         // //  await WriteString(0x10DD4A80, Command + "\0"); // Write Command
         //   await Write<bool>(0x10DD4A80 - 0x1B, true); // Execute
         //   await Task.Delay(15); // Wait
          //  await Write<bool>(0x10DD4A80 - 0x1B, false); // Stop spam if Input-Command is wrong
          //  await WriteString(0x10DD4A80, "\0"); // clear Input-Command
       // }
        private void RennysThing_Load(object sender, EventArgs e)
        {
            // Init with console messages

            ConsoleOut("Call of Duty Black Ops : Cold War Trainer has Started.....");
            if (!backgroundWorker1.IsBusy) backgroundWorker1.RunWorkerAsync();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            // Updates Zombie TP distance, syncs both the trackbar and numud

            zmTeleportDistance = trackBar2.Value;
            numericUpDown2.Value = Convert.ToInt32(trackBar2.Value);

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            // Updates Zombie TP distance, syncs both the trackbar and numud

            zmTeleportDistance = Convert.ToInt32(numericUpDown2.Value);
            trackBar2.Value = Convert.ToInt32(numericUpDown2.Value);

        }


        // Unusused trackbars and numuds, couldn't get the player position to update in my limited time of testing

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            numericUpDown3.Value = trackBar3.Value;

            frozenPlayerPos.X = (float)numericUpDown3.Value;
            UpdatePlayerPosition(frozenPlayerPos);
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {

            trackBar3.Value = Convert.ToInt32(numericUpDown3.Value);

            frozenPlayerPos.X = (float)numericUpDown3.Value;
            UpdatePlayerPosition(frozenPlayerPos);
        }

      

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            numericUpDown4.Value = trackBar4.Value;

            frozenPlayerPos.Z = (float)numericUpDown4.Value;
            UpdatePlayerPosition(frozenPlayerPos);
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            trackBar4.Value = Convert.ToInt32(numericUpDown4.Value);

            frozenPlayerPos.Z = (float)numericUpDown4.Value;
            UpdatePlayerPosition(frozenPlayerPos);
        }


        // Backround worker function

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    // if trainer isnt enabled, do nothing yet
                    if (!trainerOn) continue;

                    // get all processes called "BlackOpsColdWar"
                    var gameProcs = Process.GetProcessesByName("BlackOpsColdWar");

                    // if there aren't any processes, update the game message label and do nothing
                    if (gameProcs.Length < 1)
                    {
                        UpdateLabel(lblGameRunning, "Game is not running", "Red");
                        continue;
                    }

                    // get first process from the gameProcs array
                    gameProc = gameProcs[0];

                    // set gamePID to the Id of the gameProc
                    gamePID = gameProc.Id;

                    // update the label as needed, if for whatever reason the gamePID doesnt exist, update the label and do nothing
                    if (gamePID > 0)
                    {
                        UpdateLabel(lblGameRunning, "Game is running! PID: " + gamePID, "Green");
                    }
                    else
                    {
                        UpdateLabel(lblGameRunning, "Game is not running", "Red");
                        continue;
                    }

                    // opens the process or something, not 100% still learning all this terminology
                    hProc = cwapi.OpenProcess(cwapi.ProcessAccessFlags.All, false, gameProc.Id);

                    // if the base address isn't uptodate, update it
                    if (baseAddress != cwapi.GetModuleBaseAddress(gameProc, "BlackOpsColdWar.exe")) baseAddress = cwapi.GetModuleBaseAddress(gameProc, "BlackOpsColdWar.exe");

                    // cache the base addresses for these various pointers
                    if (PlayerCompPtr != cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + PlayerBase.ToInt64()), new int[] { 0 }))
                        PlayerCompPtr = cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + PlayerBase.ToInt64()), new int[] { 0 });

                    if (PlayerPedPtr != cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + PlayerBase.ToInt64() + 0x8), new int[] { 0 }))
                        PlayerPedPtr = cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + PlayerBase.ToInt64() + 0x8), new int[] { 0 });

                    if (ZMGlobalBase != cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + PlayerBase.ToInt64() + 0x60), new int[] { 0 }))
                        ZMGlobalBase = cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + PlayerBase.ToInt64() + 0x60), new int[] { 0 });

                    if (ZMBotBase != cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + PlayerBase.ToInt64() + 0x68), new int[] { 0 }))
                        ZMBotBase = cwapi.FindDMAAddy(hProc, (IntPtr)(baseAddress.ToInt64() + PlayerBase.ToInt64()) + 0x68, new int[] { 0 });

                    if (ZMBotBase != (IntPtr)0x0 && ZMBotBase != (IntPtr)0x68 && ZMBotListBase != cwapi.FindDMAAddy(hProc, ZMBotBase + ZM_Bot_List_Offset, new int[] { 0 }))
                        ZMBotListBase = cwapi.FindDMAAddy(hProc, ZMBotBase + ZM_Bot_List_Offset, new int[] { 0 });

                    // create new byte array for player coordinates, reads them, and then sets the XYZ coordinates accordingly
                    byte[] playerCoords = new byte[12];
                    cwapi.ReadProcessMemory(hProc, PlayerPedPtr + PP_Coords, playerCoords, 12, out _);
                    var origx = BitConverter.ToSingle(playerCoords, 0);
                    var origy = BitConverter.ToSingle(playerCoords, 4);
                    var origz = BitConverter.ToSingle(playerCoords, 8);
                    // updates the current playerposition with a Vector3 created from the xyz coordinates
                    updatedPlayerPos = new Vector3((float)Math.Round(origx, 4), (float)Math.Round(origy, 4), (float)Math.Round(origz, 4));

                    // unused, no idea what i was doing - something something setting player to the last known position if the freezeplayer checkbox is checked
                    if (freezePlayer)
                    {
                        if (frozenPlayerPos == Vector3.Zero) frozenPlayerPos = (lastKnownPlayerPos == Vector3.Zero) ? updatedPlayerPos : lastKnownPlayerPos;

                        UpdatePlayerPosition(frozenPlayerPos);
                    }




                    // on first loop set player speed in GUI and tool to ingame speed
                    if (getPlayer().playerSpeed < 0)
                    {
                        byte[] plrSpd = new byte[4];
                        cwapi.ReadProcessMemory(hProc, PlayerCompPtr + PC_RunSpeed, plrSpd, 4, out _);
                        trackBar1.Value = Convert.ToInt32(BitConverter.ToSingle(plrSpd, 0));
                        numericUpDown1.Value = Convert.ToDecimal(BitConverter.ToSingle(plrSpd, 0));
                    }

                    // if ammo is frozen, set all weapon ammo to 20
                    if (getPlayer().freezeAmmo)
                    {
                        for (int i = 1; i < 6; i++)
                        {
                            cwapi.WriteProcessMemory(hProc, PlayerCompPtr + PC_Ammo + (i * 0x4), 20, 4, out _);
                        }
                    }

                    // if godmode is checked, enable godmode, else disable it
                    if (getPlayer().godMode)
                    {
                        cwapi.WriteProcessMemory(hProc, PlayerCompPtr + PC_GodMode, 0xA0, 1, out _);
                    }
                    else
                    {
                        cwapi.WriteProcessMemory(hProc, PlayerCompPtr + PC_GodMode, 0x20, 1, out _);
                    }

                    if (getPlayer().jumpToggle)
                    {

                        byte[] jumpBuffer = new byte[4];

                        Buffer.BlockCopy(BitConverter.GetBytes(getPlayer().playerJump), 0, jumpBuffer, 0, 4);
;
                        cwapi.WriteProcessMemory(hProc, (IntPtr)(baseAddress.ToInt64() + JumpHeightBase.ToInt64())+0x130, jumpBuffer, 4, out _);
                    }

                    if (getPlayer().oneHpZombies || getPlayer().cursorspawn || getPlayer().spawnonpos)
                    {
                        byte[] enemyPosBuffer = new byte[12];

                        if (getPlayer().cursorspawn || getPlayer().spawnonpos)
                        {
                            // gets current player position
                            byte[] playerHeadingXY = new byte[4];
                            byte[] playerHeadingZ = new byte[4];
                            cwapi.ReadProcessMemory(hProc, PlayerPedPtr + PP_Heading_XY, playerHeadingXY, 4, out _);
                            cwapi.ReadProcessMemory(hProc, PlayerPedPtr + PP_Heading_Z, playerHeadingZ, 4, out _);

                            // some stack overflow magic to get the direction the player is facing and getting a position in front of the player
                            var pitch = -ConvertToRadians(BitConverter.ToSingle(playerHeadingZ, 0));
                            var yaw = ConvertToRadians(BitConverter.ToSingle(playerHeadingXY, 0));
                            var x = Convert.ToSingle(Math.Cos(yaw) * Math.Cos(pitch));
                            var y = Convert.ToSingle(Math.Sin(yaw) * Math.Cos(pitch));
                            var z = Convert.ToSingle(Math.Sin(pitch));

                            // im guessing just a straight up BitConverter.GetBytes could have worked for writing vector3s to memory instead of this kinda messy solution
                            var newEnemyPos = updatedPlayerPos + (new Vector3(x, y, z) * zmTeleportDistance);

                            if (getPlayer().spawnonpos)
                            {
                                newEnemyPos = teleportSpawnPos;
                            }

                            Buffer.BlockCopy(BitConverter.GetBytes(newEnemyPos.X), 0, enemyPosBuffer, 0, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(newEnemyPos.Y), 0, enemyPosBuffer, 4, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(newEnemyPos.Z), 0, enemyPosBuffer, 8, 4);
                        }

                        for (int i = 0; i < 90; i++)
                        {
                            if (getPlayer().oneHpZombies)
                            {
                                cwapi.WriteProcessMemory(hProc, ZMBotListBase + (ZM_Bot_ArraySize_Offset * i) + ZM_Bot_Health, 1, 4, out _);
                                cwapi.WriteProcessMemory(hProc, ZMBotListBase + (ZM_Bot_ArraySize_Offset * i) + ZM_Bot_MaxHealth, 1, 4, out _);
                            }

                            if (getPlayer().cursorspawn || getPlayer().spawnonpos)
                            {
                                cwapi.WriteProcessMemory(hProc, ZMBotListBase + (ZM_Bot_ArraySize_Offset * i) + ZM_Bot_Coords, enemyPosBuffer, 12, out _);
                            }
                        }
                    }


                    // infrared vision toggle
                    if (getPlayer().infared)
                    {
                        cwapi.WriteProcessMemory(hProc, PlayerCompPtr + PC_InfraredVision, new byte[] { 0x10 }, 1, out _);
                    }
                    else
                    {
                        cwapi.WriteProcessMemory(hProc, PlayerCompPtr + PC_InfraredVision, new byte[] { 0x0 }, 1, out _);
                    }

                    if (getPlayer().infiniteEss)
                    {
                        cwapi.WriteProcessMemory(hProc, PlayerCompPtr + PC_Points, 100000, 8, out _);
                    }

       
                        /**
                        // if the value is 0 or less, set both weapon xp and player xp modifiers to their defaults in the game (in case someone has a legit xp booster or something)
                        if (numericUpDown4.Value <= 0)
                        {
                            byte[] _tempBuffer = new byte[4];
                            cwapi.ReadProcessMemory(hProc, (IntPtr)(baseAddress.ToInt64() + ZMXPScaleBase.ToInt64()) + XPGun_Offset, _tempBuffer, 4, out _);
                            numericUpDown4.Value = (decimal)BitConverter.ToSingle(_tempBuffer, 0);
                            trackBar4.Value = (int)BitConverter.ToSingle(_tempBuffer, 0);
                        }

                        if (numericUpDown2.Value <= 0)
                        {
                            byte[] _tempBuffer = new byte[4];
                            cwapi.ReadProcessMemory(hProc, (IntPtr)(baseAddress.ToInt64() + ZMXPScaleBase.ToInt64()) + XPUNK02_Offset, _tempBuffer, 4, out _);
                            numericUpDown2.Value = (decimal)BitConverter.ToSingle(_tempBuffer, 0);
                            trackBar2.Value = (int)BitConverter.ToSingle(_tempBuffer, 0);
                        }
                        **/
                        // writes the xp modifier values to memory, i guess just a straight up BitConverter.GetBytes could've worked without the creation of the byte buffers
                        byte[] tempBuffer1 = new byte[4];
                        Buffer.BlockCopy(BitConverter.GetBytes(getPlayer().gunXpModifier), 0, tempBuffer1, 0, 4);
                        byte[] tempBuffer2 = new byte[4];
                        Buffer.BlockCopy(BitConverter.GetBytes(getPlayer().xpModifier), 0, tempBuffer2, 0, 4);

                        if (getPlayer().gunxpmod)
                        {
                            cwapi.WriteProcessMemory(hProc, (IntPtr)(baseAddress.ToInt64() + ZMXPScaleBase.ToInt64()) + XPGun_Offset, tempBuffer1, 4, out _);

                        }
                        if (getPlayer().playerxpmod){

                            cwapi.WriteProcessMemory(hProc, (IntPtr)(baseAddress.ToInt64() + ZMXPScaleBase.ToInt64()) + XPUNK02_Offset, tempBuffer2, 4, out _);

                        }
                    

                    // gets zombies left and updates the label of the Zombies left counter
                    byte[] zombiesLeft = new byte[4];
                    cwapi.ReadProcessMemory(hProc, ZMGlobalBase + ZM_Global_ZMLeftCount, zombiesLeft, 4, out _);
                    lblZombiesLeft.Text = "Zombies Left: " + BitConverter.ToInt32(zombiesLeft, 0);

                    // updates the lastknownplayerpos variable to the current players position
                    lastKnownPlayerPos = updatedPlayerPos;

                    // if there was an error with these memory reads/writes, output it to the gui console
                    if (Marshal.GetLastWin32Error() != 0)
                    {
                        ConsoleOut(Marshal.GetLastWin32Error().ToString());
                    }
                }
                // if an error happened during the loop, output that to the gui console
                catch (Exception err)
                {
                    ConsoleOut(err.Message);
                }
            }
        }




        private void trackBar4_Scroll_1(object sender, EventArgs e)
        {
            getPlayer().gunXpModifier = (float)trackBar4.Value;
            numericUpDown4.Value = (decimal)trackBar4.Value;
        }

        private void numericUpDown4_ValueChanged_1(object sender, EventArgs e)
        {
            getPlayer().gunXpModifier = (float)numericUpDown4.Value;
            trackBar4.Value = (int)numericUpDown4.Value;
        }

        private void trackBar2_Scroll_1(object sender, EventArgs e)
        {
            getPlayer().xpModifier = (float)trackBar2.Value;
            numericUpDown2.Value = (decimal)trackBar2.Value;
        }

        private void numericUpDown2_ValueChanged_1(object sender, EventArgs e)
        {
            getPlayer().xpModifier = (float)numericUpDown2.Value;
            trackBar2.Value = (int)numericUpDown2.Value;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (getPlayer().godMode)
            {
                this.button2.Image = CWthing.Properties.Resources.offbutton;
                getPlayer().godMode = false;
            }
            else {
                this.button2.Image = CWthing.Properties.Resources.onbutton;
                getPlayer().godMode = true;
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (getPlayer().infiniteEss)
            {
                this.button3.Image = CWthing.Properties.Resources.offbutton;
                getPlayer().infiniteEss = false;
            }
            else
            {
                this.button3.Image = CWthing.Properties.Resources.onbutton;
                getPlayer().infiniteEss = true;
            }
        }


        private void button14_Click(object sender, EventArgs e)
        {
            if (getPlayer().playerxpmod)
            {
                this.button14.Image = CWthing.Properties.Resources.offbutton;
                getPlayer().playerxpmod = false;
            }
            else
            {
                this.button14.Image = CWthing.Properties.Resources.onbutton;
                getPlayer().playerxpmod = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (getPlayer().freezeAmmo)
            {
                this.button4.Image = CWthing.Properties.Resources.offbutton;
                getPlayer().freezeAmmo = false;
            }
            else
            {
                this.button4.Image = CWthing.Properties.Resources.onbutton;
                getPlayer().freezeAmmo = true;
            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            if (getPlayer().oneHpZombies)
            {
                this.button5.Image = CWthing.Properties.Resources.offbutton;
                getPlayer().oneHpZombies = false;
            }
            else
            {
                this.button5.Image = CWthing.Properties.Resources.onbutton;
                getPlayer().oneHpZombies = true;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (getPlayer().infared)
            {
                this.button6.Image = CWthing.Properties.Resources.offbutton;
                getPlayer().infared = false;
            }
            else
            {
                this.button6.Image = CWthing.Properties.Resources.onbutton;
                getPlayer().infared = true;
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (getPlayer().speedToggle)
            {
                this.button10.Image = CWthing.Properties.Resources.offbutton;
                getPlayer().speedToggle = false;
            }
            else
            {
                this.button10.Image = CWthing.Properties.Resources.onbutton;
                getPlayer().speedToggle = true;
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (getPlayer().jumpToggle)
            {
                this.button11.Image = CWthing.Properties.Resources.offbutton;
                getPlayer().jumpToggle = false;
            }
            else
            {
                this.button11.Image = CWthing.Properties.Resources.onbutton;
                getPlayer().jumpToggle = true;
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (getPlayer().gunxpmod)
            {
                this.button12.Image = CWthing.Properties.Resources.offbutton;
                getPlayer().gunxpmod = false;
            }
            else
            {
                this.button12.Image = CWthing.Properties.Resources.onbutton;
                getPlayer().gunxpmod = true;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (getPlayer().freezeplayer)
            {
                this.button9.Image = CWthing.Properties.Resources.offbutton;
                getPlayer().freezeplayer = false;
            }
            else
            {
                this.button9.Image = CWthing.Properties.Resources.onbutton;
                getPlayer().freezeplayer = true;
            }
        }


        // Don't mind the generic control names and their relevant functions, was too lazy to change them every time.

        private void numericUpDown3_ValueChanged_1(object sender, EventArgs e)
        {
            // Updates playerSpeed and syncs the trackBar and numericUpDown, then writes the value to the player speed memory address.

            getPlayer().playerSpeed = (float)numericUpDown1.Value;
            trackBar1.Value = Convert.ToInt32(numericUpDown1.Value);

            if (getPlayer().jumpToggle)
            {
                byte[] jumpBuffer = new byte[4];

                Buffer.BlockCopy(BitConverter.GetBytes(getPlayer().playerJump), 0, jumpBuffer, 0, 4);
                ;
                cwapi.WriteProcessMemory(hProc, (IntPtr)(baseAddress.ToInt64() + JumpHeightBase.ToInt64()) + 0x130, jumpBuffer, 4, out _);
            }
            }

        private void trackBar3_Scroll_1(object sender, EventArgs e)
        {

            getPlayer().playerSpeed = trackBar3.Value;
            numericUpDown3.Value = trackBar3.Value;


            if (getPlayer().jumpToggle)
            {

                byte[] jumpBuffer = new byte[4];

                Buffer.BlockCopy(BitConverter.GetBytes(getPlayer().playerJump), 0, jumpBuffer, 0, 4);
                ;
                cwapi.WriteProcessMemory(hProc, (IntPtr)(baseAddress.ToInt64() + JumpHeightBase.ToInt64()) + 0x130, jumpBuffer, 4, out _);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (getPlayer().cursorspawn)
            {
                this.button7.Image = CWthing.Properties.Resources.offbutton;
                getPlayer().cursorspawn = false;
            }
            else
            {
                this.button7.Image = CWthing.Properties.Resources.onbutton;
                getPlayer().cursorspawn = true;
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (getPlayer().spawnonpos)
            {
                this.button15.Image = CWthing.Properties.Resources.offbutton;
                getPlayer().spawnonpos = false;
            }
            else
            {
                this.button15.Image = CWthing.Properties.Resources.onbutton;
                getPlayer().spawnonpos = true;
                this.teleportSpawnPos = updatedPlayerPos;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            getPlayer().setweaponid = comboBox1.SelectedIndex;

            int weaponId = 0;

            //Change to real weapon id from the index
            switch (getPlayer().setweaponid)
            {
                case 0://No item
                    weaponId = 0;
                    break;
                case 1: //Sledgehammer
                    weaponId = 21;
                    break;
                case 2://Porters X2 Ray Gun
                    weaponId = 298;
                    break;
             case 3://Glitter Knife
                    weaponId = 282;
                    break;
             case 4://Drone Squad
                    weaponId = 58;
                    break;
                case 5://XM4
                    weaponId = 236;
                    break;
                case 6://AK47
                    weaponId = 245;
                    break;
                case 7://Krig - 6
                    weaponId = 228;
                    break;
                case 8://QBZ - 83
                    weaponId = 247;
                    break;
                case 9://FFAR
                    weaponId = 248;
                    break;
                case 10://GROZA
                    weaponId = 270;
                    break;
                case 11://MP5
                    weaponId = 293;
                    break;
                case 12://Milano 821
                    weaponId = 286;
                    break;
                case 13://AK-74u
                    weaponId = 314;
                    break;
                case 14://KSP-45
                    weaponId = 305;
                    break;
                case 15://Bullfrog
                    weaponId = 258;
                    break;
                case 16://Mac 10
                    weaponId = 315;
                    break;
                case 17://Gallo SA12
                    weaponId = 322;
                    break;
                case 18://Hauer
                    weaponId = 309;
                    break;
                case 19://Streetsweeper
                    weaponId = 206;
                    break;
                case 20://Pellington
                    weaponId = 265;
                    break;
                case 21://LW3 - Tundra
                    weaponId = 225;
                    break;
                case 22://M82
                    weaponId = 241;
                    break;
                case 23://Type 63
                    weaponId = 294;
                    break;
                case 24://M16
                    weaponId = 311;
                    break;
                case 25://DMR
                    weaponId = 212;
                    break;
                case 26://Stoner 63
                    weaponId = 271;
                    break;
                case 27://RPD
                    weaponId = 307;
                    break;
                case 28://M60
                    weaponId = 243;
                    break;
                case 29://1911
                    weaponId = 230;
                    break;
                case 30://Magnum
                    weaponId = 224;
                    break;
                case 31://Diamatti
                    weaponId = 323;
                    break;
                case 32://Knfie
                    weaponId = 318;
                    break;
                case 33://Sledgehammer
                    weaponId = 288;
                    break;
                case 34://Wakizashi
                    weaponId = 257;
                    break;
                case 35://RPG
                    weaponId = 237;
                    break;
                case 36://Cigma
                    weaponId = 264;
                    break;
                case 37://M79
                    weaponId = 299;
                    break;
                case 38://AUG
                    weaponId = 212;
                    break;


                default:
                    break;
            }
            getPlayer().setweaponid = weaponId;
            numericUpDown5.Value = weaponId;
            if (getPlayer().setweapon)
            {

                cwapi.WriteProcessMemory(hProc, PlayerCompPtr + PC_SetWeaponID, weaponId, 4, out _);

            }
        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            byte[] enemyPosBuffer = new byte[12];
            int index = comboBox2.SelectedIndex;

           // gets current player position
            byte[] playerHeadingXY = new byte[4];
            byte[] playerHeadingZ = new byte[4];
            cwapi.ReadProcessMemory(hProc, PlayerPedPtr + PP_Heading_XY, playerHeadingXY, 4, out _);
            cwapi.ReadProcessMemory(hProc, PlayerPedPtr + PP_Heading_Z, playerHeadingZ, 4, out _);

            // some stack overflow magic to get the direction the player is facing and getting a position in front of the player
            var pitch = -ConvertToRadians(BitConverter.ToSingle(playerHeadingZ, 0));
            var yaw = ConvertToRadians(BitConverter.ToSingle(playerHeadingXY, 0));
            ///float x = Convert.ToSingle(Math.Cos(yaw) * Math.Cos(pitch));
            ///float y = Convert.ToSingle(Math.Sin(yaw) * Math.Cos(pitch));
            ///float z = Convert.ToSingle(Math.Sin(pitch));
            var x = 892.8182;
            var y = 49.16034;
            var z = 51.3728;



            switch (index)
            {
                case 0://To Host config for now
                    x = 892.8182;
                    y = 49.16034;
                    z = 51.3728;

                    break;
                case 1://Mb Spawn
                    x = 892.8182;
                    y = 49.16034;
                    z = 51.3728;
                    break;
                case 2://Mb Nacht
                    x = -70.68573;
                    y = 735.1465;
                    z = 1.125;
                    break;
                case 3://Mb Airplane
                    x = 192.0121;
                    y = 2281.86;
                    z = 220.6327;
                    break;
                case 4://Mb Swamp
                    x = -1100.365;
                    y = 331.656;
                    z = -42.21415;
                    break;
                case 5://Mb Power Room
                    x = 504.4311;
                    y = -337.5037;
                    z = -671.875;
                    break;
                case 6://Mb Lab 1
                    x = 732.1216;
                    y = 1886.927;
                    z = -287.875;
                    break;
                case 7: //Mb Lab 2
                    x = -1482.76;
                    y = 9.853629;
                    z = -367.875;
                    break;
                case 8: //Spawn
                    x = 744.556;
                    y = -351.8875;
                    z = -33.54857;
                    break;
                case 9: //Airplane
                    x = -210.1693;
                    y = 1580.252;
                    z = 343.98;
                    break;
                case 10://Sniper's Nest
                    x = 33.50085;
                    y = 793.6158;
                    z = 293.125;
                    break;
                case 11: //Swamp
                    x = -1751.83;
                    y = 225.7206;
                    z = -34.31071;
                    break;
                case 12: //Pack a punch
                    x = 622.7785;
                    y = -182.1142;
                    z = -543.875;

                    break;
                case 13://Power Room
                    x = 1024.632;
                    y = -706.3374;
                    z = -255.875;
                    break;
                default:
                    break;
            }

            var playerNewPos = new Vector3((float) x, (float) y, (float) z);

            Buffer.BlockCopy(BitConverter.GetBytes(playerNewPos.X), 0, enemyPosBuffer, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(playerNewPos.Y), 0, enemyPosBuffer, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(playerNewPos.Z), 0, enemyPosBuffer, 8, 4);

            cwapi.WriteProcessMemory(hProc, PlayerCompPtr + PC_Coords, enemyPosBuffer, 12, out _);

        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (getPlayer().setweapon)
            {
                this.button13.Image = CWthing.Properties.Resources.offbutton;
                getPlayer().setweapon = false;
            }
            else
            {
                this.button13.Image = CWthing.Properties.Resources.onbutton;
                getPlayer().setweapon = true;
            }
        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            int weaponId = (int) numericUpDown5.Value;
            if (getPlayer().setweapon)
            {

                cwapi.WriteProcessMemory(hProc, PlayerCompPtr + PC_SetWeaponID, weaponId, 4, out _);

            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void label21_Click(object sender, EventArgs e)
        {

        }

        private void button16_Click(object sender, EventArgs e)
        {

        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                mouseX = MousePosition.X - 202;
                mouseY = MousePosition.Y - 10;

                this.SetDesktopLocation(mouseX, mouseY);
            }
        }




        // attempted to set the player position in a similar way to how I TP the zombies but it doesn't appear to work
        public void UpdatePlayerPosition(Vector3 pos)
        {
            byte[] tempPosBuffer = new byte[12];
            Buffer.BlockCopy(BitConverter.GetBytes(pos.X), 0, tempPosBuffer, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(pos.Y), 0, tempPosBuffer, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(pos.Z), 0, tempPosBuffer, 8, 4);

            cwapi.WriteProcessMemory(hProc, PlayerPedPtr + PP_Coords, tempPosBuffer, 12, out _);
        }

        // outputs a string to the gui console
        public void ConsoleOut(string str)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(ConsoleOut), new object[] { str });
                return;
            }
            DateTime curDT = DateTime.Now;
            txtConsole.AppendText(curDT.ToString("d-MMM-yyyy HH:mm:ss - ") + str + Environment.NewLine);
        }

        // updates a label with new text and colour, with multi-thread support for when the background worker needs to do it
        public void UpdateLabel(Label label, string text, string color = "Black")
        {
            if (this.InvokeRequired)
            {
                label.Invoke((MethodInvoker)delegate ()
                {
                    label.Text = text;
                    label.ForeColor = Color.FromName(color);
                });
                return;
            }
            label.Text = text;
            label.ForeColor = Color.FromName(color);
        }

        // something for something that i dont use anymore
        public string ToHex(object num)
        {
            return string.Format("0x{0:X}", num);
        }

        // converts a degree angle to radians, for the tp zombies feature
        public double ConvertToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }
    }
}