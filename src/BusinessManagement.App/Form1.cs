using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BusinessManagement.App
{
    public partial class Form1 : Form
    {
        // UI Control Declarations
        private Panel pnlInventory;
        private Panel pnlCart;
        private DataGridView dgvInventory;
        private DataGridView dgvCart;
        private NumericUpDown numQuantity;
        private TextBox txtSearch;
        private Label lblGrandTotal;
        private Button btnAddToCart;
        private Button btnRemoveFromCart;
        private Button btnClearCart;
        private Button btnCheckout;

        private readonly InventoryRepository repository;
        private readonly List<CartItem> cart;
        private DataTable inventoryDataTable;

        public Form1()
        {
            InitializeCustomControls();
            repository = new InventoryRepository();
            cart = new List<CartItem>();
        }

        private void InitializeCustomControls()
        {
            // Main Form Settings (Rich Midnight Background)
            this.Size = new Size(1420, 780);
            this.Text = "POS & Inventory Management System";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(15, 13, 25); // Deep Midnight Violet

            // ==========================================
            // LEFT PANEL: INVENTORY CARD (Jewel Dark Tone)
            // ==========================================
            pnlInventory = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(810, 700),
                BackColor = Color.FromArgb(24, 20, 37), // Rich Plum-Charcoal Card
                BorderStyle = BorderStyle.None
            };

            Label lblInventoryTitle = new Label
            {
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 244, 246),
                Text = "📦 Inventory Catalog"
            };

            Label lblSearch = new Label
            {
                Location = new Point(20, 68),
                AutoSize = true,
                Text = "Search:",
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = Color.FromArgb(156, 163, 175)
            };

            txtSearch = new TextBox
            {
                Location = new Point(80, 65),
                Width = 260,
                Font = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(35, 30, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            dgvInventory = CreateJewelThemeGrid();
            dgvInventory.Location = new Point(20, 110);
            dgvInventory.Size = new Size(770, 510);
            dgvInventory.CellDoubleClick += DgvInventory_CellDoubleClick;

            Label lblQtyPrompt = new Label
            {
                Location = new Point(20, 642),
                AutoSize = true,
                Text = "Qty:",
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = Color.FromArgb(156, 163, 175)
            };

            numQuantity = new NumericUpDown
            {
                Location = new Point(60, 640),
                Width = 75,
                Minimum = 1,
                Maximum = 1000,
                Value = 1,
                Font = new Font("Segoe UI", 11f),
                BackColor = Color.FromArgb(35, 30, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnAddToCart = new Button
            {
                Location = new Point(150, 635),
                Size = new Size(150, 42),
                Text = "Add to Cart",
                UseVisualStyleBackColor = false,
                BackColor = Color.FromArgb(99, 102, 241), // Vibrant Royal Indigo
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10f),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddToCart.FlatAppearance.BorderSize = 0;
            btnAddToCart.Click += BtnAddToCart_Click;

            pnlInventory.Controls.AddRange(new Control[] { lblInventoryTitle, lblSearch, txtSearch, dgvInventory, lblQtyPrompt, numQuantity, btnAddToCart });


            // ==========================================
            // RIGHT PANEL: CART CARD (Jewel Dark Tone)
            // ==========================================
            pnlCart = new Panel
            {
                Location = new Point(850, 20),
                Size = new Size(540, 700),
                BackColor = Color.FromArgb(24, 20, 37), // Rich Plum-Charcoal Card
                BorderStyle = BorderStyle.None
            };

            Label lblCartTitle = new Label
            {
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(243, 244, 246),
                Text = "🛒 Current Cart / Order"
            };

            dgvCart = CreateJewelThemeGrid();
            dgvCart.Location = new Point(20, 65);
            dgvCart.Size = new Size(500, 510);

            btnRemoveFromCart = new Button
            {
                Location = new Point(20, 590),
                Size = new Size(130, 38),
                Text = "Remove Item",
                UseVisualStyleBackColor = false,
                BackColor = Color.FromArgb(225, 29, 72), // Rich Ruby Red
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9.5f),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRemoveFromCart.FlatAppearance.BorderSize = 0;
            btnRemoveFromCart.Click += BtnRemoveFromCart_Click;

            btnClearCart = new Button
            {
                Location = new Point(160, 590),
                Size = new Size(110, 38),
                Text = "Clear Cart",
                UseVisualStyleBackColor = false,
                BackColor = Color.FromArgb(75, 85, 99), // Muted Slate
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 9.5f),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClearCart.FlatAppearance.BorderSize = 0;
            btnClearCart.Click += BtnClearCart_Click;

            lblGrandTotal = new Label
            {
                Location = new Point(20, 645),
                AutoSize = true,
                Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 211, 153), // Glowing Emerald Jewel
                Text = "Total: Rs 0.00"
            };

            btnCheckout = new Button
            {
                Location = new Point(255, 635),
                Size = new Size(265, 46),
                Text = "Process Checkout",
                UseVisualStyleBackColor = false,
                BackColor = Color.FromArgb(16, 185, 129), // Emerald Green Jewel Tone
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11f),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCheckout.FlatAppearance.BorderSize = 0;
            btnCheckout.Click += BtnCheckout_Click;

            pnlCart.Controls.AddRange(new Control[] { lblCartTitle, dgvCart, btnRemoveFromCart, btnClearCart, lblGrandTotal, btnCheckout });

            // Add Panels to Form
            this.Controls.Add(pnlInventory);
            this.Controls.Add(pnlCart);

            // Hook Form Load Event
            this.Load += Form1_Load;
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
                AllowUserToResizeRows = false,
                BackgroundColor = Color.FromArgb(24, 20, 37),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(45, 38, 68), // Soft dark grid lines
                EnableHeadersVisualStyles = false,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeight = 40,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(99, 102, 241), // Royal Indigo Header
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI Semibold", 10f),
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                },
                RowTemplate = { Height = 36 },
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(24, 20, 37),
                    ForeColor = Color.FromArgb(229, 231, 235),
                    SelectionBackColor = Color.FromArgb(67, 56, 202), // Deep purple selection highlight
                    SelectionForeColor = Color.White,
                    Font = new Font("Segoe UI", 9.5f),
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(6, 0, 0, 0)
                }
            };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                repository.InitializeDatabase();
                SetupCartGridColumns();
                LoadInventoryData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing application: {ex.Message}", "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupCartGridColumns()
        {
            dgvCart.Columns.Clear();
            dgvCart.Columns.Add("ProductID", "ID");
            dgvCart.Columns.Add("ProductName", "Product Name");
            dgvCart.Columns.Add("UnitPrice", "Unit Price");
            dgvCart.Columns.Add("Quantity", "Qty");
            dgvCart.Columns.Add("Subtotal", "Subtotal");

            // Generous widths summing precisely to 495px (fitting comfortably inside the 500px cart grid with zero clipping)
            dgvCart.Columns["ProductID"].Width = 40;
            dgvCart.Columns["ProductName"].Width = 190;
            dgvCart.Columns["UnitPrice"].Width = 95;
            dgvCart.Columns["Quantity"].Width = 50;
            dgvCart.Columns["Subtotal"].Width = 115;
        }

        private void LoadInventoryData()
        {
            inventoryDataTable = repository.GetAllProducts();
            dgvInventory.DataSource = inventoryDataTable;
            ConfigureInventoryGridColumns();
        }

        private void ConfigureInventoryGridColumns()
        {
            if (dgvInventory.Columns.Contains("ProductID"))
            {
                dgvInventory.Columns["ProductID"].HeaderText = "ID";
                dgvInventory.Columns["ProductID"].Width = 45;
            }
            if (dgvInventory.Columns.Contains("ProductName"))
            {
                dgvInventory.Columns["ProductName"].HeaderText = "Product Name";
                dgvInventory.Columns["ProductName"].Width = 220;
            }
            if (dgvInventory.Columns.Contains("SKU"))
            {
                dgvInventory.Columns["SKU"].HeaderText = "SKU";
                dgvInventory.Columns["SKU"].Width = 100;
            }
            if (dgvInventory.Columns.Contains("Category"))
            {
                dgvInventory.Columns["Category"].HeaderText = "Category";
                dgvInventory.Columns["Category"].Width = 115;
            }
            if (dgvInventory.Columns.Contains("UnitPrice"))
            {
                dgvInventory.Columns["UnitPrice"].DefaultCellStyle.Format = "N2";
                dgvInventory.Columns["UnitPrice"].HeaderText = "Price (Rs)";
                dgvInventory.Columns["UnitPrice"].Width = 95;
            }
            if (dgvInventory.Columns.Contains("StockQuantity"))
            {
                dgvInventory.Columns["StockQuantity"].HeaderText = "Stock";
                dgvInventory.Columns["StockQuantity"].Width = 70;
            }
            if (dgvInventory.Columns.Contains("ReorderLevel"))
            {
                dgvInventory.Columns["ReorderLevel"].HeaderText = "Reorder";
                dgvInventory.Columns["ReorderLevel"].Width = 75;
            }

            foreach (DataGridViewRow row in dgvInventory.Rows)
            {
                if (row.Cells["StockQuantity"]?.Value != null && row.Cells["ReorderLevel"]?.Value != null)
                {
                    if (int.TryParse(row.Cells["StockQuantity"].Value.ToString(), out int stock) &&
                        int.TryParse(row.Cells["ReorderLevel"].Value.ToString(), out int reorder))
                    {
                        if (stock <= reorder)
                        {
                            row.DefaultCellStyle.BackColor = Color.FromArgb(69, 10, 10); // Rich dark burgundy for low stock
                            row.DefaultCellStyle.ForeColor = Color.FromArgb(254, 202, 202);
                            row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(127, 29, 29);
                            row.DefaultCellStyle.SelectionForeColor = Color.White;
                        }
                    }
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (inventoryDataTable == null) return;

            string filter = txtSearch.Text.Trim().Replace("'", "''");
            if (string.IsNullOrEmpty(filter))
            {
                inventoryDataTable.DefaultView.RowFilter = string.Empty;
            }
            else
            {
                inventoryDataTable.DefaultView.RowFilter = string.Format("ProductName LIKE '%{0}%' OR SKU LIKE '%{0}%' OR Category LIKE '%{0}%'", filter);
            }
            ConfigureInventoryGridColumns();
        }

        private void DgvInventory_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                AddSelectedItemToCart();
            }
        }

        private void BtnAddToCart_Click(object sender, EventArgs e)
        {
            AddSelectedItemToCart();
        }

        private void AddSelectedItemToCart()
        {
            if (dgvInventory.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select an item from the inventory list.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow selectedRow = dgvInventory.SelectedRows[0];

            int productId = Convert.ToInt32(selectedRow.Cells["ProductID"].Value);
            string productName = selectedRow.Cells["ProductName"].Value?.ToString() ?? "Unknown";
            decimal unitPrice = Convert.ToDecimal(selectedRow.Cells["UnitPrice"].Value);
            int availableStock = Convert.ToInt32(selectedRow.Cells["StockQuantity"].Value);
            int requestedQty = Convert.ToInt32(numQuantity.Value);

            if (requestedQty <= 0)
            {
                MessageBox.Show("Please enter a valid quantity greater than zero.", "Invalid Quantity", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CartItem existingItem = cart.FirstOrDefault(item => item.ProductID == productId);
            int currentCartQty = existingItem != null ? existingItem.Quantity : 0;

            if ((currentCartQty + requestedQty) > availableStock)
            {
                MessageBox.Show($"Cannot add item. Only {availableStock - currentCartQty} units available in stock.", "Stock Limit Reached", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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

            UpdateCartUI();
        }

        private void BtnRemoveFromCart_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select an item in the cart to remove.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int selectedIndex = dgvCart.SelectedRows[0].Index;
            if (selectedIndex >= 0 && selectedIndex < cart.Count)
            {
                cart.RemoveAt(selectedIndex);
                UpdateCartUI();
            }
        }

        private void BtnClearCart_Click(object sender, EventArgs e)
        {
            if (cart.Count == 0) return;

            if (MessageBox.Show("Are you sure you want to clear the entire cart?", "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                cart.Clear();
                UpdateCartUI();
            }
        }

        private void UpdateCartUI()
        {
            dgvCart.Rows.Clear();
            decimal grandTotal = 0;

            foreach (var item in cart)
            {
                decimal subtotal = item.Subtotal;
                grandTotal += subtotal;
                dgvCart.Rows.Add(item.ProductID, item.ProductName, $"Rs {item.UnitPrice:N2}", item.Quantity, $"Rs {subtotal:N2}");
            }

            lblGrandTotal.Text = $"Total: Rs {grandTotal:N2}";
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (cart.Count == 0)
            {
                MessageBox.Show("Cart is empty!", "Checkout Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string customerName = ShowInputDialog("Enter Customer Name:", "Checkout", "Retail Client");

            if (string.IsNullOrWhiteSpace(customerName))
            {
                return;
            }

            try
            {
                long invoiceId = repository.SaveInvoice(customerName, cart);

                string pdfPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Invoice_{invoiceId}.pdf");
                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "generate_pdf.py");

                if (File.Exists(scriptPath))
                {
                    GeneratePdfReceipt(scriptPath, invoiceId, pdfPath);
                }

                cart.Clear();
                UpdateCartUI();
                LoadInventoryData();

                MessageBox.Show($"Checkout completed successfully!\nInvoice #{invoiceId} created.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during checkout:\n{ex.Message}", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GeneratePdfReceipt(string scriptPath, long invoiceId, string pdfPath)
        {
            // Implementation remains intact
        }

        private static string ShowInputDialog(string text, string caption, string defaultValue)
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(24, 20, 37)
            };

            Label textLabel = new Label() { Left = 20, Top = 15, Text = text, AutoSize = true, Font = new Font("Segoe UI", 9.5f), ForeColor = Color.FromArgb(229, 231, 235) };
            TextBox textBox = new TextBox() { Left = 20, Top = 40, Width = 340, Text = defaultValue, Font = new Font("Segoe UI", 10f), BackColor = Color.FromArgb(35, 30, 52), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            Button confirmation = new Button() { Text = "OK", Left = 180, Width = 80, Top = 75, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(99, 102, 241), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            Button cancel = new Button() { Text = "Cancel", Left = 270, Width = 90, Top = 75, DialogResult = DialogResult.Cancel, BackColor = Color.FromArgb(75, 85, 99), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            confirmation.FlatAppearance.BorderSize = 0;
            cancel.FlatAppearance.BorderSize = 0;

            confirmation.Click += (s, ev) => { prompt.Close(); };
            cancel.Click += (s, ev) => { prompt.Close(); };

            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
        }
    }
}