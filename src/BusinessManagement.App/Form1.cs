using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace BusinessManagement.App
{
    public partial class Form1 : Form
    {
        private InventoryRepository repository;
        private DataGridView dgvInventory;
        private DataGridView dgvCart;
        private TextBox txtSearch;
        private TextBox txtCustomerName;
        private NumericUpDown numQuantity;
        private Button btnAddToCart;
        private Button btnEditThreshold;
        private Button btnLowStockAlerts;
        private Button btnZReport;
        private Button btnInvoiceManager;
        private Button btnCheckout;
        private Label lblCartTotal;

        private List<CartItem> cart = new List<CartItem>();

        public Form1()
        {
            repository = new InventoryRepository();
            repository.InitializeDatabase();
            InitializeCustomControls();
            LoadInventoryData();
        }

        private void InitializeCustomControls()
        {
            this.Size = new Size(1280, 740);
            this.Text = "Nexus POS & Inventory Management";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(15, 13, 25); // Midnight Jewel-Tone Background

            // --- Left Panel: Inventory Catalog ---
            Panel pnlInventory = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(620, 660),
                BackColor = Color.FromArgb(24, 20, 37)
            };

            Label lblInventoryTitle = new Label
            {
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 244, 246),
                Text = "Product Inventory Catalog"
            };

            Label lblSearch = new Label
            {
                Location = new Point(20, 60),
                AutoSize = true,
                ForeColor = Color.FromArgb(156, 163, 175),
                Text = "Search Products:"
            };

            txtSearch = new TextBox
            {
                Location = new Point(130, 57),
                Size = new Size(465, 27),
                BackColor = Color.FromArgb(35, 30, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            dgvInventory = new DataGridView
            {
                Location = new Point(20, 95),
                Size = new Size(575, 505),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.FromArgb(24, 20, 37),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(45, 38, 68),
                EnableHeadersVisualStyles = false
            };
            dgvInventory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(99, 102, 241);
            dgvInventory.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvInventory.ColumnHeadersHeight = 35;
            dgvInventory.DefaultCellStyle.BackColor = Color.FromArgb(24, 20, 37);
            dgvInventory.DefaultCellStyle.ForeColor = Color.FromArgb(229, 231, 235);
            dgvInventory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(79, 70, 229);

            // --- Low Stock Row Highlighting ---
            dgvInventory.CellFormatting += (sender, e) =>
            {
                if (e.RowIndex >= 0 && dgvInventory.Rows[e.RowIndex].DataBoundItem is DataRowView rowView)
                {
                    if (rowView.Row.Table.Columns.Contains("StockQuantity") && rowView.Row.Table.Columns.Contains("ReorderLevel"))
                    {
                        var stockVal = rowView["StockQuantity"];
                        var reorderVal = rowView["ReorderLevel"];

                        if (stockVal != DBNull.Value && reorderVal != DBNull.Value)
                        {
                            int stock = Convert.ToInt32(stockVal);
                            int reorder = Convert.ToInt32(reorderVal);

                            if (stock <= reorder)
                            {
                                e.CellStyle.BackColor = Color.FromArgb(127, 29, 29); // Dark Red / Maroon
                                e.CellStyle.ForeColor = Color.FromArgb(254, 202, 202); // Light Pink Text
                            }
                        }
                    }
                }
            };

            Label lblQtyPrompt = new Label
            {
                Location = new Point(20, 620),
                AutoSize = true,
                ForeColor = Color.FromArgb(156, 163, 175),
                Text = "Qty:"
            };

            numQuantity = new NumericUpDown
            {
                Location = new Point(55, 617),
                Size = new Size(50, 27),
                Minimum = 1,
                Maximum = 1000,
                Value = 1,
                BackColor = Color.FromArgb(35, 30, 52),
                ForeColor = Color.White
            };

            btnAddToCart = new Button
            {
                Location = new Point(112, 615),
                Size = new Size(110, 36),
                Text = "Add to Cart ➔",
                BackColor = Color.FromArgb(99, 102, 241),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddToCart.FlatAppearance.BorderSize = 0;
            btnAddToCart.Click += BtnAddToCart_Click;

            btnEditThreshold = new Button
            {
                Location = new Point(228, 615),
                Size = new Size(100, 36),
                Text = "Threshold",
                BackColor = Color.FromArgb(75, 85, 99),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEditThreshold.FlatAppearance.BorderSize = 0;
            btnEditThreshold.Click += BtnEditThreshold_Click;

            btnLowStockAlerts = new Button
            {
                Location = new Point(334, 615),
                Size = new Size(115, 36),
                Text = "⚠️ Low Stock",
                BackColor = Color.FromArgb(225, 29, 72),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLowStockAlerts.FlatAppearance.BorderSize = 0;
            btnLowStockAlerts.Click += (s, e) =>
            {
                using (var lowStockForm = new LowStockForm())
                {
                    if (lowStockForm.ShowDialog(this) == DialogResult.OK)
                    {
                        foreach (var item in lowStockForm.RestockCartItems)
                        {
                            var existing = cart.Find(c => c.ProductID == item.ProductID);
                            if (existing != null)
                            {
                                existing.Quantity += item.Quantity;
                            }
                            else
                            {
                                cart.Add(item);
                            }
                        }

                        txtCustomerName.Text = "Supplier Restock Order";
                        RefreshCartGrid();
                    }
                }
            };

            btnZReport = new Button
            {
                Location = new Point(455, 615),
                Size = new Size(140, 36),
                Text = "📊 Z-Report",
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnZReport.FlatAppearance.BorderSize = 0;
            btnZReport.Click += (s, e) =>
            {
                using (var zReportForm = new ZReportForm())
                {
                    zReportForm.ShowDialog(this);
                }
            };

            pnlInventory.Controls.AddRange(new Control[] {
                lblInventoryTitle, lblSearch, txtSearch, dgvInventory,
                lblQtyPrompt, numQuantity, btnAddToCart, btnEditThreshold, btnLowStockAlerts, btnZReport
            });

            // --- Right Panel: Active POS Checkout Cart ---
            Panel pnlCheckout = new Panel
            {
                Location = new Point(660, 20),
                Size = new Size(580, 660),
                BackColor = Color.FromArgb(24, 20, 37)
            };

            Label lblCheckoutTitle = new Label
            {
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 244, 246),
                Text = "Active Checkout Cart"
            };

            // Invoice Manager Button relocated to top right of checkout panel
            btnInvoiceManager = new Button
            {
                Location = new Point(390, 15),
                Size = new Size(170, 34),
                Text = "📄 Invoice Manager",
                BackColor = Color.FromArgb(75, 85, 99),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnInvoiceManager.FlatAppearance.BorderSize = 0;
            btnInvoiceManager.Click += (s, e) =>
            {
                using (var invoiceForm = new InvoiceManagerForm())
                {
                    invoiceForm.ShowDialog(this);
                }
            };

            Label lblCustomer = new Label
            {
                Location = new Point(20, 68),
                AutoSize = true,
                ForeColor = Color.FromArgb(156, 163, 175),
                Text = "Customer / Vendor:"
            };

            txtCustomerName = new TextBox
            {
                Location = new Point(150, 65),
                Size = new Size(410, 27),
                Text = "Walk-in Customer",
                BackColor = Color.FromArgb(35, 30, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            dgvCart = new DataGridView
            {
                Location = new Point(20, 105),
                Size = new Size(540, 420),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.FromArgb(24, 20, 37),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(45, 38, 68),
                EnableHeadersVisualStyles = false
            };
            dgvCart.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(75, 85, 99);
            dgvCart.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvCart.ColumnHeadersHeight = 35;
            dgvCart.DefaultCellStyle.BackColor = Color.FromArgb(24, 20, 37);
            dgvCart.DefaultCellStyle.ForeColor = Color.FromArgb(229, 231, 235);

            lblCartTotal = new Label
            {
                Location = new Point(20, 545),
                AutoSize = true,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 211, 153), // Emerald Green
                Text = "Total: $0.00"
            };

            btnCheckout = new Button
            {
                Location = new Point(360, 595),
                Size = new Size(200, 45),
                Text = "Process Checkout",
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += BtnCheckout_Click;

            pnlCheckout.Controls.AddRange(new Control[] {
                lblCheckoutTitle, btnInvoiceManager, lblCustomer, txtCustomerName, dgvCart, lblCartTotal, btnCheckout
            });

            this.Controls.AddRange(new Control[] { pnlInventory, pnlCheckout });
        }

        private void LoadInventoryData()
        {
            DataTable dt = repository.GetAllProducts();
            dgvInventory.DataSource = dt;
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            DataTable dt = repository.GetAllProducts();
            DataView dv = dt.DefaultView;
            dv.RowFilter = $"ProductName LIKE '%{txtSearch.Text}%' OR SKU LIKE '%{txtSearch.Text}%' OR Category LIKE '%{txtSearch.Text}%'";
            dgvInventory.DataSource = dv.ToTable();
        }

        private void BtnAddToCart_Click(object sender, EventArgs e)
        {
            if (dgvInventory.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product to add.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dgvInventory.SelectedRows[0];
            int productId = Convert.ToInt32(row.Cells["ProductID"].Value);
            string productName = row.Cells["ProductName"].Value.ToString();
            decimal unitPrice = Convert.ToDecimal(row.Cells["UnitPrice"].Value);
            int stockQuantity = Convert.ToInt32(row.Cells["StockQuantity"].Value);
            int requestedQty = (int)numQuantity.Value;

            if (requestedQty > stockQuantity)
            {
                MessageBox.Show($"Insufficient stock available. Only {stockQuantity} left.", "Stock Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var existingItem = cart.Find(c => c.ProductID == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += requestedQty;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductID = productId,
                    ProductName = productName,
                    UnitPrice = unitPrice,
                    Quantity = requestedQty
                });
            }

            RefreshCartGrid();
        }

        private void RefreshCartGrid()
        {
            dgvCart.DataSource = null;
            dgvCart.DataSource = cart;

            decimal total = 0;
            foreach (var item in cart)
            {
                total += item.Subtotal;
            }
            lblCartTotal.Text = $"Total: ${total:N2}";
        }

        private void BtnEditThreshold_Click(object sender, EventArgs e)
        {
            if (dgvInventory.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product to edit its threshold.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = dgvInventory.SelectedRows[0];
            int productId = Convert.ToInt32(row.Cells["ProductID"].Value);
            string productName = row.Cells["ProductName"].Value.ToString();
            int currentThreshold = Convert.ToInt32(row.Cells["ReorderLevel"].Value);

            string input = Microsoft.VisualBasic.Interaction.InputBox(
                $"Enter new reorder threshold for {productName}:",
                "Edit Reorder Threshold",
                currentThreshold.ToString());

            if (int.TryParse(input, out int newThreshold))
            {
                repository.UpdateReorderLevel(productId, newThreshold);
                LoadInventoryData();
                MessageBox.Show("Threshold updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (cart.Count == 0)
            {
                MessageBox.Show("Your cart is empty.", "Checkout Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string customerName = string.IsNullOrWhiteSpace(txtCustomerName.Text) ? "Walk-in Customer" : txtCustomerName.Text;
                long invoiceId = repository.SaveInvoice(customerName, cart);

                MessageBox.Show($"Checkout completed successfully! Invoice #{invoiceId} generated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                cart.Clear();
                RefreshCartGrid();
                LoadInventoryData();
                txtCustomerName.Text = "Walk-in Customer";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Checkout failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}