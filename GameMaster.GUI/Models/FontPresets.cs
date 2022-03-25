using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameMaster.GUI.Models
{
    public static class FontPresets
    {
        public const string LeaderCrown = "👑";

        public readonly static FontFamily IdFontFamily;
        public readonly static int IdFontSize;
        public readonly static Typeface IdTypeface;

        public readonly static FontFamily EmojiFontFamily;
        public readonly static int EmojiFontSize;
        public readonly static Typeface EmojiTypeface;

        static FontPresets()
        {
            // Id Font
            IdFontFamily = new FontFamily("Arial");
            IdFontSize = 10;
            IdTypeface = new Typeface(IdFontFamily, IdFontSize);

            // Emoji Font
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                EmojiFontFamily = new FontFamily("Segoe UI Emoji");
            else
                EmojiFontFamily = new FontFamily("Noto Color Emoji");
            EmojiFontSize = 9;
            EmojiTypeface = new Typeface(EmojiFontFamily, EmojiFontSize);
        }
    }
}
