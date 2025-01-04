using FluentAssertions;
using PrsiGame.Errors;
using PrsiGame.Types;

namespace PrsiGame.UnitTests;

public class GameTypeTests
{
    [Fact]
    public void AddTurn_FirstTurn_Succeeds()
    {
        var game = PrsiEngine.NewGame(PrsiEngine.ShuffleCards(), new GameSetup(PlayerCount: 5, PlayerCardCount: 5));

        var player = game.Players.First();
        player.CardsOnHand[0] = CardId.QueenOfSpades;

        var queenTurn = QueenTurn.Create(player, QueenCard.Create(CardId.QueenOfSpades).Value, CardColor.Hearts);
        var result = game.AddTurn(queenTurn);

        result.IsSuccess.Should().BeTrue();
        game.Turns.Count.Should().Be(1);
        game.Turns.First().Should().Be(queenTurn);
        game.PlayerQueue.Count.Should().Be(4);
        game.PlayerQueue.Contains(player).Should().BeFalse();
    }

    [Fact]
    public void AddTurn_FirstTurn_RemovesCardFromPlayerHand()
    {
        var game = PrsiEngine.NewGame(PrsiEngine.ShuffleCards(), new GameSetup(PlayerCount: 5, PlayerCardCount: 5));

        var player = game.Players.First();
        player.CardsOnHand[0] = CardId.QueenOfSpades;

        var queenTurn = QueenTurn.Create(player, QueenCard.Create(CardId.QueenOfSpades).Value, CardColor.Hearts);
        var result = game.AddTurn(queenTurn);

        result.IsSuccess.Should().BeTrue();
        player.CardsOnHand.Should().HaveCount(4);
        player.CardsOnHand.Should().NotContain(CardId.QueenOfSpades);
    }

    [Fact]
    public void AddTurn_WhenPlayerDiscardsLastCard_IsAddedToWinners()
    {
        var game = PrsiEngine.NewGame(PrsiEngine.ShuffleCards(), new GameSetup(PlayerCount: 5, PlayerCardCount: 5));

        var player = game.Players.First();
        player.CardsOnHand.Clear();
        player.CardsOnHand.Add(CardId.QueenOfSpades);

        var queenTurn = QueenTurn.Create(player, QueenCard.Create(CardId.QueenOfSpades).Value, CardColor.Spades);
        var result = game.AddTurn(queenTurn);

        result.IsSuccess.Should().BeTrue();
        game.Winners.Should().HaveCount(1);
        game.Winners.First().Should().Be(player);
        game.PlayerQueue.Count.Should().Be(4);
        game.PlayerQueue.Contains(player).Should().BeFalse();
        player.CardsOnHand.Should().HaveCount(0);
    }

