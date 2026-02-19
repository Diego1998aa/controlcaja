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
            this.Size = new Size(560, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            UITheme.ApplyTheme(this);

            // Header
            Panel header = UITheme.CrearHeaderBar(
                idProducto.HasValue ? "Editar Producto" : "Nuevo Producto",
                idProducto.HasValue ? "Modifique los datos del producto" : "Complete los datos del nuevo producto"
            );

            // Panel scrollable
            Panel panelContenido = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(30, 15, 30, 15),
                BackColor = UITheme.DarkBackground
            };

            int y = 10;
            int fieldWidth = 470;

            // Código de Barras
            panelContenido.Controls.Add(CrearLabel("CÓDIGO DE BARRAS", 0, y));
            y += 22;
            txtCodigo = CrearTextBox(0, y, fieldWidth);
            panelContenido.Controls.Add(txtCodigo);
            y += 45;

            // SKU
            panelContenido.Controls.Add(CrearLabel("SKU (OPCIONAL)", 0, y));
            y += 22;
            txtSKU = CrearTextBox(0, y, fieldWidth);
            panelContenido.Controls.Add(txtSKU);
            y += 45;

            // Nombre
            panelContenido.Controls.Add(CrearLabel("NOMBRE DEL PRODUCTO", 0, y));
            y += 22;
            txtNombre = CrearTextBox(0, y, fieldWidth);
            panelContenido.Controls.Add(txtNombre);
            y += 45;

            // Descripción
            panelContenido.Controls.Add(CrearLabel("DESCRIPCIÓN", 0, y));
            y += 22;
            txtDescripcion = new TextBox
            {
                Location = new Point(0, y),
                Size = new Size(fieldWidth, 55),
                Multiline = true,
                Font = UITheme.FontRegular
            };
            UITheme.StyleTextBox(txtDescripcion);
            panelContenido.Controls.Add(txtDescripcion);
            y += 65;

            // Categoría
            panelContenido.Controls.Add(CrearLabel("CATEGORÍA", 0, y));
            y += 22;
            cboCategoria = new ComboBox
            {
                Location = new Point(0, y),
                Size = new Size(230, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UITheme.FontRegular,
                BackColor = Color.FromArgb(55, 65, 81),
                ForeColor = UITheme.TextPrimary,
                FlatStyle = FlatStyle.Flat
            };
            panelContenido.Controls.Add(cboCategoria);
            y += 45;

            // Precios en dos columnas
            int col1X = 0;
            int col2X = 245;
            int colW = 225;

            panelContenido.Controls.Add(CrearLabel("PRECIO COMPRA", col1X, y));
            panelContenido.Controls.Add(CrearLabel("PRECIO VENTA", col2X, y));
            y += 22;
            txtPrecioC = CrearTextBox(col1X, y, colW);
            txtPrecioV = CrearTextBox(col2X, y, colW);
            panelContenido.Controls.Add(txtPrecioC);
            panelContenido.Controls.Add(txtPrecioV);
            y += 45;

            // Stock en dos columnas
            panelContenido.Controls.Add(CrearLabel("STOCK ACTUAL", col1X, y));
            panelContenido.Controls.Add(CrearLabel("STOCK MÍNIMO", col2X, y));
            y += 22;
            txtStock = CrearTextBox(col1X, y, colW);
            txtStockMin = CrearTextBox(col2X, y, colW);
            txtStockMin.Text = "5";
            panelContenido.Controls.Add(txtStock);
            panelContenido.Controls.Add(txtStockMin);
            y += 55;

            // Botones
            RoundedButton btnGuardar = new RoundedButton
            {
                Text = "GUARDAR",
                IconText = "\U0001F4BE",
                Location = new Point(0, y),
                Size = new Size(225, 48),
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
                Location = new Point(245, y),
                Size = new Size(225, 48),
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            UITheme.StyleButton(btnCancelar, UITheme.DangerColor);
            btnCancelar.Click += (s, e) => this.Close();
            panelContenido.Controls.Add(btnCancelar);

            this.Controls.Add(panelContenido);
            this.Controls.Add(header);
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
                Size = new Size(width, 30),
                Font = new Font("Segoe UI", 11)
            };
            UITheme.StyleTextBox(txt);
            return txt;
        }

        private void CargarCategorias()
        {
            cboCategoria.DataSource = ProductoService.ObtenerCategorias();
            cboCategoria.DisplayMember = "NombreCategoria";
            cboCategoria.ValueMember = "IdCategoria";
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
                    MessageBox.Show("Precio de compra inválido", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPrecioC.Focus();
                    return;
                }

                if (!decimal.TryParse(txtPrecioV.Text, out precioV) || precioV < 0)
                {
                    MessageBox.Show("Precio de venta inválido", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPrecioV.Focus();
                    return;
                }

                if (!int.TryParse(txtStock.Text, out stock) || stock < 0)
                {
                    MessageBox.Show("Stock actual inválido", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtStock.Focus();
                    return;
                }

                if (!int.TryParse(txtStockMin.Text, out stockMin) || stockMin < 0)
                {
                    MessageBox.Show("Stock mínimo inválido", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    p.IdProducto = idProducto.Value;
                    ProductoService.ActualizarProducto(p);
                    MessageBox.Show("Producto actualizado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    ProductoService.CrearProducto(p);
                    MessageBox.Show("Producto creado correctamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
