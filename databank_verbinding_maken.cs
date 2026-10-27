using MySqlConnector;

public sealed record Leerling(int Id, string Naam, int Leeftijd);

public static class DatabaseApp
{
    private static readonly string ConnectionString =
        new MySqlConnectionStringBuilder
        {
            Server = "localhost",
            Port = 3306,
            Database = "test",
            UserID = "root",
            Password = "password"
        }.ConnectionString;

    public static void Run()
    {
        try
        {
            EnsureDatabase();
        }
        catch (MySqlException foutmelding)
        {
            Console.WriteLine($"Database initialiseren mislukt: {foutmelding.Message}");
            return;
        }

        bool actief = true;
        while (actief)
        {
            Console.WriteLine("------------------------------------------");
            Console.WriteLine("1. leerlingen bekijken");
            Console.WriteLine("2. leerling verwijderen");
            Console.WriteLine("3. afsluiten");
            Console.WriteLine("4. leerling toevoegen");
            Console.WriteLine("5. leerling aanpassen");
            Console.Write("keuze: ");

            switch (ParseMenuChoice(Console.ReadLine()))
            {
                case MenuAction.View:
                    BekijkLeerlingen();
                    break;
                case MenuAction.Delete:
                    VerwijderLeerling();
                    break;
                case MenuAction.Exit:
                    actief = false;
                    break;
                case MenuAction.Add:
                    VoegLeerlingToe();
                    break;
                case MenuAction.Update:
                    PasLeerlingAan();
                    break;
                default:
                    Console.WriteLine("Ongeldige keuze. Kies een nummer van 1 tot en met 5.");
                    break;
            }
        }

        Console.WriteLine("Dit is het einde.");
    }

    internal static MenuAction? ParseMenuChoice(string? keuze) =>
        keuze switch
        {
            "1" => MenuAction.View,
            "2" => MenuAction.Delete,
            "3" or null => MenuAction.Exit,
            "4" => MenuAction.Add,
            "5" => MenuAction.Update,
            _ => null
        };

    public static void EnsureDatabase()
    {
        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();

        const string createTableSql = """
            CREATE TABLE IF NOT EXISTS floep (
                Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                Naam VARCHAR(255) NOT NULL,
                Leeftijd INT NOT NULL
            )
            """;
        using var command = new MySqlCommand(createTableSql, connection);
        command.ExecuteNonQuery();
    }

    public static List<Leerling> GetLeerlingen()
    {
        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();
        const string sql = "SELECT Id, Naam, Leeftijd FROM floep ORDER BY Id";
        using var command = new MySqlCommand(sql, connection);
        using var reader = command.ExecuteReader();

        var leerlingen = new List<Leerling>();
        while (reader.Read())
        {
            leerlingen.Add(new Leerling(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetInt32(2)));
        }

        return leerlingen;
    }

    public static void AddLeerling(string naam, int leeftijd)
    {
        ValidateLeerling(naam, leeftijd);

        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();
        const string sql = "INSERT INTO floep (Naam, Leeftijd) VALUES (@naam, @leeftijd)";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@naam", naam.Trim());
        command.Parameters.AddWithValue("@leeftijd", leeftijd);
        command.ExecuteNonQuery();
    }

