using Imap.App.Evidence;
using Imap.App.Evidence.Checks;
using Imap.App.Windows;

namespace Imap.App;

public sealed class MainForm : Form
{
    private const string ProductDisplayName = "Intentionally Misunderstood AGPL Program";

    private readonly WindowsPrintAgent agent;
    private readonly bool startupInitialized;
    private readonly DataGridView evidenceGrid = new();
    private readonly Label summaryLabel = new();
    private IReadOnlyList<EvidenceRow> currentRows = [];

    public MainForm(WindowsPrintAgent agent, bool startupInitialized)
    {
        this.agent = agent;
        this.startupInitialized = startupInitialized;

        InitializeLayout();
        RunChecks();
    }

    private void InitializeLayout()
    {
        Text = ProductDisplayName;
        MinimumSize = new Size(980, 600);
        ClientSize = new Size(1120, 690);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(246, 247, 249);
        Font = new Font("Segoe UI", 9.5F);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(16)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(CreateHeaderPanel(), 0, 0);
        root.Controls.Add(CreateGrid(), 0, 1);
        root.Controls.Add(CreateFooterPanel(), 0, 2);

        Controls.Add(root);
    }

    private Control CreateHeaderPanel()
    {
        var panel = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 14),
            Padding = new Padding(14, 12, 14, 12),
            BackColor = Color.White
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var title = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 16F, FontStyle.Regular),
            ForeColor = Color.FromArgb(28, 35, 43),
            Text = ProductDisplayName
        };

        var subtitle = new Label
        {
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 0),
            ForeColor = Color.FromArgb(82, 91, 103),
            Text = "Read-only integration observations for the Windows printing subsystem."
        };

        summaryLabel.AutoSize = true;
        summaryLabel.Anchor = AnchorStyles.Right;
        summaryLabel.Font = new Font("Segoe UI Semibold", 9.5F);
        summaryLabel.ForeColor = Color.FromArgb(54, 64, 76);

        var titleStack = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };
        titleStack.Controls.Add(title);
        titleStack.Controls.Add(subtitle);

        panel.Controls.Add(titleStack, 0, 0);
        panel.Controls.Add(summaryLabel, 1, 0);

        return panel;
    }

    private Control CreateGrid()
    {
        evidenceGrid.AllowUserToAddRows = false;
        evidenceGrid.AllowUserToDeleteRows = false;
        evidenceGrid.AllowUserToResizeRows = false;
        evidenceGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        evidenceGrid.BackgroundColor = Color.White;
        evidenceGrid.BorderStyle = BorderStyle.None;
        evidenceGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        evidenceGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        evidenceGrid.ColumnHeadersHeight = 38;
        evidenceGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        evidenceGrid.Dock = DockStyle.Fill;
        evidenceGrid.EnableHeadersVisualStyles = false;
        evidenceGrid.GridColor = Color.FromArgb(228, 232, 238);
        evidenceGrid.MultiSelect = false;
        evidenceGrid.ReadOnly = true;
        evidenceGrid.RowHeadersVisible = false;
        evidenceGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        evidenceGrid.DefaultCellStyle.BackColor = Color.White;
        evidenceGrid.DefaultCellStyle.ForeColor = Color.FromArgb(31, 36, 42);
        evidenceGrid.DefaultCellStyle.Padding = new Padding(8, 6, 8, 6);
        evidenceGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(226, 236, 249);
        evidenceGrid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(31, 36, 42);
        evidenceGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
        evidenceGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(236, 240, 245);
        evidenceGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(46, 55, 66);
        evidenceGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9F);
        evidenceGrid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
        evidenceGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(236, 240, 245);
        evidenceGrid.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.FromArgb(46, 55, 66);

        evidenceGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Claim",
            HeaderText = "Evidence Claim",
            FillWeight = 26,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
        evidenceGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Action",
            HeaderText = "Technical Action",
            FillWeight = 27,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
        evidenceGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Status",
            HeaderText = "Status",
            FillWeight = 11,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });
        evidenceGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Observed",
            HeaderText = "Observed Result",
            FillWeight = 36,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            SortMode = DataGridViewColumnSortMode.NotSortable
        });

        foreach (DataGridViewColumn column in evidenceGrid.Columns)
        {
            column.HeaderCell.Style.BackColor = Color.FromArgb(236, 240, 245);
            column.HeaderCell.Style.ForeColor = Color.FromArgb(46, 55, 66);
            column.HeaderCell.Style.SelectionBackColor = Color.FromArgb(236, 240, 245);
            column.HeaderCell.Style.SelectionForeColor = Color.FromArgb(46, 55, 66);
        }

        return evidenceGrid;
    }

    private Control CreateFooterPanel()
    {
        var refreshButton = CreateButton("Refresh");
        refreshButton.Click += (_, _) => RunChecks();

        var copyButton = CreateButton("Copy Report");
        copyButton.Click += (_, _) =>
        {
            Clipboard.SetText(ReportFormatter.Format(currentRows, DateTimeOffset.Now));
            summaryLabel.Text = $"{BuildSummary(currentRows)} | report copied";
        };

        var aboutButton = CreateButton("About");
        aboutButton.Click += (_, _) => ShowAbout();

        var buttonPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 14, 0, 0)
        };
        buttonPanel.Controls.Add(aboutButton);
        buttonPanel.Controls.Add(copyButton);
        buttonPanel.Controls.Add(refreshButton);

        return buttonPanel;
    }

    private static Button CreateButton(string text)
    {
        return new Button
        {
            AutoSize = true,
            BackColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(8, 0, 0, 0),
            Padding = new Padding(12, 5, 12, 5),
            Text = text,
            UseVisualStyleBackColor = false
        };
    }

    private void RunChecks()
    {
        var runner = new EvidenceRunner(EvidenceCheckFactory.Create(agent, startupInitialized));
        currentRows = runner.Run();
        RenderRows(currentRows);
    }

    private void RenderRows(IReadOnlyList<EvidenceRow> rows)
    {
        evidenceGrid.Rows.Clear();

        foreach (var row in rows)
        {
            var rowIndex = evidenceGrid.Rows.Add(row.Claim, row.TechnicalAction, row.Status.ToString(), row.ObservedResult);
            var gridRow = evidenceGrid.Rows[rowIndex];
            gridRow.Cells["Status"].Style.BackColor = StatusBackColor(row.Status);
            gridRow.Cells["Status"].Style.ForeColor = StatusForeColor(row.Status);
            gridRow.Cells["Status"].Style.Font = new Font("Segoe UI Semibold", 9F);
        }

        evidenceGrid.ClearSelection();
        summaryLabel.Text = BuildSummary(rows);
    }

    private static string BuildSummary(IReadOnlyList<EvidenceRow> rows)
    {
        var pass = rows.Count(row => row.Status == EvidenceStatus.Pass);
        var unavailable = rows.Count(row => row.Status == EvidenceStatus.Unavailable);
        var unsupported = rows.Count(row => row.Status == EvidenceStatus.Unsupported);
        var fail = rows.Count(row => row.Status == EvidenceStatus.Fail);

        return $"{pass} pass | {unavailable} unavailable | {unsupported} unsupported | {fail} fail";
    }

    private static Color StatusBackColor(EvidenceStatus status)
    {
        return status switch
        {
            EvidenceStatus.Pass => Color.FromArgb(223, 244, 232),
            EvidenceStatus.Fail => Color.FromArgb(252, 226, 226),
            EvidenceStatus.Unavailable => Color.FromArgb(237, 240, 244),
            EvidenceStatus.Unsupported => Color.FromArgb(237, 240, 244),
            _ => Color.White
        };
    }

    private static Color StatusForeColor(EvidenceStatus status)
    {
        return status switch
        {
            EvidenceStatus.Pass => Color.FromArgb(25, 105, 56),
            EvidenceStatus.Fail => Color.FromArgb(164, 37, 37),
            EvidenceStatus.Unavailable => Color.FromArgb(78, 86, 96),
            EvidenceStatus.Unsupported => Color.FromArgb(78, 86, 96),
            _ => SystemColors.ControlText
        };
    }

    private static void ShowAbout()
    {
        const string text =
            "I.M.A.P. is satire/commentary about over-reading ordinary software integration patterns as automatic license triggers.\n\n" +
            "This program is AGPL-licensed and performs read-only inspection of the Windows printing subsystem. It does not submit print jobs, modify printers, install drivers, or change Windows Update settings.\n\n" +
            "This is not legal advice, and this program does not claim that Windows violates the AGPL.";

        MessageBox.Show(text, "About I.M.A.P.", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
