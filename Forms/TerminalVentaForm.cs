using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private Label lblCantItems;
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
            this.KeyPreview = true;
            this.KeyDown += TerminalVentaForm_KeyDown;

            // Header
            Panel header = UITheme.CrearHeaderBar("Terminal de Venta", "Escanee o busque productos por código");

            // --- Panel Superior (Buscador) ---
            Panel panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(20, 15, 20, 10),
                BackColor = UITheme.SurfaceColor
            };

            Label lblBuscar = new Label
            {
                Text = "\U0001F50D  Código / SKU del Producto:",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                Dock = DockStyle.Top,
                Height = 25
            };

            txtBuscar = new TextBox
            {
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(55, 65, 81),
                ForeColor = UITheme.TextPrimary,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtBuscar.KeyDown += TxtBuscar_KeyDown;

            panelTop.Controls.Add(txtBuscar);
            panelTop.Controls.Add(lblBuscar);

            // --- Panel Inferior (Total y Botón) ---
            Panel panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 90,
                Padding = new Padding(20, 10, 20, 10),
                BackColor = UITheme.SurfaceColor
            };

            // Separador superior
            Panel sepBottom = new Panel
            {
                Dock = DockStyle.Top,
                Height = 2,
                BackColor = UITheme.PrimaryColor
            };
            panelBottom.Controls.Add(sepBottom);

            lblCantItems = new Label
            {
                Text = "0 productos en el carrito",
                Font = new Font("Segoe UI", 10),
                ForeColor = UITheme.TextSecondary,
                AutoSize = true,
                Location = new Point(20, 18)
            };

            lblTotal = new Label
            {
                Text = "Total: $0",
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = UITheme.SuccessColor,
                AutoSize = true,
                Location = new Point(20, 42)
            };

            RoundedButton btnEnviar = new RoundedButton
            {
                Text = "ENVIAR A CAJA",
                IconText = "\U0001F4E4",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ButtonColor = UITheme.PrimaryColor,
                HoverColor = UITheme.PrimaryLight,
                PressColor = UITheme.PrimaryDark,
                Size = new Size(280, 55),
                Radius = 8
            };
            btnEnviar.Click += BtnEnviar_Click;

            // Posicionar el botón a la derecha
            panelBottom.Resize += (s, e) =>
            {
                btnEnviar.Location = new Point(panelBottom.Width - btnEnviar.Width - 20, 18);
            };

            Label lblAtajo = new Label
            {
                Text = "F12",
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = UITheme.TextMuted,
                BackColor = Color.FromArgb(55, 65, 81),
                AutoSize = false,
                Size = new Size(35, 18),
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelBottom.Resize += (s2, e2) =>
            {
                lblAtajo.Location = new Point(panelBottom.Width - btnEnviar.Width - 60, 36);
            };

            panelBottom.Controls.Add(lblCantItems);
            panelBottom.Controls.Add(lblTotal);
            panelBottom.Controls.Add(btnEnviar);
            panelBottom.Controls.Add(lblAtajo);

            // --- Grid Central ---
            dgvProductos = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                Font = new Font("Segoe UI", 12)
            };
            UITheme.StyleDataGridView(dgvProductos);
            dgvProductos.RowTemplate.Height = 42;

            dgvProductos.Columns.Add("Producto", "Producto");
            dgvProductos.Columns.Add("Precio", "Precio");
            dgvProductos.Columns.Add("Cantidad", "Cant.");
            dgvProductos.Columns.Add("Subtotal", "Subtotal");

            dgvProductos.Columns[0].FillWeight = 50;
            dgvProductos.Columns[2].FillWeight = 12;

            dgvProductos.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex >= 0)
                {
                    carrito.RemoveAt(e.RowIndex);
                    ActualizarGrid();
                }
            };

            // Tip de eliminación
            Label lblTip = new Label
            {
                Text = "Doble clic en un producto para eliminarlo del carrito",
                Font = UITheme.FontSmall,
                ForeColor = UITheme.TextMuted,
                Dock = DockStyle.Bottom,
                Height = 22,
                Padding = new Padding(10, 0, 0, 0)
            };

            // Ensamblaje
            this.Controls.Add(dgvProductos);
            this.Controls.Add(lblTip);
            this.Controls.Add(panelBottom);
            this.Controls.Add(panelTop);
            this.Controls.Add(header);

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
                e.SuppressKeyPress = true;
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
                MessageBox.Show("Producto no encontrado", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            lblCantItems.Text = string.Format("{0} producto{1} en el carrito", carrito.Count, carrito.Count != 1 ? "s" : "");
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
                int idPedido = PedidoService.CrearPedido(SesionActual.UsuarioActivo.IdUsuario, carrito);
                MostrarMensajeExito(idPedido);
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
            msgForm.Size = new Size(500, 350);
            msgForm.StartPosition = FormStartPosition.CenterScreen;
            msgForm.FormBorderStyle = FormBorderStyle.None;
            msgForm.BackColor = UITheme.DarkBackground;

            // Fondo con gradiente
            msgForm.Paint += (s, e) =>
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    msgForm.ClientRectangle,
                    Color.FromArgb(20, 83, 45),
                    Color.FromArgb(22, 101, 52),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, msgForm.ClientRectangle);
                }
            };

            Label lblCheck = new Label
            {
                Text = "\u2705",
                Font = new Font("Segoe UI Emoji", 48),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 100,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblTitulo = new Label
            {
                Text = "PEDIDO ENVIADO A CAJA",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblNumero = new Label
            {
                Text = "Pedido #" + idPedido,
                Font = new Font("Segoe UI", 48, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblInstruccion = new Label
            {
                Text = "Presione ENTER o ESC para continuar",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(200, 255, 200),
                Dock = DockStyle.Bottom,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };

            msgForm.Controls.Add(lblNumero);
            msgForm.Controls.Add(lblTitulo);
            msgForm.Controls.Add(lblCheck);
            msgForm.Controls.Add(lblInstruccion);

            msgForm.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
                    msgForm.Close();
            };

            msgForm.ShowDialog();
        }
    }
}
