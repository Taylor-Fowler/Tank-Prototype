using System.Text.RegularExpressions;

[System.Serializable]
public class UserData
{
    public int Player_ID = -1;
    public string Username = "Guest";
    public string Device_ID;
    public int 
        Games_Played,
        Kills,
        Deaths,
        Assists,
        Wins,
        Losses;


    public static bool ValidateUsername(string username)
    {
        // Regular expression to check for alpha numeric only
        // Source: https://stackoverflow.com/a/1046743/6184424
        Regex regularExpression = new Regex("^[a-zA-Z0-9]*$");
        return regularExpression.IsMatch(username) || username == "Guest";
    }
}

