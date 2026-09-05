namespace BlazorMyanmarAstroHoro.Services;

/// <summary>Myanmar calendar path ported from CleanVB6/KDATA.frm: Cld, MCld, Nocld, Wagyi, Wangal and Nowa.</summary>
public static class MyanmarCalendarCalculator
{
    private const double HaragoneBase = 1_577_917_828d;
    private const double KaliCycle = 4_320_000d;
    private const double IntercalaryFactor = 25_082_252d;

    private static readonly HashSet<int> LateDayCorrectionYears = [1273, 1278, 1283, 1308, 1309, 1313, 1314, 1316, 1317, 1318, 1319, 1324, 1327, 1329, 1330, 1332, 1333, 1335, 1340, 1343, 1344, 1345, 1348, 1349, 1350, 1354, 1355, 1359, 1360, 1364, 1365, 1366, 1370];
    private static readonly HashSet<int> ExtraMonthYears = [1272, 1277, 1280, 1288, 1291, 1296, 1299, 1307, 1310, 1315, 1326, 1353];
    private static readonly HashSet<int> WagyiYears = [1263, 1269, 1272, 1277, 1282, 1288, 1293, 1299, 1304, 1307, 1312, 1315, 1323, 1326, 1331, 1339, 1342, 1347, 1353, 1358, 1363, 1369, 1372, 1377];
    private static readonly HashSet<int> WangalYears = [1266, 1274, 1280, 1285, 1291, 1296, 1301, 1310, 1317, 1320, 1328, 1334, 1336, 1344, 1350, 1355, 1361, 1366, 1374, 1380];
    private static readonly HashSet<int> NowaYears = [1264, 1265, 1267, 1268, 1270, 1271, 1273, 1275, 1276, 1278, 1279, 1281, 1283, 1284, 1286, 1287, 1289, 1290, 1292, 1294, 1295, 1297, 1298, 1300, 1302, 1303, 1305, 1306, 1308, 1309, 1311, 1313, 1314, 1316, 1318, 1319, 1321, 1322, 1324, 1325, 1327, 1329, 1330, 1332, 1333, 1335, 1337, 1338, 1340, 1341, 1343, 1345, 1346, 1348, 1349, 1351, 1352, 1354, 1356, 1357, 1359, 1360, 1362, 1364, 1365, 1367, 1368, 1370, 1371, 1373, 1375, 1376, 1378, 1379];
    private static readonly string[] Weekdays = ["တနင်္ဂနွေ", "တနင်္လာ", "အင်္ဂါ", "ဗုဒ္ဓဟူး", "ကြာသပတေး", "သောကြာ", "စနေ"];

    public static MyanmarCalendarResult Calculate(double twn)
    {
        var kaliYear = (int)Math.Floor(Math.Floor(twn) * KaliCycle / HaragoneBase);
        var sawana = HaragoneBase * kaliYear / KaliCycle;
        var haragone = (long)Math.Floor(sawana);
        var kyamat = HaragoneBase * kaliYear - Math.Floor(sawana) * KaliCycle;
        if (kyamat > 0d) haragone++;
        var tithi = haragone + (long)Math.Floor(haragone * IntercalaryFactor / HaragoneBase);
        var lunarDayBase = (int)(tithi % 30);
        var dayInSolarYear = (int)Math.Floor(twn - kaliYear * HaragoneBase / KaliCycle) + 1;
        var myanmarYear = kaliYear - 3739;

        if (kaliYear > 5001 && kaliYear < 5110)
        {
            lunarDayBase = lunarDayBase == 0 ? 31 : lunarDayBase + 1;
        }
        if (LateDayCorrectionYears.Contains(myanmarYear)) lunarDayBase--;
        lunarDayBase += myanmarYear switch { 1334 => 29, 1269 or 1372 => 31, 1377 or 1380 => 1, _ => 0 };
        if (ExtraMonthYears.Contains(myanmarYear)) lunarDayBase += 30;

        var calendarType = GetCalendarType(kaliYear, myanmarYear);
        var (month, phase, lunarDay) = ResolveMonth(calendarType, lunarDayBase, dayInSolarYear);
        return new MyanmarCalendarResult
        {
            KaliYear = kaliYear,
            MyanmarEraYear = ResolveEraYear(kaliYear),
            EraLabel = ResolveEraLabel(kaliYear),
            CalendarType = calendarType,
            Month = month,
            Phase = phase,
            LunarDay = lunarDay,
            DayInSolarYear = dayInSolarYear,
            Weekday = Weekdays[(int)((haragone + 5) % 7)],
            Tithi = tithi,
            LunarMonthNumber = tithi / 30
        };
    }

