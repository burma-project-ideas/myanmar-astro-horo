namespace BlazorMyanmarAstroHoro.Services;

/// <summary>21-year Suria Siddhanta BO table ported from CleanVB6/Kdata3.bas:Kp2.</summary>
public static class BoCalculator
{
    private const double HaragoneBase = 1_577_917_828d;
    private const double KaliCycle = 4_320_000d;
    private const double IntercalaryFactor = 25_082_252d;
    private const double AdimatFactor = 1_593_336d;
    private const double YetnginFactor = 837_748d;

    public static List<BoCalendarRow> Calculate(int kaliYear, int rows = 21)
    {
        var result = new List<BoCalendarRow>(rows);
        for (var offset = 0; offset < rows; offset++)
        {
            var kali = kaliYear + offset;
            var sawana = HaragoneBase * kali / KaliCycle;
            var haragone = (long)Math.Floor(sawana);
            var kyamat = HaragoneBase * kali - haragone * KaliCycle;
            if (kyamat > 0d) haragone++;
            var tithi = haragone + (long)Math.Floor(haragone * IntercalaryFactor / HaragoneBase);
            var lunarMonth = tithi / 30;
            var lunarDay = tithi % 30;
            result.Add(new BoCalendarRow
            {
                KaliYear = kali,
                Sawana = (long)Math.Floor(sawana),
                Kyamat = (long)Math.Floor(kyamat),
                Tithi = tithi,
                LunarMonth = lunarMonth,
                LunarDay = lunarDay,
                Weekday = (int)((haragone + 5) % 7),
                Adimat = (long)Math.Floor(kali * AdimatFactor / KaliCycle),
                Yetngin = (long)Math.Floor(kali * YetnginFactor / KaliCycle)
            });
        }
        return result;
    }
}
