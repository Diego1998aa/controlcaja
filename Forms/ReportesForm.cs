using System;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using SistemaPOS.Services;
using SistemaPOS.Helpers;

namespace SistemaPOS.Forms
{
    public partial class ReportesForm : Form
    {
        private DateTimePicker dtpInicio, dtpFin;
        private DataGridView dgvReporte;
        private CheckBox chkDetalle;

        public ReportesForm()
        {
            InitializeComponent();
            // Cargar datos iniciales al abrir
            this.Load += (s, e) => BtnGenerar_Click(null, null);
        }

        private void InitializeComponent()
        {
            this.Text = "Reportes";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            UITheme.ApplyTheme(this);

            // HEADER BAR
            var header = UITheme.CrearHeaderBar("Reportes de Ventas", "Análisis y estadísticas del sistema");
            this.Controls.Add(header);

            // Panel de filtros
            Panel panelFiltros = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = UITheme.PanelBackground
            };

            Label lblDesde = new Label
            {
                Text = "DESDE:",
                ForeColor = UITheme.TextSecondary,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true
            };
            panelFiltros.Controls.Add(lblDesde);

            dtpInicio = new DateTimePicker
            {
                Location = new Point(20, 35),
                Format = DateTimePickerFormat.Short,
                Width = 120,
                Font = UITheme.FontRegular
            };
            dtpInicio.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            UITheme.StyleDateTimePicker(dtpInicio);
            panelFiltros.Controls.Add(dtpInicio);

            Label lblHasta = new Label
            {
                Text = "HASTA:",
                ForeColor = UITheme.TextSecondary,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(160, 15),
                AutoSize = true
            };
            panelFiltros.Controls.Add(lblHasta);

            dtpFin = new DateTimePicker
            {
                Location = new Point(160, 35),
                Format = DateTimePickerFormat.Short,
                Width = 120,
                Font = UITheme.FontRegular
            };
            UITheme.StyleDateTimePicker(dtpFin);
            panelFiltros.Controls.Add(dtpFin);

            chkDetalle = new CheckBox
            {
                Text = "Ver detalle por venta",
                ForeColor = UITheme.TextPrimary,
                Font = UITheme.FontRegular,
                Location = new Point(300, 38),
                AutoSize = true,
                Checked = true
            };
            panelFiltros.Controls.Add(chkDetalle);

            RoundedButton btnGenerar = new RoundedButton
            {
                Text = "🔍 FILTRAR",
                Location = new Point(490, 28),
                Size = new Size(130, 36),
                BackColor = UITheme.PrimaryColor
            };
            btnGenerar.Click += BtnGenerar_Click;
            panelFiltros.Controls.Add(btnGenerar);

            RoundedButton btnVerTodo = new RoundedButton
            {
                Text = "📊 VER TODO EL HISTORIAL",
                Location = new Point(640, 28),
                Size = new Size(230, 36),
                BackColor = UITheme.AccentColor
            };
            btnVerTodo.Click += (s, e) => CargarTodoHistorial();
            panelFiltros.Controls.Add(btnVerTodo);

            this.Controls.Add(panelFiltros);

            // DataGridView
            dgvReporte = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = UITheme.DarkBackground,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };
            UITheme.StyleDataGridView(dgvReporte);

            this.Controls.Add(dgvReporte);
            this.Controls.Add(panelFiltros);
        }

        private void BtnGenerar_Click(object sender, EventArgs e)
        {
            DataTable datos;
            if (chkDetalle.Checked)
            {
                datos = VentaService.ObtenerVentasDetalladas(dtpInicio.Value, dtpFin.Value);
            }
            else
            {
                datos = VentaService.ObtenerResumenVentas(dtpInicio.Value, dtpFin.Value);
            }
            dgvReporte.DataSource = datos;
            FormatearGrid();
        }

        private void CargarTodoHistorial()
        {
            var datos = VentaService.ObtenerVentasDetalladas(null, null);
            dgvReporte.DataSource = datos;
            FormatearGrid();
        }

        private void FormatearGrid()
        {
            if (dgvReporte.Columns["Total"] != null)
                dgvReporte.Columns["Total"].DefaultCellStyle.Format = "C0";

            if (dgvReporte.Columns["Subtotal"] != null)
                dgvReporte.Columns["Subtotal"].DefaultCellStyle.Format = "C0";

            if (dgvReporte.Columns["IVA"] != null)
                dgvReporte.Columns["IVA"].DefaultCellStyle.Format = "C0";

            if (dgvReporte.Columns["TotalVentas"] != null)
                dgvReporte.Columns["TotalVentas"].DefaultCellStyle.Format = "C0";

            if (dgvReporte.Columns["TotalEfectivo"] != null)
                dgvReporte.Columns["TotalEfectivo"].DefaultCellStyle.Format = "C0";

            if (dgvReporte.Columns["TotalTarjeta"] != null)
                dgvReporte.Columns["TotalTarjeta"].DefaultCellStyle.Format = "C0";

            if (dgvReporte.Columns["TotalTransferencia"] != null)
                dgvReporte.Columns["TotalTransferencia"].DefaultCellStyle.Format = "C0";
        }
    }
}
