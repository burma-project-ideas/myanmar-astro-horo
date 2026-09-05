namespace BlazorMyanmarAstroHoro.Services;

/// <summary>Vimshottari Dasa sequence ported from CleanVB6/Kdata2.bas:Dasa.</summary>
public static class DasaCalculator
{
    private static readonly string[] Lords = ["Ketu", "Venus", "Sun", "Moon", "Mars", "Rahu", "Jupiter", "Saturn", "Mercury"];
    private static readonly int[] Years = [7, 20, 6, 10, 7, 18, 16, 19, 17];

    public static DasaResult Calculate(double moonLongitude, DateOnly birthDate, DateOnly asOfDate)
    {
        var nakshatra = (int)Math.Floor(moonLongitude / (360d / 27d));
        var lordIndex = nakshatra % Lords.Length;
        var progress = (moonLongitude / (360d / 27d)) % 1d;
        var firstDuration = (1d - progress) * Years[lordIndex];
        var periods = new List<DasaPeriod>();
        var start = birthDate;

        for (var offset = 0; offset < Lords.Length; offset++)
        {
            var index = (lordIndex + offset) % Lords.Length;
            var duration = offset == 0 ? firstDuration : Years[index];
            var end = AddYears(start, duration);
            periods.Add(new DasaPeriod { Lord = Lords[index], Start = start, End = end, DurationYears = duration });
            start = end;
        }

        var current = periods.FirstOrDefault(period => asOfDate >= period.Start && asOfDate < period.End) ?? periods[^1];
        var currentLord = Array.IndexOf(Lords, current.Lord);
        var subPeriods = BuildSubPeriods(current, currentLord);
        var currentSub = subPeriods.FirstOrDefault(period => asOfDate >= period.Start && asOfDate < period.End) ?? subPeriods[^1];
        return new DasaResult { MajorPeriods = periods, CurrentMajor = current, SubPeriods = subPeriods, CurrentSub = currentSub };
    }

    private static List<DasaPeriod> BuildSubPeriods(DasaPeriod major, int majorLord)
    {
        var result = new List<DasaPeriod>();
        var start = major.Start;
        for (var offset = 0; offset < Lords.Length; offset++)
        {
            var index = (majorLord + offset) % Lords.Length;
            var duration = major.DurationYears * Years[index] / 120d;
            var end = offset == Lords.Length - 1 ? major.End : AddYears(start, duration);
            result.Add(new DasaPeriod { Lord = Lords[index], Start = start, End = end, DurationYears = duration });
            start = end;
        }
        return result;
    }

    private static DateOnly AddYears(DateOnly value, double years) => value.AddDays((int)Math.Round(years * 365.2425d, MidpointRounding.AwayFromZero));
}
