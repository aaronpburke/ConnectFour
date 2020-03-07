using ConnectFour.DataLayer.Models;
using System.Linq;

namespace ConnectFour.DataLayer.Repositories.GameRepository
{
    public interface IGameRepository : IGenericRepository<string, Game> { }
    public class GameRepository : GenericRepository<string, Game>, IGameRepository
    {
        public GameRepository(DataContext context)
            : base(context)
        {

        }
    }
}
