using Newtonsoft.Json;
using Questao2;

public class Program
{
    public static void Main()
    {
        string teamName = "Paris Saint-Germain";
        int year = 2013;
        int totalGoals = getTotalScoredGoals(teamName, year).Result;

        Console.WriteLine("Team " + teamName + " scored " + totalGoals + " goals in " + year);

        teamName = "Chelsea";
        year = 2014;
        totalGoals = getTotalScoredGoals(teamName, year).Result;

        Console.WriteLine("Team " + teamName + " scored " + totalGoals + " goals in " + year);
    }

    public static async Task<int> getTotalScoredGoals(string team, int year)
    {
        int totalGoals = 0;
        using (HttpClient client = new HttpClient())
        {
            // Consulta onde o time é team1
            totalGoals += await GetGoalsFromMatches(client, year, "team1", team, "team1goals");

            // Consulta onde o time é team2
            totalGoals += await GetGoalsFromMatches(client, year, "team2", team, "team2goals");
        }

        return totalGoals;
    }

    private static async Task<int> GetGoalsFromMatches(HttpClient client, int year, string teamParam, string teamName, string goalKey)
    {
        int page = 1;
        int totalGoals = 0;
        int totalPages = 1;

        while (page <= totalPages)
        {
            string url = $"https://jsonmock.hackerrank.com/api/football_matches?year={year}&{teamParam}={Uri.EscapeDataString(teamName)}&page={page}";
            var response = await client.GetStringAsync(url);
            var data = JsonConvert.DeserializeObject<ApiResponse>(response);

            totalPages = data.total_pages;

            foreach (var match in data.data)
            {
                if (int.TryParse(goalKey == "team1goals" ? match.team1goals : match.team2goals, out int goals))
                {
                    totalGoals += goals;
                }
            }

            page++;
        }

        return totalGoals;
    }
}
