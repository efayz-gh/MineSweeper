using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MineSweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static readonly SolidColorBrush HiddenCellColor = Brushes.DarkGray;
        private static readonly SolidColorBrush RevealedCellColor = Brushes.White;
        private static readonly SolidColorBrush FlaggedCellColor = Brushes.Red;

        private MineSweeperGame _game;

        private MineSweeperGame.Difficulty Difficulty
        {
            get => _game.DifficultyProperty;
            set
            {
                ChangeGridSize(value.Width, value.Height);
                _game = new MineSweeperGame(value);
                TilesRemainingTextBlock.Text = $"{_game.Remaining} tiles remaining";
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            _game = new MineSweeperGame(MineSweeperGame.Medium);
            DifficultyComboBox.SelectedIndex = 1;
        }

        # region Drawing

        // Changes the size of the grid (called when the difficulty is changed)
        private void ChangeGridSize(int width, int height)
        {
            GameGrid.Children.Clear();
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();

            for (var i = 0; i < height; i++)
                GameGrid.RowDefinitions.Add(new RowDefinition());

            for (var x = 0; x < width; x++)
            {
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition());

                for (var y = 0; y < height; y++)
                {
                    var button = new Button
                    {
                        Content = "",
                        Tag = (x, y),
                        BorderThickness = new Thickness(1),
                        Background = HiddenCellColor
                    };
                    button.Click += Cell_Click;
                    button.MouseRightButtonDown += Cell_RightClick;
                    Grid.SetRow(button, y);
                    Grid.SetColumn(button, x);
                    GameGrid.Children.Add(button);
                }
            }

            InvalidateVisual();
        }

        // Updates the UI to reflect the state of the game
        private void UpdateUI()
        {
            TilesRemainingTextBlock.Text = $"{_game.Remaining} tiles remaining";

            for (var x = 0; x < _game.Width; x++)
            for (var y = 0; y < _game.Height; y++)
            {
                var cell = _game[x, y];
                var button = (Button)GameGrid.Children.Cast<UIElement>()
                    .First(e => Grid.GetRow(e) == y && Grid.GetColumn(e) == x);

                button.Background = cell.State switch
                {
                    MineSweeperGame.CellState.Hidden => HiddenCellColor,
                    MineSweeperGame.CellState.Flagged => FlaggedCellColor,
                    MineSweeperGame.CellState.Revealed => RevealedCellColor,
                    _ => throw new ArgumentOutOfRangeException()
                };

                button.Content = cell.State switch
                {
                    MineSweeperGame.CellState.Hidden => "",
                    MineSweeperGame.CellState.Flagged => "🚩",
                    MineSweeperGame.CellState.Revealed => cell.Content switch
                    {
                        MineSweeperGame.CellContent.Empty => "",
                        MineSweeperGame.CellContent.One => "1",
                        MineSweeperGame.CellContent.Two => "2",
                        MineSweeperGame.CellContent.Three => "3",
                        MineSweeperGame.CellContent.Four => "4",
                        MineSweeperGame.CellContent.Five => "5",
                        MineSweeperGame.CellContent.Six => "6",
                        MineSweeperGame.CellContent.Seven => "7",
                        MineSweeperGame.CellContent.Eight => "8",
                        MineSweeperGame.CellContent.Mine => "💣",
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

            InvalidateVisual();
        }

        # endregion Drawing

        private void NewGameButton_Click(object sender, RoutedEventArgs e)
        {
            _game = new MineSweeperGame(Difficulty);

            UpdateUI();
        }

        private void DifficultyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            Difficulty = (MineSweeperGame.Difficulty)((ComboBoxItem)DifficultyComboBox.SelectedItem).Tag;

        private void Cell_Click(object sender, RoutedEventArgs e)
        {
            var (x, y) = ((int, int))((Button)sender).Tag;

            if (!_game.Started)
                _game.Start(x, y);

            _game.Reveal(x, y, out _);

            // TODO: Handle game over

            UpdateUI();
        }

        private void Cell_RightClick(object sender, RoutedEventArgs e)
        {
            var (x, y) = ((int, int))((Button)sender).Tag;

            if (!_game.Started)
                _game.Start(x, y);

            _game.Flag(x, y);

            UpdateUI();
        }
    }
}