    private static string GetCalendarType(int kaliYear, int myanmarYear)
    {
        if (WagyiYears.Contains(myanmarYear)) return "ဝါကြီး";
        if (WangalYears.Contains(myanmarYear)) return "ဝါငယ်";
        if (NowaYears.Contains(myanmarYear)) return "နှစ်ရိုး";
        if (kaliYear <= 5119) return "နှစ်ရိုး";

        var fraction = (kaliYear * (1_593_336d / KaliCycle)) % 1d;
        var leap = fraction < (1_593_336d / KaliCycle) || fraction + (1_593_336d / KaliCycle) >= 1d;
        if (!leap) return "နှစ်ရိုး";
        var cycle = 5.156681962d;
        var yearBucket = Math.Floor(kaliYear / cycle + .0001d);
        return yearBucket == Math.Floor((kaliYear - 1) / cycle + .0001d) || yearBucket == Math.Floor((kaliYear + 1) / cycle + .0001d) || yearBucket == Math.Floor((kaliYear - 2) / cycle + .0001d)
            ? "ဝါငယ်" : "ဝါကြီး";
    }

    private static (string Month, string Phase, int Day) ResolveMonth(string calendarType, int lunarDayBase, int dayInSolarYear)
    {
        var (months, lengths) = calendarType switch
        {
            "ဝါကြီး" => (new[] { "တန်ခူး", "ကဆုန်", "နယုန်", "ပထမဝါဆို", "ဒုတိယဝါဆို", "ဝါခေါင်", "တော်သလင်း", "သီတင်းကျွတ်", "တန်ဆောင်မုန်း", "နတ်တော်", "ပြာသို", "တပို့တွဲ", "တပေါင်း", "နှောင်းတန်ခူး", "ကဆုန်" }, new[] { 29, 30, 30, 30, 30, 29, 30, 29, 30, 29, 30, 29, 30, 29, 30 }),
            "ဝါငယ်" => (new[] { "တန်ခူး", "ကဆုန်", "နယုန်", "ဝါဆို", "ဝါခေါင်", "တော်သလင်း", "သီတင်းကျွတ်", "တန်ဆောင်မုန်း", "နတ်တော်", "ပြာသို", "တပို့တွဲ", "တပေါင်း", "နှောင်းတန်ခူး", "ကဆုန်" }, new[] { 29, 30, 29, 30, 30, 29, 30, 29, 30, 29, 30, 29, 30, 29 }),
            _ => (new[] { "တန်ခူး", "ကဆုန်", "နယုန်", "ဝါဆို", "ဝါခေါင်", "တော်သလင်း", "သီတင်းကျွတ်", "တန်ဆောင်မုန်း", "နတ်တော်", "ပြာသို", "တပို့တွဲ", "တပေါင်း", "နှောင်းတန်ခူး", "ကဆုန်" }, new[] { 29, 30, 29, 30, 29, 30, 29, 30, 29, 30, 29, 30, 29, 30 })
        };
        var day = lunarDayBase + dayInSolarYear;
        var span = lengths.Sum();
        day = ((day - 1) % span + span) % span + 1;
        var offset = 0;
        for (var i = 0; i < lengths.Length; i++)
        {
            if (day <= offset + lengths[i])
            {
                var dayOfMonth = day - offset;
                return (months[i], dayOfMonth <= 15 ? "လဆန်း" : "လဆုတ်", dayOfMonth <= 15 ? dayOfMonth : dayOfMonth - 15);
            }
            offset += lengths[i];
        }
        throw new InvalidOperationException("Myanmar calendar month resolution failed.");
    }

    private static int ResolveEraYear(int kaliYear) => kaliYear switch { < 2410 => kaliYear + 6236, < 2558 => kaliYear - 2409, < 3180 => kaliYear - 2557, < 3740 => kaliYear - 3179, _ => kaliYear - 3739 };
    private static string ResolveEraLabel(int kaliYear) => kaliYear < 3740 ? "သမိုင်းခေတ် သက္ကရာဇ်" : "မြန်မာကောဇာ သက္ကရာဇ်";
}
