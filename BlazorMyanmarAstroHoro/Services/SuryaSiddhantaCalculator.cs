namespace BlazorMyanmarAstroHoro.Services;

/// <summary>Port of the Makaranta/Surya Siddhanta calculation flow in the reference CleanVB6 KDATA.frm.</summary>
public sealed class SuryaSiddhantaCalculator
{
    private const double Rad = Math.PI / 180d;
    private const double KaliYugaDays = 1_577_917_828d;

    public double JulianDay { get; private set; }
    public double Twn { get; private set; }
    public double KaliYear { get; private set; }
    public double Ayanamsa { get; private set; }
    public double LocalSiderealTime { get; private set; }
    public double Ascendant { get; private set; }
    public double SunriseTimeInHours { get; private set; }
    public double SunsetTimeInHours { get; private set; }
    public double DayArc { get; private set; }
    public double NightArc { get; private set; }
    public int LunarDay { get; private set; }
    public bool IsWaningMoon { get; private set; }
    public int Nakshatra { get; private set; }
    // Sun, Moon, Mars, Mercury, Jupiter, Venus, Saturn, Rahu, Ketu.
    public double[] Planets { get; } = new double[9];
    public int[] RasiSigns { get; } = new int[9];
    public int[] NavamsaSigns { get; } = new int[9];
    public int[] DrekkanaSigns { get; } = new int[9];
    // The twelve bhava midpoints and their boundaries, matching KDATA.frm's h(1..12) and c(1..12).
    public double[] HouseCusps { get; } = new double[12];
    public double[] HouseBoundaries { get; } = new double[12];

    public void Calculate(int year, int month, int day, int hour, int minute, int second, double longitude, double latitude, double timezoneOffset)
    {
        _ = new DateTime(year, month, day, hour, minute, second);
        if (latitude is < -89.9d or > 89.9d) throw new ArgumentOutOfRangeException(nameof(latitude));

        CalculateJulianDay(year, month, day);
        CalculateEpochDays(hour, minute, second, longitude, timezoneOffset);
        CalculatePlanetaryPositions();
        CalculateAyanamsaAndAscendant(latitude);
        CalculateHouseCusps(latitude);
        CalculateSunriseSunset(latitude);
        CalculateLunarValues();
        CalculateChartSigns();
    }

    private void CalculateJulianDay(int year, int month, int day)
    {
        var y = (double)year;
        var m = (double)month;
        if (m < 3) { m += 12; y--; }
        var b = 0d;
        if (year > 1582 || (year == 1582 && (month > 10 || (month == 10 && day >= 15))))
        {
            var century = Math.Floor(y / 100d);
            b = 2d - century + Math.Floor(century / 4d);
        }
        JulianDay = b + Math.Floor(365.25d * y) - 694_025d + Math.Floor(30.6001d * (m + 1d)) + day - .5d + 2_415_020d;
    }

    private void CalculateEpochDays(int hour, int minute, int second, double longitude, double timezoneOffset)
    {
        var time = hour + minute / 60d + second / 3600d;
        var yy = JulianDay - 2_444_240.5d + (time - timezoneOffset) / 24d + timezoneOffset / 24d;
        var degrees = 100.80247d + 360d * (yy / 365.25d) + .000021165061d * (1d + yy) + longitude + 9.97d / 3600d + (time - timezoneOffset) * 15d;
        LocalSiderealTime = Normalize(degrees / 15d, 24d);
        Twn = Math.Floor(JulianDay) - 588_465d + time / 24d;
    }

