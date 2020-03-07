using ConnectFour.DataLayer.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ConnectFour.UnitTests
{
    public class GameBoardTests
    {
        [Fact]
        public void FullColumnRejectsMove()
        {
            var gameBoard = new GameBoard(rows: 4, columns: 4, winningChainLength: 4);
            
            for (int play = 0; play < 4; play++)
            {
                gameBoard.DropToken(column: 0, player: 0).Should().BeTrue();
            }
            gameBoard.DropToken(column: 0, player: 0).Should().BeFalse();
        }

        [Fact]
        public void FullBoard()
        {
            const int rows = 4;
            const int columns = 4;
            const int chainLength = 4;  // doesn't matter
            var gameBoard = new GameBoard(rows: rows, columns: columns, winningChainLength: chainLength);

            // Fill up the first M columns
            for (int column = 0; column < (columns - 1); column++)
            {
                for (int row = 0; row < rows; row++)
                {
                    gameBoard.DropToken(column: column, player: 0).Should().BeTrue();
                    gameBoard.IsFull().Should().BeFalse();
                }
            }

            // Place N-1 tokens in the last column
            for (int row = 0; row < (rows - 1); row++)
            {
                gameBoard.DropToken(column: (columns - 1), player: 0).Should().BeTrue();
                gameBoard.IsFull().Should().BeFalse();
            }

            // Place the last token to fill up the board
            gameBoard.DropToken(column: (columns - 1), player: 0).Should().BeTrue();
            gameBoard.IsFull().Should().BeTrue();
        }

        [Fact]
        public void FirstColumnWin()
        {
            const int rows = 4;
            const int columns = 4;
            const int chainLength = 4;
            var gameBoard = new GameBoard(rows: rows, columns: columns, winningChainLength: chainLength);

            // Drop N-1 tokens in the first column; the winner should only exist on the last token
            for (int play = 0; play < (columns - 1); play++)
            {
                gameBoard.DropToken(column: 0, player: 0).Should().BeTrue();
                gameBoard.HasWinner().Should().BeFalse();
            }

            // Play the winning token
            gameBoard.DropToken(column: 0, player: 0).Should().BeTrue();
            gameBoard.HasWinner().Should().BeTrue();
            gameBoard.GetWinner().Should().Be(0);
        }

        [Fact]
        public void FirstRowWin()
        {
            const int rows = 4;
            const int columns = 4;
            const int chainLength = 4;
            var gameBoard = new GameBoard(rows: rows, columns: columns, winningChainLength: chainLength);

            // Drop 4 tokens in the first row; the winner should only exist on the last token
            for (int column = 0; column < (columns - 1); column++)
            {
                gameBoard.DropToken(column: column, player: 0).Should().BeTrue();
                gameBoard.HasWinner().Should().BeFalse();
            }

            // Play the winning token
            gameBoard.DropToken(column: (columns - 1), player: 0).Should().BeTrue();
            gameBoard.HasWinner().Should().BeTrue();
            gameBoard.GetWinner().Should().Be(0);
        }
    }
}
