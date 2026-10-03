using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BusinessManagement.App
{
    public class InvoiceManagerForm : Form
    {
        private DataGridView dgvInvoices;
        private DataGridView dgvInvoiceItems;
        private TextBox txtSearchInvoice;
        private Button btnOpenPdf;
        private readonly InventoryRepository repository;
        private DataTable invoicesTable;

        public InvoiceManagerForm()
        {
            repository = new InventoryRepository();
            InitializeComponent();
            LoadInvoices();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1100, 650);
            this.Text = "Invoice History & Tracking";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(15, 13, 25);

            Label lblTitle = new Label
            {
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 244, 246),
                Text = "📄 Sales Invoices & Transaction History"
            };

            Label lblSearch = new Label
            {
                Location = new Point(20, 68),
                AutoSize = true,
                Text = "Search Client:",
                Font = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(156, 163, 175)
            };

            txtSearchInvoice = new TextBox
            {
                Location = new Point(120, 65),
                Width = 250,
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(35, 30, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearchInvoice.TextChanged += TxtSearchInvoice_TextChanged;

            // Invoices Master Grid
            dgvInvoices = CreateJewelThemeGrid();
            dgvInvoices.Location = new Point(20, 110);
            dgvInvoices.Size = new Size(520, 430);
            dgvInvoices.SelectionChanged += DgvInvoices_SelectionChanged;

            // Invoice Line Items Detail Grid
            Label lblDetailsTitle = new Label
            {
                Location = new Point(560, 68),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 244, 246),
                Text = "Selected Invoice Breakdown"
            };

            dgvInvoiceItems = CreateJewelThemeGrid();
            dgvInvoiceItems.Location = new Point(560, 110);
            dgvInvoiceItems.Size = new Size(500, 430);

            btnOpenPdf = new Button
            {
                Location = new Point(20, 558),
                Size = new Size(180, 40),
                Text = "Open PDF Receipt",
                UseVisualStyleBackColor = false,
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10f),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnOpenPdf.FlatAppearance.BorderSize = 0;
            btnOpenPdf.Click += BtnOpenPdf_Click;

            this.Controls.AddRange(new Control[] { lblTitle, lblSearch, txtSearchInvoice, dgvInvoices, lblDetailsTitle, dgvInvoiceItems, btnOpenPdf });
        }

        private DataGridView CreateJewelThemeGrid()
        {
            return new DataGridView
            {
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.FromArgb(24, 20, 37),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(45, 38, 68),
                EnableHeadersVisualStyles = false,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeight = 35,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(99, 102, 241),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Semibold", 9.5f)
                },
                RowTemplate = { Height = 32 },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(24, 20, 37),
                    ForeColor = Color.FromArgb(229, 231, 235),
                    SelectionBackColor = Color.FromArgb(67, 56, 202),
                    SelectionForeColor = Color.White,
                    Font = new Font("Segoe UI", 9f)
                }
            };
        }

        private void LoadInvoices()
        {
            invoicesTable = repository.GetAllInvoices(); // Ensure this method is added to your repository
            dgvInvoices.DataSource = invoicesTable;

            if (dgvInvoices.Columns.Contains("InvoiceID")) dgvInvoices.Columns["InvoiceID"].Width = 70;
            if (dgvInvoices.Columns.Contains("CustomerName")) dgvInvoices.Columns["CustomerName"].Width = 180;
            if (dgvInvoices.Columns.Contains("TotalAmount"))
            {
                dgvInvoices.Columns["TotalAmount"].Width = 110;
                dgvInvoices.Columns["TotalAmount"].DefaultCellStyle.Format = "N2";
            }
            if (dgvInvoices.Columns.Contains("InvoiceDate")) dgvInvoices.Columns["InvoiceDate"].Width = 140;
        }

        private void TxtSearchInvoice_TextChanged(object sender, EventArgs e)
        {
            if (invoicesTable == null) return;
            string filter = txtSearchInvoice.Text.Trim().Replace("'", "''");
            invoicesTable.DefaultView.RowFilter = string.IsNullOrEmpty(filter) ? string.Empty : $"CustomerName LIKE '%{filter}%'";
        }

        private void DgvInvoices_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvInvoices.SelectedRows.Count > 0)
            {
                int invoiceId = Convert.ToInt32(dgvInvoices.SelectedRows[0].Cells["InvoiceID"].Value);
                LoadInvoiceItems(invoiceId);
            }
        }

        private void LoadInvoiceItems(int invoiceId)
        {
            DataTable itemsTable = repository.GetInvoiceItems(invoiceId); // Ensure this method is added to your repository
            dgvInvoiceItems.DataSource = itemsTable;

            if (dgvInvoiceItems.Columns.Contains("ProductName")) dgvInvoiceItems.Columns["ProductName"].Width = 220;
            if (dgvInvoiceItems.Columns.Contains("UnitPrice")) dgvInvoiceItems.Columns["UnitPrice"].DefaultCellStyle.Format = "N2";
            if (dgvInvoiceItems.Columns.Contains("Quantity")) dgvInvoiceItems.Columns["Quantity"].Width = 60;
            if (dgvInvoiceItems.Columns.Contains("Subtotal")) dgvInvoiceItems.Columns["Subtotal"].DefaultCellStyle.Format = "N2";
        }

        private void BtnOpenPdf_Click(object sender, EventArgs e)
        {
            if (dgvInvoices.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an invoice to view its receipt.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int invoiceId = Convert.ToInt32(dgvInvoices.SelectedRows[0].Cells["InvoiceID"].Value);
            string pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Invoice_{invoiceId}.pdf");

            if (File.Exists(pdfPath))
            {
                Process.Start(new ProcessStartInfo(pdfPath) { UseShellExecute = true });
            }
            else
            {
                MessageBox.Show($"PDF receipt file not found at:\n{pdfPath}", "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}