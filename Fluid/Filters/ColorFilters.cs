﻿using System;
using System.Drawing;
using System.Linq;
using Fluid.Values;

namespace Fluid.Filters
{
    public static class ColorFilters
    {
        public static FilterCollection WithColorFilters(this FilterCollection filters)
        {
            filters.AddFilter("color_to_rgb", ToRgb);
            filters.AddFilter("color_to_hex", ToHex);
            filters.AddFilter("color_to_hsl", ToHsl);
            filters.AddFilter("color_extract", ColorExtract);

            return filters;
        }

        public static FluidValue ToRgb(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var value = input.ToStringValue();
            if (HexColor.TryParse(value, out HexColor hexColor))
            {
                var rgbColor = (RgbColor)hexColor;

                return new StringValue(rgbColor.ToString());
            }
            else if (HslColor.TryParse(value, out HslColor hslColor))
            {
                var rgbColor = (RgbColor)hslColor;

                return new StringValue(rgbColor.ToString());
            }
            else
            {
                return NilValue.Empty;
            }
        }

        public static FluidValue ToHex(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var value = input.ToStringValue();
            if (RgbColor.TryParse(value, out RgbColor rgbColor))
            {
                var hexColor = (HexColor)rgbColor;

                return new StringValue(hexColor.ToString());
            }
            else if (HslColor.TryParse(value, out HslColor hslColor))
            {
                var hexColor = (HexColor)hslColor;

                return new StringValue(hexColor.ToString());
            }
            else
            {
                return NilValue.Empty;
            }
        }

        public static FluidValue ToHsl(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var value = input.ToStringValue();
            if (HexColor.TryParse(value, out HexColor hexColor))
            {
                var hslColor = (HslColor)hexColor;

                return new StringValue(hslColor.ToString());
            }
            else if (RgbColor.TryParse(value, out RgbColor rgbColor))
            {
                var hslColor = (HslColor)rgbColor;

                return new StringValue(hslColor.ToString());
            }
            else
            {
                return NilValue.Empty;
            }
        }

        public static FluidValue ColorExtract(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var value = input.ToStringValue();
            RgbColor rgbColor;
            HslColor hslColor;
            if (HexColor.TryParse(value, out HexColor hexColor))
            {
                rgbColor = (RgbColor)hexColor;
                hslColor = (HslColor)hexColor;
            }
            else if (RgbColor.TryParse(value, out rgbColor))
            {
                hslColor = (HslColor)rgbColor;
            }
            else if (HslColor.TryParse(value, out hslColor))
            {
                rgbColor = (RgbColor)hslColor;
            }
            else
            {
                return NilValue.Empty;
            }

            return arguments.At(0).ToStringValue() switch
            {
                "alpha" => new StringValue(rgbColor.A.ToString()),
                "red" => new StringValue(rgbColor.R.ToString()),
                "green" => new StringValue(rgbColor.G.ToString()),
                "blue" => new StringValue(rgbColor.B.ToString()),
                "hue" => new StringValue(hslColor.H.ToString()),
                "saturation" => new StringValue(Convert.ToInt32(hslColor.S * 100.0).ToString()),
                "lightness" => new StringValue(Convert.ToInt32(hslColor.L * 100.0).ToString()),
                _ => NilValue.Empty,
            };
        }

        private struct HexColor
        {
            public static readonly HexColor Empty = default;

            public HexColor(string red, string green, string blue)
            {
                if (!IsHexadecimal(red))
                {
                    throw new ArgumentNullException(nameof(red), "The red value is not hexadecimal");
                }

                if (!IsHexadecimal(green))
                {
                    throw new ArgumentNullException(nameof(green), "The green value is not hexadecimal");
                }

                if (!IsHexadecimal(blue))
                {
                    throw new ArgumentNullException(nameof(blue), "The blue value is not hexadecimal");
                }

                R = red;
                G = green;
                B = blue;
            }

            public string R { get; }

            public string G { get; }

            public string B { get; }

            public static bool TryParse(string value, out HexColor color)
            {
                color = HexColor.Empty;

                if (String.IsNullOrEmpty(value))
                {
                    return false;
                }

                if (value[0] == '#')
                {
                    string red, blue, green;
                    switch (value.Length)
                    {
                        case 4:
                            red = Char.ToString(value[1]);
                            green = Char.ToString(value[2]);
                            blue = Char.ToString(value[3]);
                            if (IsHexadecimal(red) && IsHexadecimal(green) && IsHexadecimal(blue))
                            {
                                color = new HexColor(red, green, blue);

                                return true;
                            }

                            break;
                        case 7:
                            red = value.Substring(1, 2);
                            green = value.Substring(3, 2);
                            blue = value.Substring(5, 2);
                            if (IsHexadecimal(red) && IsHexadecimal(green) && IsHexadecimal(blue))
                            {
                                color = new HexColor(red, green, blue);

                                return true;
                            }

                            break;
                    }
                }

                return false;
            }