    [Fact]
    public void AddTurn_WhenPlayersHandIsEmpty_Fails()
    {
        var game = PrsiEngine.NewGame(PrsiEngine.ShuffleCards(), new GameSetup(PlayerCount: 5, PlayerCardCount: 5));

        var player = game.Players.First();
        player.CardsOnHand.Clear();

        var queenTurn = QueenTurn.Create(player, QueenCard.Create(CardId.QueenOfSpades).Value, CardColor.Hearts);
        var result = game.AddTurn(queenTurn);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void AddTurn_WhenItsNotPlayersTurn_Fails()
    {
        var game = PrsiEngine.NewGame(PrsiEngine.ShuffleCards(), new GameSetup(PlayerCount: 5, PlayerCardCount: 5));

        var queenTurn = QueenTurn.Create(game.Players[1], QueenCard.Create(CardId.QueenOfSpades).Value, CardColor.Hearts);
        var result = game.AddTurn(queenTurn);

        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void AddTurn_WhenInvalidTurnIsMade_Fails()
    {
        var game = PrsiEngine.NewGame(PrsiEngine.ShuffleCards(), new GameSetup(PlayerCount: 2, PlayerCardCount: 5));

        var firstPlayer = game.Players[0];
        firstPlayer.CardsOnHand[0] = CardId.QueenOfSpades;
        var firstTurn = QueenTurn.Create(firstPlayer, QueenCard.Create(CardId.QueenOfSpades).Value, CardColor.Hearts);;
        var firstResult = game.AddTurn(firstTurn);
        var secondPlayer = game.Players[1];
        secondPlayer.CardsOnHand[0] = CardId.NineOfDiamonds;
        var secondTurn = RegularTurn.Create(secondPlayer, RegularCard.Create(CardId.NineOfDiamonds).Value);;
        var secondResult = game.AddTurn(secondTurn);

        game.DiscardPile.Count.Should().Be(2);
        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeFalse();
        secondResult.Errors.First().Should().BeOfType(typeof(InvalidTurnError));
        secondResult.Errors.First().Message.Should().Contain("color");
    }

    [Fact]
    public void AddTurn_WhenEverybodyTurnedInARound_QueuesPlayersForNewRound()
    {
        var game = PrsiEngine.NewGame(PrsiEngine.ShuffleCards(), new GameSetup(PlayerCount: 2, PlayerCardCount: 5));

        var firstPlayer = game.Players[0];
        firstPlayer.CardsOnHand[0] = CardId.QueenOfSpades;
        var firstTurn = QueenTurn.Create(firstPlayer, QueenCard.Create(CardId.QueenOfSpades).Value, CardColor.Hearts);;
        var firstResult = game.AddTurn(firstTurn);

        var secondPlayer = game.Players[1];
        secondPlayer.CardsOnHand[0] = CardId.NineOfHearts;
        var secondTurn = RegularTurn.Create(secondPlayer, RegularCard.Create(CardId.NineOfHearts).Value);;
        var secondResult = game.AddTurn(secondTurn);

        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeTrue();
        game.DiscardPile.Count.Should().Be(3);
        game.PlayerQueue.Distinct().Should().HaveCount(2);
    }

    [Fact]
    public void AddTurn_WhenRoundEndsWithWin_DoesntCueWinner()
    {
        var game = PrsiEngine.NewGame(PrsiEngine.ShuffleCards(), new GameSetup(PlayerCount: 2, PlayerCardCount: 5));

        var firstPlayer = game.Players[0];
        firstPlayer.CardsOnHand[0] = CardId.QueenOfSpades;
        var firstTurn = QueenTurn.Create(firstPlayer, QueenCard.Create(CardId.QueenOfSpades).Value, CardColor.Hearts);;
        var firstResult = game.AddTurn(firstTurn);

        var secondPlayer = game.Players[1];
        secondPlayer.CardsOnHand.Clear();
        secondPlayer.CardsOnHand.Add(CardId.NineOfHearts);
        var secondTurn = RegularTurn.Create(secondPlayer, RegularCard.Create(CardId.NineOfHearts).Value);;
        var secondResult = game.AddTurn(secondTurn);

        firstResult.IsSuccess.Should().BeTrue();
        secondResult.IsSuccess.Should().BeTrue();
        game.PlayerQueue.Should().HaveCount(1);
        game.Winners.Should().HaveCount(1);
        game.Winners.Should().Contain(secondPlayer);
    }

    [Fact]
    public void AddTurn_WhenRoundEndsWithWinSinglePlayerInGame_SetsStateToFinished()
    {
        var game = PrsiEngine.NewGame(PrsiEngine.ShuffleCards(), new GameSetup(PlayerCount: 3, PlayerCardCount: 5));

        var firstPlayer = game.Players[0];
        firstPlayer.CardsOnHand[0] = CardId.QueenOfSpades;
        var firstTurn = QueenTurn.Create(firstPlayer, QueenCard.Create(CardId.QueenOfSpades).Value, CardColor.Hearts);
        game.AddTurn(firstTurn);

        var secondPlayer = game.Players[1];
        secondPlayer.CardsOnHand[0] = CardId.NineOfHearts;
        var secondTurn = RegularTurn.Create(secondPlayer, RegularCard.Create(CardId.NineOfHearts).Value);
        game.AddTurn(secondTurn);

        var thirdPlayer = game.Players[2];
        thirdPlayer.CardsOnHand.Clear();
        thirdPlayer.CardsOnHand.Add(CardId.QueenOfHearts);
        var thirdTurn = QueenTurn.Create(thirdPlayer, QueenCard.Create(CardId.QueenOfHearts).Value, CardColor.Diamonds);
        var r = game.AddTurn(thirdTurn);

        game.Winners.Should().HaveCount(1);
        game.Winners.Should().Contain(thirdPlayer);
        game.State.Should().Be(GameState.Finished);
    }
}
