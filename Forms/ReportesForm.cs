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
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.StartPosition = FormStartPosition.CenterParent;

            Panel panelTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(37, 37, 38) };
            
            Label lblTitulo = new Label { Text = "REPORTES DE VENTAS", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 10), AutoSize = true };
            
            Label lblDe = new Label { Text = "Desde:", ForeColor = Color.White, Location = new Point(20, 45), AutoSize = true };
            dtpInicio = new DateTimePicker { Location = new Point(70, 42), Format = DateTimePickerFormat.Short, Width = 100 };
            // Establecer inicio de mes por defecto
            dtpInicio.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            Label lblA = new Label { Text = "Hasta:", ForeColor = Color.White, Location = new Point(190, 45), AutoSize = true };
            dtpFin = new DateTimePicker { Location = new Point(240, 42), Format = DateTimePickerFormat.Short, Width = 100 };

            Button btnGenerar = new Button { Text = "Filtrar", BackColor = Color.SteelBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(360, 40), Size = new Size(100, 25) };
            btnGenerar.Click += BtnGenerar_Click;

            chkDetalle = new CheckBox 
            { 
                Text = "Ver Detalle por Venta", 
                ForeColor = Color.White, 
                Location = new Point(480, 44), 
                AutoSize = true,
                Checked = true // Por defecto ver detalle
            };

            Button btnVerTodo = new Button { Text = "Ver Todo el Historial", BackColor = Color.DarkSlateGray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(650, 40), Size = new Size(150, 25) };
            btnVerTodo.Click += (s, e) => CargarTodoHistorial();

            panelTop.Controls.AddRange(new Control[] { lblTitulo, lblDe, dtpInicio, lblA, dtpFin, btnGenerar, chkDetalle, btnVerTodo });

            dgvReporte = new DataGridView { 
                Dock = DockStyle.Fill, 
                BackgroundColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.Black,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false
            };
            dgvReporte.DefaultCellStyle.BackColor = Color.White;
            dgvReporte.DefaultCellStyle.ForeColor = Color.Black;
            dgvReporte.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);

            this.Controls.Add(dgvReporte);
            this.Controls.Add(panelTop);
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