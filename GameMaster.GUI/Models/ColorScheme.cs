using Avalonia.Media;


namespace GameMaster.GUI.Models
{
    public static class ColorScheme
    {
        public static readonly ISolidColorBrush Empty = Brushes.White;
        public static readonly ISolidColorBrush Piece = Brushes.Black;
        public static readonly ISolidColorBrush ShamPiece = Brushes.DarkViolet;
        public static readonly ISolidColorBrush RedPlayer = Brushes.Red;
        public static readonly ISolidColorBrush BluePlayer = Brushes.Blue;
        public static readonly ISolidColorBrush RedGoal = Brushes.LightCoral;
        public static readonly ISolidColorBrush BlueGoal = Brushes.LightSkyBlue;
        public static readonly ISolidColorBrush NonGoal = Brushes.Wheat;
        public static readonly ISolidColorBrush Border = Brushes.Black;
        public static readonly ISolidColorBrush RedGoalAreaBorder = Brushes.LightCoral;
        public static readonly ISolidColorBrush BlueGoalAreaBorder = Brushes.LightSkyBlue;
        public static readonly ISolidColorBrush CoveredGoal = Brushes.Yellow;
        public static readonly ISolidColorBrush IdText = Brushes.White;
        public static readonly ISolidColorBrush Leader = Brushes.Black;
    }
}
