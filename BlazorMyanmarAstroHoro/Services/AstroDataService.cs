using System.Globalization;

namespace BlazorMyanmarAstroHoro.Services;

public sealed class AstroInputModel
{
    public string Name { get; set; } = "";
    public int Year { get; set; } = DateTime.Today.Year;
    public int Month { get; set; } = DateTime.Today.Month;
    public int Day { get; set; } = DateTime.Today.Day;
    public int Hour { get; set; } = DateTime.Now.Hour;
    public int Minute { get; set; } = DateTime.Now.Minute;
    public double Latitude { get; set; } = 16.8661;
    public double Longitude { get; set; } = 96.1951;
    public string LocationName { get; set; } = "Yangon";
    public bool IsLocalMeanTime { get; set; }
    public double? StandardTimeOffset { get; set; } = 6.5d;
}

public sealed class AstroSummaryRow { public string Label { get; set; } = ""; public string Value { get; set; } = ""; }
public sealed class PlanetaryStatusRow { public string Planet { get; set; } = ""; public string Position { get; set; } = ""; public string Navamsa { get; set; } = ""; public string Status { get; set; } = ""; }
public sealed class DasaPeriod { public string Lord { get; init; } = ""; public DateOnly Start { get; init; } public DateOnly End { get; init; } public double DurationYears { get; init; } }
public sealed class DasaResult { public List<DasaPeriod> MajorPeriods { get; init; } = []; public DasaPeriod? CurrentMajor { get; init; } public List<DasaPeriod> SubPeriods { get; init; } = []; public DasaPeriod? CurrentSub { get; init; } }
public sealed class BoCalendarRow { public int KaliYear { get; init; } public long Sawana { get; init; } public long Kyamat { get; init; } public long Tithi { get; init; } public long LunarMonth { get; init; } public long LunarDay { get; init; } public int Weekday { get; init; } public long Adimat { get; init; } public long Yetngin { get; init; } }
public sealed class MyanmarCalendarResult { public int KaliYear { get; init; } public int MyanmarEraYear { get; init; } public string EraLabel { get; init; } = ""; public string CalendarType { get; init; } = ""; public string Month { get; init; } = ""; public string Phase { get; init; } = ""; public int LunarDay { get; init; } public int DayInSolarYear { get; init; } public string Weekday { get; init; } = ""; public long Tithi { get; init; } public long LunarMonthNumber { get; init; } }
public sealed class HouseCuspRow { public int House { get; init; } public string Cusp { get; init; } = ""; public string Boundary { get; init; } = ""; }
public sealed class TraditionalIndicatorRow { public string Label { get; init; } = ""; public string Value { get; init; } = ""; public string Note { get; init; } = ""; }
public sealed class AstroResultModel
{
    public List<AstroSummaryRow> SummaryTable { get; } = [];
    public List<string> RasiHouses { get; } = Enumerable.Repeat("", 12).ToList();
    public List<string> NavamsaHouses { get; } = Enumerable.Repeat("", 12).ToList();
    public List<string> DrekkanaHouses { get; } = Enumerable.Repeat("", 12).ToList();
    public List<PlanetaryStatusRow> DetailedPlanets { get; } = [];
    public List<HouseCuspRow> HouseCusps { get; } = [];
    public List<TraditionalIndicatorRow> TraditionalIndicators { get; } = [];
    public DasaResult? Dasa { get; set; }
    public List<BoCalendarRow> BoCalendar { get; } = [];
    public MyanmarCalendarResult? MyanmarCalendar { get; set; }
}

public sealed class AstroDataService
{
    private static readonly string[] Signs = ["Aries", "Taurus", "Gemini", "Cancer", "Leo", "Virgo", "Libra", "Scorpio", "Sagittarius", "Capricorn", "Aquarius", "Pisces"];
    private static readonly string[] Planets = ["Sun", "Moon", "Mars", "Mercury", "Jupiter", "Venus", "Saturn", "Rahu", "Ketu"];
    private static readonly string[] Marks = ["၁", "၂", "၃", "၄", "၅", "၆", "၇", "၈", "၉"];
    private static readonly string[] Weekdays = ["တနင်္ဂနွေ", "တနင်္လာ", "အင်္ဂါ", "ဗုဒ္ဓဟူး", "ကြာသပတေး", "သောကြာ", "စနေ"];
    private static readonly string[] Digits = ["၀", "၁", "၂", "၃", "၄", "၅", "၆", "၇", "၈", "၉"];

