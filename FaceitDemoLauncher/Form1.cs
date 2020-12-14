﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FaceitDemoLauncher
{
    /// <summary>
    /// Main window class.
    /// </summary>
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Mostly autogenerated GUI stuff.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AllowDrop = true;
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoEllipsis = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(10);
            this.label1.Size = new System.Drawing.Size(283, 150);
            this.label1.TabIndex = 0;
            this.label1.Text = "Drop <demo>.dem.gz here";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label1.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnLabel1DragDrop);
            this.label1.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnLabel1DragEnter);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(295, 169);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Drop area";
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(6, 13);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Browse file...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.OnBrowseButtonClick);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Enabled = false;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(204, 13);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(85, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Extract...";
            this.button2.UseMnemonic = false;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.OnExtractButtonClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupBox1.Location = new System.Drawing.Point(0, 169);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(295, 42);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            // 
            // Form1
            // 
            this.AcceptButton = this.button2;
            this.AllowDrop = true;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(295, 211);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(250, 250);
            this.Name = "Form1";
            this.Text = "Faceit Demo Launcher";
            this.Load += new System.EventHandler(this.OnMainWindowLoaded);
            this.Shown += new System.EventHandler(this.OnMainWindowShown);
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void OnMainWindowLoaded(object sender, EventArgs e)
        {
            this.Menu = new MainMenu();
            var menuitems = new MenuItem[]
            {
                new MenuItem("File"),
                new MenuItem("Edit"),
                new MenuItem("Help")
            };
            menuitems[0].MenuItems.Add("Open...", OnBrowseButtonClick);
            menuitems[0].MenuItems.Add("Exit", OnExitClick);
            menuitems[1].MenuItems.Add("Change csgo folder...", OnChangeFolderClick);
            menuitems[2].MenuItems.Add("About", OnShowInfoClick);
            // TODO loput itemit
            foreach (var item in menuitems)
            {
                this.Menu.MenuItems.Add(item);
            }
        }

        private void OnMainWindowShown(object sender, EventArgs e)
        {
            if (!Program.UpdateCounterStrikeInstallPath()) { this.Close(); }
            if (Program.FetchDemoFromArguments())
                UpdateFormElements();
        }

        private void OnLabel1DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void OnLabel1DragDrop(object sender, DragEventArgs e)
        {
            string draggedFile = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            Program.UpdateCompressedFile(draggedFile);
            UpdateFormElements();
        }

        private void OnBrowseButtonClick(object sender, EventArgs e)
        {
            OpenFileDialog filePickerDialog = new OpenFileDialog
            {
                Filter = "Compressed demo files (*.dem.gz)|*.dem.gz"
            };
            DialogResult result = filePickerDialog.ShowDialog();
            if (result != DialogResult.OK)
                return;
            bool success = Program.UpdateCompressedFile(filePickerDialog.FileName);
            if (success)
                UpdateFormElements();
        }

        private void OnExtractButtonClick(object sender, EventArgs e)
        {
            if (Program.ExtractAndShow())
                Close();
        }

        private void OnChangeFolderClick(object sender, EventArgs e)
        {
            Program.UpdateCounterStrikeInstallPath(false);
        }

        private void OnShowInfoClick(object sender, EventArgs e)
        {
            MessageBox.Show($"Faceit demo launcher v.{Program.CurrentVersion}", "About");
        }

        private void OnExitClick(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Update form's elements to match the state of the selected demo file (if any).
        /// </summary>
        /// <param name="validFileSelected">True if file is selected</param>
        public void UpdateFormElements(bool validFileSelected = true)
        {
            if (validFileSelected)
            {
                this.label1.Text = Program.dropAreaTextRoot + Path.GetFileName(Program.compressedFilePath);
                this.button2.Enabled = true;
            }
            else
            {
                this.label1.Text = Program.DefaultDropAreaText;
                this.button2.Enabled = false;
            }
        }
    }
}