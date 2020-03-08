using System.ComponentModel.DataAnnotations;
using static ConnectFour.DataLayer.Models.GameMove;

namespace ConnectFour.Api.Models.DTOs
{
    /// <summary>
    /// DTO used in <seealso cref="Controllers.GameActionsApiController.PlayMove"/>
    /// </summary>
    public class GameMoveDetails
    {
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
    }
}
