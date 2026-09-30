import sys
import json
from reportlab.lib.pagesizes import letter
from reportlab.pdfgen import canvas

def generate_invoice(invoice_data):
    pdf_path = f"Invoice_{invoice_data['invoice_id']}.pdf"
    c = canvas.Canvas(pdf_path, pagesize=letter)
    
    # Header
    c.setFont("Helvetica-Bold", 18)
    c.drawString(50, 750, "BUSINESS MANAGEMENT SYSTEM")
    c.setFont("Helvetica", 10)
    c.drawString(50, 735, f"Invoice #: {invoice_data['invoice_id']}")
    c.drawString(50, 720, f"Customer: {invoice_data['customer_name']}")
    
    # Table Headers
    c.line(50, 700, 550, 700)
    c.drawString(50, 685, "Item")
    c.drawString(300, 685, "Qty")
    c.drawString(400, 685, "Price")
    c.drawString(480, 685, "Total")
    c.line(50, 675, 550, 675)
    
    # Line Items
    y = 655
    for item in invoice_data['items']:
        c.drawString(50, y, item['name'])
        c.drawString(300, y, str(item['qty']))
        c.drawString(400, y, f"${item['price']:.2f}")
        c.drawString(480, y, f"${item['total']:.2f}")
        y -= 20
        
    c.line(50, y, 550, y)
    c.setFont("Helvetica-Bold", 12)
    c.drawString(400, y - 25, f"Grand Total: ${invoice_data['grand_total']:.2f}")
    
    c.save()
    print(f"Successfully generated {pdf_path}")

if __name__ == "__main__":
    sample_data = {
        "invoice_id": 1001,
        "customer_name": "Acme Corp",
        "grand_total": 150.00,
        "items": [
            {"name": "Wireless Mouse", "qty": 2, "price": 25.00, "total": 50.00},
            {"name": "Mechanical Keyboard", "qty": 1, "price": 100.00, "total": 100.00}
        ]
    }
    generate_invoice(sample_data)
