﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceitDemoLauncher
{
    /// <summary>
    /// Form that asks the user what to do next with the demofile.
    /// </summary>
    public partial class MessageForm : Form
    {
        public MessageForm()
        {
            InitializeComponent();
            button1.Click += (object sender, EventArgs e) => {
                Program.messageFormReturnCode = Program.MessageFormCode.Launch;
                Close();
            };

            button2.Text = Button2DefaultText;
            button2.Click += (object sender, EventArgs e) => {
                button2.Text = "Copied!";
                var textTimer = new System.Windows.Forms.Timer() {
                    Interval = 2500
                };
                textTimer.Tick += (object senderToTimer, EventArgs eTimer) =>
                {
                    button2.Text = Button2DefaultText;
                    textTimer.Enabled = false;
                };
                textTimer.Enabled = true;
                Clipboard.SetText("playdemo " + System.IO.Path.GetFileNameWithoutExtension(Program.compressedFilePath));
            };

            button3.Click += (object sender, EventArgs e) => {
                Program.messageFormReturnCode = Program.MessageFormCode.Cancel;
                Close();
            };
        }
    }
}