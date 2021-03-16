using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace NSCPatcher
{
    public partial class Window : Form
    {
        string APKname = "";
        int seconds = 0;

        public Window()
        {
            InitializeComponent();
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data") || !File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\apktool.jar") || !File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\apksigner.jar") || !File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\network_security_config.xml") || !File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\cert.pem") || !File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\key.pk8"))
            {
                MessageBox.Show("You're missing critical file(s),\nThe program will not function at all or properly.\n\nA new folder will be created but won't have all the required files.", "Critical error");
                PatchButton.Enabled = false;
                groupBox1.Text = "Please install the required files...";
            }
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Patched_APKs"))
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"Patched_APKs");
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp"))
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp");

            resetbat();
        }
        private void PatchButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileD = new OpenFileDialog();
            fileD.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            fileD.Title = "Select the APK you want to patch";
            fileD.Filter = "Android application (*.apk*)|*.apk*";
            fileD.RestoreDirectory = true;
            if (fileD.ShowDialog() == DialogResult.OK)
            {
                timer.Start();

                PatchButton.Enabled = false;
                progressBar1.Value = 0;
                Program.takaovi_github.groupBox1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.groupBox1.Text = "Starting to patch your APK..."; }));

                APKname = Path.GetFileName(fileD.FileName).Replace(" ", "_");
                File.Copy(fileD.FileName, AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp\" + APKname, true);

                decompile();
            }
        }

        private void decompile()
        {
            string str = File.ReadAllText((AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\decompile.bat"));
            str = str.Replace("<path>", AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp\" + APKname);
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\decompile.bat", str);

            string str6 = File.ReadAllText((AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\decompile.bat"));
            str6 = str6.Replace("apktool.jar", AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\apktool.jar");
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\decompile.bat", str6);

            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.Arguments = "/k \"" + AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\" + "decompile.bat" + "\"";
            p.EnableRaisingEvents = true;
            p.Exited += new EventHandler(p_Exited);
            p.Start();

            progressBar1.Value = 15;
            Program.takaovi_github.groupBox1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.groupBox1.Text = "Decompiling APK..."; }));

            //Continue after decompiled
            void p_Exited(object sender, EventArgs e)
            {
                //Remove old .apk file since it's not needed anymore
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp\" + APKname);

                patch();
            }
        }

        private void patch()
        {
            Program.takaovi_github.progressBar1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.progressBar1.Value = 45; }));
            Program.takaovi_github.groupBox1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.groupBox1.Text = "Patching..."; }));

            string biggestfoldername = "xml";

            int count = 0;

            //Get the file name without ".apk"
            string APKfolder = APKname.Remove(APKname.Length - 4);

            //This string will be added to the Manifest file
            string ManifestPatchString = "android:networkSecurityConfig=\"@" + biggestfoldername + "/network_security_config\"";

            Program.takaovi_github.progressBar1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.progressBar1.Value = 50; }));
            Program.takaovi_github.groupBox1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.groupBox1.Text = "Placing the network security config..."; }));
            //Copy network_security_config file to the application's xml path//////////
            try
            {
                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp\" + APKfolder + @"\res\xml"))
                    File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\network_security_config.xml", AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp\" + APKfolder + @"\res\xml\network_security_config.xml");

                else
                {
                    var allFolders = new List<string>();
                    string path = AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp\" + APKfolder + @"\res\";
                    foreach (string folder in Directory.GetDirectories(path))
                    {
                        if (count < (from file in Directory.EnumerateFiles(folder, "*.xml", SearchOption.AllDirectories) select file).Count())
                        {
                            string name = folder.Remove(0, folder.LastIndexOf('\\') + 1);
                            count = (from file in Directory.EnumerateFiles(folder, "*.xml", SearchOption.AllDirectories) select file).Count();
                            biggestfoldername = name;
                        }
                    }

                    if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp\" + APKfolder + @"\res\" + biggestfoldername))
                        File.Copy(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\network_security_config.xml", AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp\" + APKfolder + @"\res\" + biggestfoldername + @"\network_security_config.xml");
                    ManifestPatchString = "android:networkSecurityConfig=\"@" + biggestfoldername + "/network_security_config\"";

                    //MessageBox.Show(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp\" + APKfolder + @"\res\" + biggestfoldername + @"\network_security_config.xml");
                }
            }
            catch
            {
            }

            Program.takaovi_github.progressBar1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.progressBar1.Value = 55; }));
            Program.takaovi_github.groupBox1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.groupBox1.Text = "Editing the manifest..."; }));
            ////Edit the Manifest file//////////////////////////////////////////////
            try
            {
                string str3 = File.ReadAllText((AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp\" + APKfolder + @"\AndroidManifest.xml"));
                if (!str3.Contains("android:networkSecurityConfig"))
                {
                    str3 = str3.Replace("<application", "<application " + ManifestPatchString);
                }
                else
                {
                    Match match = Regex.Match(str3, "android:networkSecurityConfig=\"([^\"]*)");
                    if (match.Success)
                    {
                        str3 = str3.Replace(match.Groups[1].Value, "@" + biggestfoldername + "/network_security_config");
                    }
                }
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp\" + APKfolder + @"\AndroidManifest.xml", str3);
            }
            catch
            {
            }

            //Path for the Manifest file to be checked 
            string check = File.ReadAllText((AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp\" + APKfolder + @"\AndroidManifest.xml"));

            Program.takaovi_github.progressBar1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.progressBar1.Value = 60; }));
            Program.takaovi_github.groupBox1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.groupBox1.Text = "Verifying..."; }));

            //Check Manifest and xml folder, continue if patches were successful
            if (check.Contains("android:networkSecurityConfig=\"@" + biggestfoldername + "/network_security_config\"") && File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp\" + APKfolder + @"\res\" + biggestfoldername + @"\network_security_config.xml"))
            {
                compile();
            }
            else
            {
                timer.Stop();
                seconds = 0;
                Program.takaovi_github.groupBox1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.groupBox1.Text = "Failed"; }));
                MessageBox.Show("Something went wrong with the patch.\nYou most likely need to patch the application manually as the APK is probably using a different location for network_security_config.xml.", "Oops!");
                resetbat();
                Program.takaovi_github.PatchButton.Invoke(new MethodInvoker(delegate { Program.takaovi_github.PatchButton.Enabled = true; }));
                Program.takaovi_github.progressBar1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.progressBar1.Value = 0; }));
                Program.takaovi_github.groupBox1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.groupBox1.Text = "Ready to patch..."; }));
            }
        }

        private void compile()
        {
            Program.takaovi_github.progressBar1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.progressBar1.Value = 65; }));
            Program.takaovi_github.groupBox1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.groupBox1.Text = "Compiling APK..."; }));

            //Get the file name without ".apk"
            string APKfolder = APKname.Remove(APKname.Length - 4);

            //Edit compile.bat
            string str4 = File.ReadAllText((AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\compile.bat"));
            str4 = str4.Replace("<name>", APKfolder);
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\compile.bat", str4);
            //Edit compile.bat
            string str7 = File.ReadAllText((AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\compile.bat"));
            str7 = str7.Replace("<path2>", AppDomain.CurrentDomain.BaseDirectory + @"Patched_APKs\" + APKname);
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\compile.bat", str7);
            //Edit compile.bat
            string str6 = File.ReadAllText((AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\compile.bat"));
            str6 = str6.Replace("apktool.jar", AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\apktool.jar");
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\compile.bat", str6);

            //Start compiling APK
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.Arguments = "/k \"" + AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\" + "compile.bat" + "\"";
            p.EnableRaisingEvents = true;
            p.Exited += new EventHandler(p_Exited);
            p.Start();

            void p_Exited(object sender, EventArgs e)
            {
                Program.takaovi_github.progressBar1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.progressBar1.Value = 85; }));

                sign();
            }
        }

        private void sign()
        {
            //Edit sign.bat
            string str4 = File.ReadAllText((AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\sign.bat"));
            str4 = str4.Replace("<apkpath>", AppDomain.CurrentDomain.BaseDirectory + @"Patched_APKs\" + APKname);
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\sign.bat", str4);

            //Start signing APK
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.Arguments = "/k \"" + AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\" + "sign.bat" + "\"";
            p.EnableRaisingEvents = true;
            p.Exited += new EventHandler(p_Exited);
            p.Start();

            void p_Exited(object sender, EventArgs e)
            {
                //If compiled successfully
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"Patched_APKs\" + APKname))
                {
                    //File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"Patched_APKs\" + APKname);
                    //File.Move(AppDomain.CurrentDomain.BaseDirectory + @"Patched_APKs\" + APKname + "_", AppDomain.CurrentDomain.BaseDirectory + @"Patched_APKs\" + APKname + "_".Replace("_", ""));

                    resetbat();

                    //Get the file name without ".apk"
                    string APKfolder = APKname.Remove(APKname.Length - 4);

                    Program.takaovi_github.progressBar1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.progressBar1.Value = 100; }));
                    Program.takaovi_github.groupBox1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.groupBox1.Text = "Success! Ready to patch again..."; }));
                    timer.Stop();
                    seconds = 0;

                    //Open folder to show the user the patched APK
                    Process.Start(AppDomain.CurrentDomain.BaseDirectory + @"Patched_APKs");

                    Program.takaovi_github.PatchButton.Invoke(new MethodInvoker(delegate { Program.takaovi_github.PatchButton.Enabled = true; }));
                }
                else
                {
                    timer.Stop();
                    seconds = 0;
                    this.Text = "NSCPatcher @ Github.com/Takaovi/NSCPatcher | Elapsed " + seconds + " seconds";
                    Program.takaovi_github.groupBox1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.groupBox1.Text = "Failed"; }));
                    MessageBox.Show("Something went wrong with compiling...", "Oops!");
                    resetbat();
                    Program.takaovi_github.PatchButton.Invoke(new MethodInvoker(delegate { Program.takaovi_github.PatchButton.Enabled = true; }));
                    Program.takaovi_github.progressBar1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.progressBar1.Value = 0; }));
                    Program.takaovi_github.groupBox1.Invoke(new MethodInvoker(delegate { Program.takaovi_github.groupBox1.Text = "Ready to patch..."; }));
                }
            }
        }

        private void Window_FormClosing(object sender, FormClosingEventArgs a)
        {
            if (timer.Enabled == true)
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to exit?", "Confirm", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    timer.Stop();

                    foreach (Process Proc in Process.GetProcesses())
                        if (Proc.ProcessName.Equals("cmd"))
                            Proc.Kill();

                    Process[] p = Process.GetProcessesByName("cmd");
                    if (p.Length == 0)
                    {
                        if (APKname.Length > 0)
                        {
                            try
                            {
                                //Remove unpatched old APK if not already removed
                                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp\" + APKname))
                                {
                                    File.Delete(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp\" + APKname);
                                }

                                //Remove decompiled APK folder just to be sure it's removed
                                string APKfolder = APKname.Remove(APKname.Length - 4);

                                if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp\" + APKfolder))
                                {

                                    Directory.Delete(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\Temp\" + APKfolder, true);
                                }
                            }
                            catch
                            {
                                //Rip
                            }
                        }
                    }
                }
                else if (dialogResult == DialogResult.No)
                {
                    a.Cancel = true;
                }
            }
            resetbat();
        }

        private void resetbat()
        {
            //Reverse edits on compile.bat
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\compile.bat", @"cd NSCPatcher_Data\Temp\<name>" + "\njava -jar apktool.jar b -f -o <path2>" + "\nexit");

            //Reverse edits on decompile.bat
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\decompile.bat", @"cd NSCPatcher_Data\Temp" + "\njava -jar apktool.jar d -f <path>" + "\nexit");

            //Reverse edits on sign.bat
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + @"NSCPatcher_Data\sign.bat", "cd NSCPatcher_Data" + "\njava -jar \"apksigner.jar\" sign --key \"key.pk8\" --cert \"cert.pem\" --v4-signing-enabled false --out \"<apkpath>\" \"<apkpath>\"" + "\nexit");
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            seconds++;
            this.Text = "NSCPatcher @ Github.com/Takaovi/NSCPatcher | Elapsed " + seconds + " seconds";
        }

        private void Window_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Nothing
        }
    }
}
