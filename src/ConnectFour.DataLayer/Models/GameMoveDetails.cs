using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using static ConnectFour.DataLayer.Models.GameMove;

namespace ConnectFour.DataLayer.Models
{
    /// <summary>
    /// DTO used in <seealso cref="Controllers.GameActionsApiController.PlayMove"/>
    /// </summary>
    public class GameMoveDetails
    {
        /// <summary>
        /// 
        /// </summary>
        public string Player { get; set; }

        /// <summary>
        /// Type of the player move
        /// </summary>
        [Required]
        public MoveType Type { get; set; }

        /// <summary>
        /// The column the token was dropped into, if <see cref="Type"/> is <see cref="MoveType.MOVE"/>
        /// </summary>
        [Required]
        public int Column { get; set; }

        /// <summary>
        /// Creates a new DTO from the corresponding Entity class
        /// </summary>
        /// <param name="game"><see cref="Game"/> containing the move (with player information)</param>
        /// <param name="move"><see cref="GameMove"/> to </param>
        /// <returns></returns>
        public static GameMoveDetails FromEntity(Game game, GameMove move)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (move == null)
            {
                throw new ArgumentNullException(nameof(move));
            }

            if (game.Players == null)
            {
                throw new ArgumentNullException("Player information not yet loaded for the game!");
            }

            return new GameMoveDetails()
            {
                Player = game.Players.FirstOrDefault(p => p.Id == move.PlayerId)?.Name,
                Column = move.Column,
                Type = move.Type
            };
        }
    }
}
