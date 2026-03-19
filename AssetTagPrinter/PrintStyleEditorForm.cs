using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace AssetTagPrinter
{
    public sealed class PrintStyleEditorForm : Form
    {
        private readonly ComboBox _cmbHeaderFamily = new ComboBox();
        private readonly NumericUpDown _nudHeaderSize = new NumericUpDown();
        private readonly CheckBox _chkHeaderBold = new CheckBox();
        private readonly CheckBox _chkHeaderItalic = new CheckBox();
        private readonly CheckBox _chkHeaderUnderline = new CheckBox();

        private readonly ComboBox _cmbSecondaryFamily = new ComboBox();
        private readonly NumericUpDown _nudSecondarySize = new NumericUpDown();
        private readonly CheckBox _chkSecondaryBold = new CheckBox();
        private readonly CheckBox _chkSecondaryItalic = new CheckBox();
        private readonly CheckBox _chkSecondaryUnderline = new CheckBox();

        private readonly ComboBox _cmbBodyFamily = new ComboBox();
        private readonly NumericUpDown _nudBodySize = new NumericUpDown();
        private readonly CheckBox _chkBodyBold = new CheckBox();
        private readonly CheckBox _chkBodyItalic = new CheckBox();
        private readonly CheckBox _chkBodyUnderline = new CheckBox();

        private readonly NumericUpDown _nudLeftMargin = new NumericUpDown();
        private readonly NumericUpDown _nudTopMargin = new NumericUpDown();
        private readonly NumericUpDown _nudLineSpacing = new NumericUpDown();
        private readonly NumericUpDown _nudLogoMaxWidth = new NumericUpDown();

        private readonly Button _btnOk = new Button();
        private readonly Button _btnCancel = new Button();

        private readonly PrintStyleSettings _workingSettings;

        public PrintStyleSettings ResultSettings { get; private set; }

        public PrintStyleEditorForm(PrintStyleSettings currentSettings)
        {
            _workingSettings = currentSettings.Clone();
            ResultSettings = currentSettings.Clone();

            InitializeComponent();
            LoadFonts();
            BindSettingsToControls();
        }

        private void InitializeComponent()
        {
            Text = "Print Style Customization";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(620, 410);

            Label lblInfo = new Label
            {
                Left = 12,
                Top = 12,
                Width = 590,
                Height = 18,
                Text = "Customize appearance only. Content text stays the same.",
            };
            Controls.Add(lblInfo);

            CreateSectionEditor("Header (company line)", 12, 40, _cmbHeaderFamily, _nudHeaderSize, _chkHeaderBold, _chkHeaderItalic, _chkHeaderUnderline);
            CreateSectionEditor("Secondary (lower company details)", 12, 130, _cmbSecondaryFamily, _nudSecondarySize, _chkSecondaryBold, _chkSecondaryItalic, _chkSecondaryUnderline);
            CreateSectionEditor("Body (ID/label/other lines)", 12, 220, _cmbBodyFamily, _nudBodySize, _chkBodyBold, _chkBodyItalic, _chkBodyUnderline);

            GroupBox gboxLayout = new GroupBox
            {
                Text = "Layout",
                Left = 400,
                Top = 40,
                Width = 205,
                Height = 220
            };

            Label lblLeft = new Label { Left = 12, Top = 30, Width = 90, Text = "Left margin:" };
            _nudLeftMargin.Left = 105;
            _nudLeftMargin.Top = 27;
            _nudLeftMargin.Width = 70;
            _nudLeftMargin.Minimum = 0;
            _nudLeftMargin.Maximum = 60;
            _nudLeftMargin.DecimalPlaces = 0;

            Label lblTop = new Label { Left = 12, Top = 70, Width = 90, Text = "Top margin:" };
            _nudTopMargin.Left = 105;
            _nudTopMargin.Top = 67;
            _nudTopMargin.Width = 70;
            _nudTopMargin.Minimum = 0;
            _nudTopMargin.Maximum = 60;
            _nudTopMargin.DecimalPlaces = 0;

            Label lblSpacing = new Label { Left = 12, Top = 110, Width = 90, Text = "Line spacing:" };
            _nudLineSpacing.Left = 105;
            _nudLineSpacing.Top = 107;
            _nudLineSpacing.Width = 70;
            _nudLineSpacing.Minimum = 0;
            _nudLineSpacing.Maximum = 20;
            _nudLineSpacing.DecimalPlaces = 0;

            Label lblLogoWidth = new Label { Left = 12, Top = 150, Width = 90, Text = "Logo size (%):" };
            _nudLogoMaxWidth.Left = 105;
            _nudLogoMaxWidth.Top = 147;
            _nudLogoMaxWidth.Width = 70;
            _nudLogoMaxWidth.Minimum = 10;
            _nudLogoMaxWidth.Maximum = 200;
            _nudLogoMaxWidth.DecimalPlaces = 0;

            gboxLayout.Controls.Add(lblLeft);
            gboxLayout.Controls.Add(_nudLeftMargin);
            gboxLayout.Controls.Add(lblTop);
            gboxLayout.Controls.Add(_nudTopMargin);
            gboxLayout.Controls.Add(lblSpacing);
            gboxLayout.Controls.Add(_nudLineSpacing);
            gboxLayout.Controls.Add(lblLogoWidth);
            gboxLayout.Controls.Add(_nudLogoMaxWidth);
            Controls.Add(gboxLayout);

            _btnOk.Text = "Apply";
            _btnOk.Left = 420;
            _btnOk.Top = 355;
            _btnOk.Width = 85;
            _btnOk.DialogResult = DialogResult.OK;
            _btnOk.Click += BtnOk_Click;

            _btnCancel.Text = "Cancel";
            _btnCancel.Left = 520;
            _btnCancel.Top = 355;
            _btnCancel.Width = 85;
            _btnCancel.DialogResult = DialogResult.Cancel;

            Controls.Add(_btnOk);
            Controls.Add(_btnCancel);

            AcceptButton = _btnOk;
            CancelButton = _btnCancel;
        }

        private void CreateSectionEditor(
            string title,
            int left,
            int top,
            ComboBox family,
            NumericUpDown size,
            CheckBox bold,
            CheckBox italic,
            CheckBox underline)
        {
            GroupBox box = new GroupBox
            {
                Text = title,
                Left = left,
                Top = top,
                Width = 380,
                Height = 85
            };

            Label lblFamily = new Label { Left = 12, Top = 26, Width = 45, Text = "Font:" };
            family.Left = 60;
            family.Top = 22;
            family.Width = 210;
            family.DropDownStyle = ComboBoxStyle.DropDownList;

            Label lblSize = new Label { Left = 278, Top = 26, Width = 35, Text = "Size:" };
            size.Left = 315;
            size.Top = 22;
            size.Width = 55;
            size.Minimum = 6;
            size.Maximum = 48;
            size.DecimalPlaces = 1;
            size.Increment = 0.5M;

            bold.Left = 60;
            bold.Top = 53;
            bold.Width = 60;
            bold.Text = "Bold";

            italic.Left = 130;
            italic.Top = 53;
            italic.Width = 60;
            italic.Text = "Italic";

            underline.Left = 200;
            underline.Top = 53;
            underline.Width = 80;
            underline.Text = "Underline";

            box.Controls.Add(lblFamily);
            box.Controls.Add(family);
            box.Controls.Add(lblSize);
            box.Controls.Add(size);
            box.Controls.Add(bold);
            box.Controls.Add(italic);
            box.Controls.Add(underline);

            Controls.Add(box);
        }

        private void LoadFonts()
        {
            var fontNames = FontFamily.Families
                .Select(f => f.Name)
                .OrderBy(n => n)
                .ToArray();

            _cmbHeaderFamily.Items.AddRange(fontNames);
            _cmbSecondaryFamily.Items.AddRange(fontNames);
            _cmbBodyFamily.Items.AddRange(fontNames);
        }

        private void BindSettingsToControls()
        {
            SetSectionControls(_workingSettings.Header, _cmbHeaderFamily, _nudHeaderSize, _chkHeaderBold, _chkHeaderItalic, _chkHeaderUnderline);
            SetSectionControls(_workingSettings.Secondary, _cmbSecondaryFamily, _nudSecondarySize, _chkSecondaryBold, _chkSecondaryItalic, _chkSecondaryUnderline);
            SetSectionControls(_workingSettings.Body, _cmbBodyFamily, _nudBodySize, _chkBodyBold, _chkBodyItalic, _chkBodyUnderline);

            _nudLeftMargin.Value = (decimal)_workingSettings.LeftMargin;
            _nudTopMargin.Value = (decimal)_workingSettings.TopMargin;
            _nudLineSpacing.Value = (decimal)_workingSettings.ExtraLineSpacing;
            _nudLogoMaxWidth.Value = (decimal)_workingSettings.LogoMaxWidthPercent;
        }

        private static void SetSectionControls(
            TextSectionStyle section,
            ComboBox family,
            NumericUpDown size,
            CheckBox bold,
            CheckBox italic,
            CheckBox underline)
        {
            if (family.Items.Contains(section.FontFamily))
            {
                family.SelectedItem = section.FontFamily;
            }
            else if (family.Items.Count > 0)
            {
                family.SelectedIndex = 0;
            }

            decimal clamped = Math.Min(size.Maximum, Math.Max(size.Minimum, (decimal)section.Size));
            size.Value = clamped;

            bold.Checked = section.Style.HasFlag(FontStyle.Bold);
            italic.Checked = section.Style.HasFlag(FontStyle.Italic);
            underline.Checked = section.Style.HasFlag(FontStyle.Underline);
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            ResultSettings = new PrintStyleSettings
            {
                Header = ReadSection(_cmbHeaderFamily, _nudHeaderSize, _chkHeaderBold, _chkHeaderItalic, _chkHeaderUnderline),
                Secondary = ReadSection(_cmbSecondaryFamily, _nudSecondarySize, _chkSecondaryBold, _chkSecondaryItalic, _chkSecondaryUnderline),
                Body = ReadSection(_cmbBodyFamily, _nudBodySize, _chkBodyBold, _chkBodyItalic, _chkBodyUnderline),
                LeftMargin = (float)_nudLeftMargin.Value,
                TopMargin = (float)_nudTopMargin.Value,
                ExtraLineSpacing = (float)_nudLineSpacing.Value,
                LogoMaxWidthPercent = (float)_nudLogoMaxWidth.Value
            };
        }

        private static TextSectionStyle ReadSection(
            ComboBox family,
            NumericUpDown size,
            CheckBox bold,
            CheckBox italic,
            CheckBox underline)
        {
            FontStyle style = FontStyle.Regular;
            if (bold.Checked)
            {
                style |= FontStyle.Bold;
            }

            if (italic.Checked)
            {
                style |= FontStyle.Italic;
            }

            if (underline.Checked)
            {
                style |= FontStyle.Underline;
            }

            return new TextSectionStyle(
                family.SelectedItem?.ToString() ?? "Consolas",
                (float)size.Value,
                style);
        }
    }
}