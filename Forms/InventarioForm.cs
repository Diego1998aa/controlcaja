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
            // VALIDACIÓN DE PERMISOS
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

            // Panel Top (Filters & Actions)
            Panel pnlTop = new Panel 
            { 
                Dock = DockStyle.Top, 
                Height = 100, 
                BackColor = UITheme.PanelBackground, 
                Padding = new Padding(20) 
            };
            
            Label lblTitulo = new Label 
            { 
                Text = "GESTIÓN DE INVENTARIO", 
                Font = UITheme.FontTitle, 
                ForeColor = UITheme.TextPrimary, 
                AutoSize = true, 
                Location = new Point(20, 15) 
            };
            
            txtBuscar = new TextBox { Width = 300, Location = new Point(20, 55) };
            UITheme.StyleTextBox(txtBuscar);
            // txtBuscar.PlaceholderText = "Buscar producto..."; // Not supported in .NET Framework
            txtBuscar.TextChanged += (s, e) => CargarProductos();

            cboCategoria = new ComboBox 
            { 
                Width = 200, 
                Location = new Point(340, 55), 
                DropDownStyle = ComboBoxStyle.DropDownList, 
                FlatStyle = FlatStyle.Flat, 
                BackColor = UITheme.DarkBackground, 
                ForeColor = UITheme.TextPrimary, 
                Font = UITheme.FontText 
            };
            // Items se cargan en CargarCategorias
            cboCategoria.SelectedIndexChanged += (s, e) => CargarProductos();

            chkBajoStock = new CheckBox 
            { 
                Text = "Solo bajo stock", 
                Font = UITheme.FontText, 
                ForeColor = UITheme.TextPrimary, 
                Location = new Point(560, 58), 
                AutoSize = true 
            };
            chkBajoStock.CheckedChanged += (s, e) => CargarProductos();

            // Solo mostrar botón Agregar si tiene permisos
            if (SesionActual.TienePermiso(Permiso.EditarProductos))
            {
                Button btnAgregar = new Button { Text = "+ Nuevo", Width = 120, Height = 35, Location = new Point(750, 50) };
                UITheme.StyleButton(btnAgregar, UITheme.PrimaryColor);
                btnAgregar.Click += BtnAgregar_Click;
                pnlTop.Controls.Add(btnAgregar);
            }

            Button btnActualizar = new Button { Text = "Actualizar", Width = 120, Height = 35, Location = new Point(880, 50) };
            UITheme.StyleButton(btnActualizar, UITheme.SuccessColor);
            btnActualizar.Click += (s, e) => CargarProductos();

            pnlTop.Controls.AddRange(new Control[] { lblTitulo, txtBuscar, cboCategoria, chkBajoStock, btnActualizar });

            // Panel Bottom (Actions)
            Panel pnlBottom = new Panel 
            { 
                Dock = DockStyle.Bottom, 
                Height = 80, 
                BackColor = UITheme.PanelBackground, 
                Padding = new Padding(20) 
            };
            
            // Panel Bottom - botones según permisos
            if (SesionActual.TienePermiso(Permiso.EditarProductos))
            {
                Button btnEditar = new Button { Text = "Editar", Width = 130, Height = 40, Location = new Point(20, 20) };
                UITheme.StyleButton(btnEditar, UITheme.AccentColor);
                btnEditar.Click += BtnEditar_Click;
                pnlBottom.Controls.Add(btnEditar);
            }

            if (SesionActual.TienePermiso(Permiso.AjustarStock))
            {
                Button btnAjustar = new Button { Text = "Ajustar Stock", Width = 150, Height = 40, Location = new Point(170, 20) };
                UITheme.StyleButton(btnAjustar, Color.SteelBlue);
                btnAjustar.Click += BtnAjustarStock_Click;
                pnlBottom.Controls.Add(btnAjustar);
            }

            if (SesionActual.TienePermiso(Permiso.EditarProductos))
            {
                Button btnEliminar = new Button { Text = "Eliminar", Width = 130, Height = 40, Location = new Point(340, 20) };
                UITheme.StyleButton(btnEliminar, UITheme.ErrorColor);
                btnEliminar.Click += BtnEliminar_Click;
                pnlBottom.Controls.Add(btnEliminar);
            }

            // Grid
            dgvProductos = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = UITheme.DarkBackground,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                EnableHeadersVisualStyles = false
            };
            
            ConfigurarEstiloGrid();

            this.Controls.Add(dgvProductos);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlTop);

            CargarCategorias();
        }

        private void ConfigurarEstiloGrid()
        {
            dgvProductos.DefaultCellStyle.BackColor = UITheme.PanelBackground;
            dgvProductos.DefaultCellStyle.ForeColor = UITheme.TextPrimary;
            dgvProductos.DefaultCellStyle.SelectionBackColor = UITheme.AccentColor;
            dgvProductos.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvProductos.ColumnHeadersDefaultCellStyle.BackColor = UITheme.PrimaryColor;
            dgvProductos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProductos.ColumnHeadersDefaultCellStyle.Font = UITheme.FontBold;
            dgvProductos.DefaultCellStyle.Font = UITheme.FontText;

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

                // ALERTAS VISUALES DE STOCK
                if (p.SinStock())
                {
                    dgvProductos.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(139, 0, 0); // Rojo oscuro
                    dgvProductos.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.White;
                }
                else if (p.TieneBajoStock())
                {
                    dgvProductos.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 140, 0); // Naranja
                    dgvProductos.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Black;
                }
            }
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            if (new ProductoEditForm().ShowDialog() == DialogResult.OK)
            {
                CargarProductos();
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["IdProducto"].Value);
                if (new ProductoEditForm(id).ShowDialog() == DialogResult.OK)
                {
                    CargarProductos();
                }
            }
            else
            {
                MessageBox.Show("Seleccione un producto para editar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnAjustarStock_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["IdProducto"].Value);
                string nombre = dgvProductos.SelectedRows[0].Cells["Nombre"].Value.ToString();
                int stock = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["Stock"].Value);

                if (new AjustarStockForm(id, nombre, stock).ShowDialog() == DialogResult.OK)
                {
                    CargarProductos();
                }
            }
            else
            {
                MessageBox.Show("Seleccione un producto para ajustar stock.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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
                        MessageBox.Show("Producto eliminado (desactivado) correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarProductos();
                    }
                    else
                    {
                        MessageBox.Show("Error al eliminar el producto.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Seleccione un producto para eliminar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
