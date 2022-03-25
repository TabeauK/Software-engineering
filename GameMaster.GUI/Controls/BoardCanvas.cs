using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Text;
using GameMaster.GUI.Models;
using static GameMaster.BoardView;

namespace GameMaster.GUI.Controls
{
    public class BoardCanvas : Canvas
    {
        private BoardView boardState = null;

        // Przypisanie do BoardState powoduje (ponowne) narysowanie planszy
        public BoardView BoardState
        {
            get => boardState;
            set
            {
                boardState = value;
                this.InvalidateVisual();
            }
        }
        private const double FieldSize = 20;
        private const double LineThickness = 1;
        private const double inFieldSize = FieldSize - LineThickness;
        private const double holdingSize = 2 * inFieldSize / 3;



        // Render uruchamia się po każdej inwalidacji - zmiana rozmiaru okna, InvalidateVisual(), ...
        public override void Render(DrawingContext context)
        {
            base.Render(context);

            if (BoardState == null)
                return;

            double pixelBoardWidth = BoardState.Width * FieldSize + LineThickness;
            double pixelBoardHeight = BoardState.Height * FieldSize + LineThickness;

            double xScale = this.Bounds.Width / pixelBoardWidth;
            double yScale = this.Bounds.Height / pixelBoardHeight;
            double scale = Math.Min(xScale, yScale);

            double xTranslation = (this.Bounds.Width - pixelBoardWidth * scale) / 2;
            double yTranslation = (this.Bounds.Height - pixelBoardHeight * scale) / 2;
            // Najpierw przesunięcie by plansza była na środku, potem skalowanie by zmieściła się w kontrolce
            using (context.PushPreTransform(Matrix.CreateTranslation(xTranslation, yTranslation)))
            using (context.PushPreTransform(Matrix.CreateScale(scale, scale)))
            {
                // Całe rysowanie musi być tu
                ColorFields(context, pixelBoardWidth, pixelBoardHeight);

                double redGoalAreaBorder = FieldSize * BoardState.GoalAreaHeight + (double)LineThickness / 2;
                DrawBorder(context, ColorScheme.RedGoalAreaBorder, redGoalAreaBorder, pixelBoardWidth);
                double blueGoalAreaBorder = (int)pixelBoardHeight - FieldSize * BoardState.GoalAreaHeight - (double)LineThickness / 2;
                DrawBorder(context, ColorScheme.BlueGoalAreaBorder, blueGoalAreaBorder, pixelBoardWidth);

                DrawIds(context, pixelBoardHeight);
                if (BoardState.RedLeader != null) HiglightLeader(context, BoardState.RedLeader.Value, pixelBoardHeight);
                if (BoardState.BlueLeader != null) HiglightLeader(context, BoardState.BlueLeader.Value, pixelBoardHeight);
            }
        }

        private void HiglightLeader(DrawingContext context, (int x, int y) player, double pixelBoardHeight)
        {
            var text = new FormattedText
            {
                Text = FontPresets.LeaderCrown,
                Typeface = FontPresets.EmojiTypeface,
                TextAlignment = TextAlignment.Center,
                Constraint = new Size(inFieldSize, FontPresets.EmojiFontSize)
            };
            context.DrawText(ColorScheme.IdText,
                    new Point(
                        player.y * FieldSize + LineThickness,
                        pixelBoardHeight - (player.x + 1) * FieldSize - FontPresets.EmojiFontSize / 2),
                    text);

        }

        private void DrawBorder(DrawingContext context, ISolidColorBrush b, double height, double width)
        {
            var p = new Pen(b, LineThickness, DashStyle.Dash);
            context.DrawLine(p, new Point(0, height), new Point(width, height));
        }

        enum Holding
        {
            Sham,Piece,Nothing
        };

