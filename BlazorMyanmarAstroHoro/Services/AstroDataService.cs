namespace BlazorMyanmarAstroHoro.Services;

public class AstroInputModel
{
    public int Year { get; set; } = DateTime.Now.Year;
    public int Month { get; set; } = DateTime.Now.Month;
    public int Day { get; set; } = DateTime.Now.Day;
    public int Hour { get; set; } = DateTime.Now.Hour;
    public int Minute { get; set; } = DateTime.Now.Minute;
    public double Latitude { get; set; } = 16.8661; // Yangon default
    public double Longitude { get; set; } = 96.1951; // Yangon default
    public string LocationName { get; set; } = "Yangon";
}

public class AstroResultModel
{
    // Myanmar Calendar
    public string MyanmarYear { get; set; } = "";
    public string MyanmarMonth { get; set; } = "";
    public string MyanmarDay { get; set; } = "";
    public string MoonPhase { get; set; } = "";
    
    // Astronomical
    public string Thithi { get; set; } = "";
    public string Nakshatra { get; set; } = "";
    public string Yoga { get; set; } = "";
    public string Karana { get; set; } = "";
    
    // Planetary
    public string DayOfWeekMyanmar { get; set; } = "";
    public string Sunrise { get; set; } = "";
    public string Sunset { get; set; } = "";
    
    // Zodiac
    public string SunSign { get; set; } = "";
    public string MoonSign { get; set; } = "";
    
    // Planetary Positions
    public Dictionary<string, string> PlanetaryPositions { get; set; } = new();
    
    // Tarin / Hawrat (Fortune)
    public string Tarin { get; set; } = "";
    public string Hawrat { get; set; } = "";
}

public class AstroDataService
{
    private static readonly string[] Nakshatras = new[]
    {
        "Ashwini (အာသွင်နီ)", "Bharani (ဘရဏီ)", "Krittika (ကြတ္တိကာ)",
        "Rohini (ရောဟိဏီ)", "Mrigashirsha (မိဂသီ)", "Ardra (အဒြာ)",
        "Punarvasu (ပုဏ္ဏဗုဆူ)", "Pushya (ပုဿ)", "Ashlesha (အာသလေသ)",
        "Magha (မဃာ)", "Purva Phalguni (ပုဗ္ဗဖာလ္ဂုနီ)", "Uttara Phalguni (ဥတ္တရာဖာလ္ဂုနီ)",
        "Hasta (ဟတ္ထာ)", "Chitra (စိတြာ)", "Swati (သွတီ)",
        "Vishakha (ဝိသာခါ)", "Anuradha (အနုရာဓါ)", "Jyeshtha (ဇေဋ္ဌာ)",
        "Mula (မူလာ)", "Purva Ashadha (ပုဗ္ဗာသာဠ္ဟာ)", "Uttara Ashadha (ဥတ္တရာသာဠ္ဟာ)",
        "Shravana (သရဝဏ)", "Dhanishta (ဓနိဋ္ဌာ)", "Shatabhisha (သတဘိသျ)",
        "Purva Bhadrapada (ပုဗ္ဗဘာဒြပဒ)", "Uttara Bhadrapada (ဥတ္တရာဘာဒြပဒ)",
        "Revati (ရေဝတီ)"
    };

    private static readonly string[] ZodiacSigns = new[]
    {
        "Aries (မိဿ)", "Taurus (ပြိဿ)", "Gemini (မေထုန်)",
        "Cancer (ကရကဋ)", "Leo (သိဟ်)", "Virgo (ကန်)",
        "Libra (တူ)", "Scorpio (ပြိစ္ဆာ)", "Sagittarius (ဓနု)",
        "Capricorn (မကာရ)", "Aquarius (ကုမ္ဘ)", "Pisces (မိန်)"
    };

    private static readonly string[] MyanmarMonths = new[]
    {
        "တန်ခူး", "ကဆုန်", "နယုန်", "ဝါဆို", "ဝါခေါင်",
        "တော်သလင်း", "သီတင်းကျွတ်", "တန်ဆောင်မုန်း", "နတ်တော်",
        "ပြာသို", "တပို့တွဲ", "တပေါင်း"
    };

    private static readonly string[] MyanmarDays = new[]
    {
        "တနင်္ဂနွေ", "တနင်္လာ", "အင်္ဂါ", "ဗုဒ္ဓဟူး", 
        "ကြာသပတေး", "သောကြာ", "စနေ"
    };

