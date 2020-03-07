using System;
using System.Collections.Generic;

namespace ConnectFour.DataLayer.Models
{
    public partial class GameBoard : IEntity<int>
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key back to the containing <see cref="Game"/>
        /// </summary>
        public string GameId { get; set; }

        /// <summary>
        /// Two-dimensional game board representing rows and columns.
        /// Default value of null means no piece is yet played in that location.
        /// Otherwise, the value is equal to the player index in the containing game.
        /// </summary>
        /// <remarks>This is allocated as [columns][rows] instead of the more conventional [rows][columns]
        /// since the style of this game largely cares more about columns than rows, so indexing is easier.
        /// </remarks>
        private readonly int?[,] _board;

        /// <summary>
        /// Number of columns on the game board
        /// </summary>
        public int Columns { get; protected set; }

        /// <summary>
        /// Number of rows on the game board
        /// </summary>
        public int Rows { get; protected set; }

        /// <summary>
        /// Default winning chain length
        /// </summary>
        public const int DEFAULT_WINNING_CHAIN_LENGTH = 4;

        /// <summary>
        /// Number of connecting tokens to determine a winner
        /// </summary>
        public int WinningChainLength { get; protected set; } = DEFAULT_WINNING_CHAIN_LENGTH;

        /// <summary>
        /// Player ID of the winning player, if any.
        /// </summary>
        public int Winner { get; private set; } = NO_WINNER;
        public const int NO_WINNER = -1;

        // -------------
        // Relationships

