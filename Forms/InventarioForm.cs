using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using SistemaPOS.Models;
using SistemaPOS.Services;
using SistemaPOS.Helpers;

namespace SistemaPOS.Forms
{
    public partial class InventarioForm : Form
    {
        private DataGridView dgvProductos;
        private TextBox txtBuscar;
        private ComboBox cboCategoria;
        private CheckBox chkBajoStock;

        public InventarioForm()
        {
            if (!SesionActual.TienePermiso(Permiso.VerInventario))
            {
                MessageBox.Show("No tiene permisos para acceder al inventario", "Acceso Denegado",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                return;
            }

            InitializeComponent();
            CargarProductos();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1100, 700);
            this.BackColor = UITheme.DarkBackground;

            // Header
            Panel header = UITheme.CrearHeaderBar("Gestión de Inventario", "Administre sus productos y stock");

            // Panel de filtros
            Panel pnlFiltros = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = UITheme.SurfaceColor,
                Padding = new Padding(15, 10, 15, 10)
            };

            txtBuscar = new TextBox
            {
                Width = 280,
                Height = 30,
                Location = new Point(15, 12),
                Font = new Font("Segoe UI", 10)
            };
            UITheme.StyleTextBox(txtBuscar);
            txtBuscar.TextChanged += (s, e) => CargarProductos();

            // Placeholder simulado
            Label lblPlaceholder = new Label
            {
                Text = "\U0001F50D  Buscar producto...",
                Font = new Font("Segoe UI", 10),
                ForeColor = UITheme.TextMuted,
                Location = new Point(20, 16),
                AutoSize = true,
                BackColor = Color.FromArgb(55, 65, 81),
                Cursor = Cursors.IBeam
            };
            lblPlaceholder.Click += (s, e) => txtBuscar.Focus();
            txtBuscar.TextChanged += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(txtBuscar.Text);
            txtBuscar.GotFocus += (s, e) => lblPlaceholder.Visible = false;
            txtBuscar.LostFocus += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(txtBuscar.Text);

            cboCategoria = new ComboBox
            {
                Width = 180,
                Location = new Point(310, 12),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(55, 65, 81),
                ForeColor = UITheme.TextPrimary,
                Font = new Font("Segoe UI", 10)
            };
            cboCategoria.SelectedIndexChanged += (s, e) => CargarProductos();

            chkBajoStock = new CheckBox
            {
                Text = "Solo bajo stock",
                Font = new Font("Segoe UI", 10),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(510, 14),
                AutoSize = true
            };
            chkBajoStock.CheckedChanged += (s, e) => CargarProductos();

            // Botones de acción en los filtros
            if (SesionActual.TienePermiso(Permiso.EditarProductos))
            {
                Button btnAgregar = new Button { Text = "+ Nuevo Producto", Width = 150, Height = 32, Location = new Point(700, 10) };
                UITheme.StyleButton(btnAgregar, UITheme.PrimaryColor);
                btnAgregar.Click += BtnAgregar_Click;
                pnlFiltros.Controls.Add(btnAgregar);
            }

            Button btnActualizar = new Button { Text = "\u21BB Actualizar", Width = 110, Height = 32, Location = new Point(860, 10) };
            UITheme.StyleButton(btnActualizar, UITheme.AccentColor);
            btnActualizar.Click += (s, e) => CargarProductos();

            pnlFiltros.Controls.AddRange(new Control[] { txtBuscar, lblPlaceholder, cboCategoria, chkBajoStock, btnActualizar });

            // Panel inferior de acciones
            Panel pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = UITheme.SurfaceColor,
                Padding = new Padding(15, 10, 15, 10)
            };

            int btnX = 15;
            if (SesionActual.TienePermiso(Permiso.EditarProductos))
            {
                Button btnEditar = new Button { Text = "\u270F  Editar", Width = 120, Height = 38, Location = new Point(btnX, 10) };
                UITheme.StyleButton(btnEditar, UITheme.PrimaryColor);
                btnEditar.Click += BtnEditar_Click;
                pnlBottom.Controls.Add(btnEditar);
                btnX += 135;
            }

            if (SesionActual.TienePermiso(Permiso.AjustarStock))
            {
                Button btnAjustar = new Button { Text = "\U0001F4E6 Ajustar Stock", Width = 150, Height = 38, Location = new Point(btnX, 10) };
                UITheme.StyleButton(btnAjustar, UITheme.InfoColor);
                btnAjustar.Click += BtnAjustarStock_Click;
                pnlBottom.Controls.Add(btnAjustar);
                btnX += 165;
            }

