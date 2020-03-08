using ConnectFour.DataLayer.Models;

namespace ConnectFour.ServiceLayer.Models
{
    /// <summary>
    /// Configurable game options
    /// </summary>
    public class GameOptions
    {
        /// <summary>
        /// Number of connecting tokens to determine a winner
        /// </summary>
        public int WinningChainLength { get; set; } = GameBoard.DEFAULT_WINNING_CHAIN_LENGTH;
    }
}
