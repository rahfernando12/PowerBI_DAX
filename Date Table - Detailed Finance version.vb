-- this date table is primarily for finance teams 

Dim_Date = 
VAR StartYear = 2020
VAR EndYear   = 2030
VAR TodayDate        = TODAY()
VAR TodayMonthStart  = DATE ( YEAR ( TodayDate ), MONTH ( TodayDate ), 1 )
VAR TodayQuarterStart =
    VAR TM = MONTH ( TodayDate )
    VAR TQStartMonth = ( INT ( ( TM - 1 ) / 3 ) * 3 ) + 1
    RETURN DATE ( YEAR ( TodayDate ), TQStartMonth, 1 )
VAR TodayYearStart   = DATE ( YEAR ( TodayDate ), 1, 1 )
-- Set the first month of your fiscal year (e.g. 1 = Jan, 4 = Apr, 7 = Jul)
VAR FiscalYearStartMonth = 4
RETURN
ADDCOLUMNS (
    CALENDAR ( DATE ( StartYear, 1, 1 ), DATE ( EndYear, 12, 31 ) ),

    -- Basic keys & labels
    "DateKey",        INT ( FORMAT ( [Date], "YYYYMMDD" ) ),
    "Year",           YEAR ( [Date] ),
    "Month Number",   MONTH ( [Date] ),
    "Month Name",     FORMAT ( [Date], "MMMM" ),
    "Month Short",    FORMAT ( [Date], "MMM" ),
    "Day",            DAY ( [Date] ),
    "Day of Week",    WEEKDAY ( [Date], 2 ),         -- 1 = Monday, 7 = Sunday
    "Day Name",       FORMAT ( [Date], "dddd" ),
    "Is Weekday",     IF ( WEEKDAY ( [Date], 2 ) <= 5, TRUE (), FALSE () ),

    -- Calendar period groupings
    "Quarter Number", INT ( ( MONTH ( [Date] ) - 1 ) / 3 ) + 1,
    "Quarter",        "Q" & INT ( ( MONTH ( [Date] ) - 1 ) / 3 ) + 1,
    "Year-Quarter",   FORMAT ( [Date], "YYYY" ) & "-Q" &
                      INT ( ( MONTH ( [Date] ) - 1 ) / 3 ) + 1,
    "Year-Month",     FORMAT ( [Date], "YYYY-MM" ),
    "Year-Month Short", FORMAT ( [Date], "YYYY-" ) & FORMAT ( [Date], "MMM" ),

    -- Calendar period boundaries & flags
    "Start of Month",   EOMONTH ( [Date], -1 ) + 1,
    "End of Month",     EOMONTH ( [Date], 0 ),
    "Is Month Start",   IF ( [Date] = EOMONTH ( [Date], -1 ) + 1, TRUE (), FALSE () ),
    "Is Month End",     IF ( [Date] = EOMONTH ( [Date], 0 ), TRUE (), FALSE () ),

    "Start of Quarter",
        VAR MonthNoQ = MONTH ( [Date] )
        VAR QuarterStartMonth = ( INT ( ( MonthNoQ - 1 ) / 3 ) * 3 ) + 1
        RETURN DATE ( YEAR ( [Date] ), QuarterStartMonth, 1 ),

    "End of Quarter",
        VAR MonthNoQ2 = MONTH ( [Date] )
        VAR QuarterEndMonth   = ( INT ( ( MonthNoQ2 - 1 ) / 3 ) * 3 ) + 3
        RETURN EOMONTH ( DATE ( YEAR ( [Date] ), QuarterEndMonth, 1 ), 0 ),

    "Start of Year", DATE ( YEAR ( [Date] ), 1, 1 ),
    "End of Year",   DATE ( YEAR ( [Date] ), 12, 31 ),
    "Is Year Start", IF ( [Date] = DATE ( YEAR ( [Date] ), 1, 1 ), TRUE (), FALSE () ),
    "Is Year End",   IF ( [Date] = DATE ( YEAR ( [Date] ), 12, 31 ), TRUE (), FALSE () ),

    -- Week-related (simple calendar week)
    "Week of Year", WEEKNUM ( [Date], 2 ),

    -- Fiscal calendar (based on FiscalYearStartMonth)
    "Fiscal Year",
        VAR ShiftForFY = 12 - FiscalYearStartMonth + 1
        VAR ShiftedFYDate = EDATE ( [Date], ShiftForFY )
        RETURN YEAR ( ShiftedFYDate ),

    "Fiscal Period",
        VAR ShiftBack = FiscalYearStartMonth - 1
        VAR ShiftedFPDate = EDATE ( [Date], -ShiftBack )
        RETURN MONTH ( ShiftedFPDate ),

    "Fiscal Quarter",
        VAR ShiftBackQ = FiscalYearStartMonth - 1
        VAR ShiftedFQDate = EDATE ( [Date], -ShiftBackQ )
        VAR FiscalMonthNo = MONTH ( ShiftedFQDate )
        RETURN INT ( ( FiscalMonthNo - 1 ) / 3 ) + 1,

    "Fiscal Year-Period",
        VAR FY =
            VAR ShiftForFY2 = 12 - FiscalYearStartMonth + 1
            VAR ShiftedFYDate2 = EDATE ( [Date], ShiftForFY2 )
            RETURN YEAR ( ShiftedFYDate2 )
        VAR FP =
            VAR ShiftBack2 = FiscalYearStartMonth - 1
            VAR ShiftedFPDate2 = EDATE ( [Date], -ShiftBack2 )
            RETURN MONTH ( ShiftedFPDate2 )
        RETURN FORMAT ( FY, "0000" ) & "-" & FORMAT ( FP, "00" ),

    "Fiscal Year-Quarter",
        VAR FY3 =
            VAR ShiftForFY3 = 12 - FiscalYearStartMonth + 1
            VAR ShiftedFYDate3 = EDATE ( [Date], ShiftForFY3 )
            RETURN YEAR ( ShiftedFYDate3 )
        VAR FQ3 =
            VAR ShiftBack3 = FiscalYearStartMonth - 1
            VAR ShiftedFQDate3 = EDATE ( [Date], -ShiftBack3 )
            VAR FiscalMonthNo3 = MONTH ( ShiftedFQDate3 )
            RETURN INT ( ( FiscalMonthNo3 - 1 ) / 3 ) + 1
        RETURN "FY" & FORMAT ( FY3, "0000" ) & " Q" & FQ3,

-- OFFSET COLUMNS (relative to today)
    "Date Offset", INT ( [Date] - TodayDate ),
    -- Month offset (using start of month): 0 = current month, -1 = prior month, etc.
    "Month Offset",
        VAR CurrMonthStart = EOMONTH ( [Date], -1 ) + 1
        RETURN DATEDIFF ( CurrMonthStart, TodayMonthStart, MONTH ),
    -- Quarter offset (using start of quarter)
    "Quarter Offset",
        VAR M  = MONTH ( [Date] )
        VAR QStartMonth = ( INT ( ( M - 1 ) / 3 ) * 3 ) + 1
        VAR CurrQuarterStart = DATE ( YEAR ( [Date] ), QStartMonth, 1 )
        RETURN DATEDIFF ( CurrQuarterStart, TodayQuarterStart, QUARTER ),
    -- Year offset (using start of year)
    "Year Offset",
        VAR CurrYearStart = DATE ( YEAR ( [Date] ), 1, 1 )
        RETURN DATEDIFF ( CurrYearStart, TodayYearStart, YEAR ),

---------------------------------------------------
-- SORT COLUMNS (numeric, for "Sort by column")
---------------------------------------------------
    -- Month sort (for Month Name / Month Short)
    "Month Sort", MONTH ( [Date] ),

    -- Day-of-week sort (for Day Name)
    "Day of Week Sort", WEEKDAY ( [Date], 2 ),    -- 1 = Monday

    -- Year-Month sort (for Year-Month text labels)
    "Year-Month Sort",
        YEAR ( [Date] ) * 100 +
        MONTH ( [Date] ),

    -- Year-Quarter sort (for Year-Quarter text labels)
    "Year-Quarter Sort",
        YEAR ( [Date] ) * 10 +
        INT ( ( MONTH ( [Date] ) - 1 ) / 3 ) + 1,

    -- Fiscal Year-Period sort (for Fiscal Year-Period labels)
    "Fiscal Year-Period Sort",
        VAR FY_Sort_YP =
            VAR ShiftForFY4 = 12 - FiscalYearStartMonth + 1
            VAR ShiftedFYDate4 = EDATE ( [Date], ShiftForFY4 )
            RETURN YEAR ( ShiftedFYDate4 )
        VAR FP_Sort_YP =
            VAR ShiftBack4 = FiscalYearStartMonth - 1
            VAR ShiftedFPDate4 = EDATE ( [Date], -ShiftBack4 )
            RETURN MONTH ( ShiftedFPDate4 )
        RETURN FY_Sort_YP * 100 + FP_Sort_YP,

    -- Fiscal Year-Quarter sort (for Fiscal Year-Quarter labels)
    "Fiscal Year-Quarter Sort",
        VAR FY_Sort_FQ =
            VAR ShiftForFY5 = 12 - FiscalYearStartMonth + 1
            VAR ShiftedFYDate5 = EDATE ( [Date], ShiftForFY5 )
            RETURN YEAR ( ShiftedFYDate5 )
        VAR FQ_Sort_FQ =
            VAR ShiftBack5 = FiscalYearStartMonth - 1
            VAR ShiftedFQDate5 = EDATE ( [Date], -ShiftBack5 )
            VAR FiscalMonthNo5 = MONTH ( ShiftedFQDate5 )
            RETURN INT ( ( FiscalMonthNo5 - 1 ) / 3 ) + 1
        RETURN FY_Sort_FQ * 10 + FQ_Sort_FQ
)

