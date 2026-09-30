using System;
using System.Drawing;
using System.Windows.Forms;
using BusinessManagement.Core;

namespace BusinessManagement.App
{
    public partial class AddProductForm : Form
    {
        public Product NewProduct { get; private set; }

        private TextBox txtSKU, txtName, txtCategory, txtPrice, txtStock, txtReorder;
        private Button btnSave, btnCancel;

        public AddProductForm()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Add New Inventory Product";
            this.Width = 380;
            this.Height = 320;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            int lblX = 20, txtX = 130, startY = 20, spacing = 35;

            // SKU
            this.Controls.Add(new Label { Text = "SKU:", Location = new Point(lblX, startY), AutoSize = true });
            txtSKU = new TextBox { Location = new Point(txtX, startY - 3), Width = 200 };
            this.Controls.Add(txtSKU);

            // Product Name
            this.Controls.Add(new Label { Text = "Product Name:", Location = new Point(lblX, startY + spacing), AutoSize = true });
            txtName = new TextBox { Location = new Point(txtX, startY + spacing - 3), Width = 200 };
            this.Controls.Add(txtName);

            // Category
            this.Controls.Add(new Label { Text = "Category:", Location = new Point(lblX, startY + spacing * 2), AutoSize = true });
            txtCategory = new TextBox { Location = new Point(txtX, startY + spacing * 2 - 3), Width = 200 };
            this.Controls.Add(txtCategory);

            // Unit Price
            this.Controls.Add(new Label { Text = "Unit Price ($):", Location = new Point(lblX, startY + spacing * 3), AutoSize = true });
            txtPrice = new TextBox { Location = new Point(txtX, startY + spacing * 3 - 3), Width = 200 };
            this.Controls.Add(txtPrice);

            // Stock Quantity
            this.Controls.Add(new Label { Text = "Stock Quantity:", Location = new Point(lblX, startY + spacing * 4), AutoSize = true });
            txtStock = new TextBox { Location = new Point(txtX, startY + spacing * 4 - 3), Width = 200 };
            this.Controls.Add(txtStock);

            // Reorder Level
            this.Controls.Add(new Label { Text = "Reorder Level:", Location = new Point(lblX, startY + spacing * 5), AutoSize = true });
            txtReorder = new TextBox { Location = new Point(txtX, startY + spacing * 5 - 3), Width = 200, Text = "5" };
            this.Controls.Add(txtReorder);

            // Buttons
            btnSave = new Button { Text = "Save", Location = new Point(130, startY + spacing * 6), Width = 95, DialogResult = DialogResult.OK };
            btnCancel = new Button { Text = "Cancel", Location = new Point(235, startY + spacing * 6), Width = 95, DialogResult = DialogResult.Cancel };

            btnSave.Click += BtnSave_Click;

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSKU.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("SKU and Product Name are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || !int.TryParse(txtStock.Text, out int stock))
            {
                MessageBox.Show("Please enter valid numeric values for Price and Stock.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            int.TryParse(txtReorder.Text, out int reorder);

            NewProduct = new Product
            {
                SKU = txtSKU.Text.Trim(),
                ProductName = txtName.Text.Trim(),
                Category = txtCategory.Text.Trim(),
                UnitPrice = price,
                StockQuantity = stock,
                ReorderLevel = reorder
            };
        }
    }
}