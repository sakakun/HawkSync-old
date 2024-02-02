using HawkSync_RC.classes;
using HawkSync_RC.TVFunctions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace HawkSync_RC
{
    public partial class RC_PopupSaveRotation : Form
    {

        AppState State { get; }
        int ArrayID { get; }
        List<MapList> SelectedMaps { get; }
        bool SaveRotation { get; set; }
        TVRotationManagerCMD TVRotationManagerCMD { get; set; }

        public RC_PopupSaveRotation(AppState state, RCSetup setup, int arrayID, ref List<MapList> selectedMaps)
        {
            State = state;
            ArrayID = arrayID;
            SelectedMaps = selectedMaps;
            TVRotationManagerCMD = new TVRotationManagerCMD(setup, state, arrayID);
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveRotation = true;
            if (TVRotationManagerCMD.CreateRotation(SelectedMaps, textBox1.Text) != OpenClass.Status.SUCCESS)
            {
                MessageBox.Show("Something went wrong file trying to save the rotation.", "Error");
            }
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // cancel
            this.Close();
        }

        private void Popup_SaveRotation_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (SaveRotation == true)
            {
                MessageBox.Show("Rotation Saved!", "Success");
            }
        }
    }
}
