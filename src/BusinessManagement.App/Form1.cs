using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using BusinessManagement.Core;

namespace BusinessManagement.App
{
    public partial class Form1 : Form
    {
        private readonly InventoryRepository _repository;
        private readonly List<CartItem> _cart = new List<CartItem>();

        // Controls
        private DataGridView dgvInventory;
        private DataGridView dgvCart;
        private Button btnAddProduct;
        private Button btnAddToCart;
        private Button btnRemoveFromCart;
        private Button btnCheckout;
        private TextBox txtSearch;
        private NumericUpDown numQuantity;
        private Label lblGrandTotal;

        public Form1()
        {
            InitializeComponent();
            _repository = new InventoryRepository();
            SetupCustomUI();
            LoadInventory();
            UpdateCartGrid();
        }

        private void SetupCustomUI()
        {
            this.Text = "Business Management System - Point of Sale & Inventory";
            this.Width = 1200;
            this.Height = 700;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Main Split Panel Layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f)); // Left: Catalog
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f)); // Right: Cart

            // ================= LEFT PANE: CATALOG =================
            var leftPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(5)
            };
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45f));
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45f));

            // Search Bar & Add Product Panel
            var searchPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight
            };

            var lblSearch = new Label { Text = "Search:", AutoSize = true, Margin = new Padding(0, 8, 5, 0), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            txtSearch = new TextBox { Width = 220, Font = new Font("Segoe UI", 9.5f) };
            txtSearch.TextChanged += (s, e) => LoadInventory(txtSearch.Text.Trim());

            btnAddProduct = new Button
            {
                Text = "+ New Product",
                Width = 120,
                Height = 28,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                BackColor = Color.LightSteelBlue,
                Margin = new Padding(10, 0, 0, 0)
            };
            btnAddProduct.Click += BtnAddProduct_Click;

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(btnAddProduct);

            // Inventory DataGrid
            dgvInventory = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            dgvInventory.DataBindingComplete += DgvInventory_DataBindingComplete;

            // Add to Cart Controls
            var addToCartPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight
            };

            var lblQty = new Label { Text = "Qty:", AutoSize = true, Margin = new Padding(0, 8, 5, 0), Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            numQuantity = new NumericUpDown { Value = 1, Minimum = 1, Maximum = 999, Width = 70, Font = new Font("Segoe UI", 9.5f) };

            btnAddToCart = new Button
            {
                Text = "Add to Cart ➔",
                Width = 140,
                Height = 30,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.DarkSeaGreen,
                ForeColor = Color.White,
                Margin = new Padding(15, 0, 0, 0)
            };
            btnAddToCart.Click += BtnAddToCart_Click;

            addToCartPanel.Controls.Add(lblQty);
            addToCartPanel.Controls.Add(numQuantity);
            addToCartPanel.Controls.Add(btnAddToCart);

            leftPanel.Controls.Add(searchPanel, 0, 0);
            leftPanel.Controls.Add(dgvInventory, 0, 1);
            leftPanel.Controls.Add(addToCartPanel, 0, 2);

            // ================= RIGHT PANE: CART =================
            var rightPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(5)
            };
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45f));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 55f));

            var cartHeaderPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight
            };

            var lblCartTitle = new Label
            {
                Text = "Current Order Cart",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 5, 15, 0)
            };

            btnRemoveFromCart = new Button
            {
                Text = "Remove Selected Item",
                Width = 160,
                Height = 28,
                Font = new Font("Segoe UI", 8.5f),
                BackColor = Color.MistyRose
            };
            btnRemoveFromCart.Click += BtnRemoveFromCart_Click;

            cartHeaderPanel.Controls.Add(lblCartTitle);
            cartHeaderPanel.Controls.Add(btnRemoveFromCart);

            // Cart DataGrid
            dgvCart = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            // Total & Checkout Panel
            var checkoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            checkoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            checkoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));

            lblGrandTotal = new Label
            {
                Text = "Total: $0.00",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            btnCheckout = new Button
            {
                Text = "Checkout & Print Invoice PDF",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                Margin = new Padding(5)
            };
            btnCheckout.Click += BtnCheckout_Click;

            checkoutPanel.Controls.Add(lblGrandTotal, 0, 0);
            checkoutPanel.Controls.Add(btnCheckout, 1, 0);

            rightPanel.Controls.Add(cartHeaderPanel, 0, 0);
            rightPanel.Controls.Add(dgvCart, 0, 1);
            rightPanel.Controls.Add(checkoutPanel, 0, 2);

            // Assemble Main Layout
            mainLayout.Controls.Add(leftPanel, 0, 0);
            mainLayout.Controls.Add(rightPanel, 1, 0);

            this.Controls.Add(mainLayout);
        }

        private void LoadInventory(string searchQuery = "")
        {
            var data = string.IsNullOrWhiteSpace(searchQuery)
                ? _repository.GetProducts()
                : _repository.SearchProducts(searchQuery);

            dgvInventory.DataSource = null;
            dgvInventory.DataSource = data;
        }

        private void UpdateCartGrid()
        {
            dgvCart.DataSource = null;
            dgvCart.DataSource = _cart.ToList();

            decimal total = _cart.Sum(item => item.Subtotal);
            lblGrandTotal.Text = $"Total: ${total:N2}";
        }

        private void BtnAddToCart_Click(object sender, EventArgs e)
        {
            if (dgvInventory.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product from the catalog.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var product = (Product)dgvInventory.SelectedRows[0].DataBoundItem;
            int desiredQty = (int)numQuantity.Value;

            if (desiredQty > product.StockQuantity)
            {
                MessageBox.Show($"Cannot add {desiredQty} units. Only {product.StockQuantity} in stock!", "Stock Limit Exceeded", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var existingCartItem = _cart.FirstOrDefault(c => c.ProductID == product.ProductID);
            if (existingCartItem != null)
            {
                if (existingCartItem.Quantity + desiredQty > product.StockQuantity)
                {
                    MessageBox.Show($"Adding {desiredQty} more exceeds available stock ({product.StockQuantity}).", "Stock Limit Exceeded", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                existingCartItem.Quantity += desiredQty;
            }
            else
            {
                _cart.Add(new CartItem
                {
                    ProductID = product.ProductID,
                    SKU = product.SKU,
                    ProductName = product.ProductName,
                    UnitPrice = product.UnitPrice,
                    Quantity = desiredQty
                });
            }

            UpdateCartGrid();
        }

        private void BtnRemoveFromCart_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count == 0) return;

            var selectedCartItem = (CartItem)dgvCart.SelectedRows[0].DataBoundItem;
            _cart.Remove(selectedCartItem);
            UpdateCartGrid();
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (_cart.Count == 0)
            {
                MessageBox.Show("Cart is empty. Add products before checking out.", "Empty Cart", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Prepare Python JSON payload
            var invoicePayload = new
            {
                invoice_id = new Random().Next(1000, 9999),
                customer_name = "Retail Client",
                grand_total = _cart.Sum(i => i.Subtotal),
                items = _cart.Select(i => new
                {
                    name = i.ProductName,
                    qty = i.Quantity,
                    price = i.UnitPrice,
                    total = i.Subtotal
                }).ToArray()
            };

            string jsonString = JsonSerializer.Serialize(invoicePayload);
            string pythonPath = "py";
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\python_engine\pdf_generator.py");

            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = $"\"{scriptPath}\" \"{jsonString.Replace("\"", "\\\"")}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd();

                        // Deduct all cart items in a single transaction
                        if (_repository.DeductStockBatch(_cart))
                        {
                            _cart.Clear();
                            UpdateCartGrid();
                            LoadInventory(txtSearch.Text.Trim());
                            MessageBox.Show($"{result}\nBatch stock update applied successfully!", "Checkout Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to complete transaction due to insufficient stock.", "Transaction Aborted", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Execution failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAddProduct_Click(object sender, EventArgs e)
        {
            using (var addForm = new AddProductForm())
            {
                if (addForm.ShowDialog() == DialogResult.OK && addForm.NewProduct != null)
                {
                    _repository.AddProduct(addForm.NewProduct);
                    LoadInventory();
                    MessageBox.Show("New product saved successfully!", "Product Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void DgvInventory_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dgvInventory.Rows)
            {
                if (row.DataBoundItem is Product product)
                {
                    if (product.StockQuantity <= product.ReorderLevel)
                    {
                        row.DefaultCellStyle.BackColor = Color.MistyRose;
                        row.DefaultCellStyle.ForeColor = Color.DarkRed;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.White;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                    }
                }
            }
        }
    }
}