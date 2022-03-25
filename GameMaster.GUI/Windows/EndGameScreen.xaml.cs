using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using GameMaster.GUI.Models;
using System;

namespace GameMaster.GUI.Windows
{
    public class EndGameScreen : Window
    {
        TextBlock 
            _winMessage,
            _score,
            _moreInfo;
        Button _exitButton;

        public EndGameScreen() { }
        public EndGameScreen(EndGameInfo info)
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            switch (info.winningTeam)
            {
                case CommunicationUtils.Structures.TeamColor.Blue:
                    _winMessage.Foreground = Brushes.DarkBlue;
                    _winMessage.Text = "Team Blue Wins!";
                    break;
                case CommunicationUtils.Structures.TeamColor.Red:
                    _winMessage.Foreground = Brushes.DarkRed;
                    _winMessage.Text = "Team Red Wins!";
                    break;
                default:
                    break;
            }

            _score.Text = info.redScore + ":" + info.blueScore;
            _moreInfo.Text = "Game length: " + info.time.ToString(@"hh\:mm\:ss\.ff");

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            _winMessage = this.FindControl<TextBlock>("winMessage");
            _score = this.FindControl<TextBlock>("score");
            _moreInfo = this.FindControl<TextBlock>("moreInfo");

            _exitButton = this.FindControl<Button>("exitButton");

            _exitButton.Click += _exitButton_Click;

        }

        private void _exitButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
