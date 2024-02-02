using HawkSync_RC.classes;
using HawkSync_RC.TVFunctions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HawkSync_RC
{
    public partial class RC_PopupLoadRotation : Form
    {
        AppState _state;
        RCSetup _setup;
        int ArrayID;

        public RC_PopupLoadRotation(AppState state, RCSetup setup, int arrayID)
        {
            _state = state;
            ArrayID = arrayID;
            _setup = setup;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
            {
                return; // nothing selected...
            }
            // load rotation
            RC_ServerManager.loadList = _state.Instances[ArrayID].savedmaprotations[listBox1.SelectedIndex].mapcycle;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // cancel
            this.Close();
        }

        private void Popup_LoadRotation_Load(object sender, EventArgs e)
        {
            foreach (var item in _state.Instances[ArrayID].savedmaprotations)
            {
                listBox1.Items.Add(item.Description);
            }
        }
    }
}