    private void CalculatePlanetaryPositions()
    {
        var b = new[] { 4_320_000d, 57_753_336d, 2_296_832d, 17_937_060d, 364_220d, 7_022_376d, 146_568d, 232_238d, 488_203d };
        var ap = new[] { 387d, 0d, 204d, 368d, 900d, 535d, 39d };
        KaliYear = Twn * 4_320_000d / KaliYugaDays;
        var pm = b.Select(x => Fraction(x * Twn / KaliYugaDays) * 360d).ToArray();
        var apm = ap.Select(x => Fraction(x * (1_955_880_000d + KaliYear) / 4_320_000_000d) * 360d).ToArray();
        var correction = new[] { -.0582d, -1.020865741d, .966884259d, -4.251194444d, -3.00200463d, -6.100291667d, 5.005763889d, 184.11025d, 85.66124537d };
        for (var i = 0; i < pm.Length; i++) pm[i] = Normalize(pm[i] + correction[i]);
        apm[2] += 1; apm[3] += 13; apm[4] -= 1; apm[5] -= 2; apm[6] += 12;
        for (var i = 0; i < apm.Length; i++) if (apm[i] < pm[i]) apm[i] += 360d;

        Planets[0] = Math.Sin((apm[0] - pm[0]) * Rad) * 2.183333333d + pm[0];
        Planets[1] = Math.Sin((pm[8] - pm[1]) * Rad) * 5.05d + pm[1];
        Planets[2] = Outer(pm[2], pm[0], apm[2], .64444444d, .2d);
        Planets[3] = Inner(pm[3], pm[0], apm[3], .3666666666d, .0777777777d);
        Planets[4] = Outer(pm[4], pm[0], apm[4], .2d, .0888888888d);
        Planets[5] = Inner(pm[5], pm[0], apm[5], .7222222222d, .030555555d);
        Planets[6] = Outer(pm[6], pm[0], apm[6], .11111111111d, .13333333333d);
        Planets[7] = 360d - pm[7];
        Planets[8] = Planets[7] + 180d;
        for (var i = 0; i < Planets.Length; i++) Planets[i] = Normalize(Planets[i]);
    }

    // These are direct translations of the three epicycle passes used by the VB6 Mars/Jupiter/Saturn and Mercury/Venus branches.
    private static double Outer(double planet, double sun, double apogee, double eccentricity, double epicycle)
    {
        var first = AtanCorrection(sun - planet, eccentricity) / 2d + planet;
        var second = Equation(apogee - first, epicycle) / 2d + first;
        var third = Equation(apogee - second, epicycle) + planet;
        return AtanCorrection(sun - third, eccentricity) + third;
    }

    private static double Inner(double planet, double sun, double apogee, double eccentricity, double epicycle)
    {
        var first = AtanCorrection(planet - sun, eccentricity) / 2d + sun;
        var second = Equation(apogee - first, epicycle) / 2d + first;
        var third = Equation(apogee - second, epicycle) + sun;
        return AtanCorrection(planet - third, eccentricity) + third;
    }

    private static double AtanCorrection(double angle, double factor) => Math.Atan(factor * Math.Sin(angle * Rad) / (1d + factor * Math.Cos(angle * Rad))) / Rad;
    private static double Equation(double angle, double factor)
    {
        var value = Math.Sin(angle * Rad) * factor;
        return Math.Sqrt(1d + value * value) * Math.Atan(value) / Rad;
    }

    private void CalculateAyanamsaAndAscendant(double latitude)
    {
        var presg = Fraction(Math.Floor((KaliYear + 88d) / 1800d) / 4d) * 4d;
        Ayanamsa = Normalize(KaliYear + 88d, 1800d) * .015d;
        if (presg is 1d or 3d) Ayanamsa = 27d - Ayanamsa + .015d;
        // VB6 truncates local sidereal time to whole seconds before applying the ascendant formula.
        var lst = Math.Floor(LocalSiderealTime) + Math.Floor((LocalSiderealTime % 1d) * 60d) / 60d;
        var degrees = lst * 15d;
        var quadrant = (int)Math.Floor(lst / 6d);
        var adjustment = quadrant is 1 or 2 ? 180d : quadrant == 3 ? 360d : 0d;
        Ascendant = Normalize(Math.Atan(Math.Sin(23.45d * Rad) * Math.Tan(latitude * Rad) / Math.Cos(degrees * Rad) + Math.Cos(23.45d * Rad) * Math.Tan(degrees * Rad)) / Rad + 90d + adjustment - Ayanamsa);
    }

