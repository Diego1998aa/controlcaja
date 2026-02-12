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
            {
                CargarProducto();
            }
        }

        private void InitializeComponent()
        {
            this.Text = idProducto.HasValue ? "Editar Producto" : "Nuevo Producto";
            this.Size = new Size(600, 750);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            UITheme.ApplyTheme(this);

            Panel panelPrincipal = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(540, 670),
                BackColor = UITheme.PanelBackground,
                Padding = new Padding(20)
            };

            int y = 20;

            // Título
            Label lblTitulo = new Label
            {
                Text = idProducto.HasValue ? "EDITAR PRODUCTO" : "NUEVO PRODUCTO",
                Font = UITheme.FontTitle,
                ForeColor = UITheme.TextPrimary,
                Location = new Point(20, y),
                AutoSize = false,
                Size = new Size(500, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            panelPrincipal.Controls.Add(lblTitulo);
            y += 60;

            // Código de Barras
            panelPrincipal.Controls.Add(CrearLabel("Código de Barras:", 20, y));
            txtCodigo = CrearTextBox(20, y + 25, 500);
            panelPrincipal.Controls.Add(txtCodigo);
            y += 70;

            // SKU
            panelPrincipal.Controls.Add(CrearLabel("SKU (Opcional):", 20, y));
            txtSKU = CrearTextBox(20, y + 25, 500);
            panelPrincipal.Controls.Add(txtSKU);
            y += 70;

            // Nombre
            panelPrincipal.Controls.Add(CrearLabel("Nombre del Producto:", 20, y));
            txtNombre = CrearTextBox(20, y + 25, 500);
            panelPrincipal.Controls.Add(txtNombre);
            y += 70;

            // Descripción
            panelPrincipal.Controls.Add(CrearLabel("Descripción:", 20, y));
            txtDescripcion = new TextBox
            {
                Location = new Point(20, y + 25),
                Size = new Size(500, 60),
                Multiline = true,
                Font = UITheme.FontRegular
            };
            UITheme.StyleTextBox(txtDescripcion);
            panelPrincipal.Controls.Add(txtDescripcion);
            y += 95;

            // Categoría
            panelPrincipal.Controls.Add(CrearLabel("Categoría:", 20, y));
            cboCategoria = new ComboBox
            {
                Location = new Point(20, y + 25),
                Size = new Size(240, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UITheme.FontRegular
            };
            panelPrincipal.Controls.Add(cboCategoria);
            y += 70;

            // Precios y Stock en dos columnas
            int col1X = 20;
            int col2X = 270;
            int yRow = y;

            panelPrincipal.Controls.Add(CrearLabel("Precio Compra:", col1X, yRow));
            txtPrecioC = CrearTextBox(col1X, yRow + 25, 230);
            panelPrincipal.Controls.Add(txtPrecioC);

            panelPrincipal.Controls.Add(CrearLabel("Precio Venta:", col2X, yRow));
            txtPrecioV = CrearTextBox(col2X, yRow + 25, 230);
            panelPrincipal.Controls.Add(txtPrecioV);
            yRow += 70;

            panelPrincipal.Controls.Add(CrearLabel("Stock Actual:", col1X, yRow));
            txtStock = CrearTextBox(col1X, yRow + 25, 230);
            panelPrincipal.Controls.Add(txtStock);

            panelPrincipal.Controls.Add(CrearLabel("Stock Mínimo:", col2X, yRow));
            txtStockMin = CrearTextBox(col2X, yRow + 25, 230);
            txtStockMin.Text = "5"; // Valor por defecto
            panelPrincipal.Controls.Add(txtStockMin);
            yRow += 70;

            // Botones
            Button btnGuardar = new Button
            {
                Text = "GUARDAR",
                Location = new Point(20, yRow + 20),
                Size = new Size(240, 50),
                Font = UITheme.FontSubtitle
            };
            UITheme.StyleButton(btnGuardar, UITheme.SuccessColor);
            btnGuardar.Click += BtnGuardar_Click;
            panelPrincipal.Controls.Add(btnGuardar);

            Button btnCancelar = new Button
            {
                Text = "CANCELAR",
                Location = new Point(280, yRow + 20),
                Size = new Size(240, 50),
                Font = UITheme.FontSubtitle
            };
            UITheme.StyleButton(btnCancelar, UITheme.ErrorColor);
            btnCancelar.Click += (s, e) => this.Close();
            panelPrincipal.Controls.Add(btnCancelar);

            this.Controls.Add(panelPrincipal);
        }

        private Label CrearLabel(string texto, int x, int y)
        {
            Label lbl = new Label
            {
                Text = texto,
                Font = UITheme.FontBold,
                ForeColor = UITheme.TextSecondary,
                Location = new Point(x, y),
                AutoSize = true
            };
            return lbl;
        }

        private TextBox CrearTextBox(int x, int y, int width)
        {
            TextBox txt = new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 30),
                Font = UITheme.FontRegular
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
                // Validaciones
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
                    p.IdProducto = idProducto.Value;
                    ProductoService.ActualizarProducto(p);
                    MessageBox.Show("Producto actualizado correctamente", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    ProductoService.CrearProducto(p);
                    MessageBox.Show("Producto creado correctamente", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message, "Acceso Denegado",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Error de Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
