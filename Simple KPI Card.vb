Total Revenue SVG KPI = 
VAR KPIVLU =
SWITCH(
    TRUE(),
    ABS([Total Revenue]) >= 1000000000,   FORMAT([Total Revenue]/1000000000, "£0.0""B"""),
    ABS([Total Revenue]) >= 1000000,      FORMAT([Total Revenue]/1000000,    "£0.0""M"""),
    ABS([Total Revenue]) >= 1000,         FORMAT([Total Revenue]/1000,       "£0""K"""),
    FORMAT([Total Revenue], "£#,0")
)

VAR KpiLabel =
    "Total Revenue"

VAR YoYValue =
    [Total Revenue YoY Change %]

VAR YoYFormattedRaw =
    IF ( NOT ISBLANK ( YoYValue ), FORMAT ( YoYValue, "0.0%" ), "N/A" )

VAR YoYLabelText =
    IF (
        ISBLANK ( YoYValue ),
        "vs LY: N/A",
        "vs LY: " &
            IF ( YoYValue >= 0, "+" & YoYFormattedRaw, YoYFormattedRaw )
    )
-- Positions
VAR IconPosX      = 30   -- horizontal position of the icon centre-ish
VAR IconPosY      = 60  -- vertical position (tweak this to centre nicely)
VAR IconScale     = 0.55 -- size of the icon

VAR TextBlockX    = 100  -- left edge of the text block (value/label/YoY)
VAR KPIVLUY     = 80
VAR KpiLabelY     = 110
VAR YoYBaseY      = 140  -- baseline for the YoY group

-- Colours
VAR BgColor           = "#FFFFFF"
VAR BorderColor       = "#E2E8F0"
VAR IconBgColor       = "#F3F6FA"
VAR IconBorderColor   = "#CBD5E0"
VAR TextPrimaryColor  = "#1A202C"
VAR TextSecondaryColor= "#4A5568"
VAR TextYoYColor      = "#2D3748"
VAR ArrowUpColor      = "#38A169"
VAR ArrowDownColor    = "#E53E3E"
VAR ArrowNeutralColor = "#A0AEC0"

VAR IsPositive = YoYValue > 0
VAR IsNegative = YoYValue < 0

VAR ArrowColor =
    IF (
        ISBLANK ( YoYValue ),
        ArrowNeutralColor,
        IF ( IsPositive, ArrowUpColor, ArrowDownColor )
    )

-- Arrow points relative to the centered <g transform="translate(150,138)">
VAR ArrowPointsUp   = "0,0 10,-12 20,0"
VAR ArrowPointsDown = "0,-15 10,0  20,-15"
VAR ArrowPoints =
    IF ( IsNegative, ArrowPointsDown, ArrowPointsUp )

-- SVG building blocks
VAR SvgHeader =
    "data:image/svg+xml;utf8," &
    "<svg width=""300"" height=""180"" viewBox=""0 0 300 180"" xmlns=""http://www.w3.org/2000/svg"">"

VAR SvgBackground =
    "<rect x=""0"" y=""0"" width=""300"" height=""180"" rx=""16"" fill=""" & BgColor &
    """ stroke=""" & BorderColor & """ stroke-width=""1"" />"

-- Icon placeholder top-left (you can replace this with your own icon paths later)
VAR SvgIconPlaceholder =
    "<g transform=""translate(" & IconPosX & "," & IconPosY & ") scale(" & IconScale & ")"">" &
    "<svg xmlns='http://www.w3.org/2000/svg' height='100px' viewBox='0 -960 960 960' width='100px' fill='#1D7044'>" &
    "<path d='M220-60 80-200l140-140 57 56-44 44h494l-43-44 56-56 140 140L740-60l-57-56 44-44H233l43 44-56 56Zm260-460q-50 0-85-35t-35-85q0-50 35-85t85-35q50 0 85 35t35 85q0 50-35 85t-85 35ZM200-400q-33 0-56.5-23.5T120-480v-320q0-33 23.5-56.5T200-880h560q33 0 56.5 23.5T840-800v320q0 33-23.5 56.5T760-400H200Zm80-80h400q0-33 23.5-56.5T760-560v-160q-33 0-56.5-23.5T680-800H280q0 33-23.5 56.5T200-720v160q33 0 56.5 23.5T280-480Zm-80 0v-320 320Z'/>" &
    "</svg>" &
    "</g>"
VAR SvgKPIVLU =
    "<text x=""" & TextBlockX & """ y=""" & KPIVLUY &
        """ text-anchor=""start"" fill=""" & TextPrimaryColor &
        """ font-size=""46"" font-weight=""600"" font-family=""Segoe UI"">" &
        KPIVLU & "</text>"

VAR SvgKpiLabel =
    "<text x=""" & TextBlockX & """ y=""" & KpiLabelY &
        """ text-anchor=""start"" fill=""" & TextSecondaryColor &
        """ font-size=""24"" font-family=""Segoe UI"">" &
        KpiLabel & "</text>"


-- Centered YoY group: arrow + text
VAR SvgYoYGroup =
    "<g transform=""translate(" & TextBlockX & "," & YoYBaseY & ")"">" &
        -- arrow directly under the label, aligned to the text block
        "<polygon points="""&ArrowPoints&""" fill=""" & ArrowColor & """ />" &
        -- YoY text to the right of the arrow
        "<text x=""30"" y=""3"" text-anchor=""start"" fill=""" & TextYoYColor &
            """ font-size=""22"" font-family=""Segoe UI"">" &
            YoYLabelText & "</text>" &
    "</g>"

VAR SvgFooter = "</svg>"

RETURN
    SvgHeader &
    SvgBackground &
    SvgIconPlaceholder &
    SvgKPIVLU &
    SvgKpiLabel &
    SvgYoYGroup &
    SvgFooter