    public Task<AstroResultModel> CalculateAstroAsync(AstroInputModel input)
    {
        _ = new DateTime(input.Year, input.Month, input.Day, input.Hour, input.Minute, 0);
        if (input.Longitude is < -180d or > 180d) throw new ArgumentOutOfRangeException(nameof(input.Longitude));
        var calculator = new SuryaSiddhantaCalculator();
        calculator.Calculate(input.Year, input.Month, input.Day, input.Hour, input.Minute, 0, input.Longitude, input.Latitude, input.IsLocalMeanTime ? input.Longitude / 15d : input.StandardTimeOffset ?? 6.5d);
        var result = new AstroResultModel();
        result.MyanmarCalendar = MyanmarCalendarCalculator.Calculate(calculator.Twn);
        PopulateSummary(result, input, calculator, result.MyanmarCalendar);
        PopulateCharts(result, calculator);
        PopulateHouseCusps(result, calculator);
        PopulateTraditionalIndicators(result, input, calculator);
        PopulatePlanets(result, calculator);
        result.Dasa = DasaCalculator.Calculate(calculator.Planets[1], new DateOnly(input.Year, input.Month, input.Day), DateOnly.FromDateTime(DateTime.Today));
        result.BoCalendar.AddRange(BoCalculator.Calculate((int)Math.Floor(calculator.KaliYear)));
        return Task.FromResult(result);
    }

    private static void PopulateSummary(AstroResultModel result, AstroInputModel input, SuryaSiddhantaCalculator c, MyanmarCalendarResult calendar)
    {
        var date = new DateTime(input.Year, input.Month, input.Day);
        result.SummaryTable.AddRange([
            new() { Label = "ကလိယုဂ်", Value = Myanmar(calendar.KaliYear) },
            new() { Label = calendar.EraLabel, Value = Myanmar(calendar.MyanmarEraYear) },
            new() { Label = "ခရစ်နှစ်", Value = Myanmar(input.Year) },
            new() { Label = "မြန်မာလ", Value = $"{calendar.Month} {calendar.Phase} {Myanmar(calendar.LunarDay)} ရက် ({calendar.CalendarType})" },
            new() { Label = "နေ့", Value = calendar.Weekday },
            new() { Label = "အချိန်", Value = $"{Myanmar(input.Hour)}:{Myanmar(input.Minute).PadLeft(2, '၀')}" },
            new() { Label = "နေရာ", Value = input.LocationName },
            new() { Label = "အယန", Value = Dms(c.Ayanamsa) },
            new() { Label = "လဂ်", Value = Position(c.Ascendant) },
            new() { Label = "တိထီ", Value = $"{(c.IsWaningMoon ? "လဆုတ်" : "လဆန်း")} {Myanmar(c.LunarDay)} ရက်" },
            new() { Label = "နက္ခတ်", Value = Myanmar(c.Nakshatra + 1) },
            new() { Label = "နေထွက်", Value = Clock(c.SunriseTimeInHours) },
            new() { Label = "နေဝင်", Value = Clock(c.SunsetTimeInHours) },
            new() { Label = "တွက်နည်း", Value = input.IsLocalMeanTime ? "ဒေသပျမ်းမျှအချိန်" : $"မြို့စံတော်ချိန် ({UtcOffset(input.StandardTimeOffset ?? 6.5d)})" }
        ]);
    }

    private static void PopulateCharts(AstroResultModel result, SuryaSiddhantaCalculator c)
    {
        for (var i = 0; i < Marks.Length; i++)
        {
            Add(result.RasiHouses, c.RasiSigns[i], Marks[i]);
            Add(result.NavamsaHouses, c.NavamsaSigns[i], Marks[i]);
            Add(result.DrekkanaHouses, c.DrekkanaSigns[i], Marks[i]);
        }
        var sign = (int)Math.Floor(c.Ascendant / 30d);
        Add(result.RasiHouses, sign, "လဂ်");
        var part = (int)Math.Floor((c.Ascendant % 30d) / (10d / 3d));
        var start = sign % 3 switch { 0 => 0, 1 => 9, _ => 6 };
        Add(result.NavamsaHouses, (start + part) % 12, "လဂ်");
        Add(result.DrekkanaHouses, (sign + 4 * (int)Math.Floor((c.Ascendant % 30d) / 10d)) % 12, "လဂ်");
    }

    private static void PopulatePlanets(AstroResultModel result, SuryaSiddhantaCalculator c)
    {
        for (var i = 0; i < Planets.Length; i++) result.DetailedPlanets.Add(new()
        {
            Planet = Planets[i], Position = Position(c.Planets[i]), Navamsa = Signs[c.NavamsaSigns[i]], Status = Dignity(i, c.RasiSigns[i])
        });
    }

