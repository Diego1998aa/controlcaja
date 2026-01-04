using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using SistemaPOS.Models;
using SistemaPOS.Services;

namespace SistemaPOS.Forms
{
    public partial class PuntoVentaForm : Form
    {
        private List<ItemVenta> itemsVenta;
        private TextBox txtBusqueda;
        private DataGridView dgvProductos;
        private Label lblSubtotal, lblIVA, lblTotal;
        private decimal tasaIVA = 0.16m; // 16% IVA (ajustar según tu país)

        public PuntoVentaForm()
        {
            itemsVenta = new List<ItemVenta>();
            InitializeComponent();
            ActualizarTotales();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1000, 650);
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Panel izquierdo - Lista de productos
            Panel panelIzquierdo = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(650, 600),
                BackColor = Color.FromArgb(37, 37, 38)
            };

            // Campo de búsqueda
            Label lblBuscar = new Label
            {
                Text = "Código de Barras / Buscar Producto:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                AutoSize = true
            };

            txtBusqueda = new TextBox
            {
                Font = new Font("Segoe UI", 12),
                Location = new Point(10, 35),
                Size = new Size(500, 30)
            };
            txtBusqueda.KeyPress += TxtBusqueda_KeyPress;

            Button btnBuscar = new Button
            {
                Text = "🔍",
                Font = new Font("Segoe UI", 14),
                Location = new Point(520, 35),
                Size = new Size(50, 30),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnBuscar.FlatAppearance.BorderSize = 0;
            btnBuscar.Click += BtnBuscar_Click;

            Button btnNuevaVenta = new Button
            {
                Text = "Nueva Venta",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(580, 35),
                Size = new Size(60, 30),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnNuevaVenta.FlatAppearance.BorderSize = 0;
            btnNuevaVenta.Click += BtnNuevaVenta_Click;

            // DataGridView de productos en venta
            dgvProductos = new DataGridView
            {
                Location = new Point(10, 75),
                Size = new Size(630, 400),
                BackgroundColor = Color.FromArgb(45, 45, 48),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            dgvProductos.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            dgvProductos.DefaultCellStyle.ForeColor = Color.White;
            dgvProductos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dgvProductos.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvProductos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 37, 38);
            dgvProductos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProductos.EnableHeadersVisualStyles = false;

            dgvProductos.Columns.Add("Codigo", "Código");
            dgvProductos.Columns.Add("Nombre", "Producto");
            dgvProductos.Columns.Add("Cantidad", "Cantidad");
            dgvProductos.Columns.Add("Precio", "Precio Unit.");
            dgvProductos.Columns.Add("Subtotal", "Subtotal");
            dgvProductos.Columns[0].Width = 100;
            dgvProductos.Columns[2].Width = 80;
            dgvProductos.Columns[3].Width = 100;
            dgvProductos.Columns[4].Width = 100;

            // Botones de acciones
            Button btnQuitar = new Button
            {
                Text = "❌ Quitar",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(10, 485),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(204, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnQuitar.FlatAppearance.BorderSize = 0;
            btnQuitar.Click += BtnQuitar_Click;

            Button btnCantidad = new Button
            {
                Text = "📝 Cantidad",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(170, 485),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCantidad.FlatAppearance.BorderSize = 0;
            btnCantidad.Click += BtnCantidad_Click;

            panelIzquierdo.Controls.Add(lblBuscar);
            panelIzquierdo.Controls.Add(txtBusqueda);
            panelIzquierdo.Controls.Add(btnBuscar);
            panelIzquierdo.Controls.Add(btnNuevaVenta);
            panelIzquierdo.Controls.Add(dgvProductos);
            panelIzquierdo.Controls.Add(btnQuitar);
            panelIzquierdo.Controls.Add(btnCantidad);

            // Panel derecho - Totales y cobro
            Panel panelDerecho = new Panel
            {
                Location = new Point(670, 10),
                Size = new Size(310, 600),
                BackColor = Color.FromArgb(37, 37, 38)
            };

            Label lblTituloTotal = new Label
            {
                Text = "RESUMEN DE VENTA",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                AutoSize = false,
                Size = new Size(290, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Panel de totales
            Panel panelTotales = new Panel
            {
                Location = new Point(10, 50),
                Size = new Size(290, 150),
                BackColor = Color.FromArgb(45, 45, 48)
            };

            lblSubtotal = CrearLabelTotal("Subtotal:", "$0.00", 10);
            lblIVA = CrearLabelTotal("IVA (16%):", "$0.00", 50);
            lblTotal = CrearLabelTotal("TOTAL:", "$0.00", 90);
            lblTotal.Font = new Font("Segoe UI", 18, FontStyle.Bold);

            panelTotales.Controls.Add(lblSubtotal);
            panelTotales.Controls.Add(lblIVA);
            panelTotales.Controls.Add(lblTotal);

            // Botón cobrar
            Button btnCobrar = new Button
            {
                Text = "💰 COBRAR",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(10, 220),
                Size = new Size(290, 70),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCobrar.FlatAppearance.BorderSize = 0;
            btnCobrar.Click += BtnCobrar_Click;

            // Botón cancelar
            Button btnCancelar = new Button
            {
                Text = "🚫 CANCELAR VENTA",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(10, 300),
                Size = new Size(290, 50),
                BackColor = Color.FromArgb(139, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += BtnCancelar_Click;

            panelDerecho.Controls.Add(lblTituloTotal);
            panelDerecho.Controls.Add(panelTotales);
            panelDerecho.Controls.Add(btnCobrar);
            panelDerecho.Controls.Add(btnCancelar);

            this.Controls.Add(panelIzquierdo);
            this.Controls.Add(panelDerecho);
        }

        private Label CrearLabelTotal(string texto, string valor, int y)
        {
            Label lbl = new Label
            {
                Text = $"{texto}\n{valor}",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White,
                Location = new Point(10, y),
                AutoSize = false,
                Size = new Size(270, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            return lbl;
        }

        private void TxtBusqueda_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                BuscarYAgregarProducto();
                e.Handled = true;
            }
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            BuscarYAgregarProducto();
        }

        private void BuscarYAgregarProducto()
        {
            string codigo = txtBusqueda.Text.Trim();
            if (string.IsNullOrEmpty(codigo))
                return;

            Producto producto = ProductoService.BuscarPorCodigo(codigo);

            if (producto != null)
            {
                AgregarProductoAVenta(producto);
                txtBusqueda.Clear();
                txtBusqueda.Focus();
            }
            else
            {
                MessageBox.Show("Producto no encontrado.", "Búsqueda", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtBusqueda.SelectAll();
            }
        }

        private void AgregarProductoAVenta(Producto producto)
        {
            if (producto.StockActual <= 0)
            {
                MessageBox.Show("Producto sin stock disponible.", "Sin Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var itemExistente = itemsVenta.FirstOrDefault(i => i.Producto.IdProducto == producto.IdProducto);

            if (itemExistente != null)
            {
                if (itemExistente.Cantidad < producto.StockActual)
                {
                    itemExistente.IncrementarCantidad();
                }
                else
                {
                    MessageBox.Show("No hay suficiente stock disponible.", "Stock Insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                itemsVenta.Add(new ItemVenta(producto, 1));
            }

            ActualizarGrid();
            ActualizarTotales();
        }

        private void ActualizarGrid()
        {
            dgvProductos.Rows.Clear();
            foreach (var item in itemsVenta)
            {
                dgvProductos.Rows.Add(
                    item.Producto.CodigoBarras ?? item.Producto.SKU,
                    item.Producto.Nombre,
                    item.Cantidad,
                    item.Producto.PrecioVenta.ToString("C"),
                    item.Subtotal.ToString("C")
                );
            }
        }

        private void ActualizarTotales()
        {
            decimal subtotal = itemsVenta.Sum(i => i.Subtotal);
            decimal iva = subtotal * tasaIVA;
            decimal total = subtotal + iva;

            lblSubtotal.Text = $"Subtotal:\n{subtotal:C}";
            lblIVA.Text = $"IVA ({tasaIVA:P0}):\n{iva:C}";
            lblTotal.Text = $"TOTAL:\n{total:C}";
        }

        private void BtnQuitar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count > 0)
            {
                int index = dgvProductos.SelectedRows[0].Index;
                itemsVenta.RemoveAt(index);
                ActualizarGrid();
                ActualizarTotales();
            }
        }

        private void BtnCantidad_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count > 0)
            {
                int index = dgvProductos.SelectedRows[0].Index;
                ItemVenta item = itemsVenta[index];

                string input = Microsoft.VisualBasic.Interaction.InputBox(
                    $"Cantidad actual: {item.Cantidad}\nStock disponible: {item.Producto.StockActual}\n\nIngrese nueva cantidad:",
                    "Cambiar Cantidad",
                    item.Cantidad.ToString()
                );

                if (int.TryParse(input, out int nuevaCantidad) && nuevaCantidad > 0)
                {
                    if (nuevaCantidad <= item.Producto.StockActual)
                    {
                        item.Cantidad = nuevaCantidad;
                        item.CalcularSubtotal();
                        ActualizarGrid();
                        ActualizarTotales();
                    }
                    else
                    {
                        MessageBox.Show("Cantidad supera el stock disponible.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void BtnNuevaVenta_Click(object sender, EventArgs e)
        {
            if (itemsVenta.Count > 0)
            {
                DialogResult result = MessageBox.Show(
                    "¿Desea cancelar la venta actual y comenzar una nueva?",
                    "Nueva Venta",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    LimpiarVenta();
                }
            }
            else
            {
                LimpiarVenta();
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            if (itemsVenta.Count > 0)
            {
                DialogResult result = MessageBox.Show(
                    "¿Está seguro que desea cancelar esta venta?",
                    "Cancelar Venta",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    LimpiarVenta();
                }
            }
        }

        private void BtnCobrar_Click(object sender, EventArgs e)
        {
            if (itemsVenta.Count == 0)
            {
                MessageBox.Show("No hay productos en la venta.", "Venta Vacía", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CobrarForm cobrarForm = new CobrarForm(itemsVenta, tasaIVA);
            if (cobrarForm.ShowDialog() == DialogResult.OK)
            {
                LimpiarVenta();
                MessageBox.Show("Venta registrada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LimpiarVenta()
        {
            itemsVenta.Clear();
            ActualizarGrid();
            ActualizarTotales();
            txtBusqueda.Clear();
            txtBusqueda.Focus();
        }
    }
}