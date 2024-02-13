using System;
using System.Windows.Forms;

namespace HawkSync_RC
{
    public partial class ClientOptions : Form
    {
        private AppState _state;
        private readonly RCSetup RCSetup;

        public ClientOptions(AppState state, RCSetup setup)
        {
            InitializeComponent();
            _state = state;
            RCSetup = setup;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RCSetup.RCConfig.Configs["Main"].Set("InternalRefresh", (int)numericUpDown1.Value);
            RCSetup.RCConfig.Save();
            MessageBox.Show(
                "Settings have been saved successfully!\nYou must restart for the new settings to take effect.",
                "Success");
            Close();
        }

        private void ClientOptions_Load(object sender, EventArgs e)
        {
            if (RCSetup.RCConfig.Configs["Main"].GetInt("InternalRefresh") > numericUpDown1.Maximum ||
                RCSetup.RCConfig.Configs["Main"].GetInt("InternalRefresh") < numericUpDown1.Minimum)
            {
                MessageBox.Show("Do not tamper for the BMT files manually. This is how shit breaks.", "Fuck off");
                Close();
                RCSetup.RCConfig.Configs["Main"].Set("InternalRefresh", 6);
                RCSetup.RCConfig.Save();
                return;
            }

            numericUpDown1.Value = RCSetup.RCConfig.Configs["Main"].GetInt("InternalRefresh");
        }
    }
}