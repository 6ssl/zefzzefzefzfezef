using MySqlConnector;

namespace LeerlingenGui;

internal sealed class LeerlingenForm : Form
{
    private static readonly Color AchtergrondKleur = Color.FromArgb(245, 247, 251);
    private static readonly Color KaartKleur = Color.White;
    private static readonly Color TekstKleur = Color.FromArgb(30, 41, 59);
    private static readonly Color GedempteTekstKleur = Color.FromArgb(100, 116, 139);
    private static readonly Color AccentKleur = Color.FromArgb(79, 70, 229);
    private static readonly Color RandKleur = Color.FromArgb(226, 232, 240);
    private static readonly Color FoutKleur = Color.FromArgb(185, 28, 28);

    private readonly DataGridView _leerlingenGrid = new();
    private readonly TextBox _naamTextBox = new();
    private readonly NumericUpDown _leeftijdNumeric = new();
    private readonly Label _statusLabel = new();
    private readonly Label _aantalLabel = new();
    private readonly Label _legeLijstLabel = new();

    public LeerlingenForm()
    {
        Text = "Leerlingen | Beheer";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(860, 620);
        Size = new Size(1120, 760);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = AchtergrondKleur;
        ForeColor = TekstKleur;
        Font = new Font("Segoe UI", 9.5f);

        BuildInterface();
        Shown += (_, _) => LaadLeerlingen();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(30, 24, 30, 22),
            ColumnCount = 1,
            RowCount = 3,
            BackColor = AchtergrondKleur
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 222));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildListCard(), 0, 1);
        root.Controls.Add(BuildEditorCard(), 0, 2);
        Controls.Add(root);
    }

    private Control BuildHeader()
    {
        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 18)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var titleBlock = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Dock = DockStyle.Left,
            Margin = Padding.Empty
        };
        titleBlock.Controls.Add(new Label
        {
            Text = "Leerlingenbeheer",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 23, FontStyle.Bold),
            ForeColor = TekstKleur,
            Margin = new Padding(0, 0, 0, 2)
        });
        titleBlock.Controls.Add(new Label
        {
            Text = "Bekijk en beheer de leerlingen in je klas.",
            AutoSize = true,
            Font = new Font("Segoe UI", 10),
            ForeColor = GedempteTekstKleur,
            Margin = Padding.Empty
        });
        header.Controls.Add(titleBlock, 0, 0);

        var actions = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Anchor = AnchorStyles.Right,
            Margin = new Padding(0, 4, 0, 0)
        };
        var summary = new Panel
        {
            AutoSize = true,
            BackColor = KaartKleur,
            Padding = new Padding(16, 10, 16, 10),
            Margin = new Padding(0, 0, 10, 0)
        };
        summary.Controls.Add(_aantalLabel);
        _aantalLabel.AutoSize = true;
        _aantalLabel.Text = "0 leerlingen";
        _aantalLabel.Font = new Font("Segoe UI Semibold", 10, FontStyle.Bold);
        _aantalLabel.ForeColor = AccentKleur;
        _aantalLabel.Dock = DockStyle.Fill;
        _aantalLabel.TextAlign = ContentAlignment.MiddleCenter;
        actions.Controls.Add(summary);
        actions.Controls.Add(CreateButton(
            "Vernieuwen",
            Color.White,
            TekstKleur,
            (_, _) => LaadLeerlingen()));
        header.Controls.Add(actions, 1, 0);

        return header;
    }

    private Control BuildListCard()
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = KaartKleur,
            Padding = new Padding(20),
            Margin = new Padding(0, 0, 0, 18)
        };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = KaartKleur
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(new Label
        {
            Text = "Alle leerlingen",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
            ForeColor = TekstKleur,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 0, 0, 8)
        }, 0, 0);

        ConfigureGrid();
        var gridArea = new Panel { Dock = DockStyle.Fill, BackColor = KaartKleur };
        gridArea.Controls.Add(_leerlingenGrid);
        _legeLijstLabel.Text = "Nog geen leerlingen\nVoeg hieronder je eerste leerling toe.";
        _legeLijstLabel.Dock = DockStyle.Fill;
        _legeLijstLabel.TextAlign = ContentAlignment.MiddleCenter;
        _legeLijstLabel.Font = new Font("Segoe UI", 11);
        _legeLijstLabel.ForeColor = GedempteTekstKleur;
        _legeLijstLabel.Visible = false;
        gridArea.Controls.Add(_legeLijstLabel);
        layout.Controls.Add(gridArea, 0, 1);
        card.Controls.Add(layout);
        return card;
    }

    private void ConfigureGrid()
    {
        _leerlingenGrid.Dock = DockStyle.Fill;
        _leerlingenGrid.BackgroundColor = KaartKleur;
        _leerlingenGrid.BorderStyle = BorderStyle.None;
        _leerlingenGrid.ReadOnly = true;
        _leerlingenGrid.AllowUserToAddRows = false;
        _leerlingenGrid.AllowUserToDeleteRows = false;
        _leerlingenGrid.AllowUserToResizeRows = false;
        _leerlingenGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _leerlingenGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        _leerlingenGrid.MultiSelect = false;
        _leerlingenGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _leerlingenGrid.RowHeadersVisible = false;
        _leerlingenGrid.ColumnHeadersHeight = 42;
        _leerlingenGrid.RowTemplate.Height = 40;
        _leerlingenGrid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _leerlingenGrid.GridColor = RandKleur;
        _leerlingenGrid.EnableHeadersVisualStyles = false;
        _leerlingenGrid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        _leerlingenGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
        _leerlingenGrid.ColumnHeadersDefaultCellStyle.ForeColor = GedempteTekstKleur;
        _leerlingenGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold);
        _leerlingenGrid.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
        _leerlingenGrid.DefaultCellStyle.BackColor = KaartKleur;
        _leerlingenGrid.DefaultCellStyle.ForeColor = TekstKleur;
        _leerlingenGrid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(238, 242, 255);
        _leerlingenGrid.DefaultCellStyle.SelectionForeColor = TekstKleur;
        _leerlingenGrid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
        _leerlingenGrid.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
        _leerlingenGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
        _leerlingenGrid.SelectionChanged += (_, _) => VulFormulier();
        _leerlingenGrid.DataBindingComplete += (_, _) => FormatteerKolommen();
    }

    private void FormatteerKolommen()
    {
        if (_leerlingenGrid.Columns[nameof(Leerling.Id)] is { } idColumn)
        {
            idColumn.HeaderText = "ID";
            idColumn.FillWeight = 22;
        }

        if (_leerlingenGrid.Columns[nameof(Leerling.Naam)] is { } naamColumn)
        {
            naamColumn.HeaderText = "NAAM";
            naamColumn.FillWeight = 100;
        }

        if (_leerlingenGrid.Columns[nameof(Leerling.Leeftijd)] is { } leeftijdColumn)
        {
            leeftijdColumn.HeaderText = "LEEFTIJD";
            leeftijdColumn.FillWeight = 32;
        }
    }

    private Control BuildEditorCard()
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = KaartKleur,
            Padding = new Padding(20, 14, 20, 14)
        };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 4,
            BackColor = KaartKleur
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 19));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));

        layout.Controls.Add(new Label
        {
            Text = "Leerlinggegevens",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 12, FontStyle.Bold),
            ForeColor = TekstKleur,
            Anchor = AnchorStyles.Left
        }, 0, 0);

        layout.Controls.Add(CreateFieldLabel("Naam"), 0, 1);
        layout.Controls.Add(CreateFieldLabel("Leeftijd"), 1, 1);
        _naamTextBox.Dock = DockStyle.Fill;
        _naamTextBox.MaxLength = 255;
        _naamTextBox.BorderStyle = BorderStyle.FixedSingle;
        _naamTextBox.BackColor = Color.FromArgb(248, 250, 252);
        _naamTextBox.ForeColor = TekstKleur;
        _naamTextBox.Margin = new Padding(0, 0, 12, 0);
        _leeftijdNumeric.Dock = DockStyle.Fill;
        _leeftijdNumeric.Minimum = 0;
        _leeftijdNumeric.Maximum = int.MaxValue;
        _leeftijdNumeric.BorderStyle = BorderStyle.FixedSingle;
        _leeftijdNumeric.BackColor = Color.FromArgb(248, 250, 252);
        _leeftijdNumeric.ForeColor = TekstKleur;
        _leeftijdNumeric.Margin = new Padding(0, 0, 12, 0);
        layout.Controls.Add(_naamTextBox, 0, 2);
        layout.Controls.Add(_leeftijdNumeric, 1, 2);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = false,
            WrapContents = true,
            FlowDirection = FlowDirection.LeftToRight,
            Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
            Margin = new Padding(0, 0, 0, 3),
            Padding = new Padding(0, 0, 0, 0)
        };
        buttons.Controls.Add(CreateButton("Toevoegen", AccentKleur, Color.White, (_, _) => VoegLeerlingToe()));
        buttons.Controls.Add(CreateButton("Wijzigen", Color.FromArgb(238, 242, 255), AccentKleur, (_, _) => WijzigLeerling()));
        buttons.Controls.Add(CreateButton("Verwijderen", Color.FromArgb(254, 242, 242), FoutKleur, (_, _) => VerwijderLeerling()));
        layout.Controls.Add(buttons, 2, 2);

        _statusLabel.AutoSize = true;
        _statusLabel.ForeColor = GedempteTekstKleur;
        _statusLabel.Font = new Font("Segoe UI", 9);
        _statusLabel.Anchor = AnchorStyles.Left;
        _statusLabel.Margin = new Padding(0, 5, 0, 0);
        layout.Controls.Add(_statusLabel, 0, 3);
        layout.SetColumnSpan(_statusLabel, 3);

        card.Controls.Add(layout);
        return card;
    }

    private static Label CreateFieldLabel(string text) => new()
    {
        Text = text,
        AutoSize = true,
        ForeColor = GedempteTekstKleur,
        Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold),
        Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
        Margin = new Padding(0, 0, 0, 7)
    };

    private static Button CreateButton(string text, Color background, Color foreground, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            MinimumSize = new Size(94, 38),
            FlatStyle = FlatStyle.Flat,
            BackColor = background,
            ForeColor = foreground,
            Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 8),
            Padding = new Padding(8, 0, 8, 0)
        };
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(background, 0.12f);
        button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(background, 0.08f);
        button.Click += onClick;
        return button;
    }

    private void LaadLeerlingen()
    {
        try
        {
            DatabaseApp.EnsureDatabase();
            List<Leerling> leerlingen = DatabaseApp.GetLeerlingen();
            _leerlingenGrid.DataSource = leerlingen;
            _aantalLabel.Text = $"{leerlingen.Count} {(leerlingen.Count == 1 ? "leerling" : "leerlingen")}";
            _legeLijstLabel.Visible = leerlingen.Count == 0;
            _statusLabel.Text = "Lijst bijgewerkt.";
        }
        catch (MySqlException foutmelding)
        {
            ToonDatabaseFout(foutmelding);
        }
    }

    private void VoegLeerlingToe()
    {
        try
        {
            DatabaseApp.AddLeerling(_naamTextBox.Text, (int)_leeftijdNumeric.Value);
            LaadLeerlingen();
            _naamTextBox.Clear();
            _leeftijdNumeric.Value = 0;
            _statusLabel.Text = "Leerling is toegevoegd.";
        }
        catch (ArgumentException foutmelding)
        {
            ToonValidatieFout(foutmelding);
        }
        catch (MySqlException foutmelding)
        {
            ToonDatabaseFout(foutmelding);
        }
    }

    private void WijzigLeerling()
    {
        if (_leerlingenGrid.CurrentRow?.DataBoundItem is not Leerling geselecteerdeLeerling)
        {
            MessageBox.Show(this, "Selecteer eerst een leerling.", "Geen selectie",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            bool gewijzigd = DatabaseApp.UpdateLeerling(
                geselecteerdeLeerling.Id,
                _naamTextBox.Text,
                (int)_leeftijdNumeric.Value);
            LaadLeerlingen();
            _statusLabel.Text = gewijzigd ? "Leerling is aangepast." : "De leerling is niet meer gevonden.";
        }
        catch (ArgumentException foutmelding)
        {
            ToonValidatieFout(foutmelding);
        }
        catch (MySqlException foutmelding)
        {
            ToonDatabaseFout(foutmelding);
        }
    }

    private void VerwijderLeerling()
    {
        if (_leerlingenGrid.CurrentRow?.DataBoundItem is not Leerling geselecteerdeLeerling)
        {
            MessageBox.Show(this, "Selecteer eerst een leerling.", "Geen selectie",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DialogResult bevestiging = MessageBox.Show(
            this,
            $"Weet je zeker dat je {geselecteerdeLeerling.Naam} wilt verwijderen?",
            "Verwijderen bevestigen",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (bevestiging != DialogResult.Yes)
        {
            return;
        }

        try
        {
            bool verwijderd = DatabaseApp.DeleteLeerling(geselecteerdeLeerling.Id);
            LaadLeerlingen();
            _statusLabel.Text = verwijderd ? "Leerling is verwijderd." : "De leerling is niet meer gevonden.";
        }
        catch (MySqlException foutmelding)
        {
            ToonDatabaseFout(foutmelding);
        }
    }

    private void VulFormulier()
    {
        if (_leerlingenGrid.CurrentRow?.DataBoundItem is Leerling leerling)
        {
            _naamTextBox.Text = leerling.Naam;
            _leeftijdNumeric.Value = leerling.Leeftijd;
        }
    }

    private void ToonValidatieFout(ArgumentException foutmelding)
    {
        _statusLabel.Text = foutmelding.Message;
        MessageBox.Show(this, foutmelding.Message, "Controleer de invoer",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void ToonDatabaseFout(MySqlException foutmelding)
    {
        _statusLabel.Text = "Databasebewerking mislukt.";
        MessageBox.Show(
            this,
            $"De databasebewerking is mislukt: {foutmelding.Message}",
            "Databasefout",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