    private static void PopulateHouseCusps(AstroResultModel result, SuryaSiddhantaCalculator c)
    {
        for (var i = 0; i < 12; i++) result.HouseCusps.Add(new()
        {
            House = i + 1,
            Cusp = Position(c.HouseCusps[i]),
            Boundary = Position(c.HouseBoundaries[i])
        });
    }

    private static void PopulateTraditionalIndicators(AstroResultModel result, AstroInputModel input, SuryaSiddhantaCalculator c)
    {
        var ascendantNakshatra = NakshatraParts(c.Ascendant);
        var lunarNakshatra = NakshatraParts(c.Planets[1]);
        var solarNakshatra = NakshatraParts(Normalize(c.Planets[0] + (input.Hour * 60d + input.Minute) / 4d));
        var dini = TraditionalTime(input.Hour + input.Minute / 60d, c.DayArc, c.NightArc);
        var ascendantSign = (int)Math.Floor(c.Ascendant / 30d);
        var ascendantDegree = c.Ascendant % 30d;
        var hawratFirstGroup = (ascendantSign % 2 == 0 && ascendantDegree <= 15d) || (ascendantSign % 2 != 0 && ascendantDegree > 15d);
        var tarinSign = ascendantSign == 0 ? 12 : ascendantSign;
        var tarinPart = (int)Math.Floor(ascendantDegree / 10d);
        var tarinIndex = tarinSign + (tarinPart == 0 ? 0 : tarinPart == 1 ? 5 : 9);

        result.TraditionalIndicators.AddRange([
            new() { Label = "လဂ်နက္ခတ်", Value = FormatNakshatra(ascendantNakshatra), Note = "Lsnk — လဂ်ကို ၂၇ နက္ခတ်နှင့် ပဒ ၄ ပိုင်းဖြင့် ဖော်ပြထားသည်။" },
            new() { Label = "လနက္ခတ်", Value = FormatNakshatra(lunarNakshatra), Note = "လ၏ တည်နေရာမှ ရသော နက္ခတ်ဖြစ်ပြီး Dasa အစတွက်ချက်ရာတွင်လည်း အသုံးပြုသည်။" },
            new() { Label = "ဟောရတ်", Value = hawratFirstGroup ? "အုပ်စု ၁" : "အုပ်စု ၂", Note = "Hawrat — လဂ်ရာသီ၏ စုံ/မစုံနှင့် ရာသီအတွင်း ဒီဂရီကို သုံးသော အုပ်စုခွဲခြားမှုဖြစ်သည်။" },
            new() { Label = "တရိန်", Value = Weekdays[(tarinIndex - 1) % 7], Note = "Tarin — လဂ်ရာသီနှင့် ၁၀ ဒီဂရီပိုင်းမှ ထွက်သော ရက်သတ္တပတ်အညွှန်းဖြစ်သည်။" },
            new() { Label = "ဒိနီကာလ", Value = $"အပိုင်း {Myanmar(dini.Segment)} / ၈ — {Myanmar(dini.Nayi)} နာရီ {Myanmar(dini.Pad)} ပဒ် {Myanmar(dini.Viz)} ဗီဇ", Note = "Dini — နေ့/ညကာလအချိုးကို ရိုးရာနည်းဖြင့် ၈ ပိုင်းခွဲဖော်ပြခြင်းဖြစ်သည်။" }
        ]);

        if (input.Hour <= 6 || input.Hour >= 18)
        {
            result.TraditionalIndicators.Add(new() { Label = "အက်စထရိုနက္ခတ်", Value = FormatNakshatra(solarNakshatra), Note = "Astro — ညအချိန်တွင် နေစံနှင့် မွေးအချိန်ကို ပေါင်း၍ ရသော ရိုးရာအညွှန်းဖြစ်သည်။" });
        }
        else
        {
            var pawar = Pawar(input.Latitude, c.Planets[0], c.Ayanamsa);
            result.TraditionalIndicators.Add(new() { Label = "ပဝါး", Value = $"{Myanmar(pawar.Degree)}° {Myanmar(pawar.Minute)}' {pawar.Direction}", Note = "Pawar — နေ၏ မြောက်/တောင်ယိမ်းမှုနှင့် မွေးနေရာလတ္တီကျုအကြား ရိုးရာအချိုးမှ ရသော တန်ဖိုးဖြစ်သည်။" });
        }
    }

    private static (int Index, int Pada, int Degree, int Minute) NakshatraParts(double longitude)
    {
        var raw = Normalize(longitude) / (360d / 27d);
        var index = (int)Math.Floor(raw);
        var padaRaw = (raw - index) * 4d;
        var degreeRaw = (padaRaw - Math.Floor(padaRaw)) * 15d;
        return (index, (int)Math.Floor(padaRaw) + 1, (int)Math.Floor(degreeRaw), (int)Math.Floor((degreeRaw % 1d) * 60d));
    }