        private void ColorFields(DrawingContext context, double pixelBoardWidth, double pixelBoardHeight)
        {
            // Tło
            context.FillRectangle(ColorScheme.Border, new Rect(0, 0, pixelBoardWidth, pixelBoardHeight));

            // Pola
            for (int i = 0; i < BoardState.Height; i++)
                for (int j = 0; j < BoardState.Width; j++)
                {
                    var thisField = BoardState.Fields[i, j];
                    ISolidColorBrush fieldBrush = null;
                    Holding holding = Holding.Nothing;
                    switch (thisField)
                    {
                        case Field.Empty:
                            fieldBrush = ColorScheme.Empty;
                            break;
                        case Field.RedPlayerWithSham:
                            holding = Holding.Sham;
                            fieldBrush = ColorScheme.RedPlayer;
                            break;
                        case Field.RedPlayer:
                            fieldBrush = ColorScheme.RedPlayer;
                            break;
                        case Field.RedPlayerWithPiece:
                            holding = Holding.Piece;
                            fieldBrush = ColorScheme.RedPlayer;
                            break;
                        case Field.BluePlayerWithSham:
                            holding = Holding.Sham;
                            fieldBrush = ColorScheme.BluePlayer;
                            break;
                        case Field.BluePlayer:
                            fieldBrush = ColorScheme.BluePlayer;
                            break;
                        case Field.BluePlayerWithPiece:
                            holding = Holding.Piece;
                            fieldBrush = ColorScheme.BluePlayer;
                            break;
                        case Field.RedGoal:
                            fieldBrush = ColorScheme.RedGoal;
                            break;
                        case Field.BlueGoal:
                            fieldBrush = ColorScheme.BlueGoal;
                            break;
                        case Field.NonGoal:
                            fieldBrush = ColorScheme.NonGoal;
                            break;
                        case Field.Piece:
                            fieldBrush = ColorScheme.Piece;
                            break;
                        case Field.Sham:
                            fieldBrush = ColorScheme.ShamPiece;
                            break;
                        case Field.CoveredGoal:
                            fieldBrush = ColorScheme.CoveredGoal;
                            break;
                        default:
                            fieldBrush = Brushes.Pink;
                            break;
                    }
                    Rect rect = new Rect(
                                j * FieldSize + LineThickness,
                                pixelBoardHeight - (i + 1) * FieldSize, // współrzędne Y w DrawingContext rosną do dołu, więc odwrócenie
                                FieldSize - LineThickness,
                                FieldSize - LineThickness);
                    context.FillRectangle(fieldBrush, rect);

                    switch (holding)
                    {
                        case Holding.Sham:
                            DrawHolding(context, rect.TopLeft.X, rect.TopLeft.Y, ColorScheme.ShamPiece);
                            break;
                        case Holding.Piece:
                            DrawHolding(context, rect.TopLeft.X, rect.TopLeft.Y, ColorScheme.Piece);
                            break;
                        case Holding.Nothing:
                            break;
                    }
                }
        }

        private void DrawHolding(DrawingContext context, double x, double y, ISolidColorBrush color)
        {
            const double shift = (inFieldSize - holdingSize) / 2;
            context.FillRectangle(color,
                new Rect(
                    x + shift,
                    y + shift,
                    holdingSize,
                    holdingSize));
        }

        private void DrawIds(DrawingContext context, double pixelBoardHeight)
        {
            foreach (var (x, y, id) in BoardState.PlayerIds)
            {
                var text = new FormattedText
                {
                    Text = id.ToString(),
                    Typeface = FontPresets.IdTypeface,
                    TextAlignment = TextAlignment.Center,
                    Constraint = new Size(inFieldSize, FontPresets.IdFontSize)
                };
                context.DrawText(ColorScheme.IdText,
                        new Point(
                            y * FieldSize + LineThickness,
                            pixelBoardHeight - (x + 1) * FieldSize + (inFieldSize - FontPresets.IdFontSize)/2),
                        text);
            }
        }
    }

}
