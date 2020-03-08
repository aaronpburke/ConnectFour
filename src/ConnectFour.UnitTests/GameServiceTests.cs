using ConnectFour.DataLayer.Models;
using ConnectFour.ServiceLayer.GameService;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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
    }
}
