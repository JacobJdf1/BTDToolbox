﻿using BTDToolbox.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BTDToolbox.ProjectConfig;

namespace BTDToolbox
{
    public partial class Console : ThemedForm
    {
        //Config variables
        ConfigFile programData;
        public static float consoleLogFont;
        
        float fontSize;
        int errorCount = 0;

        private string lastMessage;
        private static Console console;
        

        public Console() : base()
        {
            InitializeComponent();
            StartUp();

            console = this;
            this.FormClosed += exitHandling;
            tabControl1.TabPages[1].Text = "Errors (" + errorCount + ")";
        }

        private void StartUp()
        {
            Deserialize_Config();
            if (programData.ExistingUser == false)
            {
                this.StartPosition = FormStartPosition.Manual;
                Rectangle resolution = Screen.PrimaryScreen.Bounds;

                int x = resolution.Width;
                int y = resolution.Height;
                int sizeX = x / 2;
                int sizeY = y / 5;

                this.Size = new Size(sizeX, sizeY);
                this.Location = new Point(x - sizeX, y - this.Height - 90);
            }
            else
            {
                this.Size = new Size(programData.Console_SizeX, programData.Console_SizeY);
                this.Location = new Point(programData.Console_PosX, programData.Console_PosY);
            }
            fontSize = programData.Console_FontSize;
            this.Font = new Font("Consolas", fontSize);
        }
        public static Console getInstance()
        {
            return console;
        }


        public void append(String log, bool canRepeat, bool force, bool notice)
        {
            if (!canRepeat && log == lastMessage) return;
            if (force) { Visible = true; BringToFront(); }
            if (notice) output_log.SelectionColor = Color.Yellow;


            DateTime now = DateTime.Now;


            string secondSeperator = ":";
            if (now.Second < 10)
                secondSeperator = ":0";
            
            string currentTime = now.Hour + ":" + now.Minute + secondSeperator + now.Second;

            try
            {
                Invoke((MethodInvoker)delegate
                {
                    output_log.AppendText("" + currentTime + " - " + ">> " + log + "\r\n");
                    output_log.ScrollToCaret();
                });

                lastMessage = log;
            }
            catch (Exception) { Environment.Exit(0); }
        }
        
        
        public void GetAnnouncement()
        {
            WebHandler web = new WebHandler();
            string url = "https://raw.githubusercontent.com/TDToolbox/BTDToolbox-2019_LiveFIles/master/toolbox%20announcements";
            try
            {
                string answer = web.WaitOn_URL(url);
                output_log.SelectionColor = Color.OrangeRed;

                if (answer.Length > 0 && answer != null)
                    ConsoleHandler.append("Announcement: " + answer);
                else
                    ConsoleHandler.append("Failed to read announcement...");
            }
            catch
            {
                ConsoleHandler.append_Notice("Something went wrong.. Failed to read announcements...");
            }
        }


        private void ExportLog_Button_Click(object sender, EventArgs e)
        {
            ConsoleHandler.append_CanRepeat("Copied console log to clipboard.");
            Clipboard.SetText(output_log.Text);
        }


        private void Deserialize_Config()
        {
            programData = Serializer.Deserialize_Config();
        }


        private void exitHandling(object sender, EventArgs e)
        {
            Serializer.SaveConfig(this, "console");
        }

        public override void close_button_Click(object sender, EventArgs e)
        {
            ConsoleHandler.append("Hiding console.");
            Serializer.SaveConfig(this, "console");
            this.Hide();
        }
    }
}
