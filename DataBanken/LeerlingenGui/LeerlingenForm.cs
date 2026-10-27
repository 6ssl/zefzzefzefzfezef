using MySqlConnector;

namespace LeerlingenGui;

internal sealed class LeerlingenForm : Form
{
    private static readonly Color AchtergrondKleur = Color.FromArgb(240, 247, 255);
    private static readonly Color KaartKleur = Color.White;
    private static readonly Color TekstKleur = Color.FromArgb(23, 37, 84);
    private static readonly Color GedempteTekstKleur = Color.FromArgb(100, 116, 139);
    private static readonly Color AccentKleur = Color.FromArgb(79, 70, 229);
    private static readonly Color RandKleur = Color.FromArgb(219, 234, 254);
    private static readonly Color FoutKleur = Color.FromArgb(225, 29, 72);
    private static readonly Color SelectieKleur = Color.FromArgb(8, 145, 178);

    private readonly DataGridView _leerlingenGrid = new();
    private readonly TextBox _naamTextBox = new();
    private readonly NumericUpDown _leeftijdNumeric = new();
    private readonly Label _statusLabel = new();
    private readonly Label _aantalLabel = new();
    private readonly Label _legeLijstLabel = new();
    private Button? _ongedaanMakenButton;
    private UndoActie? _laatsteActie;

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
        WerkOngedaanMakenKnopBij();
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
        _leerlingenGrid.RowHeadersVisible = true;
        _leerlingenGrid.RowHeadersWidth = 48;
        _leerlingenGrid.TopLeftHeaderCell.Value = "RIJ";
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
        _leerlingenGrid.DefaultCellStyle.SelectionBackColor = KaartKleur;
        _leerlingenGrid.DefaultCellStyle.SelectionForeColor = TekstKleur;
        _leerlingenGrid.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
        _leerlingenGrid.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
        _leerlingenGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 253);
        _leerlingenGrid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(250, 251, 253);
        _leerlingenGrid.AlternatingRowsDefaultCellStyle.SelectionForeColor = TekstKleur;
        _leerlingenGrid.RowHeadersDefaultCellStyle.BackColor = Color.FromArgb(236, 254, 255);
        _leerlingenGrid.RowHeadersDefaultCellStyle.ForeColor = SelectieKleur;
        _leerlingenGrid.RowHeadersDefaultCellStyle.SelectionBackColor = SelectieKleur;
        _leerlingenGrid.RowHeadersDefaultCellStyle.SelectionForeColor = Color.White;
        _leerlingenGrid.RowHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _leerlingenGrid.RowHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold);
        _leerlingenGrid.ColumnHeadersDefaultCellStyle.SelectionBackColor =
            _leerlingenGrid.ColumnHeadersDefaultCellStyle.BackColor;
        _leerlingenGrid.ColumnHeadersDefaultCellStyle.SelectionForeColor =
            _leerlingenGrid.ColumnHeadersDefaultCellStyle.ForeColor;
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
            RowCount = 5,
            BackColor = KaartKleur
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
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

        var actions = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = new Padding(0, 2, 0, 2)
        };
        for (int column = 0; column < 4; column++)
        {
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        }

        actions.Controls.Add(CreateActionButton(
            "Toevoegen",
            Color.FromArgb(79, 70, 229),
            Color.FromArgb(129, 140, 248),
            Color.White,
            (_, _) => VoegLeerlingToe()), 0, 0);
        actions.Controls.Add(CreateActionButton(
            "Wijzigen",
            Color.FromArgb(8, 145, 178),
            Color.FromArgb(34, 211, 238),
            Color.White,
            (_, _) => WijzigLeerling()), 1, 0);
        actions.Controls.Add(CreateActionButton(
            "Verwijderen",
            Color.FromArgb(225, 29, 72),
            Color.FromArgb(251, 113, 133),
            Color.White,
            (_, _) => VerwijderLeerling()), 2, 0);
        _ongedaanMakenButton = CreateActionButton(
            "Ongedaan maken",
            Color.FromArgb(124, 58, 237),
            Color.FromArgb(192, 132, 252),
            Color.White,
            (_, _) => MaakLaatsteActieOngedaan());
        actions.Controls.Add(_ongedaanMakenButton, 3, 0);
        layout.Controls.Add(actions, 0, 3);
        layout.SetColumnSpan(actions, 3);

        _statusLabel.AutoSize = true;
        _statusLabel.ForeColor = GedempteTekstKleur;
        _statusLabel.Font = new Font("Segoe UI", 9);
        _statusLabel.Anchor = AnchorStyles.Left;
        _statusLabel.Margin = new Padding(0, 5, 0, 0);
        layout.Controls.Add(_statusLabel, 0, 4);
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

    private static Button CreateButton(
        string text,
        Color background,
        Color highlight,
        Color foreground,
        EventHandler onClick)
    {
        var button = new AnimatedButton(background, highlight)
        {
            Text = text,
            AutoSize = true,
            MinimumSize = new Size(94, 38),
            ForeColor = foreground,
            Font = new Font("Segoe UI Semibold", 9, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 8, 8),
            Padding = new Padding(8, 0, 8, 0)
        };
        button.Click += onClick;
        return button;
    }

    private static Button CreateActionButton(
        string text,
        Color background,
        Color highlight,
        Color foreground,
        EventHandler onClick)
    {
        var button = CreateButton(text, background, highlight, foreground, onClick);
        button.AutoSize = false;
        button.Dock = DockStyle.Fill;
        button.Margin = new Padding(0, 0, 8, 0);
        return button;
    }

    private void LaadLeerlingen()
    {
        try
        {
            DatabaseApp.EnsureDatabase();
            long aantalInDatabase = DatabaseApp.GetLeerlingenCount();
            List<Leerling> leerlingen = DatabaseApp.GetLeerlingen();
            _leerlingenGrid.DataSource = leerlingen;
            _aantalLabel.Text = $"{aantalInDatabase} {(aantalInDatabase == 1 ? "leerling" : "leerlingen")}";
            _legeLijstLabel.Visible = leerlingen.Count == 0;
            _statusLabel.Text = aantalInDatabase == leerlingen.Count
                ? "Lijst bijgewerkt."
                : $"{aantalInDatabase} in database; {leerlingen.Count} weergegeven.";
        }
        catch (MySqlException foutmelding)
        {
            ToonDatabaseFout(foutmelding);
        }
    }

    private void WerkOngedaanMakenKnopBij()
    {
        if (_ongedaanMakenButton is null)
        {
            return;
        }

        _ongedaanMakenButton.Enabled = _laatsteActie is not null;
        _ongedaanMakenButton.AccessibleDescription = _laatsteActie is null
            ? "Er is geen actie om ongedaan te maken."
            : $"Maak de laatste {_laatsteActie.Beschrijving.ToLowerInvariant()} ongedaan.";
    }

    private void VoegLeerlingToe()
    {
        try
        {
            int id = DatabaseApp.AddLeerling(_naamTextBox.Text, (int)_leeftijdNumeric.Value);
            _laatsteActie = new ToevoegingOngedaanMaken(id);
            WerkOngedaanMakenKnopBij();
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

        string nieuweNaam = _naamTextBox.Text.Trim();
        int nieuweLeeftijd = (int)_leeftijdNumeric.Value;
        DialogResult bevestiging = MessageBox.Show(
            this,
            $"De gegevens van {geselecteerdeLeerling.Naam} wijzigen naar {nieuweNaam}, {nieuweLeeftijd} jaar?",
            "Wijziging bevestigen",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (bevestiging != DialogResult.Yes)
        {
            return;
        }

        try
        {
            bool gewijzigd = DatabaseApp.UpdateLeerling(
                geselecteerdeLeerling.Id,
                nieuweNaam,
                nieuweLeeftijd);
            if (gewijzigd)
            {
                _laatsteActie = new WijzigingOngedaanMaken(
                    geselecteerdeLeerling,
                    geselecteerdeLeerling with { Naam = nieuweNaam, Leeftijd = nieuweLeeftijd });
                WerkOngedaanMakenKnopBij();
            }

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
            if (verwijderd)
            {
                _laatsteActie = new VerwijderingOngedaanMaken(geselecteerdeLeerling);
                WerkOngedaanMakenKnopBij();
            }

            LaadLeerlingen();
            _statusLabel.Text = verwijderd ? "Leerling is verwijderd." : "De leerling is niet meer gevonden.";
        }
        catch (MySqlException foutmelding)
        {
            ToonDatabaseFout(foutmelding);
        }
    }

    private void MaakLaatsteActieOngedaan()
    {
        if (_laatsteActie is not { } actie)
        {
            return;
        }

        DialogResult bevestiging = MessageBox.Show(
            this,
            $"Weet je zeker dat je de laatste actie ({actie.Beschrijving.ToLowerInvariant()}) ongedaan wilt maken?",
            "Actie ongedaan maken",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (bevestiging != DialogResult.Yes)
        {
            return;
        }

        try
        {
            switch (actie)
            {
                case ToevoegingOngedaanMaken toevoeging:
                    if (!DatabaseApp.DeleteLeerling(toevoeging.Id))
                    {
                        ToonOngedaanMakenFout("De toegevoegde leerling is niet meer gevonden.");
                        return;
                    }

                    break;
                case WijzigingOngedaanMaken wijziging:
                    if (!DatabaseApp.UpdateLeerling(
                            wijziging.NieuweWaarden.Id,
                            wijziging.OudeWaarden.Naam,
                            wijziging.OudeWaarden.Leeftijd))
                    {
                        ToonOngedaanMakenFout("De gewijzigde leerling is niet meer gevonden.");
                        return;
                    }

                    break;
                case VerwijderingOngedaanMaken verwijdering:
                    DatabaseApp.RestoreDeletedLeerling(verwijdering.Leerling);
                    break;
                default:
                    throw new InvalidOperationException("Onbekende actie voor ongedaan maken.");
            }

            _laatsteActie = null;
            WerkOngedaanMakenKnopBij();
            LaadLeerlingen();
            _statusLabel.Text = $"{actie.Beschrijving} ongedaan gemaakt.";
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

    private void ToonOngedaanMakenFout(string bericht)
    {
        _statusLabel.Text = bericht;
        MessageBox.Show(this, bericht, "Ongedaan maken mislukt",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

    private abstract record UndoActie(string Beschrijving);

    private sealed record ToevoegingOngedaanMaken(int Id) : UndoActie("toevoeging");

    private sealed record WijzigingOngedaanMaken(Leerling OudeWaarden, Leerling NieuweWaarden)
        : UndoActie("wijziging");

    private sealed record VerwijderingOngedaanMaken(Leerling Leerling) : UndoActie("verwijdering");
}

internal sealed class AnimatedButton : Button
{
    private readonly System.Windows.Forms.Timer _animationTimer = new() { Interval = 15 };
    private readonly Color _baseColor;
    private readonly Color _highlightColor;
    private float _hoverProgress;
    private float _rippleProgress;
    private Point _rippleCenter;
    private bool _isHovered;
    private bool _isAnimatingRipple;

    public AnimatedButton(Color baseColor, Color highlightColor)
    {
        _baseColor = baseColor;
        _highlightColor = highlightColor;
        BackColor = baseColor;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        UseVisualStyleBackColor = false;
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);

        _animationTimer.Tick += Animate;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _isHovered = true;
        StartAnimation();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _isHovered = false;
        StartAnimation();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _rippleCenter = e.Location;
            _rippleProgress = 0;
            _isAnimatingRipple = true;
            StartAnimation();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        Rectangle bounds = ClientRectangle;
        bounds.Width -= 1;
        bounds.Height -= 3;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        Rectangle face = bounds;
        face.Height -= 2;
        Color firstColor = _baseColor;
        Color secondColor = _highlightColor;
        if (!Enabled)
        {
            firstColor = ControlPaint.Light(SystemColors.Control, 0.1f);
            secondColor = SystemColors.Control;
        }
        else if (_hoverProgress > 0)
        {
            firstColor = Blend(_baseColor, ControlPaint.Light(_baseColor, 0.24f), _hoverProgress);
            secondColor = Blend(_highlightColor, ControlPaint.Light(_highlightColor, 0.18f), _hoverProgress);
        }

        using (var shadowPath = CreateRoundedPath(bounds, 12))
        using (var shadowBrush = new SolidBrush(Color.FromArgb(32, _baseColor)))
        {
            e.Graphics.TranslateTransform(0, 3);
            e.Graphics.FillPath(shadowBrush, shadowPath);
            e.Graphics.ResetTransform();
        }

        using var facePath = CreateRoundedPath(face, 12);
        using var gradient = new System.Drawing.Drawing2D.LinearGradientBrush(
            face,
            firstColor,
            secondColor,
            System.Drawing.Drawing2D.LinearGradientMode.ForwardDiagonal);
        using var outline = new Pen(Color.FromArgb(60, Color.White), 1);
        e.Graphics.FillPath(gradient, facePath);

        if (_isAnimatingRipple)
        {
            System.Drawing.Drawing2D.GraphicsState state = e.Graphics.Save();
            e.Graphics.SetClip(facePath);
            float radius = MathF.Sqrt(face.Width * face.Width + face.Height * face.Height)
                * _rippleProgress;
            using var rippleBrush = new SolidBrush(
                Color.FromArgb((int)(76 * (1 - _rippleProgress)), Color.White));
            e.Graphics.FillEllipse(
                rippleBrush,
                _rippleCenter.X - radius,
                _rippleCenter.Y - radius,
                radius * 2,
                radius * 2);
            e.Graphics.Restore(state);
        }

        e.Graphics.DrawPath(outline, facePath);
        TextRenderer.DrawText(
            e.Graphics,
            Text,
            Font,
            face,
            Enabled ? ForeColor : SystemColors.GrayText,
            TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.HidePrefix);

        if (Focused && ShowFocusCues)
        {
            ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(face, -8, -6), ForeColor, firstColor);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _animationTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    private void Animate(object? sender, EventArgs e)
    {
        float targetHover = _isHovered ? 1 : 0;
        _hoverProgress = MoveTowards(_hoverProgress, targetHover, 0.14f);
        if (_isAnimatingRipple)
        {
            _rippleProgress += 0.12f;
            if (_rippleProgress >= 1)
            {
                _rippleProgress = 1;
                _isAnimatingRipple = false;
            }
        }

        Invalidate();
        if (!_isAnimatingRipple && _hoverProgress == targetHover)
        {
            _animationTimer.Stop();
        }
    }

    private void StartAnimation()
    {
        if (!_animationTimer.Enabled)
        {
            _animationTimer.Start();
        }
    }

    private static float MoveTowards(float current, float target, float amount)
    {
        if (MathF.Abs(target - current) <= amount)
        {
            return target;
        }

        return current + MathF.Sign(target - current) * amount;
    }

    private static Color Blend(Color from, Color to, float amount) => Color.FromArgb(
        (int)(from.A + (to.A - from.A) * amount),
        (int)(from.R + (to.R - from.R) * amount),
        (int)(from.G + (to.G - from.G) * amount),
        (int)(from.B + (to.B - from.B) * amount));

    private static System.Drawing.Drawing2D.GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        int diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        var corner = new Rectangle(bounds.Location, new Size(diameter, diameter));

        path.AddArc(corner, 180, 90);
        corner.X = bounds.Right - diameter;
        path.AddArc(corner, 270, 90);
        corner.Y = bounds.Bottom - diameter;
        path.AddArc(corner, 0, 90);
        corner.X = bounds.Left;
        path.AddArc(corner, 90, 90);
        path.CloseFigure();
        return path;
    }
}
