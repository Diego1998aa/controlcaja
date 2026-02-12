using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Drawing.Printing;
using SistemaPOS.Models;
using SistemaPOS.Services;
using SistemaPOS.Data;
using SistemaPOS.Helpers;

namespace SistemaPOS.Forms
{
    public partial class CobrarForm : Form
    {
        private List<ItemVenta> items;
        private decimal total;
        private decimal tasaIVA;
        private TextBox txtRecibido;
        private ComboBox cboMetodoPago;
        private TextBox txtVoucher;
        private int? idPedido;

        public CobrarForm(List<ItemVenta> items, decimal tasaIVA, int? idPedido = null)
        {
            this.items = items;
            this.tasaIVA = tasaIVA;
            this.idPedido = idPedido;
            this.total = items.Sum(i => i.Subtotal);
            InitializeComponent();
            if (idPedido.HasValue) this.Text = string.Format("Cobrar Pedido #{0}", idPedido);
        }

        private void InitializeComponent()
        {
            this.Text = "Cobrar";
            this.Size = new Size(600, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            
            UITheme.ApplyTheme(this);

            Label lblTotal = new Label 
            { 
                Text = string.Format("Total a Pagar: {0:C}", total), 
                Font = new Font("Segoe UI", 24, FontStyle.Bold), 
                ForeColor = UITheme.SuccessColor, 
                Location = new Point(40, 30), 
                AutoSize = true 
            };
            
            Label lblMetodo = new Label
            {
                Text = "Método de Pago:",
                ForeColor = UITheme.TextPrimary,
                Location = new Point(40, 100),
                AutoSize = true,
                Font = new Font("Segoe UI", 14)
            };

            cboMetodoPago = new ComboBox
            {
                Location = new Point(220, 96),
                Size = new Size(320, 32),
                Font = new Font("Segoe UI", 12),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboMetodoPago.Items.AddRange(new string[] { "Efectivo", "Tarjeta" });
            cboMetodoPago.SelectedIndex = 0;
            cboMetodoPago.SelectedIndexChanged += CboMetodoPago_SelectedIndexChanged;

            Label lblRec = new Label 
            { 
                Text = "Monto Recibido:", 
                ForeColor = UITheme.TextPrimary, 
                Location = new Point(40, 140),
                AutoSize = true,
                Font = new Font("Segoe UI", 14)
            };

            txtRecibido = new TextBox 
            { 
                Location = new Point(40, 175), 
                Size = new Size(500, 40), 
                Font = new Font("Segoe UI", 18) 
            };
            UITheme.StyleTextBox(txtRecibido);
            txtRecibido.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnPagar_Click(s, e); };

            Label lblVoucher = new Label
            {
                Text = "Número de baucher:",
                ForeColor = UITheme.TextPrimary,
                Location = new Point(40, 225),
                AutoSize = true,
                Font = new Font("Segoe UI", 14)
            };

            txtVoucher = new TextBox
            {
                Location = new Point(40, 260),
                Size = new Size(500, 40),
                Font = new Font("Segoe UI", 16)
            };
            UITheme.StyleTextBox(txtVoucher);

            Button btnPagar = new Button 
            { 
                Text = "CONFIRMAR PAGO", 
                Location = new Point(40, 320), 
                Size = new Size(500, 80), 
                Font = new Font("Segoe UI", 16, FontStyle.Bold) 
            };
            UITheme.StyleButton(btnPagar, UITheme.SuccessColor);
            btnPagar.Click += BtnPagar_Click;
            
            Button btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(40, 410),
                Size = new Size(500, 50),
                Font = new Font("Segoe UI", 12)
            };
            UITheme.StyleButton(btnCancelar, UITheme.ErrorColor);
            btnCancelar.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { lblTotal, lblMetodo, cboMetodoPago, lblRec, txtRecibido, lblVoucher, txtVoucher, btnPagar, btnCancelar });

            CboMetodoPago_SelectedIndexChanged(null, null);
        }

        private void CboMetodoPago_SelectedIndexChanged(object sender, EventArgs e)
        {
            string metodo = cboMetodoPago.SelectedItem != null ? cboMetodoPago.SelectedItem.ToString() : null;
            bool esTarjeta = metodo == "Tarjeta";
            txtVoucher.Enabled = esTarjeta;
            txtVoucher.Visible = esTarjeta;
            // Para tarjeta, el monto recibido es igual al total y deshabilitado
            if (esTarjeta)
            {
                txtRecibido.Text = total.ToString("N0");
                txtRecibido.Enabled = false;
            }
            else
            {
                txtRecibido.Enabled = true;
                txtRecibido.Clear();
            }
        }

