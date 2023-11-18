using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MineSweeper;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private static readonly SolidColorBrush HiddenCellColor = Brushes.DarkGray;
    private static readonly SolidColorBrush RevealedCellColor = Brushes.Azure;
    private static readonly SolidColorBrush FlaggedCellColor = Brushes.Tomato;
    
    private static readonly SolidColorBrush MineColor = Brushes.Red;
    

    private MineSweeperGame _game;

    private int _elapsedSeconds;
    private readonly Timer _timer = new(1000);

    private MineSweeperGame.Difficulty Difficulty
    {
        get => _game.DifficultyProperty;
        set
        {
            ChangeGridSize(value.Width, value.Height);
            _game = new MineSweeperGame(value);
            TilesRemainingTextBlock.Text = $"{_game.TilesRemaining} tiles remaining";
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        _game = new MineSweeperGame(MineSweeperGame.Medium);
        DifficultyComboBox.SelectedIndex = 1;
            
        _timer.AutoReset = true;
        _timer.Elapsed += (_, _) =>
        {
            Dispatcher.Invoke(() =>
            {
                _elapsedSeconds++;
                TimeTextBlock.Text = $"Time: {_elapsedSeconds}";
            });
        };
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
        TilesRemainingTextBlock.Text = $"{_game.TilesRemaining} tiles remaining";

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

            switch (cell.State)
            {
                case MineSweeperGame.CellState.Hidden:
                    button.Content = "";
                    break;
                case MineSweeperGame.CellState.Flagged:
                    button.Content = "🚩";
                    break;
                case MineSweeperGame.CellState.Revealed:
                    switch (cell.Content)
                    {
                        case MineSweeperGame.CellContent.Empty:
                            button.Content = "";
                            break;
                        case MineSweeperGame.CellContent.One:
                            button.Content = "1";
                            break;
                        case MineSweeperGame.CellContent.Two:
                            button.Content = "2";
                            break;
                        case MineSweeperGame.CellContent.Three:
                            button.Content = "3";
                            break;
                        case MineSweeperGame.CellContent.Four:
                            button.Content = "4";
                            break;
                        case MineSweeperGame.CellContent.Five:
                            button.Content = "5";
                            break;
                        case MineSweeperGame.CellContent.Six:
                            button.Content = "6";
                            break;
                        case MineSweeperGame.CellContent.Seven:
                            button.Content = "7";
                            break;
                        case MineSweeperGame.CellContent.Eight:
                            button.Content = "8";
                            break;
                        case MineSweeperGame.CellContent.Mine:
                            button.Content = "💣";
                            button.Background = MineColor;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
        {
            _game.Start(x, y);
            _timer.Start();
        }

        _game.Reveal(x, y, out var gameOver);
            
        // lose
        if (gameOver)
        {
            _game.RevealMines();
            UpdateUI();
                
            _timer.Stop();

            if (MessageBox.Show("Game over!", "", MessageBoxButton.OK) == MessageBoxResult.OK)
                NewGameButton_Click(null!, null!);
                
            _elapsedSeconds = 0;
            TimeTextBlock.Text = $"Time: {_elapsedSeconds}";
                
            return;
        }

        UpdateUI();
            
        // win
        if (_game.TilesRemaining == 0)
        {
            _timer.Stop();
                
            if (MessageBox.Show("You win!", "", MessageBoxButton.OK) == MessageBoxResult.OK)
                NewGameButton_Click(null!, null!);
                
            _elapsedSeconds = 0;
            TimeTextBlock.Text = $"Time: {_elapsedSeconds}";
        }
    }

    private void Cell_RightClick(object sender, RoutedEventArgs e)
    {
        if (!_game.Started)
            return;

        var (x, y) = ((int, int))((Button)sender).Tag;

        _game.Flag(x, y);

        UpdateUI();
    }
}