    private void CalculateSunriseSunset(double latitude)
    {
        var declination = Math.Asin(Math.Sin((Planets[0] + Ayanamsa) * Rad) * Math.Sin(24d * Rad)) / Rad;
        var ratio = Math.Clamp(Math.Tan(latitude * Rad) * Math.Sin(declination * Rad) / Math.Cos(declination * Rad), -1d, 1d);
        DayArc = Math.Asin(ratio) / Rad / 6d + 15d;
        NightArc = 30d - DayArc;
        SunriseTimeInHours = NightArc / 2.5d;
        // In Dini(), dayArc is half of the daytime duration: two equal halves span sunrise to sunset.
        SunsetTimeInHours = SunriseTimeInHours + 2d * DayArc / 2.5d;
    }

    private void CalculateHouseCusps(double latitude)
    {
        // Direct port of the h1..h12 / c1..c12 construction in KDATA.frm.
        var lst = Math.Floor(LocalSiderealTime) + Math.Floor((LocalSiderealTime % 1d) * 60d) / 60d;
        var degrees = lst * 15d;
        var quadrant = (int)Math.Floor(lst / 6d);
        var adjustment = quadrant is 1 or 2 ? 180d : quadrant == 3 ? 360d : 0d;
        var h1 = Math.Atan(Math.Sin(23.45d * Rad) * Math.Tan(latitude * Rad) / Math.Cos(degrees * Rad) + Math.Cos(23.45d * Rad) * Math.Tan(degrees * Rad)) / Rad + 90d + adjustment - Ayanamsa;
        var h10 = Math.Atan(Math.Tan(degrees * Rad) / Math.Cos(23.45d * Rad)) / Rad + adjustment - Ayanamsa;
        var h4 = h10 + 180d;
        var firstArc = (h4 - h1) / 6d;
        var secondArc = 30d - firstArc;

        var c1 = h1 + firstArc;
        var h2 = c1 + firstArc;
        var c2 = h2 + firstArc;
        var h3 = c2 + firstArc;
        var c3 = h3 + firstArc;
        var c4 = h4 + secondArc;
        var h5 = c4 + secondArc;
        var c5 = h5 + secondArc;
        var h6 = c5 + secondArc;
        var c6 = h6 + secondArc;

        double[] cusps = [h1, h2, h3, h4, h5, h6, h1 + 180d, h2 + 180d, h3 + 180d, h10, h5 + 180d, h6 + 180d];
        double[] boundaries = [c1, c2, c3, c4, c5, c6, c1 + 180d, c2 + 180d, c3 + 180d, c4 + 180d, c5 + 180d, c6 + 180d];
        for (var i = 0; i < 12; i++)
        {
            HouseCusps[i] = Normalize(cusps[i]);
            HouseBoundaries[i] = Normalize(boundaries[i]);
        }
    }

    private void CalculateLunarValues()
    {
        var raw = (int)Math.Floor(Normalize(Planets[1] - Planets[0]) / 12d);
        IsWaningMoon = raw >= 15;
        LunarDay = raw % 15 + 1;
        Nakshatra = (int)Math.Floor(Planets[1] / (360d / 27d));
    }

    private void CalculateChartSigns()
    {
        for (var i = 0; i < Planets.Length; i++)
        {
            var sign = (int)Math.Floor(Planets[i] / 30d);
            RasiSigns[i] = sign;
            var portion = (int)Math.Floor((Planets[i] % 30d) / (10d / 3d));
            var start = sign % 3 switch { 0 => 0, 1 => 9, _ => 6 };
            NavamsaSigns[i] = (start + portion) % 12;
            DrekkanaSigns[i] = (sign + 4 * (int)Math.Floor((Planets[i] % 30d) / 10d)) % 12;
        }
    }

    private static double Fraction(double value) => value - Math.Floor(value);
    private static double Normalize(double value, double modulo = 360d) => ((value % modulo) + modulo) % modulo;
}
