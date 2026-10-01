using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace BusinessManagement.App
{
    public partial class LowStockForm : Form
    {
        private DataGridView dgvLowStock;
        private Button btnRefresh;
        private Button btnRestockSelected;
        private Button btnRestockAll;
        private Button btnClose;
        private readonly InventoryRepository repository;

        // Property to pass selected restock items back to Form1
        public List<CartItem> RestockCartItems { get; private set; } = new List<CartItem>();

        public LowStockForm()
        {
            repository = new InventoryRepository();
            InitializeComponentCustom();
            LoadLowStockData();
        }

        private void InitializeComponentCustom()
        {
            this.Size = new Size(820, 540);
            this.Text = "⚠️ Low Stock Alert & Restock Manager";
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
                Text = "⚠️ Items Requiring Restock"
            };

            dgvLowStock = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(760, 330),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AllowUserToResizeRows = false,
                BackgroundColor = Color.FromArgb(24, 20, 37),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(45, 38, 68),
                EnableHeadersVisualStyles = false,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeight = 40,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(225, 29, 72),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Semibold", 10f),
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                },
                RowTemplate = { Height = 36 },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(24, 20, 37),
                    ForeColor = Color.FromArgb(254, 202, 202),
                    SelectionBackColor = Color.FromArgb(127, 29, 29),
                    SelectionForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f),
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(6, 0, 0, 0)
                }
            };

            btnRefresh = new Button
            {
                Location = new Point(20, 425),
                Size = new Size(100, 38),
                Text = "Refresh",
                UseVisualStyleBackColor = false,
                BackColor = Color.FromArgb(75, 85, 99),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9.5f),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => LoadLowStockData();

            btnRestockSelected = new Button
            {
                Location = new Point(130, 425),
                Size = new Size(150, 38),
                Text = "Restock Selected ➔",
                UseVisualStyleBackColor = false,
                BackColor = Color.FromArgb(99, 102, 241), // Jewel Tone Accent
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9.5f),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRestockSelected.FlatAppearance.BorderSize = 0;
            btnRestockSelected.Click += BtnRestockSelected_Click;

            btnRestockAll = new Button
            {
                Location = new Point(290, 425),
                Size = new Size(140, 38),
                Text = "Restock All ➔",
                UseVisualStyleBackColor = false,
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9.5f),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRestockAll.FlatAppearance.BorderSize = 0;
            btnRestockAll.Click += BtnRestockAll_Click;

            btnClose = new Button
            {
                Location = new Point(680, 425),
                Size = new Size(100, 38),
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

            this.Controls.AddRange(new Control[] { lblTitle, dgvLowStock, btnRefresh, btnRestockSelected, btnRestockAll, btnClose });
        }

        private void LoadLowStockData()
        {
            DataTable dt = repository.GetLowStockProducts();
            dgvLowStock.DataSource = dt;

            if (dgvLowStock.Columns.Contains("ProductID")) dgvLowStock.Columns["ProductID"].Width = 60;
            if (dgvLowStock.Columns.Contains("ProductName")) dgvLowStock.Columns["ProductName"].Width = 220;
            if (dgvLowStock.Columns.Contains("SKU")) dgvLowStock.Columns["SKU"].Width = 100;
            if (dgvLowStock.Columns.Contains("Category")) dgvLowStock.Columns["Category"].Width = 110;
            if (dgvLowStock.Columns.Contains("UnitPrice"))
            {
                dgvLowStock.Columns["UnitPrice"].DefaultCellStyle.Format = "N2";
                dgvLowStock.Columns["UnitPrice"].Width = 90;
            }
            if (dgvLowStock.Columns.Contains("StockQuantity")) dgvLowStock.Columns["StockQuantity"].Width = 90;
            if (dgvLowStock.Columns.Contains("ReorderLevel")) dgvLowStock.Columns["ReorderLevel"].Width = 90;
        }

        private void BtnRestockSelected_Click(object sender, EventArgs e)
        {
            if (dgvLowStock.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product to restock.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dgvLowStock.SelectedRows[0];
            int productId = Convert.ToInt32(row.Cells["ProductID"].Value);
            string productName = row.Cells["ProductName"].Value.ToString();
            decimal unitPrice = Convert.ToDecimal(row.Cells["UnitPrice"].Value);
            int stock = Convert.ToInt32(row.Cells["StockQuantity"].Value);
            int reorder = Convert.ToInt32(row.Cells["ReorderLevel"].Value);

            int restockQty = repository.CalculateRestockQuantity(stock, reorder);

            RestockCartItems.Clear();
            RestockCartItems.Add(new CartItem
            {
                ProductID = productId,
                ProductName = productName,
                UnitPrice = unitPrice,
                Quantity = restockQty
            });

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnRestockAll_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgvLowStock.DataSource;
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("No low stock items available to restock.", "Empty List", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            RestockCartItems.Clear();
            foreach (DataRow row in dt.Rows)
            {
                int productId = Convert.ToInt32(row["ProductID"]);
                string productName = row["ProductName"].ToString();
                decimal unitPrice = Convert.ToDecimal(row["UnitPrice"]);
                int stock = Convert.ToInt32(row["StockQuantity"]);
                int reorder = Convert.ToInt32(row["ReorderLevel"]);

                int restockQty = repository.CalculateRestockQuantity(stock, reorder);

                RestockCartItems.Add(new CartItem
                {
                    ProductID = productId,
                    ProductName = productName,
                    UnitPrice = unitPrice,
                    Quantity = restockQty
                });
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}