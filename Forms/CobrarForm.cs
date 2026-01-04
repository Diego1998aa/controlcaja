using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using SistemaPOS.Models;
using SistemaPOS.Services;

namespace SistemaPOS.Forms
{
    public partial class CobrarForm : Form
    {
        private List<ItemVenta> items;
        private decimal total;
        private decimal tasaIVA;
        private TextBox txtRecibido;

        public CobrarForm(List<ItemVenta> items, decimal tasaIVA)
        {
            this.items = items;
            this.tasaIVA = tasaIVA;
            this.total = items.Sum(i => i.Subtotal) * (1 + tasaIVA);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(37, 37, 38);

            Label lblTotal = new Label { Text = $"Total a Pagar: {total:C}", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 20), AutoSize = true };
            
            Label lblRec = new Label { Text = "Monto Recibido:", ForeColor = Color.White, Location = new Point(20, 80) };
            txtRecibido = new TextBox { Location = new Point(20, 105), Size = new Size(200, 30), Font = new Font("Segoe UI", 12) };

            Button btnPagar = new Button { Text = "CONFIRMAR PAGO", Location = new Point(20, 160), Size = new Size(340, 50), BackColor = Color.SeaGreen, ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            btnPagar.Click += BtnPagar_Click;

            this.Controls.AddRange(new Control[] { lblTotal, lblRec, txtRecibido, btnPagar });
        }

        private void BtnPagar_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtRecibido.Text, out decimal recibido) && recibido >= total)
            {
                try
                {
                    VentaService.RegistrarVenta(items, "Efectivo", recibido, SesionActual.UsuarioActivo.IdUsuario, 0, tasaIVA);
                    MessageBox.Show($"Venta Exitosa. Cambio: {(recibido - total):C}");
                    this.DialogResult = DialogResult.OK;
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
            else MessageBox.Show("Monto insuficiente o inválido");
        }
    }
}