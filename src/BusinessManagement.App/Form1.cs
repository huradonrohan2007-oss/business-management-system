using System;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using BusinessManagement.Core;

namespace BusinessManagement.App
{
    public partial class Form1 : Form
    {
        private readonly InventoryRepository _repository;
        private DataGridView dgvInventory;
        private Button btnGenerateInvoice;
        private Button btnAddProduct;
        private TextBox txtSearch;
        private TableLayoutPanel mainLayout;
        private Panel searchPanel;

        public Form1()
        {
            InitializeComponent();
            _repository = new InventoryRepository();
            SetupCustomUI();
            LoadInventory();
        }

        private void SetupCustomUI()
        {
            this.Text = "Business Management System - Inventory Control";
            this.Width = 1000;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;

            // 1. Root Layout Container
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55f));

            // 2. Top Action Bar Panel (Row 0)
            searchPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };

            Label lblSearch = new Label
            {
                Text = "Search Inventory:",
                AutoSize = true,
                Location = new Point(15, 16),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };

            txtSearch = new TextBox
            {
                Location = new Point(140, 13),
                Width = 320,
                Font = new Font("Segoe UI", 10f)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            btnAddProduct = new Button
            {
                Text = "+ Add New Product",
                Location = new Point(480, 10),
                Width = 160,
                Height = 30,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.LightSteelBlue
            };
            btnAddProduct.Click += BtnAddProduct_Click;

            searchPanel.Controls.Add(lblSearch);
            searchPanel.Controls.Add(txtSearch);
            searchPanel.Controls.Add(btnAddProduct);

            // 3. DataGridView (Row 1)
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

            // 4. Action Button (Row 2)
            btnGenerateInvoice = new Button
            {
                Text = "Generate PDF Invoice for Selected Item",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Margin = new Padding(5)
            };
            btnGenerateInvoice.Click += BtnGenerateInvoice_Click;

            // Assemble controls into TableLayoutPanel
            mainLayout.Controls.Add(searchPanel, 0, 0);
            mainLayout.Controls.Add(dgvInventory, 0, 1);
            mainLayout.Controls.Add(btnGenerateInvoice, 0, 2);

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

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadInventory(txtSearch.Text.Trim());
        }

        private void BtnAddProduct_Click(object sender, EventArgs e)
        {
            using (var addForm = new AddProductForm())
            {
                if (addForm.ShowDialog() == DialogResult.OK && addForm.NewProduct != null)
                {
                    _repository.AddProduct(addForm.NewProduct);
                    LoadInventory(); // Refresh grid automatically
                    MessageBox.Show("New product saved successfully to SQLite database!", "Product Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void BtnGenerateInvoice_Click(object sender, EventArgs e)
        {
            if (dgvInventory.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a product row from the grid first.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedProduct = (Product)dgvInventory.SelectedRows[0].DataBoundItem;

            var invoicePayload = new
            {
                invoice_id = new Random().Next(1000, 9999),
                customer_name = "Retail Client",
                grand_total = selectedProduct.UnitPrice,
                items = new[]
                {
                    new
                    {
                        name = selectedProduct.ProductName,
                        qty = 1,
                        price = selectedProduct.UnitPrice,
                        total = selectedProduct.UnitPrice
                    }
                }
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
                        MessageBox.Show(result, "Invoice Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Execution failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}