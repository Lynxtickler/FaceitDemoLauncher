using System;
using System.Windows.Forms;

namespace FaceitDemoLauncher
{
    /// <summary>
    /// Form that asks the user what to do next with the demofile.
    /// </summary>
    public partial class MessageForm : Form
    {
        private string demoName;

        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="name">Extracted demo file name</param>
        public MessageForm(string name) : this()
        {
            demoName = name;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MessageForm()
        {
            InitializeComponent();

            button1.DialogResult = DialogResult.OK;
            button3.DialogResult = DialogResult.Cancel;
            
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
                if (string.IsNullOrWhiteSpace(demoName))
                    demoName = System.IO.Path.GetFileNameWithoutExtension(Program.compressedFilePath);
                Clipboard.SetText("playdemo " + demoName);
            };
        }
    }
}
