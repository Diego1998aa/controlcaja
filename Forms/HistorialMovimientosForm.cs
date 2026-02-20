using System;
using System.Windows.Forms;
using System.Drawing;
using SistemaPOS.Models;
using SistemaPOS.Services;
using SistemaPOS.Helpers;

namespace SistemaPOS.Forms
{
    public partial class HistorialMovimientosForm : Form
    {
        private readonly int idProducto;
        private readonly string nombreProducto;
        private DataGridView dgvHistorial;
        private Label lblResumen;

        public HistorialMovimientosForm(int idProducto, string nombreProducto)
        {
            this.idProducto = idProducto;
            this.nombreProducto = nombreProducto;
            InitializeComponent();
            CargarHistorial();
        }

        private void InitializeComponent()
        {
            this.Text = "Historial de Movimientos";
            this.Size = new Size(900, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            UITheme.ApplyTheme(this);

            Panel header = UITheme.CrearHeaderBar("Historial de Movimientos", nombreProducto);

            // Panel inferior
            Panel pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 55,
                BackColor = UITheme.SurfaceColor,
                Padding = new Padding(15, 10, 15, 10)
            };

            lblResumen = new Label
            {
                Text = "Cargando...",
                Font = new Font("Segoe UI", 9),
                ForeColor = UITheme.TextSecondary,
                AutoSize = true,
                Location = new Point(15, 17)
            };
            pnlBottom.Controls.Add(lblResumen);

            Button btnCerrar = new Button
            {
                Text = "Cerrar",
                Width = 110,
                Height = 35,
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            // Posicionar a la derecha
            btnCerrar.Location = new Point(870 - 110 - 15, 10);
            UITheme.StyleButton(btnCerrar, UITheme.AccentColor);
            btnCerrar.Click += (s, e) => this.Close();
            pnlBottom.Controls.Add(btnCerrar);

            // Grid
            dgvHistorial = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };
            UITheme.StyleDataGridView(dgvHistorial);

            dgvHistorial.Columns.Add("Fecha", "Fecha y Hora");
            dgvHistorial.Columns.Add("Tipo", "Tipo");
            dgvHistorial.Columns.Add("Cantidad", "Cantidad");
            dgvHistorial.Columns.Add("Anterior", "Stock Anterior");
            dgvHistorial.Columns.Add("Nuevo", "Stock Nuevo");
            dgvHistorial.Columns.Add("Diferencia", "Diferencia");
            dgvHistorial.Columns.Add("Motivo", "Motivo / Notas");
            dgvHistorial.Columns.Add("Usuario", "Usuario");

            dgvHistorial.Columns["Fecha"].FillWeight = 80;
            dgvHistorial.Columns["Tipo"].FillWeight = 55;
            dgvHistorial.Columns["Cantidad"].FillWeight = 50;
            dgvHistorial.Columns["Anterior"].FillWeight = 60;
            dgvHistorial.Columns["Nuevo"].FillWeight = 55;
            dgvHistorial.Columns["Diferencia"].FillWeight = 55;
            dgvHistorial.Columns["Motivo"].FillWeight = 145;
            dgvHistorial.Columns["Usuario"].FillWeight = 80;

            dgvHistorial.Columns["Cantidad"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvHistorial.Columns["Anterior"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvHistorial.Columns["Nuevo"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvHistorial.Columns["Diferencia"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            this.Controls.Add(dgvHistorial);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(header);
        }

        private void CargarHistorial()
        {
            var movimientos = ProductoService.ObtenerHistorialMovimientos(idProducto);
            dgvHistorial.Rows.Clear();

            if (movimientos.Count == 0)
            {
                dgvHistorial.Rows.Add("—", "Sin movimientos", "—", "—", "—", "—",
                    "Este producto no tiene movimientos registrados", "—");
                lblResumen.Text = "Sin movimientos registrados";
                return;
            }

            int totalEntradas = 0, totalSalidas = 0;

            foreach (var m in movimientos)
            {
                int diff = m.StockNuevo - m.StockAnterior;
                string difStr = diff >= 0 ? "+" + diff.ToString() : diff.ToString();

                int rowIndex = dgvHistorial.Rows.Add(
                    m.FechaHora.ToString("dd/MM/yyyy HH:mm"),
                    m.TipoMovimiento,
                    m.Cantidad,
                    m.StockAnterior,
                    m.StockNuevo,
                    difStr,
                    string.IsNullOrEmpty(m.Motivo) ? "—" : m.Motivo,
                    m.NombreUsuario
                );

                // Colorear según tipo
                if (m.TipoMovimiento == "Entrada")
                {
                    dgvHistorial.Rows[rowIndex].Cells["Tipo"].Style.ForeColor = UITheme.SuccessColor;
                    dgvHistorial.Rows[rowIndex].Cells["Diferencia"].Style.ForeColor = UITheme.SuccessColor;
                    totalEntradas += m.Cantidad;
                }
                else if (m.TipoMovimiento == "Salida")
                {
                    dgvHistorial.Rows[rowIndex].Cells["Tipo"].Style.ForeColor = UITheme.DangerColor;
                    dgvHistorial.Rows[rowIndex].Cells["Diferencia"].Style.ForeColor = UITheme.DangerColor;
                    totalSalidas += m.Cantidad;
                }
                else
                {
                    dgvHistorial.Rows[rowIndex].Cells["Tipo"].Style.ForeColor = UITheme.WarningColor;
                    dgvHistorial.Rows[rowIndex].Cells["Diferencia"].Style.ForeColor = UITheme.WarningColor;
                }
            }

            lblResumen.Text = string.Format(
                "{0} movimiento(s)  |  Entradas: +{1}  |  Salidas: -{2}",
                movimientos.Count, totalEntradas, totalSalidas);
        }
    }
}
