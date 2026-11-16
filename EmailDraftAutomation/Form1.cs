using ClosedXML.Excel;
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace EmailDraftAutomation
{
    public partial class Form1 : Form
    {
        // Field, so every method can use the loaded data
        private DataTable _table = new DataTable();

        public Form1()
        {
            InitializeComponent();
        }

        private void LoadContacts(string path)
        {
            string[] wanted = { "Address", "City", "Contact Name", "Email" };

            _table = new DataTable();
            foreach (var name in wanted) _table.Columns.Add(name);

            using (var wb = new XLWorkbook(path))
            {
                var ws = wb.Worksheet("Contacts");
                var range = ws.RangeUsed();

                // Find which column number each wanted header is in
                var header = range.FirstRow();
                var colIndex = wanted.Select(w =>
                    header.Cells().First(c => c.GetString() == w).Address.ColumnNumber).ToArray();

                foreach (var row in range.RowsUsed().Skip(1))
                    _table.Rows.Add(colIndex.Select(i => (object)row.Cell(i).GetString()).ToArray());
            }

            dataGridView1.DataSource = _table.DefaultView;
            txtSearch.Text = "";
            UpdateCount();
        }

        private void UpdateCount()
        {
            lblCount.Text = $"{_table.DefaultView.Count:N0} of {_table.Rows.Count:N0} rows";
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog { Filter = "Excel files|*.xlsx" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    LoadContacts(dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not load the file. If it is open in Excel, close it and try again.\n\n" + ex.Message,
                        "Load failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (_table.Columns.Count == 0) return;   // nothing loaded yet

            string s = EscapeLike(txtSearch.Text.Trim());
            _table.DefaultView.RowFilter = s.Length == 0
                ? ""
                : $"[Address] LIKE '%{s}%' OR [City] LIKE '%{s}%' OR [Contact Name] LIKE '%{s}%' OR [Email] LIKE '%{s}%'";
            UpdateCount();
        }

        // Makes special characters safe inside a DataView LIKE filter
        private static string EscapeLike(string s)
        {
            return s.Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]").Replace("'", "''");
        }

        private void btnStartDrafts_Click(object sender, EventArgs e)
        {
            if (_table.Rows.Count == 0)
            {
                MessageBox.Show("Load a file first.");
                return;
            }

            // Work on the rows currently shown in the grid (respects the search filter)
            foreach (DataRowView rv in _table.DefaultView)
            {
                string email = rv["Email"].ToString().Trim();
                if (email.Length == 0) continue;           // skip contacts with no email

                string address = rv["Address"].ToString();
                string city = rv["City"].ToString();
                string name = rv["Contact Name"].ToString();
                string firstName = name.Split(' ')[0];

                // TODO: 1) check Gmail sent mail for this email since the cutoff date
                //       2) if not emailed, create the draft (To = email, CC = boss)
            }

            MessageBox.Show("Drafting logic not added yet.");
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Leave this here. Deleting it breaks the event hookup in Form1.Designer.cs.
        }
    }
}