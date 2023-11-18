using System;
using System.Collections.Generic;

namespace MineSweeper;

public class MineSweeperGame
{
    # region Constants

    public enum CellState
    {
        Hidden,
        Flagged,
        Revealed
    }

    public enum CellContent
    {
        Empty,
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Mine
    }

    public class Difficulty
    {
        public int Width { get; }
        public int Height { get; }
        public int Mines { get; }

        public Difficulty(int width, int height, int mines)
        {
            Width = width;
            Height = height;
            Mines = mines;
        }
    }

    public static readonly Difficulty Easy = new Difficulty(9, 9, 10);
    public static readonly Difficulty Medium = new Difficulty(16, 16, 40);
    public static readonly Difficulty Hard = new Difficulty(30, 16, 99);

    public class Cell
    {
        public CellState State { get; set; }
        public CellContent Content { get; set; }

        public static IEnumerable<(int x, int y)> Neighbors => new (int x, int y)[]
        {
            (-1, -1), (0, -1), (1, -1),
            (-1, 0),           (1, 0),
            (-1, 1),  (0, 1),  (1, 1)
        };
    }

    # endregion Constants

    private readonly Cell[,] _board;

    public int Width => _board.GetLength(0);
    public int Height => _board.GetLength(1);
    public Difficulty DifficultyProperty { get; } // Named to avoid conflict with the Difficulty enum
    public Cell this[int x, int y] => _board[x, y];
    public int TilesRemaining { get; private set; }
    public bool Started { get; private set; }

    public MineSweeperGame(Difficulty difficulty)
    {
        DifficultyProperty = difficulty;
        _board = new Cell[difficulty.Width, difficulty.Height];

        TilesRemaining = difficulty.Width * difficulty.Height - difficulty.Mines;

        // Initialize the board
        for (var x = 0; x < difficulty.Width; x++)
        for (var y = 0; y < difficulty.Height; y++)
            _board[x, y] = new Cell
            {
                State = CellState.Hidden,
                Content = CellContent.Empty
            };
    }

    public void Start(int clickX, int clickY)
    {
        // Place mines
        PlaceMines(DifficultyProperty.Mines, clickX, clickY);

        // Calculate numbers
        CalculateNumbers();

        // Reveal the cell the user clicked
        Reveal(clickX, clickY, out _);

        Started = true;
    }

    private void PlaceMines(int mineCount, int clickX, int clickY)
    {
        var random = new Random();

        for (var i = 0; i < mineCount; i++)
        {
            var x = random.Next(_board.GetLength(0) - 1);
            var y = random.Next(_board.GetLength(1) - 1);

            // Don't place a mine where the user clicked
            if (x == clickX && y == clickY)
                goto Retry;

            // Also don't place a mine around the user's click
            foreach (var (dx, dy) in Cell.Neighbors)
                if (x + dx == clickX && y + dy == clickY)
                    goto Retry;

            // Don't place a mine where there already is one
            if (_board[x, y].Content == CellContent.Mine)
                goto Retry;

            _board[x, y].Content = CellContent.Mine;

            continue;
            Retry:
            i--;
        }
    }

    private void CalculateNumbers()
    {
        for (var x = 0; x < _board.GetLength(0); x++)
        {
            for (var y = 0; y < _board.GetLength(1); y++)
            {
                if (_board[x, y].Content != CellContent.Mine)
                    continue;

                // If the cell is a mine, increment the number of all its neighbors
                foreach (var (dx, dy) in Cell.Neighbors)
                {
                    if (x + dx < 0 || x + dx >= _board.GetLength(0) ||
                        y + dy < 0 || y + dy >= _board.GetLength(1))
                        continue;

                    if (_board[x + dx, y + dy].Content != CellContent.Mine)
                        _board[x + dx, y + dy].Content++;
                }
            }
        }
    }

    public void Reveal(int x, int y, out bool gameOver)
    {
        // Check if the coordinates are valid (not guaranteed because of recursion)
        if (x < 0 || x >= _board.GetLength(0) ||
            y < 0 || y >= _board.GetLength(1))
        {
            gameOver = false;
            return;
        }

        var cell = _board[x, y];

        gameOver = cell.Content == CellContent.Mine && cell.State != CellState.Flagged;

        // If the cell is already revealed or flagged, don't do anything
        if (cell.State is CellState.Revealed or CellState.Flagged)
            return;

        cell.State = CellState.Revealed;
        TilesRemaining--;

        if (cell.Content != CellContent.Empty)
            return;

        // If the cell is empty, reveal all its neighbors (they can't be mines)
        foreach (var (dx, dy) in Cell.Neighbors)
            Reveal(x + dx, y + dy, out gameOver);
    }

    public void Flag(int x, int y)
    {
        var cell = _board[x, y];

        // If the cell is already revealed, don't do anything
        if (cell.State == CellState.Revealed)
            return;

        cell.State = cell.State == CellState.Flagged ? CellState.Hidden : CellState.Flagged;
    }
    
    public void RevealMines()
    {
        for (var x = 0; x < _board.GetLength(0); x++)
        for (var y = 0; y < _board.GetLength(1); y++)
            if (_board[x, y].Content == CellContent.Mine)
                _board[x, y].State = CellState.Revealed;
    }
}