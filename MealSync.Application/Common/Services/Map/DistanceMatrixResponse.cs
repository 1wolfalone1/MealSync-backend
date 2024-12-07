using Newtonsoft.Json;

namespace MealSync.Application.Common.Services.Map;

public class DistanceMatrixResponse
{
    public List<Row> Rows { get; set; }
}

public class Row
{
    public List<Element> Elements { get; set; } = new();

    [JsonIgnore]
    public int AverageDuration
    {
        get
        {
            if (Elements != null && Elements.Count > 0)
            {
                return Elements.Sum(e => e.Duration.Value) / Elements.Count;
            }

            return 0;
        }
    }
}

public class Element
{
    public string Status { get; set; }

    public Duration Duration { get; set; }

    public Distance Distance { get; set; }
}

public class Duration
{
    public string Text { get; set; }

    public int Value { get; set; }
}

public class Distance
{
    public string Text { get; set; }

    public int Value { get; set; }
}