        public virtual ICollection<GameMove> Moves { get; private set; }
    }

    public partial class GameBoard
    {
        public GameBoard(int rows, int columns, int winningChainLength)
        {
            if (rows <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rows));
            }

            if (columns <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columns));
            }

            Rows = rows;
            Columns = columns;

            WinningChainLength = winningChainLength;

            _board = new int?[Columns, Rows];
        }

        /// <summary>
        /// Drops a <paramref name="player"/> token in the specified <paramref name="column"/>.
        /// </summary>
        /// <param name="column">Column to drop the token into</param>
        /// <param name="player">Player dropping the token</param>
        /// <returns>true if the token was successfully played; otherwise false</returns>
        public bool DropToken(int column, int player)
        {
            // Find the first empty row to place the token
            for (int row = 0; row < Rows; row++)
            {
                // If it doesn't have a value, there's no token -- place the token
                if (!_board[column, row].HasValue)
                {
                    _board[column, row] = player;
                    return true;
                }
            }

            // Couldn't find an empty row to place the token
            return false;
        }

        /// <summary>
        /// Checks if a winner is found with a horizontal ("row") victory.
        /// </summary>
        /// <returns>The player ID if a winner is found; otherwise NO_WINNER</returns>
        private int CheckRowWin()
        {
            // Horizontal check
            for (int row = 0; row < Rows; row++)
            {
                var firstToken = _board[0, row];
                int chainLength = 1;
                for (int col = 1; col < Columns && chainLength < WinningChainLength; col++, chainLength++)
                {
                    // If there's no token, the chain is broken!
                    if (!_board[col, row].HasValue)
                    {
                        chainLength = 0;
                        continue;
                    }

                    // Different player found; the chain is reset!
                    if (_board[col, row].Value != firstToken)
                    {
                        chainLength = 1;
                        firstToken = _board[col, row].Value;
                        continue;
                    }
                }

                // We checked the whole row and didn't find a break -- WINNER!
                if (chainLength >= WinningChainLength)
                {
                    return firstToken.Value;
                }
            }

            // Checked every row and found nothing
            return NO_WINNER;
        }

        /// <summary>
        /// Checks if a winner is found with a vertical ("column") victory.
        /// </summary>
        /// <returns>The player ID if a winner is found; otherwise NO_WINNER</returns>
        private int CheckColumnWin()
        {
            for (int col = 0; col < Columns; col++)
            {
                var firstToken = _board[col, 0];
                int chainLength = firstToken.HasValue ? 1 : 0;
                for (int row = 1; row < Rows && chainLength < WinningChainLength; row++, chainLength++)
                {
                    // If there's no token, the chain is broken!
                    if (!_board[col, row].HasValue)
                    {
                        chainLength = 0;
                        continue;
                    }

                    // Different player found; the chain is reset!
                    if (_board[col, row].Value != firstToken)
                    {
                        chainLength = 1;
                        firstToken = _board[col, row].Value;
                        continue;
                    }
                }

                // We checked the whole row and didn't find a break -- WINNER!
                if (chainLength >= WinningChainLength)
                {
                    return firstToken.Value;
                }
            }

            // Checked every column and found nothing
            return NO_WINNER;
        }

        /// <summary>
        /// Checks if a winner is found with a diagonal victory.
        /// </summary>
        /// <returns>The player ID if a winner is found; otherwise NO_WINNER</returns>
        private int CheckDiagonalWin()
        {
            var minDimension = (Rows < Columns) ? Rows : Columns;

            // Check left-to-right upward diagonal 
            {
                var firstToken = _board[0, 0];
                int chainLength = firstToken.HasValue ? 1 : 0;
                for (int i = 1; i < minDimension && chainLength < WinningChainLength; i++, chainLength++)
                {
                    // If there's no token, the chain is broken!
                    if (!_board[i, i].HasValue)
                    {
                        chainLength = 0;
                        continue;
                    }

                    // Different player found; the chain is reset!
                    if (_board[i, i].Value != firstToken)
                    {
                        chainLength = 1;
                        firstToken = _board[i, i].Value;
                        continue;
                    }
                }

                // We checked the whole diagonal and didn't find a break -- WINNER!
                if (chainLength >= WinningChainLength)
                {
                    return firstToken.Value;
                }
            }

            // Check left-to-right downward diagonal 
            {
                var firstToken = _board[minDimension - 1, minDimension - 1];
                int chainLength = firstToken.HasValue ? 1 : 0;
                for (int i = minDimension - 1; i >= 0 && chainLength < WinningChainLength; i--, chainLength++)
                {
                    // If there's no token, the chain is broken!
                    if (!_board[i, i].HasValue)
                    {
                        chainLength = 0;
                        continue;
                    }

                    // Different player found; the chain is reset!
                    if (_board[i, i].Value != firstToken)
                    {
                        chainLength = 1;
                        firstToken = _board[i, i].Value;
                        continue;
                    }
                }

                // We checked the whole diagonal and didn't find a break -- WINNER!
                if (chainLength >= WinningChainLength)
                {
                    return firstToken.Value;
                }
            }

            // Checked both diagonals and found nothing
            return NO_WINNER;
        }

        /// <summary>
        /// Attempts to find a game winner, if any.
        /// </summary>
        /// <returns>The player ID if a winner is found; otherwise NO_WINNER</returns>
        public int GetWinner()
        {
            // If we've already calculated a winner and nothing has changed, don't waste the time to find it again
            if (Winner != NO_WINNER)
            {
                return Winner;
            }

            Winner = CheckColumnWin();
            if (Winner != NO_WINNER)
            {
                return Winner;
            }

            Winner = CheckRowWin();
            if (Winner != NO_WINNER)
            {
                return Winner;
            }

            Winner = CheckDiagonalWin();
            if (Winner != NO_WINNER)
            {
                return Winner;
            }

            return NO_WINNER;
        }

        /// <summary>
        /// Checks whether the game has a winning player
        /// </summary>
        /// <returns>true if the game has a winning player, otherwise false</returns>
        public bool HasWinner() => GetWinner() != NO_WINNER;

        /// <summary>
        /// Checks if the entire game board is full (i.e., game is over with no winner)
        /// </summary>
        /// <returns>true if all columns are full; otherwise false</returns>
        public bool IsFull()
        {
            // Check the top row of every column -- if they all have a value, then the board is full
            for (int col = 0; col < Columns; col++)
            {
                if (!_board[col, Rows - 1].HasValue)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
