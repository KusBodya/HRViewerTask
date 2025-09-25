using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace HRViewer
{
    public partial class MainForm : Form
    {
        private bool _loadingCombos;
        private DatabaseService _dbService;

        private Panel _filterPanel;
        private ComboBox _cmbStatus;
        private ComboBox _cmbDepartment;
        private ComboBox _cmbPosition;
        private TextBox _txtLastName;
        private Button _btnApplyFilter;

        private GroupBox _gbStatistics;
        private ComboBox _cmbStatusStats;
        private DateTimePicker _dtpFromDate;
        private DateTimePicker _dtpToDate;
        private RadioButton _rbHired;
        private RadioButton _rbFired;
        private Button _btnShowStats;
        private DataGridView _dgvStatistics;

        private DataGridView _dgvEmployees;
        private Panel _pnlLegend;

        private const string PLACEHOLDER_TEXT = "Введите часть фамилии";
        private bool _lastNameHasUserInput;

        public MainForm()
        {
            InitializeComponent();

            CreateFilterPanel();
            CreateMainGrid();
            CreateStatisticsPanel();
            CreateLegend();

            _dgvEmployees.BringToFront();
            _gbStatistics.BringToFront();
            _pnlLegend.BringToFront();

            InitializeDatabase();
            LoadComboBoxes();
            LoadEmployees();
        }

        private void InitializeComponent()
        {
            Text = "Система просмотра сотрудников";
            Size = new Size(1200, 800);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(1000, 600);
        }

        private void CreateFilterPanel()
        {
            _filterPanel = new Panel
            {
                Name = "filterPanel",
                Dock = DockStyle.Top,
                Height = 58,
                BackColor = Color.LightGray,
                Padding = new Padding(8)
            };

            var top = 10;
            var h = 24;

            var lblStatus = new Label { Text = "Статус:", Location = new Point(8, top + 3), AutoSize = true };
            _cmbStatus = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(60, top),
                Size = new Size(140, h)
            };

            var lblDepartment = new Label { Text = "Отдел:", Location = new Point(205, top + 3), AutoSize = true };
            _cmbDepartment = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(250, top),
                Size = new Size(150, h)
            };

            var lblPosition = new Label { Text = "Должность:", Location = new Point(405, top + 3), AutoSize = true };
            _cmbPosition = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(480, top),
                Size = new Size(170, h)
            };

            var lblLastName = new Label { Text = "Фамилия:", Location = new Point(660, top + 3), AutoSize = true };
            _txtLastName = new TextBox
            {
                Location = new Point(720, top),
                Size = new Size(170, h),
                Text = PLACEHOLDER_TEXT,
                ForeColor = Color.Gray
            };
            _txtLastName.GotFocus += TxtLastName_GotFocus;
            _txtLastName.LostFocus += TxtLastName_LostFocus;
            _txtLastName.TextChanged += (s, e) =>
            {
                _lastNameHasUserInput = _txtLastName.ForeColor != Color.Gray &&
                                        !string.IsNullOrWhiteSpace(_txtLastName.Text);
            };

            _btnApplyFilter = new Button
            {
                Text = "Применить",
                Location = new Point(900, top),
                Size = new Size(100, h),
                BackColor = Color.LightBlue
            };
            _btnApplyFilter.Click += BtnApplyFilter_Click;

            _filterPanel.Controls.AddRange(new Control[]
            {
                lblStatus, _cmbStatus,
                lblDepartment, _cmbDepartment,
                lblPosition, _cmbPosition,
                lblLastName, _txtLastName,
                _btnApplyFilter
            });

            Controls.Add(_filterPanel);
        }

        private void CreateMainGrid()
        {
            _dgvEmployees = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                GridColor = Color.LightGray
            };

            _dgvEmployees.CellFormatting += DgvEmployees_CellFormatting;
            _dgvEmployees.DataBindingComplete += DgvEmployees_DataBindingComplete;

            Controls.Add(_dgvEmployees);
        }

        private void CreateStatisticsPanel()
        {
            _gbStatistics = new GroupBox
            {
                Text = "Статистика по сотрудникам",
                Dock = DockStyle.Bottom,
                Height = 300,
                Padding = new Padding(10)
            };

            var lblStatusStats = new Label { Text = "Статус:", Location = new Point(15, 30), AutoSize = true };
            _cmbStatusStats = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(70, 27),
                Size = new Size(150, 25)
            };

            var lblPeriod = new Label { Text = "Период с:", Location = new Point(240, 30), AutoSize = true };
            _dtpFromDate = new DateTimePicker
            {
                Location = new Point(310, 27),
                Size = new Size(100, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddMonths(-1)
            };

            var lblTo = new Label { Text = "по:", Location = new Point(420, 30), AutoSize = true };
            _dtpToDate = new DateTimePicker
            {
                Location = new Point(450, 27),
                Size = new Size(100, 25),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today
            };

            _rbHired = new RadioButton { Text = "Принятые", Location = new Point(570, 30), Checked = true };
            _rbFired = new RadioButton { Text = "Уволенные", Location = new Point(660, 30) };

            _btnShowStats = new Button
            {
                Text = "Показать статистику",
                Location = new Point(770, 27),
                Size = new Size(160, 25),
                BackColor = Color.LightGreen
            };
            _btnShowStats.Click += BtnShowStats_Click;

            _dgvStatistics = new DataGridView
            {
                Location = new Point(15, 60),
                Size = new Size(_gbStatistics.Width - 30, 220),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };

            _gbStatistics.Controls.AddRange(new Control[]
            {
                lblStatusStats, _cmbStatusStats, lblPeriod, _dtpFromDate, lblTo, _dtpToDate,
                _rbHired, _rbFired, _btnShowStats, _dgvStatistics
            });

            Controls.Add(_gbStatistics);
        }

        private void CreateLegend()
        {
            _pnlLegend = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 30,
                BackColor = Color.WhiteSmoke
            };

            var lblLegend = new Label
            {
                Text = "Красным цветом выделены ФИО уволенных сотрудников",
                Location = new Point(10, 8),
                AutoSize = true,
                ForeColor = Color.DarkRed
            };

            _pnlLegend.Controls.Add(lblLegend);
            Controls.Add(_pnlLegend);
        }

        private void InitializeDatabase()
        {
            try
            {
                var connectionString = GetConnectionString();
                _dbService = new DatabaseService(connectionString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private string GetConnectionString()
        {
            const string fallback =
                "Server=BODYA\\SQLEXPRESS;Database=HR_DB;Integrated Security=true;TrustServerCertificate=true;";
            try
            {
                var cs = System.Configuration.ConfigurationManager
                    .ConnectionStrings["HRDatabase"]?.ConnectionString;
                return cs ?? ShowConnectionDialog(fallback);
            }
            catch
            {
                return ShowConnectionDialog(fallback);
            }
        }

        private string ShowConnectionDialog(string defaultValue)
        {
            using var dialog = new ConnectionDialog(defaultValue);
            if (dialog.ShowDialog() == DialogResult.OK)
                return dialog.ConnectionString;

            throw new Exception("Строка подключения не была указана");
        }

        private void LoadComboBoxes()
        {
            try
            {
                _loadingCombos = true;

                LoadComboBox(_cmbStatus, _dbService.GetStatuses(), "Все статусы");
                LoadComboBox(_cmbDepartment, _dbService.GetDepartments(), "Все отделы");
                LoadComboBox(_cmbPosition, _dbService.GetPositions(), "Все должности");
                LoadComboBox(_cmbStatusStats, _dbService.GetStatuses(), "Выберите статус");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки справочников:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _loadingCombos = false;
            }
        }

        private void LoadComboBox(ComboBox comboBox, DataTable data, string defaultText)
        {
            var table = new DataTable();
            table.Columns.Add("id", typeof(int));
            table.Columns.Add("name", typeof(string));

            if (comboBox != _cmbStatusStats)
            {
                var emptyRow = table.NewRow();
                emptyRow["id"] = DBNull.Value;
                emptyRow["name"] = defaultText;
                table.Rows.Add(emptyRow);
            }

            foreach (DataRow row in data.Rows)
                table.Rows.Add(row["id"], row["name"]);

            comboBox.ValueMember = "id";
            comboBox.DisplayMember = "name";
            comboBox.DataSource = table;

            if (comboBox.Items.Count > 0)
                comboBox.SelectedIndex = 0;
        }

        private void LoadEmployees()
        {
            try
            {
                var statusId = GetSelectedId(_cmbStatus);
                var departmentId = GetSelectedId(_cmbDepartment);
                var positionId = GetSelectedId(_cmbPosition);

                var input = _txtLastName.Text?.Trim();
                var lastNameFilter = (!string.IsNullOrEmpty(input) && _txtLastName.ForeColor != Color.Gray)
                    ? input
                    : null;

                var employees = _dbService.GetEmployees(statusId, departmentId, positionId, lastNameFilter);
                _dgvEmployees.DataSource = null;
                _dgvEmployees.DataSource = employees;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сотрудников:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int? GetSelectedId(ComboBox comboBox)
        {
            if (_loadingCombos) return null;

            var val = comboBox.SelectedValue;
            if (val == null || val == DBNull.Value) return null;

            if (val is DataRowView drv)
            {
                var inner = drv.Row?["id"];
                if (inner == null || inner == DBNull.Value) return null;
                return Convert.ToInt32(inner);
            }

            if (val is int i) return i;
            return int.TryParse(val.ToString(), out var parsed) ? parsed : (int?)null;
        }

        private void TxtLastName_GotFocus(object sender, EventArgs e)
        {
            if (!_lastNameHasUserInput)
            {
                _txtLastName.Clear();
                _txtLastName.ForeColor = Color.Black;
            }
        }

        private void TxtLastName_LostFocus(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtLastName.Text))
            {
                _lastNameHasUserInput = false;
                _txtLastName.Text = PLACEHOLDER_TEXT;
                _txtLastName.ForeColor = Color.Gray;
            }
        }

        private void BtnApplyFilter_Click(object sender, EventArgs e) => LoadEmployees();

        private void BtnShowStats_Click(object sender, EventArgs e)
        {
            try
            {
                var statusId = GetSelectedId(_cmbStatusStats);
                if (!statusId.HasValue)
                {
                    MessageBox.Show("Выберите статус для построения статистики",
                        "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_dtpFromDate.Value > _dtpToDate.Value)
                {
                    MessageBox.Show("Дата начала не может быть больше даты окончания",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var isHiredStats = _rbHired.Checked;
                var statistics = _dbService.GetEmployeeStatistics(
                    statusId.Value, _dtpFromDate.Value, _dtpToDate.Value, isHiredStats);

                _dgvStatistics.DataSource = statistics;

                if (_dgvStatistics.Columns.Contains("Date")) _dgvStatistics.Columns["Date"].HeaderText = "Дата";
                if (_dgvStatistics.Columns.Contains("Count")) _dgvStatistics.Columns["Count"].HeaderText = "Количество";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка построения статистики:\n{ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvEmployees_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (_dgvEmployees.Columns[e.ColumnIndex].Name != "Fio") return;

            var row = _dgvEmployees.Rows[e.RowIndex];
            var unemployValue = row.Cells["date_unemploy"].Value;

            if (unemployValue != null && unemployValue != DBNull.Value)
            {
                e.CellStyle.BackColor = Color.MistyRose;
                e.CellStyle.ForeColor = Color.DarkRed;
            }
        }

        private void DgvEmployees_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (_dgvEmployees.Columns.Contains("id")) _dgvEmployees.Columns["id"].Visible = false;
            if (_dgvEmployees.Columns.Contains("Fio")) _dgvEmployees.Columns["Fio"].HeaderText = "ФИО";
            if (_dgvEmployees.Columns.Contains("StatusName")) _dgvEmployees.Columns["StatusName"].HeaderText = "Статус";
            if (_dgvEmployees.Columns.Contains("DepartmentName"))
                _dgvEmployees.Columns["DepartmentName"].HeaderText = "Отдел";
            if (_dgvEmployees.Columns.Contains("PostName"))
                _dgvEmployees.Columns["PostName"].HeaderText = "Должность";
            if (_dgvEmployees.Columns.Contains("date_employ"))
                _dgvEmployees.Columns["date_employ"].HeaderText = "Дата приема";
            if (_dgvEmployees.Columns.Contains("date_unemploy"))
                _dgvEmployees.Columns["date_unemploy"].HeaderText = "Дата увольнения";
        }
    }
}
