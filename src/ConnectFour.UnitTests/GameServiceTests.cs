using ConnectFour.DataLayer.Models;
using ConnectFour.ServiceLayer;
using ConnectFour.ServiceLayer.GameService;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Xunit;

namespace ConnectFour.UnitTests
{
    internal static class DbTestHelper
    {
        /// <summary>
        /// Gets an EF DbContext, currently a fresh, in-memory
        /// SQLite database.
        /// </summary>
        /// <returns></returns>
        public static DataLayer.DataContext GetNewContext()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);
            connection.Open();

            // create in-memory context
            var builder = new DbContextOptionsBuilder<DataLayer.DataContext>();
            builder.UseSqlite(connection);

            var context = new DataLayer.DataContext(builder.Options);
            context.Database.EnsureCreated();

            return context;
        }
    }

    public class GameServiceTests
    {
        /*
         * This doesn't work -- throws null exception when trying to save. Debug later for "true" unit tests instead of
         * somewhat of an integration test since we're depending on Entity and SQLite for testing.
         * When revisiting, make mocked DbSets virtual and provide empty DataContext constructor.
         * 
        private Mock<DataLayer.DataContext> CreateMockDbContext(IEnumerable<Game> initialGames = null)
        {
            initialGames = (initialGames != null) ? initialGames : Enumerable.Empty<Game>();

            var games = initialGames.AsQueryable();

            var dbSet = new Mock<DbSet<Game>>();
            dbSet.As<IQueryable<Game>>().Setup(m => m.Provider).Returns(games.Provider);
            dbSet.As<IQueryable<Game>>().Setup(m => m.Expression).Returns(games.Expression);
            dbSet.As<IQueryable<Game>>().Setup(m => m.ElementType).Returns(games.ElementType);
            dbSet.As<IQueryable<Game>>().Setup(m => m.GetEnumerator()).Returns(games.GetEnumerator());

            var context = new Mock<DataLayer.DataContext>();
            context.Setup(c => c.Games).Returns(dbSet.Object);
            return context;
        }
        */

        [Fact]
        public void CreateGame()
        {
            using (var context = DbTestHelper.GetNewContext())
            {
                var sut = new GameService(context, new ServiceLayer.Models.GameOptions());
                NewGameDetails newGameDetails = new NewGameDetails()
                {
                    Columns = 4,
                    Rows = 4,
                    Players = new List<string>() { "Player1", "Player2" }
                };
                var createdGameId = sut.CreateNewGame(newGameDetails);
                createdGameId.Should().NotBeNullOrEmpty();

                var details = sut.GetGameDetails(createdGameId);
                details.Should().NotBeNull();
                details.Players.Should().BeEquivalentTo(newGameDetails.Players);
                details.State.Should().Be(Game.GameState.IN_PROGRESS);
                details.Winner.Should().BeNullOrEmpty();
            }
        }

        [Fact]
        public void DropPlayer()
        {
            using (var context = DbTestHelper.GetNewContext())
            {
                var sut = new GameService(context, new ServiceLayer.Models.GameOptions());
                NewGameDetails newGameDetails = new NewGameDetails()
                {
                    Columns = 4,
                    Rows = 4,
                    Players = new List<string>() { "Player1", "Player2" }
                };
                var createdGameId = sut.CreateNewGame(newGameDetails);
                createdGameId.Should().NotBeNullOrEmpty();

                // Verify initial players list
                {
                    var details = sut.GetGameDetails(createdGameId);
                    details.Should().NotBeNull();
                    details.Players.Should().BeEquivalentTo(newGameDetails.Players);
                }

                // Make sure an invalid player cannot be removed
                Action act = () => sut.RemovePlayer(createdGameId, "Invalid Player");
                act.Should().Throw<PlayerNotFoundException>();

                {
                    var details = sut.GetGameDetails(createdGameId);
                    details.Should().NotBeNull();
                    details.Players.Should().BeEquivalentTo(newGameDetails.Players);
                }

                // Remove player 2
                {
                    sut.RemovePlayer(createdGameId, "Player2");

                    // Verify
                    var details = sut.GetGameDetails(createdGameId);
                    details.Should().NotBeNull();
                    details.Players.Should().ContainSingle(p => p == "Player1");
                }
            }
        }

        [Fact]
        public void PlayerOutOfTurn()
        {
            using (var context = DbTestHelper.GetNewContext())
            {
                var sut = new GameService(context, new ServiceLayer.Models.GameOptions());
                NewGameDetails newGameDetails = new NewGameDetails()
                {
                    Columns = 4,
                    Rows = 4,
                    Players = new List<string>() { "Player1", "Player2" }
                };
                var createdGameId = sut.CreateNewGame(newGameDetails);
                createdGameId.Should().NotBeNullOrEmpty();

                // Valid play
                {
                    var move = sut.PlayMove(createdGameId, new GameMoveDetails() { Player = "Player1", Column = 0, Type = GameMove.MoveType.MOVE });
                    move.Should().NotBeNull();
                    move.MoveId.Should().Be(1);
                    move.Type.Should().Be(GameMove.MoveType.MOVE);
                    move.Column.Should().Be(0);
                }

                // Invalid play -- trying to play again and skipping Player2
                {
                    Action act = () => sut.PlayMove(createdGameId, new GameMoveDetails() { Player = "Player1", Column = 0, Type = GameMove.MoveType.MOVE });
                    act.Should().Throw<PlayerTurnException>();
                }

                // Invalid play -- Player isn't in this game
                {
                    Action act = () => sut.PlayMove(createdGameId, new GameMoveDetails() { Player = "Invalid Player", Column = 0, Type = GameMove.MoveType.MOVE });
                    act.Should().Throw<PlayerTurnException>();
                }

                // Valid play
                {
                    var move = sut.PlayMove(createdGameId, new GameMoveDetails() { Player = "Player2", Column = 0, Type = GameMove.MoveType.MOVE });
                    move.Should().NotBeNull();
                    move.MoveId.Should().Be(2);
                    move.Type.Should().Be(GameMove.MoveType.MOVE);
                    move.Column.Should().Be(0);
                }
            }
        }

        [Fact]
        public void PlaySimpleGame()
        {
            using (var context = DbTestHelper.GetNewContext())
            {
                var sut = new GameService(context, new ServiceLayer.Models.GameOptions());
                NewGameDetails newGameDetails = new NewGameDetails()
                {
                    Columns = 4,
                    Rows = 4,
                    Players = new List<string>() { "Player1", "Player2" }
                };
                var createdGameId = sut.CreateNewGame(newGameDetails);
                createdGameId.Should().NotBeNullOrEmpty();

                // Each player just plays straight into their own column and fills it except for the last piece.
                for (int i = 0; i < (newGameDetails.Rows - 1) * newGameDetails.Players.Count; i++)
                {
                    var column = i % newGameDetails.Players.Count;
                    var playerName = $"Player{column + 1}";
                    var move = sut.PlayMove(createdGameId, new GameMoveDetails() { Player = playerName, Column = column, Type = GameMove.MoveType.MOVE });
                    move.Should().NotBeNull();
                    move.MoveId.Should().Be(i + 1);
                    move.Type.Should().Be(GameMove.MoveType.MOVE);
                    move.Column.Should().Be(column);

                    // Verify the move
                    {
                        var moveDetails = sut.GetMove(createdGameId, i);
                        moveDetails.Should().NotBeNull();
                        moveDetails.Player.Should().Be(playerName);
                        moveDetails.Type.Should().Be(GameMove.MoveType.MOVE);
                        moveDetails.Column.Should().Be(column);
                    }

                    // Verify the game does not yet have a winner
                    {
                        var details = sut.GetGameDetails(createdGameId);
                        details.Should().NotBeNull();
                        details.State.Should().Be(Game.GameState.IN_PROGRESS);
                    }
                }

                // Winning play - Player 1 wins!
                {
                    var move = sut.PlayMove(createdGameId, new GameMoveDetails() { Player = "Player1", Column = 0, Type = GameMove.MoveType.MOVE });
                    move.Should().NotBeNull();
                    move.MoveId.Should().Be((newGameDetails.Rows * newGameDetails.Players.Count) - 1);
                    move.Type.Should().Be(GameMove.MoveType.MOVE);
                    move.Column.Should().Be(0);

                    var details = sut.GetGameDetails(createdGameId);
                    details.Should().NotBeNull();
                    details.State.Should().Be(Game.GameState.DONE);
                    details.Winner.Should().Be("Player1");
                }

                // Get all the moves
                var allMoves = sut.GetMoves(createdGameId);
                allMoves.Should().NotBeNull();
                allMoves.Should().HaveCount((newGameDetails.Rows * newGameDetails.Players.Count) - 1);  // First row is full, second row has all but one
                allMoves.Should().OnlyHaveUniqueItems();
            }
        }
    }
}