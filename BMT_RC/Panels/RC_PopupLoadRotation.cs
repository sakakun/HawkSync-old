using System;
using System.Windows.Forms;

namespace HawkSync_RC
{
    public partial class RC_PopupLoadRotation : Form
    {
        private RCSetup _setup;
        private readonly AppState _state;
        private readonly int ArrayID;

        public RC_PopupLoadRotation(AppState state, RCSetup setup, int arrayID)
        {
            _state = state;
            ArrayID = arrayID;
            _setup = setup;
            InitializeComponent();
        }

        private void event_loadMaps(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return; // nothing selected...
            // load rotation
            RC_ServerManager.loadList = _state.Instances[ArrayID].savedmaprotations[listBox1.SelectedIndex].mapcycle;
            this.Close();
        }

        private void event_closeWindow(object sender, EventArgs e)
        {
            RC_ServerManager.loadList.Clear();
            this.Close();
        }

        private void Popup_LoadRotation_Load(object sender, EventArgs e)
        {
            foreach (var item in _state.Instances[ArrayID].savedmaprotations) listBox1.Items.Add(item.Description);
        }
    }
}