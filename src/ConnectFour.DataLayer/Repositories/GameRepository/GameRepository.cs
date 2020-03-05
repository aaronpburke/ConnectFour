using ConnectFour.DataLayer.Models;

namespace ConnectFour.DataLayer.Repositories.GameRepository
{
    /// <summary>
    /// Collection of all games
    /// </summary>
    public class GameRepository : LocalCrudRepository<Game>, IGameRepository
    {
        /// <inheritdoc />
        protected override string GetKeyForItem(Game item)
        {
            return item.Id;
        }
    }
}
