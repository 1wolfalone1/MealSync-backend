public class DirectionResponse
{
    public List<GeocodedWaypoint> GeocodedWaypoints { get; set; }

    public List<Route> Routes { get; set; }
}

public class GeocodedWaypoint
{
    // Customize properties if you have details about the structure.
}

public class Route
{
    public Bounds Bounds { get; set; }

    public List<Leg> Legs { get; set; }

    public OverviewPolyline OverviewPolyline { get; set; }

    public List<string> Warnings { get; set; }

    public List<int> WaypointOrder { get; set; }
}

public class Bounds
{
    // Customize properties if you have details about the structure.
}

public class Leg
{
    public Distance Distance { get; set; }

    public Duration Duration { get; set; }

    public List<Step> Steps { get; set; }
}

public class Step
{
    // Customize properties if you have details about the structure.
}

public class OverviewPolyline
{
    public string Points { get; set; }
}

public class Distance
{
    public string Text { get; set; } // e.g., "1155.66 km"

    public int Value { get; set; } // e.g., 1155660 (meters)
}

public class Duration
{
    public string Text { get; set; } // e.g., "22 giờ 10 phút"

    public int Value { get; set; } // e.g., 79799 (seconds)
}