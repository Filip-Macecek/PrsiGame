using System.Collections;
using PrsiGame.Common;

namespace PrsiGame.Types;

public record Game(
    GameState State,
    GameSetup Setup,
    Stack<Turn> Turns,
    Stack<CardId> LickPile,
    Stack<CardId> DiscardPile,
    IReadOnlyList<Player> Players
)
{
    public CardColor GetCurrentColor()
    {
        var turnsCopy = new Stack<Turn>(Turns);
        var pickedColor = (CardColor?) null;

        while (pickedColor != null || turnsCopy.Count == 0)
        {
            var turn = turnsCopy.Pop();

            switch (turn)
            {
                case AceTurn aceTurn:
                    pickedColor = aceTurn.Card.Color;
                    break;
                case LickTurn _:
                    break;
                case QueenTurn queenTurn:
                    pickedColor = queenTurn.PickedColor;
                    break;
                case RegularTurn regularTurn:
                    pickedColor = regularTurn.Card.Color;
                    break;
                case SevenTurn sevenTurn:
                    pickedColor = sevenTurn.Card.Color;
                    break;
                case SkipTurn _:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(turn));
            }
        }

        return pickedColor ?? DiscardPile.Peek().ToColor();
    }
}
