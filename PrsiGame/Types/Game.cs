using System;
using System.Collections.Generic;
using System.Linq;
using FluentResults;
using PrsiGame.Common;
using PrsiGame.Errors;

namespace PrsiGame.Types
{
    public class Game
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

            Result validationResult;
            switch (turn)
            {
                case CardTurn cardTurn:
                    validationResult = cardTurn.Validate(DiscardPile.Peek().ToCardObject(), GetCurrentColor(),
                        TopCardAppliesToCurrentTurn());
                    break;
                case SkipTurn skipTurn:
                    validationResult = skipTurn.Validate(Turns.Count > 0 ? Turns.Peek() : null);
                    break;
                case LickTurn lickTurn:
                    validationResult = lickTurn.Validate(Turns.Count > 0 ? Turns.Peek().ToTurnType() : (TurnType?)null, GetRequiredLicks());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(turn));
            }

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

            if (turnsCopy.Count == 0)
            {
                return false;
            }

            var turn = turnsCopy.Pop();
            switch (turn)
            {
                case AceTurn aceTurn:
                    return true;
                case LickTurn lickTurn:
                case QueenTurn queenTurn:
                case RegularTurn regularTurn:
                    return false;
                case SevenTurn sevenTurn:
                    return true;
                case SkipTurn skipTurn:
                default:
                    return false;
            }
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
                case SkipTurn _:
                    break;
                case LickTurn lickTurn:
                    for (var i = 0; i < lickTurn.LickCount; i++)
                    {
                        if (LickPile.Count == 0)
                        {
                            LickPile = new Stack<CardId>(DiscardPile.ToList());
                            DiscardPile.Clear();
                        }
                        var lickedCard = LickPile.Pop();
                        turn.Player.CardsOnHand.Add(lickedCard);
                    }
                    break;
            }
        }
    }
}
