using System;
using System.Collections.Generic;
using System.Windows.Forms;
using HawkSync_RC.classes;
using HawkSync_RC.TVFunctions;

namespace HawkSync_RC
{
    public partial class RC_PopupSaveRotation : Form
    {
        public RC_PopupSaveRotation(AppState state, RCSetup setup, int arrayID, ref List<MapList> selectedMaps)
        {
            State = state;
            ArrayID = arrayID;
            SelectedMaps = selectedMaps;
            TVRotationManagerCMD = new rotationManager(setup, state, arrayID);
            InitializeComponent();
        }

        private AppState State { get; }
        private int ArrayID { get; }
        private List<MapList> SelectedMaps { get; }
        private bool SaveRotation { get; set; }
        private rotationManager TVRotationManagerCMD { get; }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveRotation = true;
            if (TVRotationManagerCMD.CreateRotation(SelectedMaps, textBox1.Text) != OpenClass.Status.SUCCESS)
                MessageBox.Show("Something went wrong file trying to save the rotation.", "Error");
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // cancel
            Close();
        }

        private void Popup_SaveRotation_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (SaveRotation) MessageBox.Show("Rotation Saved!", "Success");
        }
    }
}