            public override string ToString() => $"#{R}{G}{B}".ToLower();

            public static explicit operator HexColor(HslColor hslColor) => (HexColor)(RgbColor)hslColor;

            public static explicit operator HexColor(RgbColor rgbColor)
                => new HexColor(
                    rgbColor.R.ToString("X2", null),
                    rgbColor.G.ToString("X2", null),
                    rgbColor.B.ToString("X2", null));

            private static bool IsHexadecimal(string value) => value.All(c => "0123456789abcdefABCDEF".Contains(c));
        }

        private struct RgbColor
        {
            private const double DefaultTransperency = 1.0;

            private static readonly char[] _colorSeparators = new[] { '(', ',', ' ', ')' };

            public static readonly RgbColor Empty = default;

            public RgbColor(Color color) : this(color.R, color.G, color.B)
            {

            }

            public RgbColor(int red, int green, int blue, double alpha = DefaultTransperency)
            {
                if (red < 0 || red > 255)
                {
                    throw new ArgumentOutOfRangeException(nameof(red), "The red value must in rage [0-255]");
                }

                if (green < 0 || green > 255)
                {
                    throw new ArgumentOutOfRangeException(nameof(green), "The green value must in rage [0-255]");
                }

                if (blue < 0 || blue > 255)
                {
                    throw new ArgumentOutOfRangeException(nameof(blue), "The blue value must in rage [0-255]");
                }

                if (alpha < 0.0 || alpha > 1.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(alpha), "The alpha value must in rage [0-1]");
                }

                R = red;
                G = green;
                B = blue;
                A = alpha;
            }

            public double A { get; }

            public int R { get; }

            public int G { get; }

            public int B { get; }

            public static bool TryParse(string value, out RgbColor color)
            {
                if ((value.StartsWith("rgb(") || value.StartsWith("rgba(")) && value.EndsWith(")"))
                {
                    var rgbColor = value.Split(_colorSeparators, StringSplitOptions.RemoveEmptyEntries);
                    if (rgbColor.Length == 4 &&
                        Int32.TryParse(rgbColor[1], out int red) &&
                        Int32.TryParse(rgbColor[2], out int green) &&
                        Int32.TryParse(rgbColor[3], out int blue))
                    {
                        color = new RgbColor(red, green, blue);

                        return true;
                    }

                    if (rgbColor.Length == 5 &&
                        Int32.TryParse(rgbColor[1], out red) &&
                        Int32.TryParse(rgbColor[2], out green) &&
                        Int32.TryParse(rgbColor[3], out blue) &&
                        Single.TryParse(rgbColor[4], out float alpha))
                    {
                        color = new RgbColor(red, green, blue, alpha);

                        return true;
                    }
                }

                color = RgbColor.Empty;

                return false;
            }

            private static double QqhToRgb(double q1, double q2, double hue)
            {
                if (hue > 360.0)
                {
                    hue -= 360.0;
                }
                else if (hue < 0)
                {
                    hue += 360.0;
                }

                if (hue < 60.0)
                {
                    return q1 + (q2 - q1) * hue / 60.0;
                }

                if (hue < 180.0)
                {
                    return q2;
                }

                if (hue < 240.0)
                {
                    return q1 + (q2 - q1) * (240.0 - hue) / 60.0;
                }

                return q1;
            }

            public static implicit operator Color(RgbColor rgbColor)
                => Color.FromArgb(rgbColor.R, rgbColor.G, rgbColor.B);

            public static explicit operator RgbColor(Color color) => new RgbColor(color);

            public static explicit operator RgbColor(HexColor hexColor)
            {
                if (hexColor.R.Length == 1)
                {
                    var red = Convert.ToInt32(hexColor.R + hexColor.R, 16);
                    var green = Convert.ToInt32(hexColor.G + hexColor.G, 16);
                    var blue = Convert.ToInt32(hexColor.B + hexColor.B, 16);

                    return new RgbColor(red, green, blue);
                }
                else
                {
                    var red = Convert.ToInt32(hexColor.R, 16);
                    var green = Convert.ToInt32(hexColor.G, 16);
                    var blue = Convert.ToInt32(hexColor.B, 16);

                    return new RgbColor(red, green, blue);
                }
            }

            public static explicit operator RgbColor(HslColor hslColor)
            {
                // http://csharphelper.com/blog/2016/08/convert-between-rgb-and-hls-color-models-in-c/
                double p2;
                if (hslColor.L <= 0.5)
                {
                    p2 = hslColor.L * (1 + hslColor.S);
                }
                else
                {
                    p2 = hslColor.L + hslColor.S - hslColor.L * hslColor.S;
                }

                var p1 = 2.0 * hslColor.L - p2;
                double r, g, b;
                if (hslColor.S == 0.0)
                {
                    r = hslColor.L;
                    g = hslColor.L;
                    b = hslColor.L;
                }
                else
                {
                    r = QqhToRgb(p1, p2, hslColor.H + 120.0);
                    g = QqhToRgb(p1, p2, hslColor.H);
                    b = QqhToRgb(p1, p2, hslColor.H - 120.0);
                }

                return new RgbColor(
                    (int)Math.Round(r * 255.0),
                    (int)Math.Round(g * 255.0),
                    (int)Math.Round(b * 255.0),
                    hslColor.A
                    );
            }

