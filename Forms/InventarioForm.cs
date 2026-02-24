using System;
using System.IO;
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
        private Label lblConteo;
        private Panel pnlDetalle;
        private Label lblDetNombre, lblDetInfoRow;

        public InventarioForm()
        {
            if (!SesionActual.TienePermiso(Permiso.VerInventario))
            {
                MessageBox.Show("No tiene permisos para acceder al inventario.", "Acceso Denegado",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                return;
            }

            InitializeComponent();
            CargarProductos();
        }

        private void InitializeComponent()
        {
            this.Text          = "Gestión de Inventario | SistemaPOS";
            this.Size          = new Size(1150, 790);
            this.MinimumSize   = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor     = UITheme.DarkBackground;
            this.KeyPreview    = true;
            this.KeyDown      += (s, e) => { if (e.KeyCode == Keys.F5) CargarProductos(); };

            // ══════════════════════════════════════════
            // HEADER  (con carrito de supermercado)
            // ══════════════════════════════════════════
            Panel header = UITheme.CrearHeaderBar(
                "\U0001F6D2  Gestión de Inventario",
                "Administre sus productos y stock  •  F5 para actualizar");

            // ══════════════════════════════════════════
            // BARRA DE FILTROS
            // ══════════════════════════════════════════
            Panel pnlFiltros = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 55,
                BackColor = Color.FromArgb(12, 28, 52),
                Padding   = new Padding(15, 10, 15, 10)
            };

            // Contenedor oscuro para el buscador (evita borde blanco nativo del TextBox)
            Panel pnlSearch = new Panel
            {
                Location  = new Point(13, 9),
                Size      = new Size(285, 36),
                BackColor = Color.FromArgb(22, 48, 82)
            };

            txtBuscar = new TextBox
            {
                Location    = new Point(8, 7),
                Width       = 268,
                Height      = 22,
                Font        = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.None,
                BackColor   = Color.FromArgb(22, 48, 82),
                ForeColor   = UITheme.TextPrimary
            };

            Label lblPlaceholder = new Label
            {
                Text      = "\U0001F50D  Buscar producto...",
                Font      = new Font("Segoe UI", 10),
                ForeColor = UITheme.TextMuted,
                Location  = new Point(6, 8),
                AutoSize  = true,
                BackColor = Color.FromArgb(22, 48, 82),
                Cursor    = Cursors.IBeam
            };

            lblPlaceholder.Click  += (s, e) => txtBuscar.Focus();
            txtBuscar.GotFocus    += (s, e) => lblPlaceholder.Visible = false;
            txtBuscar.LostFocus   += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(txtBuscar.Text);
            txtBuscar.TextChanged += (s, e) =>
            {
                lblPlaceholder.Visible = string.IsNullOrEmpty(txtBuscar.Text);
                CargarProductos();
            };

            pnlSearch.Controls.Add(txtBuscar);
            pnlSearch.Controls.Add(lblPlaceholder);

            // ComboBox de categorías
            cboCategoria = new ComboBox
            {
                Width         = 180,
                Location      = new Point(310, 12),
                DropDownStyle = ComboBoxStyle.DropDownList,
                ItemHeight    = 22
            };
            UITheme.StyleComboBox(cboCategoria);
            cboCategoria.DrawMode  = DrawMode.OwnerDrawFixed;
            cboCategoria.DrawItem += (s, e) =>
            {
                e.DrawBackground();
                var cbo = (ComboBox)s;
                bool sel = (e.State & DrawItemState.Selected) != 0;
                using (SolidBrush bg = new SolidBrush(sel ? UITheme.PrimaryColor : UITheme.InputBackground))
                    e.Graphics.FillRectangle(bg, e.Bounds);
                if (e.Index >= 0)
                    using (SolidBrush fg = new SolidBrush(UITheme.TextPrimary))
                        e.Graphics.DrawString(cbo.GetItemText(cbo.Items[e.Index]),
                            cbo.Font, fg, e.Bounds.X + 4, e.Bounds.Y + 3);
            };
            cboCategoria.SelectedIndexChanged += (s, e) => CargarProductos();

            // CheckBox de bajo stock
            chkBajoStock = new CheckBox
            {
                Text      = "Solo bajo stock",
                Font      = new Font("Segoe UI", 10),
                ForeColor = UITheme.TextSecondary,
                Location  = new Point(510, 14),
                AutoSize  = true
            };
            chkBajoStock.CheckedChanged += (s, e) => CargarProductos();

            // Botón Nuevo Producto (solo con permiso)
            if (SesionActual.TienePermiso(Permiso.EditarProductos))
            {
                Button btnAgregar = new Button
                {
                    Text     = "+ Nuevo Producto",
                    Width    = 150,
                    Height   = 32,
                    Location = new Point(680, 10)
                };
                UITheme.StyleButton(btnAgregar, UITheme.SuccessColor);
                btnAgregar.Click += BtnAgregar_Click;
                pnlFiltros.Controls.Add(btnAgregar);
            }

            // Botón Actualizar
            Button btnActualizar = new Button
            {
                Text     = "\u21BB Actualizar",
                Width    = 110,
                Height   = 32,
                Location = new Point(845, 10)
            };
            UITheme.StyleButton(btnActualizar, UITheme.AccentColor);
            btnActualizar.Click += (s, e) => CargarProductos();

            pnlFiltros.Controls.AddRange(new Control[] { pnlSearch, cboCategoria, chkBajoStock, btnActualizar });

            // ══════════════════════════════════════════
            // BARRA DE ESTADO (conteo de productos)
            // ══════════════════════════════════════════
            Panel pnlStatus = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 24,
                BackColor = Color.FromArgb(10, 24, 45),
                Padding   = new Padding(10, 3, 10, 0)
            };

            lblConteo = new Label
            {
                Text      = "Cargando...",
                Font      = new Font("Segoe UI", 8),
                ForeColor = UITheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(10, 4)
            };
            pnlStatus.Controls.Add(lblConteo);

            // ══════════════════════════════════════════
            // PANEL INFERIOR: botones de acción + logo
            // ══════════════════════════════════════════
            Panel pnlBottom = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 70,
                BackColor = Color.FromArgb(12, 28, 52),
                Padding   = new Padding(15, 10, 15, 10)
            };

            // Separador superior del panel inferior
            Panel sepBottom = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 1,
                BackColor = UITheme.WithAlpha(UITheme.PrimaryColor, 60)
            };
            pnlBottom.Controls.Add(sepBottom);

            // Botones de acción (lado izquierdo)
            int btnX = 15;

            if (SesionActual.TienePermiso(Permiso.EditarProductos))
            {
                Button btnEditar = new Button
                {
                    Text     = "\u270F  Editar",
                    Width    = 120,
                    Height   = 38,
                    Location = new Point(btnX, 16)
                };
                UITheme.StyleButton(btnEditar, UITheme.PrimaryColor);
                btnEditar.Click += BtnEditar_Click;
                pnlBottom.Controls.Add(btnEditar);
                btnX += 135;
            }

            if (SesionActual.TienePermiso(Permiso.AjustarStock))
            {
                Button btnAjustar = new Button
                {
                    Text     = "\U0001F4E6 Ajustar Stock",
                    Width    = 150,
                    Height   = 38,
                    Location = new Point(btnX, 16)
                };
                UITheme.StyleButton(btnAjustar, UITheme.InfoColor);
                btnAjustar.Click += BtnAjustarStock_Click;
                pnlBottom.Controls.Add(btnAjustar);
                btnX += 165;
            }

            if (SesionActual.TienePermiso(Permiso.EditarProductos))
            {
                Button btnEliminar = new Button
                {
                    Text     = "\U0001F5D1  Eliminar",
                    Width    = 120,
                    Height   = 38,
                    Location = new Point(btnX, 16)
                };
                UITheme.StyleButton(btnEliminar, UITheme.DangerColor);
                btnEliminar.Click += BtnEliminar_Click;
                pnlBottom.Controls.Add(btnEliminar);
                btnX += 135;
            }

            Button btnHistorial = new Button
            {
                Text     = "\U0001F4CB Historial",
                Width    = 130,
                Height   = 38,
                Location = new Point(btnX, 16)
            };
            UITheme.StyleButton(btnHistorial, Color.FromArgb(99, 102, 241));
            btnHistorial.Click += BtnHistorial_Click;
            pnlBottom.Controls.Add(btnHistorial);

            // ── Logo + nombre empresa (lado derecho del panel inferior) ──────
            Panel pnlLogoArea = new Panel
            {
                Dock      = DockStyle.Right,
                Width     = 250,
                BackColor = Color.Transparent
            };

            PictureBox picLogo = new PictureBox
            {
                Size      = new Size(90, 40),
                Location  = new Point(12, 14),
                SizeMode  = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            string logoPath = Path.Combine(Application.StartupPath, "Resources", "logo.png");
            if (File.Exists(logoPath))
            {
                try { picLogo.Image = Image.FromFile(logoPath); }
                catch { /* sin imagen si falla */ }
            }

            Label lblEmpresa = new Label
            {
                Text      = "Desarrollado por Urbanzync SpA",
                Font      = new Font("Segoe UI", 8),
                ForeColor = UITheme.TextMuted,
                AutoSize  = true,
                Location  = new Point(108, 26)
            };

            pnlLogoArea.Controls.Add(picLogo);
            pnlLogoArea.Controls.Add(lblEmpresa);
            pnlBottom.Controls.Add(pnlLogoArea);

            // ══════════════════════════════════════════
            // DATAGRIDVIEW DE PRODUCTOS
            // ══════════════════════════════════════════
            dgvProductos = new DataGridView
            {
                Dock       = DockStyle.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible  = false,
                SelectionMode      = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly           = true,
                EditMode           = DataGridViewEditMode.EditProgrammatically
            };
            UITheme.StyleDataGridView(dgvProductos);
            ConfigurarColumnas();

            // Paleta azul marino para el inventario
            dgvProductos.DefaultCellStyle.BackColor          = Color.FromArgb(22, 48, 82);
            dgvProductos.DefaultCellStyle.ForeColor          = Color.FromArgb(220, 230, 245);
            dgvProductos.DefaultCellStyle.SelectionBackColor = Color.FromArgb(49, 130, 206);
            dgvProductos.DefaultCellStyle.SelectionForeColor = Color.FromArgb(240, 248, 255);

            dgvProductos.AlternatingRowsDefaultCellStyle.BackColor          = Color.FromArgb(17, 40, 70);
            dgvProductos.AlternatingRowsDefaultCellStyle.ForeColor          = Color.FromArgb(200, 215, 235);
            dgvProductos.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(49, 130, 206);
            dgvProductos.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.FromArgb(240, 248, 255);

            dgvProductos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(12, 28, 52);
            dgvProductos.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(180, 200, 230);
            dgvProductos.ColumnHeadersHeight = 44;
            dgvProductos.RowTemplate.Height  = 40;
            dgvProductos.GridColor           = Color.FromArgb(30, 58, 95);

            dgvProductos.DoubleClick      += (s, e) => { if (SesionActual.TienePermiso(Permiso.EditarProductos)) BtnEditar_Click(s, e); };
            dgvProductos.SelectionChanged += (s, e) => ActualizarDetalle();

            // ══════════════════════════════════════════
            // PANEL DETALLE DEL PRODUCTO SELECCIONADO
            // ══════════════════════════════════════════
            pnlDetalle = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 68,
                BackColor = Color.FromArgb(15, 35, 65)
            };

            Panel sepDetalle = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 1,
                BackColor = UITheme.WithAlpha(UITheme.TextSecondary, 50)
            };

            Label lblDetTitulo = new Label
            {
                Text      = "PRODUCTO SELECCIONADO",
                Font      = new Font("Segoe UI", 7, FontStyle.Bold),
                ForeColor = UITheme.TextMuted,
                Location  = new Point(15, 6),
                AutoSize  = true
            };

            lblDetNombre = new Label
            {
                Text      = "Haga clic en una fila de la tabla para inspeccionar el producto",
                Font      = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 160, 220),
                Location  = new Point(15, 20),
                AutoSize  = true
            };

            lblDetInfoRow = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 8),
                ForeColor = UITheme.TextSecondary,
                Location  = new Point(15, 46),
                AutoSize  = true
            };

            pnlDetalle.Controls.Add(sepDetalle);
            pnlDetalle.Controls.Add(lblDetTitulo);
            pnlDetalle.Controls.Add(lblDetNombre);
            pnlDetalle.Controls.Add(lblDetInfoRow);

            // ══════════════════════════════════════════
            // AGREGAR AL FORMULARIO  (orden → posición)
            //   Los docked al mismo borde se "apilan":
            //   el ÚLTIMO en Controls[] = más exterior.
            // ══════════════════════════════════════════
            this.Controls.Add(dgvProductos);  // Fill  – zona central
            this.Controls.Add(pnlDetalle);    // Bottom – justo debajo del grid
            this.Controls.Add(pnlStatus);     // Bottom – sobre pnlBottom
            this.Controls.Add(pnlBottom);     // Bottom – borde inferior
            this.Controls.Add(pnlFiltros);    // Top    – justo bajo el header
            this.Controls.Add(header);        // Top    – borde superior

            CargarCategorias();
        }

        // ──────────────────────────────────────────────────────────────────────
        // CONFIGURACIÓN DE COLUMNAS DEL GRID
        // ──────────────────────────────────────────────────────────────────────
        private void ConfigurarColumnas()
        {
            dgvProductos.Columns.Clear();
            dgvProductos.Columns.Add("IdProducto",   "ID");
            dgvProductos.Columns.Add("Codigo",        "Código");
            dgvProductos.Columns.Add("Nombre",        "Producto");
            dgvProductos.Columns.Add("Categoria",     "Categoría");
            dgvProductos.Columns.Add("PrecioCompra",  "P. Compra");
            dgvProductos.Columns.Add("PrecioVenta",   "P. Venta");
            dgvProductos.Columns.Add("Margen",        "% Margen");
            dgvProductos.Columns.Add("Stock",         "Stock");
            dgvProductos.Columns.Add("StockMin",      "Mín.");
            dgvProductos.Columns.Add("Estado",        "Estado");

            dgvProductos.Columns["IdProducto"].Visible = false;

            // Distribución proporcional de ancho (FillWeight)
            dgvProductos.Columns["Codigo"].FillWeight       = 65;
            dgvProductos.Columns["Nombre"].FillWeight       = 160;
            dgvProductos.Columns["Categoria"].FillWeight    = 85;
            dgvProductos.Columns["PrecioCompra"].FillWeight = 70;
            dgvProductos.Columns["PrecioVenta"].FillWeight  = 70;
            dgvProductos.Columns["Margen"].FillWeight       = 55;
            dgvProductos.Columns["Stock"].FillWeight        = 50;
            dgvProductos.Columns["StockMin"].FillWeight     = 45;
            dgvProductos.Columns["Estado"].FillWeight       = 55;

            // Alineación de columnas numéricas
            dgvProductos.Columns["PrecioCompra"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvProductos.Columns["PrecioVenta"].DefaultCellStyle.Alignment  = DataGridViewContentAlignment.MiddleRight;
            dgvProductos.Columns["Margen"].DefaultCellStyle.Alignment       = DataGridViewContentAlignment.MiddleCenter;
            dgvProductos.Columns["Stock"].DefaultCellStyle.Alignment        = DataGridViewContentAlignment.MiddleCenter;
            dgvProductos.Columns["StockMin"].DefaultCellStyle.Alignment     = DataGridViewContentAlignment.MiddleCenter;
            dgvProductos.Columns["Estado"].DefaultCellStyle.Alignment       = DataGridViewContentAlignment.MiddleCenter;
        }

        // ──────────────────────────────────────────────────────────────────────
        // CARGA DE DATOS
        // ──────────────────────────────────────────────────────────────────────
        private void CargarCategorias()
        {
            var categorias = ProductoService.ObtenerCategorias();
            cboCategoria.Items.Clear();
            cboCategoria.Items.Add("Todas las categorías");
            foreach (var cat in categorias)
                cboCategoria.Items.Add(cat.NombreCategoria);
            cboCategoria.SelectedIndex = 0;
        }

        private void CargarProductos()
        {
            string busqueda    = txtBuscar.Text.Trim().ToLower();
            string categoriaSel = cboCategoria.SelectedItem != null ? cboCategoria.SelectedItem.ToString() : null;
            bool   bajoStock   = chkBajoStock.Checked;

            var productos = ProductoService.ObtenerTodos();

            if (!string.IsNullOrEmpty(busqueda))
                productos = productos.Where(p =>
                    p.Nombre.ToLower().Contains(busqueda) ||
                    (p.CodigoBarras ?? "").ToLower().Contains(busqueda) ||
                    (p.SKU ?? "").ToLower().Contains(busqueda)).ToList();

            if (categoriaSel != "Todas las categorías" && !string.IsNullOrEmpty(categoriaSel))
                productos = productos.Where(p => p.NombreCategoria == categoriaSel).ToList();

            if (bajoStock)
                productos = productos.Where(p => p.TieneBajoStock() || p.SinStock()).ToList();

            dgvProductos.Rows.Clear();
            foreach (var p in productos)
            {
                string margen = p.PrecioVenta > 0
                    ? string.Format("{0:F1}%", ((p.PrecioVenta - p.PrecioCompra) / p.PrecioVenta) * 100)
                    : "—";

                int rowIndex = dgvProductos.Rows.Add(
                    p.IdProducto,
                    p.CodigoBarras,
                    p.Nombre,
                    p.NombreCategoria,
                    p.PrecioCompra.ToString("C"),
                    p.PrecioVenta.ToString("C"),
                    margen,
                    p.StockActual,
                    p.StockMinimo,
                    p.Estado
                );

                if (p.SinStock())
                {
                    dgvProductos.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(100, 28, 32);
                    dgvProductos.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(255, 130, 130);
                }
                else if (p.TieneBajoStock())
                {
                    dgvProductos.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(20, 55, 105);
                    dgvProductos.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(140, 210, 255);
                }
            }

            lblConteo.Text = string.Format("{0} producto(s) mostrado(s)", productos.Count);
        }

        // ──────────────────────────────────────────────────────────────────────
        // MANEJADORES DE BOTONES
        // ──────────────────────────────────────────────────────────────────────
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
                MessageBox.Show("Seleccione un producto para editar.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnAjustarStock_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count > 0)
            {
                int    id     = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["IdProducto"].Value);
                string nombre = dgvProductos.SelectedRows[0].Cells["Nombre"].Value.ToString();
                int    stock  = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["Stock"].Value);

                if (new AjustarStockForm(id, nombre, stock).ShowDialog() == DialogResult.OK)
                    CargarProductos();
            }
            else
                MessageBox.Show("Seleccione un producto para ajustar stock.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count > 0)
            {
                int    id     = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["IdProducto"].Value);
                string nombre = dgvProductos.SelectedRows[0].Cells["Nombre"].Value.ToString();

                if (MessageBox.Show(
                        string.Format("¿Está seguro de eliminar el producto '{0}'?", nombre),
                        "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        if (ProductoService.DesactivarProducto(id))
                        {
                            MessageBox.Show("Producto eliminado correctamente.", "Éxito",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            CargarProductos();
                        }
                        else
                            MessageBox.Show("Error al eliminar el producto.", "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        MessageBox.Show(ex.Message, "Acceso Denegado",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
                MessageBox.Show("Seleccione un producto para eliminar.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnHistorial_Click(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count > 0)
            {
                int    id     = Convert.ToInt32(dgvProductos.SelectedRows[0].Cells["IdProducto"].Value);
                string nombre = dgvProductos.SelectedRows[0].Cells["Nombre"].Value.ToString();
                new HistorialMovimientosForm(id, nombre).ShowDialog();
            }
            else
                MessageBox.Show("Seleccione un producto para ver su historial.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ──────────────────────────────────────────────────────────────────────
        // PANEL DE DETALLE
        // ──────────────────────────────────────────────────────────────────────
        private void ActualizarDetalle()
        {
            if (dgvProductos.SelectedRows.Count == 0 ||
                dgvProductos.SelectedRows[0].Cells["Nombre"].Value == null)
            {
                lblDetNombre.Text      = "Haga clic en una fila de la tabla para inspeccionar el producto";
                lblDetNombre.ForeColor = Color.FromArgb(100, 160, 220);
                lblDetInfoRow.Text     = "";
                return;
            }

            var    row      = dgvProductos.SelectedRows[0];
            string nombre   = row.Cells["Nombre"].Value      != null ? row.Cells["Nombre"].Value.ToString()      : "—";
            string codigo   = row.Cells["Codigo"].Value      != null ? row.Cells["Codigo"].Value.ToString()      : "—";
            string cat      = row.Cells["Categoria"].Value   != null ? row.Cells["Categoria"].Value.ToString()   : "—";
            string precioC  = row.Cells["PrecioCompra"].Value != null ? row.Cells["PrecioCompra"].Value.ToString() : "—";
            string precioV  = row.Cells["PrecioVenta"].Value  != null ? row.Cells["PrecioVenta"].Value.ToString()  : "—";
            string margen   = row.Cells["Margen"].Value      != null ? row.Cells["Margen"].Value.ToString()      : "—";
            string stock    = row.Cells["Stock"].Value       != null ? row.Cells["Stock"].Value.ToString()       : "—";
            string stockMin = row.Cells["StockMin"].Value    != null ? row.Cells["StockMin"].Value.ToString()    : "—";
            string estado   = row.Cells["Estado"].Value      != null ? row.Cells["Estado"].Value.ToString()      : "—";

            lblDetNombre.Text = nombre;

            int stockVal;
            if (int.TryParse(stock, out stockVal) && stockVal == 0)
            {
                lblDetNombre.ForeColor  = UITheme.DangerColor;
                lblDetInfoRow.ForeColor = UITheme.DangerColor;
            }
            else
            {
                lblDetNombre.ForeColor  = UITheme.TextPrimary;
                lblDetInfoRow.ForeColor = UITheme.TextSecondary;
            }

            lblDetInfoRow.Text = string.Format(
                "Código: {0}   •   Categoría: {1}   •   P. Compra: {2}   •   P. Venta: {3}" +
                "   •   Margen: {4}   •   Stock: {5} unidades  (mín.: {6})   •   Estado: {7}",
                codigo, cat, precioC, precioV, margen, stock, stockMin, estado);
        }
    }
}
