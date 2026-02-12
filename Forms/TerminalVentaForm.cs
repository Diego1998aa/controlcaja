using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SistemaPOS.Models;
using SistemaPOS.Services;
using SistemaPOS.Helpers;

namespace SistemaPOS.Forms
{
    public partial class TerminalVentaForm : Form
    {
        private TextBox txtBuscar;
        private DataGridView dgvProductos;
        private Label lblTotal;
        private List<ItemVenta> carrito;
        
        public TerminalVentaForm()
        {
            carrito = new List<ItemVenta>();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Terminal de Venta - Pedidos";
            this.Size = new Size(1000, 700);
            this.BackColor = UITheme.DarkBackground;
            this.KeyPreview = true; // Permitir capturar F12
            this.KeyDown += TerminalVentaForm_KeyDown;

            // --- Panel Superior (Buscador) ---
            Panel panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                Padding = new Padding(20),
                BackColor = UITheme.PanelBackground
            };

            Label lblBuscar = new Label
            {
                Text = "🔍 Buscar Producto (Código/SKU):",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                Dock = DockStyle.Top,
                Height = 30
            };

            txtBuscar = new TextBox
            {
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 50, // Altura visual aproximada
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            txtBuscar.KeyDown += TxtBuscar_KeyDown;

            panelTop.Controls.Add(txtBuscar);
            panelTop.Controls.Add(lblBuscar);
            this.Controls.Add(panelTop);

            // --- Panel Inferior (Total y Botón) ---
            Panel panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 100,
                Padding = new Padding(20),
                BackColor = UITheme.PanelBackground
            };

            lblTotal = new Label
            {
                Text = "Total: $0.00",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = UITheme.SuccessColor,
                AutoSize = true,
                Location = new Point(20, 30)
            };

            Button btnEnviar = new Button
            {
                Text = "ENVIAR A CAJA (F12)",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                BackColor = UITheme.PrimaryColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Right,
                Width = 300,
                Cursor = Cursors.Hand
            };
            btnEnviar.FlatAppearance.BorderSize = 0;
            btnEnviar.Click += BtnEnviar_Click;

            panelBottom.Controls.Add(lblTotal);
            panelBottom.Controls.Add(btnEnviar);
            this.Controls.Add(panelBottom);

            // --- Grid Central ---
            dgvProductos = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = UITheme.DarkBackground,
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            
            // Estilos del Grid
            dgvProductos.DefaultCellStyle.BackColor = UITheme.PanelBackground;
            dgvProductos.DefaultCellStyle.ForeColor = UITheme.TextPrimary;
            dgvProductos.DefaultCellStyle.SelectionBackColor = UITheme.PrimaryColor;
            dgvProductos.DefaultCellStyle.Font = new Font("Segoe UI", 12);
            dgvProductos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            dgvProductos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProductos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            dgvProductos.EnableHeadersVisualStyles = false;

            dgvProductos.Columns.Add("Producto", "Producto");
            dgvProductos.Columns.Add("Precio", "Precio");
            dgvProductos.Columns.Add("Cantidad", "Cant.");
            dgvProductos.Columns.Add("Subtotal", "Subtotal");
            
            // Ajuste de columnas
            dgvProductos.Columns[0].FillWeight = 50; // Nombre más ancho
            dgvProductos.Columns[2].FillWeight = 15;

            // Evento para eliminar con doble click
            dgvProductos.CellDoubleClick += (s, e) => 
            {
                if (e.RowIndex >= 0)
                {
                    carrito.RemoveAt(e.RowIndex);
                    ActualizarGrid();
                }
            };

            this.Controls.Add(dgvProductos);
            
            // Foco inicial
            this.Load += (s, e) => txtBuscar.Focus();
        }

        private void TerminalVentaForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F12)
            {
                BtnEnviar_Click(sender, e);
            }
        }

        private void TxtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string codigo = txtBuscar.Text.Trim();
                if (!string.IsNullOrEmpty(codigo))
                {
                    AgregarProducto(codigo);
                    txtBuscar.Clear();
                }
                e.SuppressKeyPress = true; // Evitar sonido de beep
            }
        }

        private void AgregarProducto(string codigo)
        {
            Producto p = ProductoService.BuscarPorCodigo(codigo);
            if (p != null)
            {
                var itemExistente = carrito.FirstOrDefault(i => i.Producto.IdProducto == p.IdProducto);
                if (itemExistente != null)
                {
                    itemExistente.Cantidad++;
                }
                else
                {
                    carrito.Add(new ItemVenta(p, 1));
                }
                ActualizarGrid();
            }
            else
            {
                MessageBox.Show("Producto no encontrado", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ActualizarGrid()
        {
            dgvProductos.Rows.Clear();
            decimal total = 0;

            foreach (var item in carrito)
            {
                dgvProductos.Rows.Add(
                    item.Producto.Nombre,
                    item.Producto.PrecioVenta.ToString("C"),
                    item.Cantidad,
                    item.Subtotal.ToString("C")
                );
                total += item.Subtotal;
            }

            lblTotal.Text = string.Format("Total: {0:C}", total);
        }

        private void BtnEnviar_Click(object sender, EventArgs e)
        {
            if (carrito.Count == 0)
            {
                MessageBox.Show("No hay productos en el pedido.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtBuscar.Focus();
                return;
            }

            try
            {
                // Guardar como PENDIENTE
                int idPedido = PedidoService.CrearPedido(SesionActual.UsuarioActivo.IdUsuario, carrito);

                // Mostrar mensaje GIGANTE
                MostrarMensajeExito(idPedido);

                // Limpiar
                carrito.Clear();
                ActualizarGrid();
                txtBuscar.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar pedido: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarMensajeExito(int idPedido)
        {
            Form msgForm = new Form();
            msgForm.Size = new Size(600, 400);
            msgForm.StartPosition = FormStartPosition.CenterScreen;
            msgForm.FormBorderStyle = FormBorderStyle.None;
            msgForm.BackColor = Color.FromArgb(34, 139, 34); // Verde éxito

            Label lblTitulo = new Label
            {
                Text = "✅ PEDIDO ENVIADO A CAJA",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 100,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblNumero = new Label
            {
                Text = "#" + idPedido,
                Font = new Font("Segoe UI", 72, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblInstruccion = new Label
            {
                Text = "Presione ENTER o ESC para continuar",
                Font = new Font("Segoe UI", 14),
                ForeColor = Color.WhiteSmoke,
                Dock = DockStyle.Bottom,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter
            };

            msgForm.Controls.Add(lblNumero);
            msgForm.Controls.Add(lblTitulo);
            msgForm.Controls.Add(lblInstruccion);

            msgForm.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
                    msgForm.Close();
            };

            msgForm.ShowDialog();
        }
    }
}