        private void BtnPagar_Click(object sender, EventArgs e)
        {
            string metodo = cboMetodoPago.SelectedItem != null ? cboMetodoPago.SelectedItem.ToString() : "Efectivo";
            if (metodo == "Tarjeta")
            {
                if (string.IsNullOrWhiteSpace(txtVoucher.Text))
                {
                    MessageBox.Show("Ingrese el número de baucher de la tarjeta.", "Dato requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtVoucher.Focus();
                    return;
                }
            }

            decimal recibido;
            if (decimal.TryParse(txtRecibido.Text, out recibido) && recibido >= total)
            {
                try
                {
                    string voucher = metodo == "Tarjeta" ? txtVoucher.Text.Trim() : null;
                    int idVenta = VentaService.RegistrarVenta(items, metodo, recibido, SesionActual.UsuarioActivo.IdUsuario, 0, tasaIVA, voucher, idPedido);
                    
                    try 
                    {
                        ImprimirTicket(idVenta, recibido, recibido - total, metodo, voucher);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Venta registrada pero hubo un error al imprimir el ticket: " + ex.Message);
                    }

                    if (metodo == "Efectivo")
                    {
                        MessageBox.Show(string.Format("Venta Exitosa. Cambio: {0:C}", recibido - total));
                    }
                    else
                    {
                        MessageBox.Show("Venta Exitosa con tarjeta.");
                    }
                    this.DialogResult = DialogResult.OK;
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
            else MessageBox.Show("Monto insuficiente o inválido");
        }

        private void ImprimirTicket(int idVenta, decimal montoRecibido, decimal cambio, string metodoPago, string voucher)
        {
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += (sender, e) =>
            {
                Graphics g = e.Graphics;
                Font fontRegular = new Font("Arial", 7);
                Font fontBold = new Font("Arial", 7, FontStyle.Bold);
                Font fontHeader = new Font("Arial", 9, FontStyle.Bold);
                Font fontBox = new Font("Arial", 9, FontStyle.Bold);
                
                int y = 10;
                int ancho = 280; 
                int x = 5;
                
                StringFormat centro = new StringFormat { Alignment = StringAlignment.Center };
                StringFormat izquierda = new StringFormat { Alignment = StringAlignment.Near };
                StringFormat derecha = new StringFormat { Alignment = StringAlignment.Far };
                Pen pen = new Pen(Color.Black, 2);

                // --- DATOS EMPRESA (Dinámicos) ---
                string nombreEmpresa = DatabaseHelper.GetConfiguracion("NombreEmpresa");
                string rutEmpresa = DatabaseHelper.GetConfiguracion("RUT");
                string direccion = DatabaseHelper.GetConfiguracion("Direccion");
                string telefono = DatabaseHelper.GetConfiguracion("Telefono");

                // --- 1. RECUADRO RUT (Header) ---
                // Simulación del recuadro rojo/negro característico
                Rectangle rectBox = new Rectangle(x + 20, y, ancho - 40, 60);
                g.DrawRectangle(pen, rectBox);
                
                int yBox = y + 5;
                g.DrawString(string.Format("R.U.T.: {0}", rutEmpresa), fontBox, Brushes.Black, new RectangleF(x, yBox, ancho, 15), centro); yBox += 15;
                g.DrawString("BOLETA ELECTRÓNICA", fontBox, Brushes.Black, new RectangleF(x, yBox, ancho, 15), centro); yBox += 15;
                g.DrawString(string.Format("Nº {0}", idVenta), fontBox, Brushes.Black, new RectangleF(x, yBox, ancho, 15), centro);
                
                y += 65;
                g.DrawString("S.I.I. - SANTIAGO PONIENTE", fontBold, Brushes.Black, new RectangleF(x, y, ancho, 15), centro); y += 25;

                // --- 2. DATOS EMPRESA ---
                // Logo placeholder (opcional)
                // g.FillRectangle(Brushes.LightGray, x + 100, y, 80, 40); y += 45;

                g.DrawString(nombreEmpresa, fontHeader, Brushes.Black, x, y); y += 15;
                g.DrawString("GIRO COMERCIAL Y DISTRIBUIDORA", fontRegular, Brushes.Black, x, y); y += 12; // Placeholder de giro
                g.DrawString("Dirección: " + direccion, fontRegular, Brushes.Black, new RectangleF(x, y, ancho, 25)); y += 25;
                g.DrawString("Teléfono: " + telefono, fontRegular, Brushes.Black, x, y); y += 12;
                g.DrawString("Sucursal: Casa Matriz", fontRegular, Brushes.Black, x, y); y += 20;

                // --- 3. DATOS TRANSACCIÓN ---
                g.DrawString("Vendedor: ", fontBold, Brushes.Black, x, y); 
                g.DrawString(SesionActual.UsuarioActivo.NombreUsuario, fontRegular, Brushes.Black, x + 60, y); y += 12;
                
                g.DrawString("Fecha emisión: ", fontBold, Brushes.Black, x, y);
                g.DrawString(DateTime.Now.ToString("dd/MM/yyyy HH:mm"), fontRegular, Brushes.Black, x + 85, y); y += 12;
                
                g.DrawString("Forma pago: ", fontBold, Brushes.Black, x, y);
                g.DrawString(metodoPago, fontRegular, Brushes.Black, x + 70, y); y += 12;
                
                g.DrawString("Nº orden: ", fontBold, Brushes.Black, x, y);
                g.DrawString(idVenta.ToString(), fontRegular, Brushes.Black, x + 60, y); y += 20;

                // --- 4. DATOS CLIENTE (Simulados/Placeholder) ---
                // Como no tenemos tabla clientes aún, mostramos campos vacíos o genéricos para cumplir formato
                g.DrawString("Señor(es): ", fontBold, Brushes.Black, x, y);
                g.DrawString("______________________", fontRegular, Brushes.Black, x + 60, y); y += 12;
                g.DrawString("RUT: ", fontBold, Brushes.Black, x, y);
                g.DrawString("______________________", fontRegular, Brushes.Black, x + 35, y); y += 12;
                g.DrawString("Dirección: ", fontBold, Brushes.Black, x, y);
                g.DrawString("______________________", fontRegular, Brushes.Black, x + 60, y); y += 20;

                // --- 5. DETALLE PRODUCTOS ---
                g.DrawString("Descripción", fontBold, Brushes.Black, x, y);
                g.DrawString("Cant", fontBold, Brushes.Black, x + 180, y);
                g.DrawString("Valor", fontBold, Brushes.Black, ancho - 5, y, derecha);
                y += 5;
                g.DrawLine(Pens.Black, x, y, ancho, y); y += 5;

                foreach (var item in items)
                {
                    string nombre = item.Producto.Nombre;
                    // Dividir nombre si es muy largo
                    if (nombre.Length > 25) nombre = nombre.Substring(0, 25) + "...";

                    g.DrawString(nombre, fontRegular, Brushes.Black, x, y);
                    g.DrawString(item.Cantidad.ToString(), fontRegular, Brushes.Black, x + 190, y); // Alineado bajo Cant
                    g.DrawString(item.Subtotal.ToString("N0"), fontRegular, Brushes.Black, ancho - 5, y, derecha);
                    y += 12;
                }
                
                y += 5;
                g.DrawLine(Pens.Black, x, y, ancho, y); y += 5;

                // --- 6. TOTALES ---
                decimal neto = Math.Round(total / (1 + tasaIVA), 0);
                decimal montoIva = total - neto;

                // Alineación derecha para montos
                float xLabels = x + 120;
                float xValues = ancho - 5;

                g.DrawString("Subtotal: $", fontRegular, Brushes.Black, xLabels, y, derecha);
                g.DrawString(neto.ToString("N0"), fontRegular, Brushes.Black, xValues, y, derecha); y += 12;

                g.DrawString("Total pagado: $", fontRegular, Brushes.Black, xLabels, y, derecha);
                g.DrawString(montoRecibido.ToString("N0"), fontRegular, Brushes.Black, xValues, y, derecha); y += 12;

                g.DrawString("Vuelto: $", fontRegular, Brushes.Black, xLabels, y, derecha);
                g.DrawString(cambio.ToString("N0"), fontRegular, Brushes.Black, xValues, y, derecha); y += 15;

                g.DrawString("TOTAL: $", fontHeader, Brushes.Black, xLabels, y, derecha);
                g.DrawString(total.ToString("N0"), fontHeader, Brushes.Black, xValues, y, derecha); y += 20;

                g.DrawString(string.Format("El IVA de esta boleta es: ${0:N0}", montoIva), fontRegular, Brushes.Black, xValues, y, derecha); y += 30;

                // --- 7. TIMBRE ELECTRÓNICO (Simulación Visual) ---
                // Dibujar un "código de barras PDF417" simulado (rectángulo con ruido)
                Rectangle rectBarcode = new Rectangle(x + 20, y, ancho - 40, 80);
                // g.DrawRectangle(Pens.Black, rectBarcode);
                
                // Simular patrón de barras aleatorio para efecto visual
                Random rnd = new Random(idVenta);
                for(int i = x + 25; i < ancho - 25; i+=2)
                {
                    int h = rnd.Next(10, 70);
                    int startY = y + 5 + rnd.Next(0, 10);
                    g.DrawLine(Pens.Black, i, startY, i, startY + h);
                }
                
                y += 85;

                g.DrawString("Timbre Electrónico S.I.I.", fontRegular, Brushes.Black, new RectangleF(x, y, ancho, 15), centro); y += 12;
                g.DrawString("Res. N°80 del 2014 - Verifique documento:", fontRegular, Brushes.Black, new RectangleF(x, y, ancho, 15), centro); y += 12;
                g.DrawString("www.sii.cl", fontBold, Brushes.Black, new RectangleF(x, y, ancho, 15), centro);
            };

            // Mostrar vista previa antes de imprimir
            PrintPreviewDialog previewDialog = new PrintPreviewDialog();
            previewDialog.Document = pd;
            previewDialog.Width = 400;
            previewDialog.Height = 600;
            previewDialog.StartPosition = FormStartPosition.CenterScreen;
            
            // Maximizar para mejor visualización si se desea, o tamaño fijo
            // previewDialog.WindowState = FormWindowState.Maximized;

            previewDialog.ShowDialog();
        }
    }
}
