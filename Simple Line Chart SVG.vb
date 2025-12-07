SVG Line chart = 
-- line chart with markers on each points and Max + Min markers 
VAR CardWidth  = 300
VAR CardHeight = 100

-- Padding around chart area
VAR PaddingLeft   = 20
VAR PaddingRight  = 20
VAR PaddingTop    = 20
VAR PaddingBottom = 20

VAR ChartLeft   = PaddingLeft
VAR ChartRight  = CardWidth - PaddingRight
VAR ChartTop    = PaddingTop
VAR ChartBottom = CardHeight - PaddingBottom

VAR LineColor   = "#3182CE"
VAR MinColor    = "#E53E3E"  -- red
VAR MaxColor    = "#38A169"  -- green

-------------------------------------------------
-- Build points table: Date + Measure + Index
-------------------------------------------------
VAR DateTableVisible =
    FILTER (
        VALUES ( Dim_Date[Date] ),
        NOT ISBLANK ( [Total Revenue] )
    )

VAR PointsBase =
    ADDCOLUMNS (
        DateTableVisible,
        "Idx",   RANKX ( DateTableVisible, Dim_Date[Date], , ASC ),
        "Value", [Total Revenue]
    )

VAR PointCount =
    COUNTROWS ( PointsBase )

-- Exit early if no points
VAR HasNoData = PointCount = 0

VAR PointsWithX =
    ADDCOLUMNS (
        PointsBase,
        "XCoord",
            VAR idx = [Idx]
            RETURN
                ChartLeft
                    + ( ChartRight - ChartLeft )
                        * ( idx - 1 )
                        / MAX ( 1, PointCount - 1 )
    )

VAR MinVal =
    MINX ( PointsWithX, [Value] )

VAR MaxVal =
    MAXX ( PointsWithX, [Value] )

VAR PointsFinal =
    ADDCOLUMNS (
        PointsWithX,
        "YCoord",
            VAR v = [Value]
            RETURN
                IF (
                    MaxVal = MinVal
                        || MaxVal = BLANK ()
                        || MinVal = BLANK (),
                    ( ChartTop + ChartBottom ) / 2.0,
                    ChartBottom
                        - ( ( v - MinVal ) / ( MaxVal - MinVal ) )
                            * ( ChartBottom - ChartTop )
                )
    )

-------------------------------------------------
-- Build SVG path string (M x y L x y ...)
-------------------------------------------------
VAR PathBody =
    IF (
        HasNoData || PointCount < 2,
        BLANK (),
        CONCATENATEX (
            PointsFinal,
            VAR idx = [Idx]
            VAR x   = [XCoord]
            VAR y   = [YCoord]
            RETURN
                IF (
                    idx = 1,
                    "M " & FORMAT ( x, "0.0" ) & " " & FORMAT ( y, "0.0" ),
                    " L " & FORMAT ( x, "0.0" ) & " " & FORMAT ( y, "0.0" )
                ),
            "",
            [Idx],
            ASC
        )
    )

VAR SvgLinePath =
    IF (
        PathBody = BLANK (),
        "",
        "<path d=""" & PathBody
            & """ fill=""none"" stroke=""" & LineColor
            & """ stroke-width=""3"" stroke-linecap=""round"" stroke-linejoin=""round"" />"
    )

-------------------------------------------------
-- Point markers: blue for all, special for min/max
-------------------------------------------------

-- Blue circle for every point
VAR SvgAllPoints =
    IF (
        HasNoData,
        "",
        CONCATENATEX (
            PointsFinal,
            "<circle cx=""" & FORMAT ( [XCoord], "0.0" )
                & """ cy=""" & FORMAT ( [YCoord], "0.0" )
                & """ r=""2.5"" fill=""#3182CE"" stroke=""#3182CE"" stroke-width=""1"" />",
            ""
        )
    )

-- Identify min point
VAR MinPointTable =
    FILTER ( PointsFinal, NOT ISBLANK ( [Value] ) && [Value] = MinVal )

VAR MinIdx =
    MINX ( MinPointTable, [Idx] )

VAR _MinX =
    MAXX ( FILTER ( PointsFinal, [Idx] = MinIdx ), [XCoord] )

VAR MinY =
    MAXX ( FILTER ( PointsFinal, [Idx] = MinIdx ), [YCoord] )

VAR SvgMinPoint =
    IF (
        HasNoData,
        "",
        "<circle cx=""" & FORMAT ( _MinX, "0.0" )
            & """ cy=""" & FORMAT ( MinY, "0.0" )
            & """ r=""3.5"" fill=""" & MinColor
            & """ stroke=""" & MinColor & """ stroke-width=""1"" />"
    )

-- Identify max point
VAR MaxPointTable =
    FILTER ( PointsFinal, NOT ISBLANK ( [Value] ) && [Value] = MaxVal )

VAR MaxIdx =
    MINX ( MaxPointTable, [Idx] )

VAR _MaxX =
    MAXX ( FILTER ( PointsFinal, [Idx] = MaxIdx ), [XCoord] )

VAR MaxY =
    MAXX ( FILTER ( PointsFinal, [Idx] = MaxIdx ), [YCoord] )

VAR SvgMaxPoint =
    IF (
        HasNoData,
        "",
        "<circle cx=""" & FORMAT ( _MaxX, "0.0" )
            & """ cy=""" & FORMAT ( MaxY, "0.0" )
            & """ r=""3.5"" fill=""" & MaxColor
            & """ stroke=""" & MaxColor & """ stroke-width=""1"" />"
    )

-------------------------------------------------
-- Build full SVG
-------------------------------------------------
VAR SvgHeader =
    "data:image/svg+xml;utf8,"
        & "<svg width=""" & CardWidth & """ height=""" & CardHeight
        & """ viewBox=""0 0 " & CardWidth & " " & CardHeight
        & """ xmlns=""http://www.w3.org/2000/svg"">"

VAR SvgBackground =
    "<rect x=""0"" y=""0"" width=""" & CardWidth & """ height=""" & CardHeight
        & """ fill=""#FFFFFF"" />"

VAR SvgFooter = "</svg>"

RETURN
    IF (
        HasNoData,
        BLANK (),
        SvgHeader
            & SvgBackground
            & SvgLinePath
            & SvgAllPoints      -- blue points added here
            & SvgMinPoint       -- overrides blue
            & SvgMaxPoint       -- overrides blue
            & SvgFooter
    )