            public override string ToString() => A == DefaultTransperency
                ? $"rgb({R}, {G}, {B})"
                : $"rgba({R}, {G}, {B}, {Math.Round(A, 1)})";
        }

        private struct HslColor
        {
            private const double DefaultTransperency = 1.0;

            private static readonly char[] _colorSeparators = new[] { '(', ',', ' ', ')' };

            public static readonly HslColor Empty = default;

            public HslColor(int hue, double saturation, double lightness, double alpha = DefaultTransperency)
            {
                if (hue < 0 || hue > 360)
                {
                    throw new ArgumentOutOfRangeException(nameof(hue), "The hue value must in rage [0-360]");
                }

                if (saturation < 0.0 || saturation > 1.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(saturation), "The saturation value must in rage [0-1]");
                }

                if (lightness < 0.0 || lightness > 1.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(lightness), "The lightness value must in rage [0-1]");
                }

                if (alpha < 0.0 || alpha > 1.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(alpha), "The alpha value must in rage [0-1]");
                }

                H = hue;
                S = saturation;
                L = lightness;
                A = alpha;
            }

            public int H { get; }

            public double S { get; }

            public double L { get; }

            public double A { get; }

            public static bool TryParse(string value, out HslColor color)
            {
                if ((value.StartsWith("hsl(") || value.StartsWith("hsla(")) && value.EndsWith(")"))
                {
                    var hslColor = value.Split(_colorSeparators, StringSplitOptions.RemoveEmptyEntries);
                    if (hslColor.Length == 4 && hslColor[2].EndsWith("%") && hslColor[3].EndsWith("%") &&
                        Int32.TryParse(hslColor[1], out int hue) &&
                        Int32.TryParse(hslColor[2].TrimEnd('%'), out int saturation) &&
                        Int32.TryParse(hslColor[3].TrimEnd('%'), out int lightness))
                    {
                        color = new HslColor(hue, saturation / 100.0, lightness / 100.0);

                        return true;
                    }

                    if (hslColor.Length == 5 && hslColor[2].EndsWith("%") && hslColor[3].EndsWith("%") &&
                        Int32.TryParse(hslColor[1], out hue) &&
                        Int32.TryParse(hslColor[2].TrimEnd('%'), out saturation) &&
                        Int32.TryParse(hslColor[3].TrimEnd('%'), out lightness) &&
                        Single.TryParse(hslColor[4], out float alpha))
                    {
                        color = new HslColor(hue, saturation / 100.0, lightness / 100.0, alpha);

                        return true;
                    }
                }

                color = HslColor.Empty;

                return false;
            }

            public static explicit operator HslColor(HexColor hexColor) => (HslColor)(RgbColor)hexColor;

            public static explicit operator HslColor(RgbColor rgbColor)
            {
                // http://csharphelper.com/blog/2016/08/convert-between-rgb-and-hls-color-models-in-c/
                double h;
                double s;
                var r = rgbColor.R / 255.0;
                var g = rgbColor.G / 255.0;
                var b = rgbColor.B / 255.0;
                var max = Math.Max(Math.Max(r, g), b);
                var min = Math.Min(Math.Min(r, g), b);
                var diff = max - min;
                var l = (max + min) / 2.0;

                if (Math.Abs(diff) < 0.00001)
                {
                    s = 0.0;
                    h = 0.0;
                }
                else
                {
                    if (l <= 0.5)
                    {
                        s = diff / (max + min);
                    }
                    else
                    {
                        s = diff / (2 - max - min);
                    }

                    var rDist = (max - r) / diff;
                    var gDist = (max - g) / diff;
                    var bDist = (max - b) / diff;

                    if (r == max)
                    {
                        h = bDist - gDist;
                    }
                    else if (g == max)
                    {
                        h = 2 + rDist - bDist;
                    }
                    else
                    {
                        h = 4 + gDist - rDist;
                    }

                    h *= 60;

                    if (h < 0)
                    {
                        h += 360;
                    }
                }

                return new HslColor(Convert.ToInt32(h), Math.Round(s, 2), Math.Round(l, 2), rgbColor.A);
            }

            public override string ToString() => A == DefaultTransperency
                ? $"hsl({H}, {S * 100.0}%, {L * 100.0}%)"
                : $"hsla({H}, {S * 100.0}%, {L * 100.0}%, {Math.Round(A, 1)})";
        }
    }
}