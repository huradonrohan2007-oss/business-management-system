using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace BusinessManagement.App
{
    public partial class ZReportForm : Form
    {
        private DateTimePicker dtpReportDate;
        private DataGridView dgvItemizedSales;
        private Label lblTotalRevenueVal;
        private Label lblTotalTransactionsVal;
        private Button btnClose;
        private readonly InventoryRepository repository;

        public ZReportForm()
        {
            repository = new InventoryRepository();
            InitializeComponentCustom();
            LoadReportData(DateTime.Today);
        }

        private void InitializeComponentCustom()
        {
            this.Size = new Size(800, 600);
            this.Text = "📊 Daily Sales Financial Report (Z-Report)";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(15, 13, 25);

            Label lblTitle = new Label
            {
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 244, 246),
                Text = "📊 End-of-Day Financial Z-Report"
            };

            Label lblDatePrompt = new Label
            {
                Location = new Point(20, 70),
                AutoSize = true,
                ForeColor = Color.FromArgb(156, 163, 175),
                Text = "Select Report Date:",
                Font = new Font("Segoe UI", 10f)
            };

            dtpReportDate = new DateTimePicker
            {
                Location = new Point(155, 68),
                Size = new Size(200, 27),
                Format = DateTimePickerFormat.Short
            };
            dtpReportDate.ValueChanged += (s, e) => LoadReportData(dtpReportDate.Value);

            // Summary Panels / Cards
            Panel pnlSummary = new Panel
            {
                Location = new Point(20, 115),
                Size = new Size(740, 90),
                BackColor = Color.FromArgb(24, 20, 37)
            };

            Label lblRevTitle = new Label
            {
                Location = new Point(20, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(156, 163, 175),
                Text = "GROSS REVENUE",
                Font = new Font("Segoe UI Semibold", 9f)
            };

            lblTotalRevenueVal = new Label
            {
                Location = new Point(20, 38),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 211, 153), // Emerald Green
                Text = "$0.00",
                Font = new Font("Segoe UI", 16, FontStyle.Bold)
            };

            Label lblTxTitle = new Label
            {
                Location = new Point(380, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(156, 163, 175),
                Text = "TOTAL TRANSACTIONS",
                Font = new Font("Segoe UI Semibold", 9f)
            };

            lblTotalTransactionsVal = new Label
            {
                Location = new Point(380, 38),
                AutoSize = true,
                ForeColor = Color.FromArgb(243, 244, 246),
                Text = "0",
                Font = new Font("Segoe UI", 16, FontStyle.Bold)
            };

            pnlSummary.Controls.AddRange(new Control[] { lblRevTitle, lblTotalRevenueVal, lblTxTitle, lblTotalTransactionsVal });

            // Itemized Grid Breakdown
            Label lblGridTitle = new Label
            {
                Location = new Point(20, 225),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 244, 246),
                Text = "Itemized Sales Breakdown"
            };

            dgvItemizedSales = new DataGridView
            {
                Location = new Point(20, 255),
                Size = new Size(740, 245),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
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
                    SelectionBackColor = Color.FromArgb(79, 70, 229),
                    SelectionForeColor = Color.White,
                    Font = new Font("Segoe UI", 9f)
                }
            };

            btnClose = new Button
            {
                Location = new Point(660, 515),
                Size = new Size(100, 36),
                Text = "Close",
                UseVisualStyleBackColor = false,
                BackColor = Color.FromArgb(75, 85, 99),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9.5f),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTitle, lblDatePrompt, dtpReportDate, pnlSummary, lblGridTitle, dgvItemizedSales, btnClose });
        }

        private void LoadReportData(DateTime date)
        {
            // 1. Load Financial Summary (Revenue & Transaction Count)
            DataTable summaryDt = repository.GetDailySalesSummary(date);
            decimal grossRevenue = 0;
            int totalTransactions = 0;

            if (summaryDt != null && summaryDt.Rows.Count > 0)
            {
                DataRow row = summaryDt.Rows[0];
                if (row["GrossRevenue"] != DBNull.Value)
                    grossRevenue = Convert.ToDecimal(row["GrossRevenue"]);
                if (row["TotalTransactions"] != DBNull.Value)
                    totalTransactions = Convert.ToInt32(row["TotalTransactions"]);
            }

            lblTotalRevenueVal.Text = $"${grossRevenue:N2}";
            lblTotalTransactionsVal.Text = totalTransactions.ToString();

            // 2. Load Itemized Breakdown Grid
            DataTable itemizedDt = repository.GetItemizedSalesSummary(date);
            dgvItemizedSales.DataSource = itemizedDt;

            if (dgvItemizedSales.Columns.Contains("ProductName")) dgvItemizedSales.Columns["ProductName"].Width = 340;
            if (dgvItemizedSales.Columns.Contains("TotalUnitsSold")) dgvItemizedSales.Columns["TotalUnitsSold"].Width = 180;
            if (dgvItemizedSales.Columns.Contains("TotalSalesRevenue"))
            {
                dgvItemizedSales.Columns["TotalSalesRevenue"].Width = 200;
                dgvItemizedSales.Columns["TotalSalesRevenue"].DefaultCellStyle.Format = "N2";
            }
        }
    }
}