using System;
using System.Windows.Forms;
using System.Drawing;
using SistemaPOS.Models;
using SistemaPOS.Services;
using SistemaPOS.Helpers;

namespace SistemaPOS.Forms
{
    public partial class ProductoEditForm : Form
    {
        private int? idProducto;
        private TextBox txtCodigo, txtSKU, txtNombre, txtDescripcion;
        private TextBox txtPrecioC, txtPrecioV, txtStock, txtStockMin;
        private ComboBox cboCategoria;
        private Label lblMargen;
        private Producto productoOriginal;

        public ProductoEditForm(int? id = null)
        {
            idProducto = id;
            InitializeComponent();
            CargarCategorias();

            if (idProducto.HasValue)
                CargarProducto();
        }

        private void InitializeComponent()
        {
            this.Text = idProducto.HasValue ? "Editar Producto" : "Nuevo Producto";
            this.Size = new Size(580, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            UITheme.ApplyTheme(this);

            Panel header = UITheme.CrearHeaderBar(
                idProducto.HasValue ? "Editar Producto" : "Nuevo Producto",
                idProducto.HasValue ? "Modifique los datos del producto" : "Complete los datos del nuevo producto"
            );

            // Panel scrollable — sin padding (usaremos offset manual para centrar)
            Panel panelContenido = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = UITheme.DarkBackground
            };

            // ── Dimensiones del layout ──────────────────────────────────────────
            // Ancho útil del panel ≈ 564px (580 - 16 bordes)
            // Margen lateral: 40px a cada lado → contenido de 484px
            int lx = 40;          // x de inicio (margen izquierdo)
            int fw = 484;         // ancho total del área de contenido
            int colW = 232;       // ancho de cada columna de 2
            int col2X = lx + colW + 20; // x de la columna derecha (gap 20px)
            int y = 18;

            // ── CÓDIGO DE BARRAS ────────────────────────────────────────────────
            panelContenido.Controls.Add(CrearLabel("CÓDIGO DE BARRAS *", lx, y));
            y += 22;
            txtCodigo = CrearTextBox(lx, y, fw);
            panelContenido.Controls.Add(txtCodigo);
            y += 46;

            // ── SKU ─────────────────────────────────────────────────────────────
            panelContenido.Controls.Add(CrearLabel("SKU (OPCIONAL)", lx, y));
            y += 22;
            txtSKU = CrearTextBox(lx, y, fw);
            panelContenido.Controls.Add(txtSKU);
            y += 46;

            // ── NOMBRE ──────────────────────────────────────────────────────────
            panelContenido.Controls.Add(CrearLabel("NOMBRE DEL PRODUCTO *", lx, y));
            y += 22;
            txtNombre = CrearTextBox(lx, y, fw);
            panelContenido.Controls.Add(txtNombre);
            y += 46;

            // ── DESCRIPCIÓN ─────────────────────────────────────────────────────
            panelContenido.Controls.Add(CrearLabel("DESCRIPCIÓN", lx, y));
            y += 22;
            txtDescripcion = new TextBox
            {
                Location = new Point(lx, y),
                Size = new Size(fw, 58),
                Multiline = true,
                Font = UITheme.FontRegular
            };
            UITheme.StyleTextBox(txtDescripcion);
            panelContenido.Controls.Add(txtDescripcion);
            y += 68;

            // ── CATEGORÍA ───────────────────────────────────────────────────────
            panelContenido.Controls.Add(CrearLabel("CATEGORÍA", lx, y));
            y += 22;
            cboCategoria = new ComboBox
            {
                Location = new Point(lx, y),
                Size = new Size(240, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                ItemHeight = 24
            };
            UITheme.StyleComboBox(cboCategoria);
            cboCategoria.DrawMode = DrawMode.OwnerDrawFixed;
            cboCategoria.DrawItem += ComboBox_DrawItem;
            panelContenido.Controls.Add(cboCategoria);
            y += 46;

            // ── Separador visual ────────────────────────────────────────────────
            Panel sep1 = new Panel { Location = new Point(lx, y), Size = new Size(fw, 1), BackColor = UITheme.WithAlpha(UITheme.TextSecondary, 60) };
            panelContenido.Controls.Add(sep1);
            y += 14;

            // ── PRECIOS (2 columnas) ─────────────────────────────────────────────
            panelContenido.Controls.Add(CrearLabel("PRECIO COMPRA *", lx, y));
            panelContenido.Controls.Add(CrearLabel("PRECIO VENTA *", col2X, y));
            y += 22;
            txtPrecioC = CrearTextBox(lx, y, colW);
            txtPrecioV = CrearTextBox(col2X, y, colW);
            panelContenido.Controls.Add(txtPrecioC);
            panelContenido.Controls.Add(txtPrecioV);
            y += 40;

            // ── Margen calculado ────────────────────────────────────────────────
            lblMargen = new Label
            {
                Text = "Margen: —",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.TextMuted,
                Location = new Point(lx, y),
                AutoSize = true
            };
            panelContenido.Controls.Add(lblMargen);
            y += 30;

            txtPrecioC.TextChanged += ActualizarMargen;
            txtPrecioV.TextChanged += ActualizarMargen;

            // ── Separador visual ────────────────────────────────────────────────
            Panel sep2 = new Panel { Location = new Point(lx, y), Size = new Size(fw, 1), BackColor = UITheme.WithAlpha(UITheme.TextSecondary, 60) };
            panelContenido.Controls.Add(sep2);
            y += 14;

            // ── STOCK (2 columnas) ───────────────────────────────────────────────
            panelContenido.Controls.Add(CrearLabel("STOCK ACTUAL *", lx, y));
            panelContenido.Controls.Add(CrearLabel("STOCK MÍNIMO *", col2X, y));
            y += 22;
            txtStock = CrearTextBox(lx, y, colW);
            txtStockMin = CrearTextBox(col2X, y, colW);
            txtStockMin.Text = "5";

            if (idProducto.HasValue)
            {
                txtStock.BackColor = Color.FromArgb(40, 50, 65);
                Label lblStockNota = new Label
                {
                    Text = "Los cambios de stock generan un movimiento automático",
                    Font = new Font("Segoe UI", 7, FontStyle.Italic),
                    ForeColor = UITheme.TextMuted,
                    Location = new Point(lx, y + 35),
                    AutoSize = true
                };
                panelContenido.Controls.Add(lblStockNota);
            }

            panelContenido.Controls.Add(txtStock);
            panelContenido.Controls.Add(txtStockMin);
            y += 70;

            // ── BOTONES ──────────────────────────────────────────────────────────
            int btnW = (fw - 14) / 2;   // dos botones con gap 14px
            int btn2X = lx + btnW + 14;

            RoundedButton btnGuardar = new RoundedButton
            {
                Text = "GUARDAR",
                IconText = "\U0001F4BE",
                Location = new Point(lx, y),
                Size = new Size(btnW, 50),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ButtonColor = UITheme.SuccessColor,
                HoverColor = UITheme.LightenColor(UITheme.SuccessColor, 20),
                PressColor = UITheme.DarkenColor(UITheme.SuccessColor, 15),
                Radius = 8
            };
            btnGuardar.Click += BtnGuardar_Click;
            panelContenido.Controls.Add(btnGuardar);

            Button btnCancelar = new Button
            {
                Text = "CANCELAR",
                Location = new Point(btn2X, y),
                Size = new Size(btnW, 50),
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            UITheme.StyleButton(btnCancelar, UITheme.DangerColor);
            btnCancelar.Click += (s, e) => this.Close();
            panelContenido.Controls.Add(btnCancelar);

            this.Controls.Add(panelContenido);
            this.Controls.Add(header);

            ConfigurarNavegacionEnter();
        }

        private void ConfigurarNavegacionEnter()
        {
            txtCodigo.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; txtSKU.Focus(); } };
            txtSKU.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; txtNombre.Focus(); } };
            txtNombre.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; cboCategoria.Focus(); } };
            txtPrecioC.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; txtPrecioV.Focus(); } };
            txtPrecioV.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; txtStock.Focus(); } };
            txtStock.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; txtStockMin.Focus(); } };
            txtStockMin.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.Handled = true; e.SuppressKeyPress = true; BtnGuardar_Click(s, e); } };
        }

        private void ActualizarMargen(object sender, EventArgs e)
        {
            decimal precioC, precioV;
            if (decimal.TryParse(txtPrecioC.Text, out precioC) &&
                decimal.TryParse(txtPrecioV.Text, out precioV) && precioV > 0)
            {
                decimal margen = ((precioV - precioC) / precioV) * 100;
                lblMargen.Text = string.Format("Margen: {0:F1}%   |   Utilidad por unidad: {1}",
                    margen, (precioV - precioC).ToString("C"));
                lblMargen.ForeColor = margen >= 0 ? UITheme.SuccessColor : UITheme.DangerColor;
            }
            else
            {
                lblMargen.Text = "Margen: —";
                lblMargen.ForeColor = UITheme.TextMuted;
            }
        }

        private Label CrearLabel(string texto, int x, int y)
        {
            return new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = UITheme.TextSecondary,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private TextBox CrearTextBox(int x, int y, int width)
        {
            TextBox txt = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 32),
                Font = new Font("Segoe UI", 11)
            };
            UITheme.StyleTextBox(txt);
            return txt;
        }

        private void ComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var cbo = (ComboBox)sender;
            bool selected = (e.State & DrawItemState.Selected) != 0;
            Color bg = selected ? UITheme.PrimaryColor : UITheme.InputBackground;
            using (SolidBrush brush = new SolidBrush(bg))
                e.Graphics.FillRectangle(brush, e.Bounds);
            string text = cbo.GetItemText(cbo.Items[e.Index]);
            using (SolidBrush fg = new SolidBrush(UITheme.TextPrimary))
                e.Graphics.DrawString(text, cbo.Font, fg, e.Bounds.X + 4, e.Bounds.Y + 4);
        }

        private void CargarCategorias()
        {
            // Asignar DisplayMember/ValueMember ANTES de DataSource para evitar flash blanco
            cboCategoria.DisplayMember = "NombreCategoria";
            cboCategoria.ValueMember = "IdCategoria";
            cboCategoria.DataSource = ProductoService.ObtenerCategorias();
        }

        private void CargarProducto()
        {
            var productos = ProductoService.ObtenerTodos(false);
            productoOriginal = productos.Find(p => p.IdProducto == idProducto.Value);

            if (productoOriginal != null)
            {
                txtCodigo.Text = productoOriginal.CodigoBarras;
                txtSKU.Text = productoOriginal.SKU;
                txtNombre.Text = productoOriginal.Nombre;
                txtDescripcion.Text = productoOriginal.Descripcion;
                txtPrecioC.Text = productoOriginal.PrecioCompra.ToString("F2");
                txtPrecioV.Text = productoOriginal.PrecioVenta.ToString("F2");
                txtStock.Text = productoOriginal.StockActual.ToString();
                txtStockMin.Text = productoOriginal.StockMinimo.ToString();

                cboCategoria.SelectedValue = productoOriginal.IdCategoria;
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtCodigo.Text))
                {
                    MessageBox.Show("El código de barras es obligatorio", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCodigo.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                {
                    MessageBox.Show("El nombre es obligatorio", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtNombre.Focus();
                    return;
                }

                decimal precioC, precioV;
                int stock, stockMin;

                if (!decimal.TryParse(txtPrecioC.Text, out precioC) || precioC < 0)
                {
                    MessageBox.Show("Precio de compra inválido", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPrecioC.Focus();
                    return;
                }

                if (!decimal.TryParse(txtPrecioV.Text, out precioV) || precioV < 0)
                {
                    MessageBox.Show("Precio de venta inválido", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPrecioV.Focus();
                    return;
                }

                if (!int.TryParse(txtStock.Text, out stock) || stock < 0)
                {
                    MessageBox.Show("Stock actual inválido", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtStock.Focus();
                    return;
                }

                if (!int.TryParse(txtStockMin.Text, out stockMin) || stockMin < 0)
                {
                    MessageBox.Show("Stock mínimo inválido", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtStockMin.Focus();
                    return;
                }

                Producto p = new Producto
                {
                    CodigoBarras = txtCodigo.Text.Trim(),
                    SKU = txtSKU.Text.Trim(),
                    Nombre = txtNombre.Text.Trim(),
                    Descripcion = txtDescripcion.Text.Trim(),
                    IdCategoria = (int)cboCategoria.SelectedValue,
                    PrecioCompra = precioC,
                    PrecioVenta = precioV,
                    StockActual = stock,
                    StockMinimo = stockMin
                };

                if (idProducto.HasValue)
                {
                    int stockAnterior = productoOriginal != null ? productoOriginal.StockActual : stock;
                    p.IdProducto = idProducto.Value;
                    ProductoService.ActualizarProducto(p);

                    if (stock != stockAnterior)
                    {
                        ProductoService.RegistrarMovimientoInventario(
                            idProducto.Value, "Ajuste",
                            Math.Abs(stock - stockAnterior),
                            stockAnterior, stock,
                            "Ajuste directo desde edición de producto");
                    }

                    MessageBox.Show("Producto actualizado correctamente", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    ProductoService.CrearProducto(p);

                    if (stock > 0)
                    {
                        var nuevo = ProductoService.BuscarPorCodigo(p.CodigoBarras);
                        if (nuevo != null)
                        {
                            ProductoService.RegistrarMovimientoInventario(
                                nuevo.IdProducto, "Entrada",
                                stock, 0, stock,
                                "Stock inicial al crear producto");
                        }
                    }

                    MessageBox.Show("Producto creado correctamente", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message, "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Error de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
