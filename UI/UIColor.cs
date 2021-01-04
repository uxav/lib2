using System;
using System.Linq;
using Crestron.SimplSharp.Reflection;

namespace UX.Lib2.UI
{
    public class UIColor
    {
        public UIColor()
            : this(0, 0, 0)
        {
        }

        public UIColor(uint red, uint green, uint blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public UIColor(string hex)
        {
            hex = hex.Replace("#", "");
            if (hex.Length != 6)
                throw new Exception("Hex color code is not correct length");
            Red = uint.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            Green = uint.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            Blue = uint.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        }

        public static UIColor FromHex(string hex)
        {
            return new UIColor(hex);
        }

        public static UIColor FromName(string name)
        {
            if (string.IsNullOrEmpty(name)) return new UIColor();
            var type = typeof (UIColors);
            var properties = type.GetCType().GetProperties(BindingFlags.Static | BindingFlags.Public);
            var property =
                properties.FirstOrDefault(p => String.Equals(p.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if(property == null) return new UIColor();
            return (UIColor)property.GetValue(type);
        }

        public string Name
        {
            get
            {
                var type = typeof(UIColors);
                var properties = type.GetCType().GetProperties(BindingFlags.Static | BindingFlags.Public);
                var property = properties.FirstOrDefault(p => ((UIColor) p.GetValue(type)).ToHex() == ToHex());
                return property == null ? ToHex() : property.Name;
            }
        }

        public string ToHex()
        {
            return "#" + Red.ToString("X2") + Green.ToString("X2") + Blue.ToString("X2");
        }

        public uint Red;
        public uint Green;
        public uint Blue;
    }

    public static class UIColors
    {
        public static UIColor AliceBlue
        {
            get { return new UIColor("#F0F8FF"); }
        }

        public static UIColor AntiqueWhite
        {
            get { return new UIColor("#FAEBD7"); }
        }

        public static UIColor Aqua
        {
            get { return new UIColor("#00FFFF"); }
        }

        public static UIColor Aquamarine
        {
            get { return new UIColor("#7FFFD4"); }
        }

        public static UIColor Azure
        {
            get { return new UIColor("#F0FFFF"); }
        }

        public static UIColor Beige
        {
            get { return new UIColor("#F5F5DC"); }
        }

        public static UIColor Bisque
        {
            get { return new UIColor("#FFE4C4"); }
        }

        public static UIColor Black
        {
            get { return new UIColor("#000000"); }
        }

        public static UIColor BlanchedAlmond
        {
            get { return new UIColor("#FFEBCD"); }
        }

        public static UIColor Blue
        {
            get { return new UIColor("#0000FF"); }
        }

        public static UIColor BlueViolet
        {
            get { return new UIColor("#8A2BE2"); }
        }

        public static UIColor Brown
        {
            get { return new UIColor("#A52A2A"); }
        }

        public static UIColor BurlyWood
        {
            get { return new UIColor("#DEB887"); }
        }

        public static UIColor CadetBlue
        {
            get { return new UIColor("#5F9EA0"); }
        }

        public static UIColor Chartreuse
        {
            get { return new UIColor("#7FFF00"); }
        }

        public static UIColor Chocolate
        {
            get { return new UIColor("#D2691E"); }
        }

        public static UIColor Coral
        {
            get { return new UIColor("#FF7F50"); }
        }

        public static UIColor CornflowerBlue
        {
            get { return new UIColor("#6495ED"); }
        }

        public static UIColor Cornsilk
        {
            get { return new UIColor("#FFF8DC"); }
        }

        public static UIColor Crimson
        {
            get { return new UIColor("#DC143C"); }
        }

        public static UIColor Cyan
        {
            get { return new UIColor("#00FFFF"); }
        }

        public static UIColor DarkBlue
        {
            get { return new UIColor("#00008B"); }
        }

        public static UIColor DarkCyan
        {
            get { return new UIColor("#008B8B"); }
        }

        public static UIColor DarkGoldenrod
        {
            get { return new UIColor("#B8860B"); }
        }

        public static UIColor DarkGray
        {
            get { return new UIColor("#A9A9A9"); }
        }

        public static UIColor DarkGreen
        {
            get { return new UIColor("#006400"); }
        }

        public static UIColor DarkKhaki
        {
            get { return new UIColor("#BDB76B"); }
        }

        public static UIColor DarkMagenta
        {
            get { return new UIColor("#8B008B"); }
        }

        public static UIColor DarkOliveGreen
        {
            get { return new UIColor("#556B2F"); }
        }

        public static UIColor DarkOrange
        {
            get { return new UIColor("#FF8C00"); }
        }

        public static UIColor DarkOrchid
        {
            get { return new UIColor("#9932CC"); }
        }

        public static UIColor DarkRed
        {
            get { return new UIColor("#8B0000"); }
        }

        public static UIColor DarkSalmon
        {
            get { return new UIColor("#E9967A"); }
        }

        public static UIColor DarkSeaGreen
        {
            get { return new UIColor("#8FBC8F"); }
        }

        public static UIColor DarkSlateBlue
        {
            get { return new UIColor("#483D8B"); }
        }

        public static UIColor DarkSlateGray
        {
            get { return new UIColor("#2F4F4F"); }
        }

        public static UIColor DarkTurquoise
        {
            get { return new UIColor("#00CED1"); }
        }

        public static UIColor DarkViolet
        {
            get { return new UIColor("#9400D3"); }
        }

        public static UIColor DeepPink
        {
            get { return new UIColor("#FF1493"); }
        }

        public static UIColor DeepSkyBlue
        {
            get { return new UIColor("#00BFFF"); }
        }

        public static UIColor DimGray
        {
            get { return new UIColor("#696969"); }
        }

        public static UIColor DodgerBlue
        {
            get { return new UIColor("#1E90FF"); }
        }

        public static UIColor Firebrick
        {
            get { return new UIColor("#B22222"); }
        }

        public static UIColor FloralWhite
        {
            get { return new UIColor("#FFFAF0"); }
        }

        public static UIColor ForestGreen
        {
            get { return new UIColor("#228B22"); }
        }

        public static UIColor Fuchsia
        {
            get { return new UIColor("#FF00FF"); }
        }

        public static UIColor Gainsboro
        {
            get { return new UIColor("#DCDCDC"); }
        }

        public static UIColor GhostWhite
        {
            get { return new UIColor("#F8F8FF"); }
        }

        public static UIColor Gold
        {
            get { return new UIColor("#FFD700"); }
        }

        public static UIColor Goldenrod
        {
            get { return new UIColor("#DAA520"); }
        }

        public static UIColor Gray
        {
            get { return new UIColor("#808080"); }
        }

        public static UIColor Green
        {
            get { return new UIColor("#008000"); }
        }

        public static UIColor GreenYellow
        {
            get { return new UIColor("#ADFF2F"); }
        }

        public static UIColor Honeydew
        {
            get { return new UIColor("#F0FFF0"); }
        }

        public static UIColor HotPink
        {
            get { return new UIColor("#FF69B4"); }
        }

        public static UIColor IndianRed
        {
            get { return new UIColor("#CD5C5C"); }
        }

        public static UIColor Indigo
        {
            get { return new UIColor("#4B0082"); }
        }

        public static UIColor Ivory
        {
            get { return new UIColor("#FFFFF0"); }
        }

        public static UIColor Khaki
        {
            get { return new UIColor("#F0E68C"); }
        }

        public static UIColor Lavender
        {
            get { return new UIColor("#E6E6FA"); }
        }

        public static UIColor LavenderBlush
        {
            get { return new UIColor("#FFF0F5"); }
        }

        public static UIColor LawnGreen
        {
            get { return new UIColor("#7CFC00"); }
        }

        public static UIColor LemonChiffon
        {
            get { return new UIColor("#FFFACD"); }
        }

        public static UIColor LightBlue
        {
            get { return new UIColor("#ADD8E6"); }
        }

        public static UIColor LightCoral
        {
            get { return new UIColor("#F08080"); }
        }

        public static UIColor LightCyan
        {
            get { return new UIColor("#E0FFFF"); }
        }

        public static UIColor LightGoldenrodYellow
        {
            get { return new UIColor("#FAFAD2"); }
        }

        public static UIColor LightGray
        {
            get { return new UIColor("#D3D3D3"); }
        }

        public static UIColor LightGreen
        {
            get { return new UIColor("#90EE90"); }
        }

        public static UIColor LightPink
        {
            get { return new UIColor("#FFB6C1"); }
        }

        public static UIColor LightSalmon
        {
            get { return new UIColor("#FFA07A"); }
        }

        public static UIColor LightSeaGreen
        {
            get { return new UIColor("#20B2AA"); }
        }

        public static UIColor LightSkyBlue
        {
            get { return new UIColor("#87CEFA"); }
        }

        public static UIColor LightSlateGray
        {
            get { return new UIColor("#778899"); }
        }

        public static UIColor LightSteelBlue
        {
            get { return new UIColor("#B0C4DE"); }
        }

        public static UIColor LightYellow
        {
            get { return new UIColor("#FFFFE0"); }
        }

        public static UIColor Lime
        {
            get { return new UIColor("#00FF00"); }
        }

        public static UIColor LimeGreen
        {
            get { return new UIColor("#32CD32"); }
        }

        public static UIColor Linen
        {
            get { return new UIColor("#FAF0E6"); }
        }

        public static UIColor Magenta
        {
            get { return new UIColor("#FF00FF"); }
        }

        public static UIColor Maroon
        {
            get { return new UIColor("#800000"); }
        }

        public static UIColor MediumAquamarine
        {
            get { return new UIColor("#66CDAA"); }
        }

        public static UIColor MediumBlue
        {
            get { return new UIColor("#0000CD"); }
        }

        public static UIColor MediumOrchid
        {
            get { return new UIColor("#BA55D3"); }
        }

        public static UIColor MediumPurple
        {
            get { return new UIColor("#9370DB"); }
        }

        public static UIColor MediumSeaGreen
        {
            get { return new UIColor("#3CB371"); }
        }

        public static UIColor MediumSlateBlue
        {
            get { return new UIColor("#7B68EE"); }
        }

        public static UIColor MediumSpringGreen
        {
            get { return new UIColor("#00FA9A"); }
        }

        public static UIColor MediumTurquoise
        {
            get { return new UIColor("#48D1CC"); }
        }

        public static UIColor MediumVioletRed
        {
            get { return new UIColor("#C71585"); }
        }

        public static UIColor MidnightBlue
        {
            get { return new UIColor("#191970"); }
        }

        public static UIColor MintCream
        {
            get { return new UIColor("#F5FFFA"); }
        }

        public static UIColor MistyRose
        {
            get { return new UIColor("#FFE4E1"); }
        }

        public static UIColor Moccasin
        {
            get { return new UIColor("#FFE4B5"); }
        }

        public static UIColor NavajoWhite
        {
            get { return new UIColor("#FFDEAD"); }
        }

        public static UIColor Navy
        {
            get { return new UIColor("#000080"); }
        }

        public static UIColor OldLace
        {
            get { return new UIColor("#FDF5E6"); }
        }

        public static UIColor Olive
        {
            get { return new UIColor("#808000"); }
        }

        public static UIColor OliveDrab
        {
            get { return new UIColor("#6B8E23"); }
        }

        public static UIColor Orange
        {
            get { return new UIColor("#FFA500"); }
        }

        public static UIColor OrangeRed
        {
            get { return new UIColor("#FF4500"); }
        }

        public static UIColor Orchid
        {
            get { return new UIColor("#DA70D6"); }
        }

        public static UIColor PaleGoldenrod
        {
            get { return new UIColor("#EEE8AA"); }
        }

        public static UIColor PaleGreen
        {
            get { return new UIColor("#98FB98"); }
        }

        public static UIColor PaleTurquoise
        {
            get { return new UIColor("#AFEEEE"); }
        }

        public static UIColor PaleVioletRed
        {
            get { return new UIColor("#DB7093"); }
        }

        public static UIColor PapayaWhip
        {
            get { return new UIColor("#FFEFD5"); }
        }

        public static UIColor PeachPuff
        {
            get { return new UIColor("#FFDAB9"); }
        }

        public static UIColor Peru
        {
            get { return new UIColor("#CD853F"); }
        }

        public static UIColor Pink
        {
            get { return new UIColor("#FFC0CB"); }
        }

        public static UIColor Plum
        {
            get { return new UIColor("#DDA0DD"); }
        }

        public static UIColor PowderBlue
        {
            get { return new UIColor("#B0E0E6"); }
        }

        public static UIColor Purple
        {
            get { return new UIColor("#800080"); }
        }

        public static UIColor Red
        {
            get { return new UIColor("#FF0000"); }
        }

        public static UIColor RosyBrown
        {
            get { return new UIColor("#BC8F8F"); }
        }

        public static UIColor RoyalBlue
        {
            get { return new UIColor("#4169E1"); }
        }

        public static UIColor SaddleBrown
        {
            get { return new UIColor("#8B4513"); }
        }

        public static UIColor Salmon
        {
            get { return new UIColor("#FA8072"); }
        }

        public static UIColor SandyBrown
        {
            get { return new UIColor("#F4A460"); }
        }

        public static UIColor SeaGreen
        {
            get { return new UIColor("#2E8B57"); }
        }

        public static UIColor SeaShell
        {
            get { return new UIColor("#FFF5EE"); }
        }

        public static UIColor Sienna
        {
            get { return new UIColor("#A0522D"); }
        }

        public static UIColor Silver
        {
            get { return new UIColor("#C0C0C0"); }
        }

        public static UIColor SkyBlue
        {
            get { return new UIColor("#87CEEB"); }
        }

        public static UIColor SlateBlue
        {
            get { return new UIColor("#6A5ACD"); }
        }

        public static UIColor SlateGray
        {
            get { return new UIColor("#708090"); }
        }

        public static UIColor Snow
        {
            get { return new UIColor("#FFFAFA"); }
        }

        public static UIColor SpringGreen
        {
            get { return new UIColor("#00FF7F"); }
        }

        public static UIColor SteelBlue
        {
            get { return new UIColor("#4682B4"); }
        }

        public static UIColor Tan
        {
            get { return new UIColor("#D2B48C"); }
        }

        public static UIColor Teal
        {
            get { return new UIColor("#008080"); }
        }

        public static UIColor Thistle
        {
            get { return new UIColor("#D8BFD8"); }
        }

        public static UIColor Tomato
        {
            get { return new UIColor("#FF6347"); }
        }

        public static UIColor Transparent
        {
            get { return new UIColor("#FFFFFF"); }
        }

        public static UIColor Turquoise
        {
            get { return new UIColor("#40E0D0"); }
        }

        public static UIColor Violet
        {
            get { return new UIColor("#EE82EE"); }
        }

        public static UIColor Wheat
        {
            get { return new UIColor("#F5DEB3"); }
        }

        public static UIColor White
        {
            get { return new UIColor("#FFFFFF"); }
        }

        public static UIColor WhiteSmoke
        {
            get { return new UIColor("#F5F5F5"); }
        }

        public static UIColor Yellow
        {
            get { return new UIColor("#FFFF00"); }
        }

        public static UIColor YellowGreen
        {
            get { return new UIColor("#9ACD32"); }
        }
    }
}