            if (SesionActual.TienePermiso(Permiso.EditarProductos))
            {
                Button btnEliminar = new Button { Text = "\U0001F5D1  Eliminar", Width = 120, Height = 38, Location = new Point(btnX, 10) };
                UITheme.StyleButton(btnEliminar, UITheme.DangerColor);
                btnEliminar.Click += BtnEliminar_Click;
                pnlBottom.Controls.Add(btnEliminar);
            }

            // Grid
            dgvProductos = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };
            UITheme.StyleDataGridView(dgvProductos);
            ConfigurarEstiloGrid();

            this.Controls.Add(dgvProductos);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlFiltros);
            this.Controls.Add(header);

            CargarCategorias();
        }

        private void ConfigurarEstiloGrid()
        {
            dgvProductos.Columns.Clear();
            dgvProductos.Columns.Add("IdProducto", "ID");
            dgvProductos.Columns.Add("Codigo", "Código");
            dgvProductos.Columns.Add("Nombre", "Producto");
            dgvProductos.Columns.Add("Categoria", "Categoría");
            dgvProductos.Columns.Add("PrecioCompra", "P. Compra");
            dgvProductos.Columns.Add("PrecioVenta", "P. Venta");
            dgvProductos.Columns.Add("Stock", "Stock");
            dgvProductos.Columns.Add("StockMin", "Stock Mín.");
            dgvProductos.Columns.Add("Estado", "Estado");

            dgvProductos.Columns["IdProducto"].Visible = false;
        }

        private void CargarCategorias()
        {
            var categorias = ProductoService.ObtenerCategorias();
            cboCategoria.Items.Clear();
            cboCategoria.Items.Add("Todas las categorías");
            foreach (var cat in categorias)
            {
                cboCategoria.Items.Add(cat.NombreCategoria);
            }
            cboCategoria.SelectedIndex = 0;
        }

        private void CargarProductos()
        {
            string busqueda = txtBuscar.Text.Trim().ToLower();
            string categoriaSel = cboCategoria.SelectedItem != null ? cboCategoria.SelectedItem.ToString() : null;
            bool bajoStock = chkBajoStock.Checked;

            var productos = ProductoService.ObtenerTodos();

            if (!string.IsNullOrEmpty(busqueda))
            {
                productos = productos.Where(p =>
                    p.Nombre.ToLower().Contains(busqueda) ||
                    p.CodigoBarras.Contains(busqueda) ||
                    p.SKU.Contains(busqueda)).ToList();
            }

            if (categoriaSel != "Todas las categorías" && !string.IsNullOrEmpty(categoriaSel))
            {
                productos = productos.Where(p => p.NombreCategoria == categoriaSel).ToList();
            }

            if (bajoStock)
            {
                productos = productos.Where(p => p.TieneBajoStock() || p.SinStock()).ToList();
            }

            dgvProductos.Rows.Clear();
            foreach (var p in productos)
            {
                int rowIndex = dgvProductos.Rows.Add(
                    p.IdProducto,
                    p.CodigoBarras,
                    p.Nombre,
                    p.NombreCategoria,
                    p.PrecioCompra.ToString("C"),
                    p.PrecioVenta.ToString("C"),
                    p.StockActual,
                    p.StockMinimo,
                    p.Estado
                );

                if (p.SinStock())
                {
                    dgvProductos.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(80, 245, 101, 101);
                    dgvProductos.Rows[rowIndex].DefaultCellStyle.ForeColor = UITheme.DangerColor;
                }
                else if (p.TieneBajoStock())
                {
                    dgvProductos.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(50, 236, 201, 75);
                    dgvProductos.Rows[rowIndex].DefaultCellStyle.ForeColor = UITheme.WarningColor;
                }
            }
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            if (new ProductoEditForm().ShowDialog() == DialogResult.OK)
                CargarProductos();
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["IdProducto"].Value);
                if (new ProductoEditForm(id).ShowDialog() == DialogResult.OK)
                    CargarProductos();
            }
            else
                MessageBox.Show("Seleccione un producto para editar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnAjustarStock_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["IdProducto"].Value);
                string nombre = dgvProductos.SelectedRows[0].Cells["Nombre"].Value.ToString();
                int stock = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["Stock"].Value);

                if (new AjustarStockForm(id, nombre, stock).ShowDialog() == DialogResult.OK)
                    CargarProductos();
            }
            else
                MessageBox.Show("Seleccione un producto para ajustar stock.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["IdProducto"].Value);
                string nombre = dgvProductos.SelectedRows[0].Cells["Nombre"].Value.ToString();

                if (MessageBox.Show(string.Format("¿Está seguro de eliminar el producto '{0}'?", nombre), "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    if (ProductoService.DesactivarProducto(id))
                    {
                        MessageBox.Show("Producto eliminado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarProductos();
                    }
                    else
                        MessageBox.Show("Error al eliminar el producto.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
                MessageBox.Show("Seleccione un producto para eliminar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
