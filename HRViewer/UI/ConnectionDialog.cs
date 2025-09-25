using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace HRViewer
{
    public class ConnectionDialog : Form
    {
        private TextBox _txtConn;
        private Button _btnTest;
        private Button _btnOk;
        private Button _btnCancel;

        public string ConnectionString => _txtConn.Text.Trim();

        public ConnectionDialog(string defaultValue = "")
        {
            Text = "Строка подключения";
            Size = new Size(700, 160);
            StartPosition = FormStartPosition.CenterParent;

            var lbl = new Label { Text = "Введите строку подключения:", Location = new Point(10, 10), AutoSize = true };
            _txtConn = new TextBox { Location = new Point(10, 35), Size = new Size(660, 25), Text = defaultValue };

            _btnTest = new Button { Text = "Тест", Location = new Point(10, 70), Size = new Size(80, 25) };
            _btnOk = new Button { Text = "OK", Location = new Point(500, 70), Size = new Size(80, 25) };
            _btnCancel = new Button { Text = "Отмена", Location = new Point(590, 70), Size = new Size(80, 25) };

            _btnTest.Click += (s, e) =>
            {
                try
                {
                    using var cn = new SqlConnection(ConnectionString);
                    cn.Open();
                    MessageBox.Show("Подключение успешно", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка подключения:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            };

            _btnOk.Click += (s, e) =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };
            _btnCancel.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            Controls.AddRange([lbl, _txtConn, _btnTest, _btnOk, _btnCancel]);
        }
    }
}