    private static string FormatNakshatra((int Index, int Pada, int Degree, int Minute) value) => $"{Nakshatras[value.Index]} — ပဒ {Myanmar(value.Pada)}၊ {Myanmar(value.Degree)}° {Myanmar(value.Minute)}'";

    private static (int Segment, int Nayi, int Pad, int Viz) TraditionalTime(double hours, double dayArc, double nightArc)
    {
        var moment = hours * 2.5d;
        var halfDay = dayArc / 2d;
        var halfNight = nightArc / 2d;
        double[] boundaries = [0d, halfNight, nightArc, nightArc + halfDay, nightArc + 2d * halfDay, nightArc + 3d * halfDay, nightArc + 4d * halfDay, nightArc + 4d * halfDay + halfNight, 60d];
        var segment = Array.FindIndex(boundaries.Skip(1).ToArray(), boundary => moment < boundary) + 1;
        if (segment == 0) segment = 8;
        var elapsed = moment - boundaries[segment - 1];
        var nayi = (int)Math.Floor(elapsed);
        var padRaw = (elapsed - nayi) * 4d;
        var pad = (int)Math.Floor(padRaw);
        var viz = (int)Math.Floor((padRaw - pad) * 15d);
        return (segment, nayi, pad, viz);
    }

    private static (int Degree, int Minute, string Direction) Pawar(double latitude, double sunLongitude, double ayanamsa)
    {
        const double radians = Math.PI / 180d;
        var a = Math.Sin((sunLongitude + ayanamsa) * radians) * Math.Sin(24d * radians);
        var solarDeclination = Math.Atan(a / (Math.Sqrt(1d - a * a) + 1e-20d)) / radians;
        var value = 7d * Math.Tan((latitude - solarDeclination) * radians);
        var absolute = Math.Abs(value);
        var degree = (int)Math.Floor(absolute);
        var minute = (int)Math.Floor((absolute - degree) * 60d);
        return (degree, minute, value >= 0d ? "မြောက်" : "တောင်");
    }

    private static string Dignity(int planet, int sign) => planet switch
    {
        0 when sign == 0 => "ဥစ္", 0 when sign == 6 => "နိစ်", 1 when sign == 1 => "ဥစ္", 1 when sign == 7 => "နိစ်",
        2 when sign == 9 => "ဥစ္", 2 when sign == 3 => "နိစ်", 3 when sign == 5 => "ဥစ္", 3 when sign == 11 => "နိစ်",
        4 when sign == 3 => "ဥစ္", 4 when sign == 9 => "နိစ်", 5 when sign == 11 => "ဥစ္", 5 when sign == 5 => "နိစ်",
        6 when sign == 6 => "ဥစ္", 6 when sign == 0 => "နိစ်", _ => "ပကတိ"
    };
    private static void Add(IList<string> houses, int sign, string value) => houses[sign] = string.IsNullOrWhiteSpace(houses[sign]) ? value : $"{houses[sign]} {value}";
    private static string Position(double longitude) => $"{Signs[(int)Math.Floor(longitude / 30d)]} {Dms(longitude % 30d)}";
    private static string Dms(double value) => $"{Math.Floor(value):0}° {Math.Floor((value % 1d) * 60d):0}'";
    private static string Clock(double hours) => $"{((int)Math.Floor(hours) + 24) % 24:00}:{(int)Math.Floor((hours % 1d) * 60d):00}";
    private static double Normalize(double value) => ((value % 360d) + 360d) % 360d;
    private static string Myanmar(int value) => string.Concat(value.ToString(CultureInfo.InvariantCulture).Select(c => Digits[c - '0']));
    private static string UtcOffset(double value) => $"UTC{(value >= 0d ? "+" : "-")}{Math.Abs(value):0.##}";
    private static readonly string[] Nakshatras = ["အဿဝဏီ", "ဘရဏီ", "ကတ္တိကာ", "ရောဟိဏီ", "မိဂသီ", "အာဒြ", "ပုန္နဖုသျှ", "ပုသျှ", "အာသလေသ", "မဃာ", "ပုဗ္ဗဖလ္ဂုနီ", "ဥတ္တရဖလ္ဂုနီ", "ဟဿတ", "စိတ္တ", "သွာတိ", "ဝိသာခါ", "အနုရာဓာ", "ဇေဋ္ဌ", "မူလ", "ပုဗ္ဗအာသဠီ", "ဥတ္တရအာသဠီ", "သရဝဏ", "ဓနိဋ္ဌ", "သတဘိသာ", "ပုဗ္ဗဘဒြပိုဒ်", "ဥတ္တရဘဒြပိုဒ်", "ရေဝတီ"];
}
