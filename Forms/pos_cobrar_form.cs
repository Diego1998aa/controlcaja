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
        private decimal tasaIVA;
        private decimal subtotal;
        private decimal iva;
        private decimal total;
        private Label lblTotal, lblRecibido, lblCambio;
        private TextBox txtMonto;
        private ComboBox cboMetodoPago;

        public CobrarForm(List<ItemVenta> items, decimal tasaIVA)
        {
            this.items = items;
            this.tasaIVA = tasaIVA;
            CalcularTotales();
            InitializeComponent();
        }

        private void CalcularTotales()
        {
            subtotal = items.Sum(i => i.Subtotal);
            iva = subtotal * tasaIVA;
            total = subtotal + iva;
        }

        private void InitializeComponent()
        {
            this.Text = "Cobrar Venta";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(37, 37, 38);

            // Panel principal
            Panel panelPrincipal = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(440, 370),
                BackColor = Color.FromArgb(45, 45, 48)
            };

            // Título
            Label lblTitulo = new Label
            {
                Text = "TOTAL A PAGAR",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = false,
                Size = new Size(400, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Total
            lblTotal = new Label
            {
                Text = total.ToString("C"),
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = Color.FromArgb(34, 139, 34),
                Location = new Point(20, 60),
                AutoSize = false,
                Size = new Size(400, 60),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Método de pago
            Label lblMetodo = new Label
            {
                Text = "Método de Pago:",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                Location = new Point(30, 140),
                AutoSize = true
            };

            cboMetodoPago = new ComboBox
            {
                Font = new Font("Segoe UI", 11),
                Location = new Point(30, 165),
                Size = new Size(380, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboMetodoPago.Items.AddRange(new string[] { "Efectivo", "Tarjeta", "Transferencia" });
            cboMetodoPago.SelectedIndex = 0;
            cboMetodoPago.SelectedIndexChanged += CboMetodoPago_SelectedIndexChanged;

            // Monto recibido
            Label lblMontoRecibido = new Label
            {
                Text = "Monto Recibido:",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                Location = new Point(30, 210),
                AutoSize = true
            };

            txtMonto = new TextBox
            {
                Font = new Font("Segoe UI", 14),
                Location = new Point(30, 235),
                Size = new Size(200, 35),
                TextAlign = HorizontalAlignment.Right
            };
            txtMonto.TextChanged += TxtMonto_TextChanged;
            txtMonto.KeyPress += TxtMonto_KeyPress;

            // Botón montos rápidos
            FlowLayoutPanel panelBotones = new FlowLayoutPanel
            {
                Location = new Point(240, 210),
                Size = new Size(170, 70),
                FlowDirection = FlowDirection.LeftToRight
            };

            Button btn50 = CrearBotonRapido("$50", 50);
            Button btn100 = CrearBotonRapido("$100", 100);
            Button btn200 = CrearBotonRapido("$200", 200);
            Button btn500 = CrearBotonRapido("$500", 500);

            panelBotones.Controls.Add(btn50);
            panelBotones.Controls.Add(btn100);
            panelBotones.Controls.Add(btn200);
            panelBotones.Controls.Add(btn500);

            // Cambio
            lblCambio = new Label
            {
                Text = "CAMBIO: $0.00",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 140, 0),
                Location = new Point(30, 290),
                AutoSize = false,
                Size = new Size(380, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Botones acción
            Button btnConfirmar = new Button
            {
                Text = "✓ CONFIRMAR VENTA",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(30, 330),
                Size = new Size(240, 45),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnConfirmar.FlatAppearance.BorderSize = 0;
            btnConfirmar.Click += BtnConfirmar_Click;

            Button btnCancelar = new Button
            {
                Text = "✗ CANCELAR",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(280, 330),
                Size = new Size(130, 45),
                BackColor = Color.FromArgb(139, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            // Agregar controles
            panelPrincipal.Controls.Add(lblTitulo);
            panelPrincipal.Controls.Add(lblTotal);
            panelPrincipal.Controls.Add(lblMetodo);
            panelPrincipal.Controls.Add(cboMetodoPago);
            panelPrincipal.Controls.Add(lblMontoRecibido);
            panelPrincipal.Controls.Add(txtMonto);
            panelPrincipal.Controls.Add(panelBotones);
            panelPrincipal.Controls.Add(lblCambio);
            panelPrincipal.Controls.Add(btnConfirmar);
            panelPrincipal.Controls.Add(btnCancelar);

            this.Controls.Add(panelPrincipal);

            // Configurar según método de pago inicial
            CboMetodoPago_SelectedIndexChanged(null, null);
        }

        private Button CrearBotonRapido(string texto, decimal valor)
        {
            Button btn = new Button
            {
                Text = texto,
                Font = new Font("Segoe UI", 9),
                Size = new Size(75, 30),
                BackColor = Color.FromArgb(63, 63, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(2)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) =>
            {
                decimal montoActual = 0;
                decimal.TryParse(txtMonto.Text, out montoActual);
                txtMonto.Text = (montoActual + valor).ToString("F2");
            };
            return btn;
        }

        private void CboMetodoPago_SelectedIndexChanged(object sender, EventArgs e)
        {
            string metodo = cboMetodoPago.SelectedItem?.ToString();
            
            if (metodo == "Tarjeta" || metodo == "Transferencia")
            {
                // Para tarjeta y transferencia, el monto es exacto
                txtMonto.Text = total.ToString("F2");
                txtMonto.Enabled = false;
                lblCambio.Text = "CAMBIO: $0.00";
            }
            else
            {
                // Para efectivo, permitir ingresar monto
                txtMonto.Enabled = true;
                txtMonto.Clear();
                txtMonto.Focus();
            }
        }

        private void TxtMonto_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Solo números y punto decimal
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // Solo un punto decimal
            if (e.KeyChar == '.' && txtMonto.Text.Contains("."))
            {
                e.Handled = true;
            }

            // Enter para confirmar
            if (e.KeyChar == (char)Keys.Enter)
            {
                BtnConfirmar_Click(sender, e);
                e.Handled = true;
            }
        }

        private void TxtMonto_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtMonto.Text, out decimal montoRecibido))
            {
                decimal cambio = montoRecibido - total;
                lblCambio.Text = $"CAMBIO: {cambio:C}";
                lblCambio.ForeColor = cambio >= 0 ? Color.FromArgb(34, 139, 34) : Color.FromArgb(220, 20, 60);
            }
            else
            {
                lblCambio.Text = "CAMBIO: $0.00";
                lblCambio.ForeColor = Color.FromArgb(255, 140, 0);
            }
        }

        private void BtnConfirmar_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(txtMonto.Text, out decimal montoRecibido))
            {
                MessageBox.Show("Por favor ingrese un monto válido.", "Monto Inválido", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (montoRecibido < total)
            {
                MessageBox.Show("El monto recibido es menor al total.", "Monto Insuficiente",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Registrar venta
                int idVenta = VentaService.RegistrarVenta(
                    items,
                    cboMetodoPago.SelectedItem.ToString(),
                    montoRecibido,
                    SesionActual.UsuarioActivo.IdUsuario,
                    0,
                    tasaIVA
                );

                // Imprimir ticket
                ImprimirTicket(idVenta, montoRecibido);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar la venta: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ImprimirTicket(int idVenta, decimal montoRecibido)
        {
            // Aquí se integraría con la impresora térmica
            // Por ahora, mostrar un preview del ticket
            
            string ticket = "================================\n";
            ticket += "        SISTEMA POS\n";
            ticket += "================================\n";
            ticket += $"Venta No: {idVenta}\n";
            ticket += $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}\n";
            ticket += $"Cajero: {SesionActual.UsuarioActivo.NombreCompleto}\n";
            ticket += "================================\n\n";

            foreach (var item in items)
            {
                ticket += $"{item.Producto.Nombre}\n";
                ticket += $"  {item.Cantidad} x {item.Producto.PrecioVenta:C} = {item.Subtotal:C}\n";
            }

            ticket += "\n================================\n";
            ticket += $"Subtotal:     {subtotal,12:C}\n";
            ticket += $"IVA ({tasaIVA:P0}):      {iva,12:C}\n";
            ticket += $"TOTAL:        {total,12:C}\n";
            ticket += "================================\n";
            ticket += $"Método: {cboMetodoPago.SelectedItem}\n";
            ticket += $"Recibido:     {montoRecibido,12:C}\n";
            ticket += $"Cambio:       {(montoRecibido - total),12:C}\n";
            ticket += "================================\n";
            ticket += "    ¡Gracias por su compra!\n";
            ticket += "================================\n";

            // Mostrar en MessageBox (en producción, enviar a impresora)
            MessageBox.Show(ticket, "Ticket de Venta", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}