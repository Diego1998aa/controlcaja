using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using SistemaPOS.Models;
using SistemaPOS.Services;

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
            InitializeComponent();
            CargarProductos();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1000, 650);
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Panel superior - Búsqueda y filtros
            Panel panelSuperior = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(970, 80),
                BackColor = Color.FromArgb(37, 37, 38)
            };

            Label lblTitulo = new Label
            {
                Text = "GESTIÓN DE INVENTARIO",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                AutoSize = true
            };

            txtBuscar = new TextBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(10, 45),
                Size = new Size(300, 25),
                PlaceholderText = "Buscar por nombre o código..."
            };
            txtBuscar.TextChanged += (s, e) => CargarProductos();

            cboCategoria = new ComboBox
            {
                Font = new Font("Segoe UI", 10),
                Location = new Point(320, 45),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboCategoria.Items.Add("Todas las categorías");
            cboCategoria.SelectedIndex = 0;
            cboCategoria.SelectedIndexChanged += (s, e) => CargarProductos();

            chkBajoStock = new CheckBox
            {
                Text = "Solo bajo stock",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(530, 45),
                AutoSize = true
            };
            chkBajoStock.CheckedChanged += (s, e) => CargarProductos();

            Button btnAgregar = new Button
            {
                Text = "➕ Nuevo Producto",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(700, 40),
                Size = new Size(130, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAgregar.FlatAppearance.BorderSize = 0;
            btnAgregar.Click += BtnAgregar_Click;

            Button btnActualizar = new Button
            {
                Text = "🔄 Actualizar",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(840, 40),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnActualizar.FlatAppearance.BorderSize = 0;
            btnActualizar.Click += (s, e) => CargarProductos();

            panelSuperior.Controls.Add(lblTitulo);
            panelSuperior.Controls.Add(txtBuscar);
            panelSuperior.Controls.Add(cboCategoria);
            panelSuperior.Controls.Add(chkBajoStock);
            panelSuperior.Controls.Add(btnAgregar);
            panelSuperior.Controls.Add(btnActualizar);

            // DataGridView de productos
            dgvProductos = new DataGridView
            {
                Location = new Point(10, 100),
                Size = new Size(970, 450),
                BackgroundColor = Color.FromArgb(45, 45, 48),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            ConfigurarEstiloGrid();

            // Botones de acción
            Button btnEditar = new Button
            {
                Text = "✏️ Editar",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(10, 560),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(255, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEditar.FlatAppearance.BorderSize = 0;
            btnEditar.Click += BtnEditar_Click;

            Button btnAjustarStock = new Button
            {
                Text = "📦 Ajustar Stock",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(140, 560),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAjustarStock.FlatAppearance.BorderSize = 0;
            btnAjustarStock.Click += BtnAjustarStock_Click;

            Button btnEliminar = new Button
            {
                Text = "🗑️ Eliminar",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(290, 560),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(139, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnEliminar.FlatAppearance.BorderSize = 0;
            btnEliminar.Click += BtnEliminar_Click;

            this.Controls.Add(panelSuperior);
            this.Controls.Add(dgvProductos);
            this.Controls.Add(btnEditar);
            this.Controls.Add(btnAjustarStock);
            this.Controls.Add(btnEliminar);

            CargarCategorias();
        }

        private void ConfigurarEstiloGrid()
        {
            dgvProductos.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            dgvProductos.DefaultCellStyle.ForeColor = Color.White;
            dgvProductos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dgvProductos.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvProductos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(37, 37, 38);
            dgvProductos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvProductos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvProductos.EnableHeadersVisualStyles = false;

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

            dgvProductos.Columns[0].Width = 50;
            dgvProductos.Columns[0].Visible = false;
            dgvProductos.Columns[1].Width = 100;
            dgvProductos.Columns[4].Width = 90;
            dgvProductos.Columns[5].Width = 90;
            dgvProductos.Columns[6].Width = 80;
            dgvProductos.Columns[7].Width = 80;
            dgvProductos.Columns[8].Width = 100;
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
            dgvProductos.Rows.Clear();

            List<Producto> productos;
            
            if (chkBajoStock.Checked)
            {
                productos = ProductoService.ObtenerConBajoStock();
            }
            else
            {
                productos = ProductoService.ObtenerTodos();
            }

            // Filtrar por búsqueda
            string busqueda = txtBuscar.Text.ToLower();
            if (!string.IsNullOrEmpty(busqueda))
            {
                productos = productos.Where(p =>
                    p.Nombre.ToLower().Contains(busqueda) ||
                    (p.CodigoBarras != null && p.CodigoBarras.ToLower().Contains(busqueda)) ||
                    (p.SKU != null && p.SKU.ToLower().Contains(busqueda))
                ).ToList();
            }

            // Filtrar por categoría
            if (cboCategoria.SelectedIndex > 0)
            {
                string categoria = cboCategoria.SelectedItem.ToString();
                productos = productos.Where(p => p.NombreCategoria == categoria).ToList();
            }

            // Agregar a grid
            foreach (var p in productos)
            {
                string estado = p.SinStock() ? "SIN STOCK" : (p.TieneBajoStock() ? "BAJO STOCK" : "OK");
                Color colorEstado = p.SinStock() ? Color.Red : (p.TieneBajoStock() ? Color.Orange : Color.Green);

                int index = dgvProductos.Rows.Add(
                    p.IdProducto,
                    p.CodigoBarras ?? p.SKU,
                    p.Nombre,
                    p.NombreCategoria ?? "-",
                    p.PrecioCompra.ToString("C"),
                    p.PrecioVenta.ToString("C"),
                    p.StockActual,
                    p.StockMinimo,
                    estado
                );

                dgvProductos.Rows[index].Cells[8].Style.ForeColor = colorEstado;
                dgvProductos.Rows[index].Cells[8].Style.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            }
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            ProductoEditForm editForm = new ProductoEditForm();
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                CargarProductos();
                MessageBox.Show("Producto agregado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnEditar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un producto.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idProducto = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells[0].Value);
            ProductoEditForm editForm = new ProductoEditForm(idProducto);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                CargarProductos();
                MessageBox.Show("Producto actualizado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnAjustarStock_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un producto.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int idProducto = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells[0].Value);
            string nombreProducto = dgvProductos.SelectedRows[0].Cells[2].Value.ToString();
            int stockActual = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells[6].Value);

            AjustarStockForm ajustarForm = new AjustarStockForm(idProducto, nombreProducto, stockActual);
            if (ajustarForm.ShowDialog() == DialogResult.OK)
            {
                CargarProductos();
                MessageBox.Show("Stock ajustado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione un producto.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "¿Está seguro que desea eliminar este producto?",
                "Confirmar Eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                int idProducto = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells[0].Value);
                if (ProductoService.DesactivarProducto(idProducto))
                {
                    CargarProductos();
                    MessageBox.Show("Producto eliminado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Error al eliminar el producto.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}