using HawkSync_SM.classes.logs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;

namespace HawkSync_SM
{
    public partial class ViewUserActivity : Form
    {
        DataTable adminsTable;
        DataTable logTable;
        AppState _state;
        public ViewUserActivity(AppState state)
        {
            InitializeComponent();
            _state = state;
            adminsTable = new DataTable();
            logTable = new DataTable();

            // setup the admins table
            adminsTable.Columns.Add("Username");
            dataGridView2.DataSource = adminsTable;
            dataGridView2.Sort(dataGridView2.Columns["Username"], ListSortDirection.Ascending);
            dataGridView2.Columns["Username"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            // setup log viewer
            logTable.Columns.Add("Time", typeof(DateTime));
            logTable.Columns.Add("Action");
            dataGridView1.DataSource = logTable;
            dataGridView1.Sort(dataGridView1.Columns["Time"], ListSortDirection.Descending);
            dataGridView1.Columns["Time"].Width = 150;
            dataGridView1.Columns["Action"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ViewUserActivity_Load(object sender, EventArgs e)
        {
            // loop through _state.admins and add them to the admins table
            for (int i = 0; i < _state.RCLogs.Count; i++)
            {
                bool found = false;
                foreach (DataRow row in adminsTable.Rows)
                {
                    if (row["Username"].ToString() == _state.RCLogs[i].Username)
                    {
                        found = true;
                    }
                }
                if (found == false)
                {
                    adminsTable.Rows.Add(_state.RCLogs[i].Username);
                }
            }
        }

        private void dataGridView2_SelectionChanged(object sender, EventArgs e)
        {
            logTable.Rows.Clear();
            string username = dataGridView2.SelectedRows[0].Cells[0].Value.ToString();
            List<RCLogs> logs = _state.RCLogs;
            foreach (var log in logs)
            {
                if (log.Username == username)
                {
                    DataRow newRow = logTable.NewRow();
                    newRow["Time"] = log.Date;
                    newRow["Action"] = log.Action;
                    logTable.Rows.Add(newRow);
                }
            }
            if (logTable.Rows.Count != 0)
            {
                dataGridView1.CurrentCell = dataGridView1.Rows[0].Cells[0];
                dataGridView1.SelectedRows[0].Selected = true;
            }
        }
    }
}
