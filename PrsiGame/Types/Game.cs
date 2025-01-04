using FluentResults;
using PrsiGame.Common;
using PrsiGame.Errors;

namespace PrsiGame.Types;

public record Game
{
    internal Game(GameState state,
        GameSetup setup,
        Stack<Turn> turns,
        Stack<CardId> lickPile,
        Stack<CardId> discardPile,
        IReadOnlyList<Player> players,
        Queue<Player> playerQueue)
    {
        State = state;
        Setup = setup;
        Turns = turns;
        LickPile = lickPile;
        DiscardPile = discardPile;
        Players = players;
        PlayerQueue = playerQueue;
        Winners = new Stack<Player>();
    }

    public GameState State { get; private set; }

    public GameSetup Setup { get; }

    public Stack<Turn> Turns { get; }

    public Stack<CardId> LickPile { get; }

    public Stack<CardId> DiscardPile { get; }

    public IReadOnlyList<Player> Players { get; }

    public Queue<Player> PlayerQueue { get;  }

    public Stack<Player> Winners { get; }

    public Result AddTurn(Turn turn)
    {
        var currentPlayer = PlayerQueue.Peek();
        if (turn.Player != currentPlayer)
        {
            return new InvalidPlayerError("It's not the player's turn.");
        }

        var validationResult = turn switch
        {
            CardTurn cardTurn => cardTurn.Validate(DiscardPile.Peek().ToCardObject(), GetCurrentColor(), TopCardAppliesToCurrentTurn()),
            _ => throw new ArgumentOutOfRangeException(nameof(turn))
        };

        if (validationResult.IsFailed)
        {
            return validationResult;
        }

        ApplyTurn(turn);

        if (turn.Player.CardsOnHand.Count == 0)
        {
            Winners.Push(turn.Player);
        }

        _ = PlayerQueue.Dequeue();
        if (PlayerQueue.Count == 0)
        {
            foreach (var player in Players.Except(Winners))
            {
                PlayerQueue.Enqueue(player);
            }
        }

        var singlePlayerLeft = (Players.Count - Winners.Count) > 0;
        State = singlePlayerLeft ? GameState.Finished : State;

        Turns.Push(turn);
        return Result.Ok();
    }

    public bool TopCardAppliesToCurrentTurn()
    {
        var turnsCopy = new Stack<Turn>(Turns);

        if (!turnsCopy.TryPop(out var turn))
        {
            return false;
        }

        return turn switch
        {
            AceTurn aceTurn => true,
            LickTurn lickTurn => false,
            QueenTurn queenTurn => false,
            RegularTurn regularTurn => false,
            SevenTurn sevenTurn => true,
            SkipTurn skipTurn => false,
            _ => false
        };
    }

    public CardColor GetCurrentColor()
    {
        var turnsCopy = new Stack<Turn>(Turns);
        var pickedColor = (CardColor?) null;

        while (pickedColor == null && turnsCopy.Count > 0)
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

    private void ApplyTurn(Turn turn)
    {
        if (turn is CardTurn cardTurn)
        {
            DiscardPile.Push(cardTurn.Card.Id);
            var removeResult = turn.Player.CardsOnHand.Remove(cardTurn.Card.Id);
        }
    }
}