    private static readonly string[] Yogas = new[]
    {
        "Vishkambha", "Priti", "Ayushman", "Saubhagya", "Shobhana",
        "Atiganda", "Sukarma", "Dhriti", "Shula", "Ganda",
        "Vriddhi", "Dhruva", "Vyaghata", "Harshana", "Vajra",
        "Siddhi", "Vyatipata", "Variyan", "Parigha", "Shiva",
        "Siddha", "Sadhya", "Shubha", "Shukla", "Brahma",
        "Indra", "Vaidhriti"
    };

    private static readonly string[] Tarins = new[]
    {
        "ယာမာ", "အဇီဝ", "ပူတိ", "ချမ်းသာ", "ဓန", "ယသ", "မိတ္တာ"
    };

    /// <summary>
    /// Mock calculation that returns sample Astro data.
    /// This will be replaced with real Surya Siddhanta calculations later.
    /// </summary>
    public async Task<AstroResultModel> CalculateAstroAsync(AstroInputModel input)
    {
        // Simulate computation delay
        await Task.Delay(1200);

        var inputDate = new DateTime(input.Year, input.Month, input.Day, input.Hour, input.Minute, 0);
        var dayOfWeek = (int)inputDate.DayOfWeek;
        var dayOfYear = inputDate.DayOfYear;
        var random = new Random(input.Year * 1000 + input.Month * 100 + input.Day);

        // Mock Myanmar Year (rough approximation: Gregorian - 638)
        int myanmarYear = input.Year - 638;

        // Mock calculations based on date seed
        int nakshatraIndex = (dayOfYear + input.Hour) % 27;
        int sunSignIndex = (input.Month - 1) % 12;
        int moonSignIndex = (input.Month + dayOfYear / 30) % 12;
        int yogaIndex = (dayOfYear + input.Minute) % 27;
        int myanmarMonthIndex = (input.Month + 8) % 12; // rough offset
        int tarinIndex = dayOfWeek % 7;

        // Calculate mock sunrise/sunset based on latitude
        double sunriseHour = 6.0 + (input.Latitude - 16.0) * 0.02;
        double sunsetHour = 18.0 - (input.Latitude - 16.0) * 0.02;

        var result = new AstroResultModel
        {
            MyanmarYear = $"{myanmarYear} ခုနှစ်",
            MyanmarMonth = MyanmarMonths[myanmarMonthIndex],
            MyanmarDay = $"{(dayOfYear % 30) + 1}",
            MoonPhase = (dayOfYear % 30) <= 15 ? "လဆန်း" : "လပြည့်ကျော်",

            Thithi = $"Thithi {(dayOfYear % 30) + 1}",
            Nakshatra = Nakshatras[nakshatraIndex],
            Yoga = Yogas[yogaIndex],
            Karana = dayOfYear % 2 == 0 ? "Bava (ဗာဝ)" : "Balava (ဗာလဝ)",

            DayOfWeekMyanmar = MyanmarDays[dayOfWeek],
            Sunrise = $"{(int)sunriseHour}:{(int)((sunriseHour % 1) * 60):D2} AM",
            Sunset = $"{(int)sunsetHour - 12}:{(int)((sunsetHour % 1) * 60):D2} PM",

            SunSign = ZodiacSigns[sunSignIndex],
            MoonSign = ZodiacSigns[moonSignIndex],

            PlanetaryPositions = new Dictionary<string, string>
            {
                { "Sun (နေ)", $"{ZodiacSigns[sunSignIndex]} {random.Next(0, 30)}°{random.Next(0, 60)}'" },
                { "Moon (လ)", $"{ZodiacSigns[moonSignIndex]} {random.Next(0, 30)}°{random.Next(0, 60)}'" },
                { "Mars (အင်္ဂါ)", $"{ZodiacSigns[random.Next(0, 12)]} {random.Next(0, 30)}°{random.Next(0, 60)}'" },
                { "Mercury (ဗုဒ္ဓ)", $"{ZodiacSigns[random.Next(0, 12)]} {random.Next(0, 30)}°{random.Next(0, 60)}'" },
                { "Jupiter (ကြာသပတေး)", $"{ZodiacSigns[random.Next(0, 12)]} {random.Next(0, 30)}°{random.Next(0, 60)}'" },
                { "Venus (သောကြာ)", $"{ZodiacSigns[random.Next(0, 12)]} {random.Next(0, 30)}°{random.Next(0, 60)}'" },
                { "Saturn (စနေ)", $"{ZodiacSigns[random.Next(0, 12)]} {random.Next(0, 30)}°{random.Next(0, 60)}'" },
            },

            Tarin = Tarins[tarinIndex],
            Hawrat = random.Next(0, 2) == 0 ? "သုဘ (Auspicious)" : "ပါပ (Inauspicious)"
        };

        return result;
    }
}