    public static bool UpdateLeerling(int id, string naam, int leeftijd)
    {
        ValidateLeerling(naam, leeftijd);
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "Het ID moet positief zijn.");
        }

        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();
        const string sql = "UPDATE floep SET Naam = @naam, Leeftijd = @leeftijd WHERE Id = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@naam", naam.Trim());
        command.Parameters.AddWithValue("@leeftijd", leeftijd);
        command.Parameters.AddWithValue("@id", id);
        if (command.ExecuteNonQuery() > 0)
        {
            return true;
        }

        using var existsCommand = new MySqlCommand(
            "SELECT EXISTS(SELECT 1 FROM floep WHERE Id = @id)",
            connection);
        existsCommand.Parameters.AddWithValue("@id", id);
        return Convert.ToInt32(existsCommand.ExecuteScalar()) == 1;
    }

    public static bool DeleteLeerling(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "Het ID moet positief zijn.");
        }

        using var connection = new MySqlConnection(ConnectionString);
        connection.Open();
        const string sql = "DELETE FROM floep WHERE Id = @id";
        using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        return command.ExecuteNonQuery() > 0;
    }

    internal enum MenuAction
    {
        View,
        Delete,
        Exit,
        Add,
        Update
    }

    private static void VoegLeerlingToe()
    {
        Console.Write("Naam: ");
        string? naam = Console.ReadLine();
        Console.Write("Leeftijd: ");
        if (!int.TryParse(Console.ReadLine(), out int leeftijd))
        {
            Console.WriteLine("Ongeldige leeftijd. Voer een positief geheel getal of 0 in.");
            return;
        }

        try
        {
            AddLeerling(naam ?? string.Empty, leeftijd);
            Console.WriteLine("Leerling is toegevoegd.");
        }
        catch (ArgumentException foutmelding)
        {
            Console.WriteLine(foutmelding.Message);
        }
        catch (MySqlException foutmelding)
        {
            Console.WriteLine($"Leerling kon niet worden toegevoegd: {foutmelding.Message}");
        }
    }

    private static void PasLeerlingAan()
    {
        Console.Write("Voer het ID in van de leerling die u wilt aanpassen: ");
        if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
        {
            Console.WriteLine("Ongeldig ID. Voer een positief geheel getal in.");
            return;
        }

        Console.Write("Nieuwe naam: ");
        string? naam = Console.ReadLine();
        Console.Write("Nieuwe leeftijd: ");
        if (!int.TryParse(Console.ReadLine(), out int leeftijd))
        {
            Console.WriteLine("Ongeldige leeftijd. Voer een positief geheel getal of 0 in.");
            return;
        }

        try
        {
            Console.WriteLine(
                UpdateLeerling(id, naam ?? string.Empty, leeftijd)
                    ? $"Leerling met ID {id} is aangepast."
                    : $"Geen leerling gevonden met ID {id}.");
        }
        catch (ArgumentException foutmelding)
        {
            Console.WriteLine(foutmelding.Message);
        }
        catch (MySqlException foutmelding)
        {
            Console.WriteLine($"Leerling kon niet worden aangepast: {foutmelding.Message}");
        }
    }

    private static void VerwijderLeerling()
    {
        Console.Write("Voer het ID in van de leerling die u wilt verwijderen: ");
        if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
        {
            Console.WriteLine("Ongeldig ID. Voer een positief geheel getal in.");
            return;
        }

        try
        {
            Console.WriteLine(
                DeleteLeerling(id)
                    ? $"Leerling met ID {id} is verwijderd."
                    : $"Geen leerling gevonden met ID {id}.");
        }
        catch (MySqlException foutmelding)
        {
            Console.WriteLine($"Leerling kon niet worden verwijderd: {foutmelding.Message}");
        }
    }

    private static void BekijkLeerlingen()
    {
        try
        {
            List<Leerling> leerlingen = GetLeerlingen();
            if (leerlingen.Count == 0)
            {
                Console.WriteLine("Er zijn nog geen leerlingen.");
                return;
            }

            foreach (Leerling leerling in leerlingen)
            {
                Console.WriteLine(
                    $"Id: {leerling.Id}, Naam: {leerling.Naam}, Leeftijd: {leerling.Leeftijd}");
            }
        }
        catch (MySqlException foutmelding)
        {
            Console.WriteLine($"Leerlingen konden niet worden opgehaald: {foutmelding.Message}");
        }
    }

    private static void ValidateLeerling(string naam, int leeftijd)
    {
        if (string.IsNullOrWhiteSpace(naam) || naam.Trim().Length > 255)
        {
            throw new ArgumentException(
                "Ongeldige naam. De naam mag niet leeg zijn en maximaal 255 tekens bevatten.",
                nameof(naam));
        }

        if (leeftijd < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(leeftijd),
                "Ongeldige leeftijd. Voer een positief geheel getal of 0 in.");
        }
    }
}
