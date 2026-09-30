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

        public Form1()
        {
            InitializeComponent();
            SetupCustomUI();
            _repository = new InventoryRepository();
            LoadInventory();
        }

        private void SetupCustomUI()
        {
            this.Text = "Business Management System - Inventory Control";
            this.Width = 850;
            this.Height = 520;

            dgvInventory = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 360,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false
            };
            dgvInventory.DataBindingComplete += DgvInventory_DataBindingComplete;

            btnGenerateInvoice = new Button
            {
                Text = "Generate PDF Invoice for Selected Item",
                Dock = DockStyle.Bottom,
                Height = 50,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnGenerateInvoice.Click += BtnGenerateInvoice_Click;

            this.Controls.Add(dgvInventory);
            this.Controls.Add(btnGenerateInvoice);
        }

        private void LoadInventory()
        {
            dgvInventory.DataSource = _repository.GetProducts();
        }

        private void DgvInventory_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Highlight low-stock items in soft red
            foreach (DataGridViewRow row in dgvInventory.Rows)
            {
                if (row.DataBoundItem is Product product)
                {
                    if (product.StockQuantity <= product.ReorderLevel)
                    {
                        row.DefaultCellStyle.BackColor = Color.MistyRose;
                        row.DefaultCellStyle.ForeColor = Color.DarkRed;
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

            // Construct payload dynamically from selected grid row
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