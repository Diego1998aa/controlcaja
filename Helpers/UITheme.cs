using System;
using System.Drawing;
using System.Windows.Forms;

namespace SistemaPOS.Helpers
{
    public static class UITheme
    {
        // Colores de Fondo
        public static readonly Color DarkBackground = Color.FromArgb(30, 30, 30);
        public static readonly Color PanelBackground = Color.FromArgb(45, 45, 48);
        public static readonly Color LightPanel = Color.FromArgb(60, 60, 60);
        public static readonly Color InputBackground = Color.FromArgb(50, 50, 50);

        // Colores de Texto
        public static readonly Color TextPrimary = Color.White;
        public static readonly Color TextSecondary = Color.LightGray;

        // Colores de Acción
        public static readonly Color PrimaryColor = Color.FromArgb(106, 27, 154); // Púrpura Personalizado
        public static readonly Color SecondaryColor = Color.FromArgb(100, 100, 100); // Gris botón
        public static readonly Color SuccessColor = Color.FromArgb(40, 167, 69); // Verde éxito
        public static readonly Color DangerColor = Color.FromArgb(220, 53, 69); // Rojo error
        public static readonly Color WarningColor = Color.FromArgb(255, 193, 7); // Amarillo advertencia
        public static readonly Color InfoColor = Color.FromArgb(23, 162, 184); // Azul info

        // Alias y Colores Adicionales
        public static readonly Color AccentColor = PrimaryColor;
        public static readonly Color ErrorColor = DangerColor;
        public static readonly Color BorderColor = Color.FromArgb(80, 80, 80);

        // Fuentes
        public static readonly Font FontTitle = new Font("Segoe UI", 16, FontStyle.Bold);
        public static readonly Font FontSubtitle = new Font("Segoe UI", 12, FontStyle.Bold);
        public static readonly Font FontRegular = new Font("Segoe UI", 10, FontStyle.Regular);
        public static readonly Font FontText = FontRegular; // Alias
        public static readonly Font FontBold = new Font("Segoe UI", 10, FontStyle.Bold);
        public static readonly Font FontSmall = new Font("Segoe UI", 9, FontStyle.Regular);

        // Métodos de ayuda
        public static void ApplyTheme(Form form)
        {
            form.BackColor = DarkBackground;
            form.ForeColor = TextPrimary;
        }

        public static void StyleButton(Button btn, Color backColor)
        {
            btn.BackColor = backColor;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            btn.Font = FontBold;
        }

        public static void StyleTextBox(TextBox txt)
        {
            txt.BackColor = InputBackground;
            txt.ForeColor = TextPrimary;
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.Font = FontRegular;
        }

        public static void StyleLabel(Label lbl, Font font = null, Color? color = null)
        {
            lbl.Font = font ?? FontRegular;
            lbl.ForeColor = color ?? TextPrimary;
        }

        public static void StyleDataGridView(DataGridView dgv)
        {
            dgv.BackgroundColor = DarkBackground;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = PanelBackground;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
            dgv.ColumnHeadersDefaultCellStyle.Font = FontBold;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = PanelBackground;
            
            dgv.DefaultCellStyle.BackColor = DarkBackground;
            dgv.DefaultCellStyle.ForeColor = TextPrimary;
            dgv.DefaultCellStyle.SelectionBackColor = PrimaryColor;
            dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
            dgv.DefaultCellStyle.Font = FontRegular;
            
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        /// <summary>
        /// Muestra una notificación al usuario
        /// </summary>
        public static void MostrarNotificacion(string mensaje, string titulo = "Notificación", MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            MessageBox.Show(mensaje, titulo, MessageBoxButtons.OK, icon);
        }

        /// <summary>
        /// Muestra un diálogo de confirmación al usuario
        /// </summary>
        public static DialogResult MostrarConfirmacion(string mensaje, string titulo = "Confirmar")
        {
            return MessageBox.Show(mensaje, titulo, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        /// <summary>
        /// Crea un panel con estilo consistente
        /// </summary>
        public static Panel CrearPanel(int x, int y, int width, int height, Color? backgroundColor = null)
        {
            return new Panel
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = backgroundColor ?? PanelBackground,
                Padding = new Padding(10)
            };
        }

        /// <summary>
        /// Crea un badge/etiqueta de estado
        /// </summary>
        public static Label CrearBadge(string texto, Color color, int x, int y)
        {
            Label badge = new Label
            {
                Text = texto,
                Font = FontBold,
                ForeColor = Color.White,
                BackColor = color,
                AutoSize = false,
                Size = new Size(100, 25),
                Location = new Point(x, y),
                TextAlign = ContentAlignment.MiddleCenter
            };
            return badge;
        }
    }
}
