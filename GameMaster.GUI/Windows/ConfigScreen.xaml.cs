using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace GameMaster.GUI
{
    public class ConfigScreen : Window
    {
        private Button
            _cancelButton,
            _nextButton;

        private TextBox
            _IPTextBox,
            _portTextBox,

            _movePenaltyTextBox,
            _askPenaltyTextBox,
            _discoverPenaltyTextBox,
            _putPenaltyTextBox,
            _checkPenaltyTextBox,
            _respondPenaltyTextBox,

            _widthTextBox,
            _heightTextBox,
            _goalHeightTextBox,
            _goalsTextBox,
            _piecesTextBox,
            _chanceShamTextBox;

        private IClassicDesktopStyleApplicationLifetime _desktop;

        public ConfigScreen() { }
        public ConfigScreen(IClassicDesktopStyleApplicationLifetime desktop)
        {
            this._desktop = desktop;
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            _cancelButton = this.FindControl<Button>("cancelButton");
            _nextButton = this.FindControl<Button>("nextButton");

            _cancelButton.Click += _cancelButton_Click;
            _nextButton.Click += _nextButton_Click;

            _IPTextBox = this.FindControl<TextBox>("IPTextBox");
            _portTextBox = this.FindControl<TextBox>("portTextBox");

            _movePenaltyTextBox = this.FindControl<TextBox>("movePenaltyTextBox");
            _askPenaltyTextBox = this.FindControl<TextBox>("askPenaltyTextBox");
            _discoverPenaltyTextBox = this.FindControl<TextBox>("discoverPenaltyTextBox");
            _putPenaltyTextBox = this.FindControl<TextBox>("putPenaltyTextBox");
            _checkPenaltyTextBox = this.FindControl<TextBox>("checkPenaltyTextBox");
            _respondPenaltyTextBox = this.FindControl<TextBox>("respondPenaltyTextBox");

            _widthTextBox = this.FindControl<TextBox>("widthTextBox");
            _heightTextBox = this.FindControl<TextBox>("heightTextBox");
            _goalHeightTextBox = this.FindControl<TextBox>("goalHeightTextBox");
            _goalsTextBox = this.FindControl<TextBox>("goalsTextBox");
            _piecesTextBox = this.FindControl<TextBox>("piecesTextBox");
            _chanceShamTextBox = this.FindControl<TextBox>("chanceShamTextBox");

            FillExampleValues();
        }

        private void _nextButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Przekazanie kontroli do okna oczekiwania
            //var waitingScreen = new WaitingScreen(_desktop);
            //waitingScreen.Position = this.Position;
            //_desktop.MainWindow = waitingScreen;
            //_desktop.MainWindow.Show();
            this.Close();
        }

        private void _cancelButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Close();
        }

        void FillExampleValues()
        {
            _IPTextBox.Text = "192.168.0.0";
            _portTextBox.Text = "12313";
            _movePenaltyTextBox.Text = "1500";
            _askPenaltyTextBox.Text = "1000";
            _discoverPenaltyTextBox.Text = "700";
            _putPenaltyTextBox.Text = "500";
            _checkPenaltyTextBox.Text = "700";
            _respondPenaltyTextBox.Text = "1000";
            _widthTextBox.Text = "40";
            _heightTextBox.Text = "40";
            _goalHeightTextBox.Text = "5";
            _goalsTextBox.Text = "5";
            _piecesTextBox.Text = "10";
            _chanceShamTextBox.Text = "0.50";
        }
    }
}
