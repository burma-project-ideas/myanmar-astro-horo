# VB6 → Blazor Migration Checklist

Reference: `myanmar_astro_horo/src/CleanVB6/MAH_SS-FOSS.vbp`.

## Core calculations

- [x] Input validation, standard time, and local mean time
- [x] Julian day, Kali year, ayanamsa, true planetary positions, nodes, Lagna, tithi, and nakshatra (`KDATA.frm`, `Kdata1.bas`)
- [x] Rasi, Navamsa, and Drekkana result charts
- [x] Dasa sequence and current sub-period (`Kdata2.bas:Dasa`)
- [x] 21-year Suria Siddhanta BO calendar table (`Kdata3.bas:Kp2`)
- [x] House/cusp construction: `h(1..12)` and `c(1..12)` (`KDATA.frm`); displayed as Bhava results
- [x] `Lsnk`, `Hawrat`, `Tarin`, `Dini`, `Astro`, and `Pawar` results, presented in Burmese (`Kdata1.bas`)
- [ ] Exact legacy latitude/declination label parity (`Kdata1.bas`)

## Myanmar calendar

- [x] Kali year, Myanmar Era, tithi, weekday, and base lunar-day values
- [x] Era-specific calendar corrections: `MCld`, `Wagyi`, `Wangal`, and `Nowa` (`KDATA.frm`); civil-date validation replaces `Ecld` and `Leapyear`
- [ ] Exact Myanmar month and intercalary-month presentation verified against the VB6 executable

## Application features

- [x] Modern input and result UI, with Burmese result guidance
- [x] Complete city directory (`Kdata4.bas`–`Kdata7.bas`): 1,152 legacy city records with coordinates and standard time
- [ ] Seconds, gender, and the remaining legacy input fields
- [x] Save/open/delete horoscope records in browser-local storage (replaces legacy `.db` text files)
- [x] Print-friendly result, including browser “Save as PDF”
- [ ] Export to a structured file format
- [ ] Visual parity for the complete legacy horoscope and BO screens

## Verification

- [x] Debug and Release builds
- [ ] Golden test cases: compare specified legacy input/output samples with the VB6 executable
- [x] Browser interaction: calculate, city directory, full result rendering, and save/delete flow
- [ ] Responsive-layout tests
