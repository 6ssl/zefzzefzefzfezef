using Xunit;

public class MenuChoiceTests
{
    [Fact]
    public void ParseMenuChoice_MapsViewDeleteExitAndAddOptions()
    {
        Assert.Equal(DatabaseApp.MenuAction.View, DatabaseApp.ParseMenuChoice("1"));
        Assert.Equal(DatabaseApp.MenuAction.Delete, DatabaseApp.ParseMenuChoice("2"));
        Assert.Equal(DatabaseApp.MenuAction.Exit, DatabaseApp.ParseMenuChoice("3"));
        Assert.Equal(DatabaseApp.MenuAction.Add, DatabaseApp.ParseMenuChoice("4"));
        Assert.Equal(DatabaseApp.MenuAction.Update, DatabaseApp.ParseMenuChoice("5"));
        Assert.Null(DatabaseApp.ParseMenuChoice("6"));
    }
}
