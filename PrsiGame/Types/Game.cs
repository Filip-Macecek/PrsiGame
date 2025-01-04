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

    public GameState State { get; set; }

    public GameSetup Setup { get; }

    public Stack<Turn> Turns { get; }

    public Stack<CardId> LickPile { get; set; }

    public Stack<CardId> DiscardPile { get; }

    public IReadOnlyList<Player> Players { get; }

    public Queue<Player> PlayerQueue { get;  }

    public Stack<Player> Winners { get; }

    public Result AddTurn(Turn turn)
    {
        if (State != GameState.Started)
        {
            return new InvalidGameStateError("The game is finished.");
        }

        var currentPlayer = PlayerQueue.Peek();
        if (turn.Player != currentPlayer)
        {
            return new InvalidPlayerError("It's not the player's turn.");
        }

        var validationResult = turn switch
        {
            CardTurn cardTurn => cardTurn.Validate(DiscardPile.Peek().ToCardObject(), GetCurrentColor(), TopCardAppliesToCurrentTurn()),
            SkipTurn skipTurn => skipTurn.Validate(Turns.Peek()),
            LickTurn lickTurn => lickTurn.Validate(Turns.Peek().ToTurnType(), GetRequiredLicks()),
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

        var singlePlayerLeft = (Players.Count - Winners.Count) == 1;
        State = singlePlayerLeft ? GameState.Finished : State;

        Turns.Push(turn);
        return Result.Ok();
    }

    public bool TopCardAppliesToCurrentTurn()
    {
        var turnsCopy = Turns.MakeCopy();

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
        var turnsCopy = Turns.MakeCopy();
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

    public int? GetRequiredLicks()
    {
        var turnsCopy = Turns.MakeCopy();
        var requiredLicks = (int?) null;

        while (requiredLicks == null && turnsCopy.Count > 0)
        {
            var turn = turnsCopy.Pop();
            if (turn is SevenTurn sevenTurn)
            {
                requiredLicks = requiredLicks.HasValue ? requiredLicks + 2 : 2;
            }
            else
            {
                break;
            }
        }

        return requiredLicks;
    }

    private void ApplyTurn(Turn turn)
    {
        switch (turn)
        {
            case CardTurn cardTurn:
                DiscardPile.Push(cardTurn.Card.Id);
                turn.Player.CardsOnHand.Remove(cardTurn.Card.Id);
                break;
            case SkipTurn:
                break;
            case LickTurn lickTurn:
                for (var i = 0; i < lickTurn.LickCount; i++)
                {
                    if (!LickPile.TryPop(out var lickedCard))
                    {
                        LickPile = new Stack<CardId>(DiscardPile.ToList());
                        DiscardPile.Clear();
                    }
                    turn.Player.CardsOnHand.Add(lickedCard);
                }

                break;
        }
    }
}
