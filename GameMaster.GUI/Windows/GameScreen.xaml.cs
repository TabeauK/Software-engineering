using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GameMaster.GUI.Controls;
using Avalonia.Diagnostics;
using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using CommunicationUtils;
using System.Collections.ObjectModel;
using System.Diagnostics;
using GameMaster.GUI.Models;
using GameMaster.GUI.Windows;
using Avalonia.Media;
using CommunicationUtils.Structures;

namespace GameMaster.GUI
{
    public class GameScreen : Window
    {
        private TextBlock
            _redScoreTextBlock,
            _blueScoreTextBlock,
            _timeTextBlock,
            _timeLabelTextBlock,
            _redPlayersCount,
            _bluePlayersCount;

        ListBox
            _colorsListBox;

        BoardCanvas
            _boardCanvas;

        private const int logEntriesLimit = 1000;
        private const int timerPeriod = 30;

        // nie thread-safe - używać tylko z UIThread
        Stopwatch clockStopwatch;

        Thread drawingThread;
        Thread gameMasterThread;

        CancellationTokenSource drawingThreadSource = new CancellationTokenSource();

        public GameScreen()
        {
            this.InitializeComponent();
            ShowLegend();

#if DEBUG
            this.AttachDevTools();
#endif
            ShowLegend();

            gameMasterThread = new Thread(() =>
            {
                var services = new ServiceCollection();
                GUIServiceProvider.ConfigureGMServices(services);
                using (ServiceProvider serviceProvider = services.BuildServiceProvider())
                {
                    GameMaster gameMaster = serviceProvider.GetService<GameMaster>();
                    GUIServiceProvider.GameMaster = gameMaster;
                    gameMaster.GameBoardGenerated += GameMaster_GameBoardGenerated;
                    gameMaster.GameStarted += GameMaster_GameStarted;
                    gameMaster.GameEnded += GameMaster_GameEnded;
                    gameMaster.Start();
                }

            });

            drawingThread = new Thread(() =>
            {
                CancellationToken token = drawingThreadSource.Token;

                while (!token.IsCancellationRequested)
                {
                    ClockUpdate();

                    var boardView = GUIServiceProvider.GameMaster.GetBoardView();

                    Dispatcher.UIThread.Post(() => Update(boardView));
                    Thread.Sleep(30);
                }
            });

            gameMasterThread.Start();
            _timeLabelTextBlock.Text = "Waiting";

            clockStopwatch = new Stopwatch();

        }

        private void GameMaster_GameBoardGenerated(object sender, EventArgs e)
        {
            drawingThread.Start();
        }

        private void Update(BoardView boardView)
        {
            _redScoreTextBlock.Text = boardView.RedScore.ToString();
            _blueScoreTextBlock.Text = boardView.BlueScore.ToString();

            _bluePlayersCount.Text = boardView.BluePlayerCount + " / " + boardView.GamePlayerCount;
            _redPlayersCount.Text = boardView.RedPlayerCount + " / " + boardView.GamePlayerCount;


            _boardCanvas.BoardState = boardView;

        }

        private void GameMaster_GameEnded(object sender, GameEndArgs args)
        {
            drawingThreadSource.Cancel();
            // ostatni boardview
            var state = GUIServiceProvider.GameMaster.GetBoardView();
            Dispatcher.UIThread.Post(() =>
            {
                Update(state);
                _timeLabelTextBlock.Text = "Finished";
                clockStopwatch.Stop();
                var info = new EndGameInfo(args.winner, state.RedScore, state.BlueScore, clockStopwatch.Elapsed);
                var end = new EndGameScreen(info);
                end.ShowDialog(this);
                end.Closed += End_Closed;
            });
        }

        private void End_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void GameMaster_GameStarted(object sender, EventArgs e)
        {
            Dispatcher.UIThread.Post(() =>
            {
                clockStopwatch.Start();
                _timeLabelTextBlock.Text = "Playing";
            });
        }

        private void ClockUpdate()
        {
            var text = clockStopwatch.Elapsed.ToString(@"mm\:ss");
            Dispatcher.UIThread.Post(() => _timeTextBlock.Text = text);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            _redScoreTextBlock = this.FindControl<TextBlock>("redScoreTextBlock");
            _blueScoreTextBlock = this.FindControl<TextBlock>("blueScoreTextBlock");
            _timeTextBlock = this.FindControl<TextBlock>("timeTextBlock");
            _timeLabelTextBlock = this.FindControl<TextBlock>("timeLabelTextBlock");

            _boardCanvas = this.FindControl<BoardCanvas>("board");

            _colorsListBox = this.FindControl<ListBox>("colorsListBox");

            _redPlayersCount = this.FindControl<TextBlock>("redPlayersCount");
            _bluePlayersCount = this.FindControl<TextBlock>("bluePlayersCount");
        }

        private void ShowLegend()
        {
            var crownBrush = new VisualBrush(new TextBlock() { Text = FontPresets.LeaderCrown,
                FontFamily = FontPresets.EmojiFontFamily, FontSize = 40 });

            _colorsListBox.Items = new[] {
                new {ItemColor = (IBrush)ColorScheme.Empty, ItemDesc = "Empty Field"},
                new {ItemColor = (IBrush)ColorScheme.BlueGoal, ItemDesc = "Blue Goal"},
                new {ItemColor = (IBrush)ColorScheme.RedGoal, ItemDesc = "Red Goal"},
                new {ItemColor = (IBrush)ColorScheme.CoveredGoal, ItemDesc = "Covered Goal"},
                new {ItemColor = (IBrush)ColorScheme.BluePlayer, ItemDesc = "Blue Player"},
                new {ItemColor = (IBrush)ColorScheme.RedPlayer, ItemDesc = "Red Player"},
                new {ItemColor = (IBrush)ColorScheme.Piece, ItemDesc = "Piece"},
                new {ItemColor = (IBrush)ColorScheme.ShamPiece, ItemDesc = "Sham"},
                new {ItemColor = (IBrush)ColorScheme.NonGoal, ItemDesc = "Not A Goal"},
                new {ItemColor = (IBrush)crownBrush, ItemDesc = "Leader"},
            };
        }

        protected override bool HandleClosing()
        {
            drawingThreadSource.Cancel();
            GUIServiceProvider.GameMaster.GameStarted -= GameMaster_GameStarted;
            return base.HandleClosing();
        }
    }
}
