using MySqlConnector;

namespace LeerlingenGui;

internal sealed class LeerlingenForm : Form
{
    private readonly DataGridView _leerlingenGrid = new();
    private readonly TextBox _naamTextBox = new();
    private readonly NumericUpDown _leeftijdNumeric = new();
    private readonly Label _statusLabel = new();

    public LeerlingenForm()
    {
        Text = "Leerlingen beheren";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(720, 480);
        Size = new Size(900, 600);
        Font = new Font("Segoe UI", 10);

        BuildInterface();
        Shown += (_, _) => LaadLeerlingen();
    }

    private void BuildInterface()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 3
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "Leerlingen",
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 14)
        };

        ConfigureGrid();
        root.Controls.Add(title, 0, 0);
        root.Controls.Add(_leerlingenGrid, 0, 1);
        root.Controls.Add(BuildEditor(), 0, 2);
        Controls.Add(root);
    }

    private void ConfigureGrid()
    {
        _leerlingenGrid.Dock = DockStyle.Fill;
        _leerlingenGrid.ReadOnly = true;
        _leerlingenGrid.AllowUserToAddRows = false;
        _leerlingenGrid.AllowUserToDeleteRows = false;
        _leerlingenGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _leerlingenGrid.MultiSelect = false;
        _leerlingenGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _leerlingenGrid.RowHeadersVisible = false;
        _leerlingenGrid.SelectionChanged += (_, _) => VulFormulier();
    }

    private Control BuildEditor()
    {
        var editor = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 4,
            RowCount = 3,
            Padding = new Padding(0, 14, 0, 0)
        };
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        _naamTextBox.Dock = DockStyle.Fill;
        _naamTextBox.MaxLength = 255;
        _leeftijdNumeric.Dock = DockStyle.Left;
        _leeftijdNumeric.Minimum = 0;
        _leeftijdNumeric.Maximum = int.MaxValue;
        _leeftijdNumeric.Width = 140;

        editor.Controls.Add(new Label { Text = "Naam", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        editor.Controls.Add(_naamTextBox, 1, 0);
        editor.Controls.Add(new Label { Text = "Leeftijd", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(16, 3, 8, 3) }, 2, 0);
        editor.Controls.Add(_leeftijdNumeric, 3, 0);

        var buttons = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            WrapContents = false,
            Margin = new Padding(0, 12, 0, 0)
        };
        buttons.Controls.Add(CreateButton("Toevoegen", (_, _) => VoegLeerlingToe()));
        buttons.Controls.Add(CreateButton("Wijzigen", (_, _) => WijzigLeerling()));
        buttons.Controls.Add(CreateButton("Verwijderen", (_, _) => VerwijderLeerling()));
        buttons.Controls.Add(CreateButton("Vernieuwen", (_, _) => LaadLeerlingen()));
        editor.Controls.Add(buttons, 0, 1);
        editor.SetColumnSpan(buttons, 4);

        _statusLabel.AutoSize = true;
        _statusLabel.Margin = new Padding(0, 10, 0, 0);
        editor.Controls.Add(_statusLabel, 0, 2);
        editor.SetColumnSpan(_statusLabel, 4);
        return editor;
    }

    private static Button CreateButton(string text, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            MinimumSize = new Size(100, 34),
            Margin = new Padding(0, 0, 8, 0)
        };
        button.Click += onClick;
        return button;
    }

    private void LaadLeerlingen()
    {
        try
        {
            DatabaseApp.EnsureDatabase();
            _leerlingenGrid.DataSource = DatabaseApp.GetLeerlingen();
            _statusLabel.Text = $"{_leerlingenGrid.Rows.Count} leerling(en) geladen.";
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
            _naamTextBox.Clear();
            LaadLeerlingen();
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
