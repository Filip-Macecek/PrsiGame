using FluentAssertions;
using PrsiGame.Types;

namespace PrsiGame.UnitTests;

public class AITests
{
    [Fact]
    public void WhenAceApplies_Skips()
    {
        var player1 = new Player([CardId.AceOfClubs, CardId.TenOfClubs]);
        var player2 = new Player([CardId.EightOfHearts, CardId.QueenOfDiamonds]);
        var game = GameFactory.NewGame([player1, player2], CardId.EightOfClubs);
        game.AddTurnUnsafe(AceTurn.Create(player1, AceCard.Create(CardId.AceOfClubs).Value));

        var aiTurn = AI.Turn(game);

        aiTurn.Should().BeOfType(typeof(SkipTurn));
    }

    [Fact]
    public void WhenAceAppliesAndHasAce_PlaysTheAce()
    {
        var player1 = new Player([CardId.AceOfClubs, CardId.TenOfClubs]);
        var player2 = new Player([CardId.EightOfHearts, CardId.AceOfDiamonds]);
        var game = GameFactory.NewGame([player1, player2], CardId.EightOfClubs);
        game.AddTurnUnsafe(AceTurn.Create(player1, AceCard.Create(CardId.AceOfClubs).Value));

        var aiTurn = AI.Turn(game);

        aiTurn.Should().BeOfType(typeof(AceTurn));
        var aceTurn = aiTurn as AceTurn;
        aceTurn.Card.Id.Should().Be(CardId.AceOfDiamonds);
    }

    [Fact]
    public void WhenSevenApplies_LicksCorrectAmount()
    {
        var player1 = new Player([CardId.SevenOfDiamonds, CardId.TenOfClubs]);
        var player2 = new Player([CardId.EightOfHearts, CardId.SevenOfClubs]);
        var game = GameFactory.NewGame([player1, player2], CardId.EightOfDiamonds);
        game.AddTurnUnsafe(SevenTurn.Create(player1, SevenCard.Create(CardId.SevenOfDiamonds).Value));
        game.AddTurnUnsafe(SevenTurn.Create(player2, SevenCard.Create(CardId.SevenOfClubs).Value));

        var aiTurn = AI.Turn(game);

        aiTurn.Should().BeOfType(typeof(LickTurn));
        var lickTurn = aiTurn as LickTurn;
        lickTurn.LickCount.Should().Be(4);
    }

    [Fact]
    public void WhenRegularCardOnTop_PlaysValueMatchingCardOnHand()
    {
        var player1 = new Player([CardId.EightOfHearts, CardId.NineOfHearts]);
        var player2 = new Player([CardId.NineOfDiamonds, CardId.SevenOfClubs]);
        var game = GameFactory.NewGame([player1, player2], CardId.TenOfHearts);
        game.AddTurnUnsafe(RegularTurn.Create(player1, RegularCard.Create(CardId.NineOfHearts).Value));

        var aiTurn = AI.Turn(game);

        aiTurn.Should().BeOfType(typeof(RegularTurn));
        var regularTurn = aiTurn as RegularTurn;
        regularTurn.Card.Id.Should().Be(CardId.NineOfDiamonds);
    }

    [Fact]
    public void WhenRegularCardOnTop_PlaysColorMatchingCardOnHand()
    {
        var player1 = new Player([CardId.EightOfHearts, CardId.NineOfHearts]);
        var player2 = new Player([CardId.JackOfHearts, CardId.SevenOfClubs]);
        var game = GameFactory.NewGame([player1, player2], CardId.TenOfHearts);
        game.AddTurnUnsafe(RegularTurn.Create(player1, RegularCard.Create(CardId.NineOfHearts).Value));

        var aiTurn = AI.Turn(game);

        aiTurn.Should().BeOfType(typeof(RegularTurn));
        var regularTurn = aiTurn as RegularTurn;
        regularTurn.Card.Id.Should().Be(CardId.JackOfHearts);
    }

    [Fact]
    public void WhenNoCardToPlay_Licks()
    {
        var player1 = new Player([CardId.EightOfHearts, CardId.NineOfHearts]);
        var player2 = new Player([CardId.EightOfDiamonds, CardId.SevenOfClubs]);
        var game = GameFactory.NewGame([player1, player2], CardId.TenOfHearts);
        game.AddTurnUnsafe(RegularTurn.Create(player1, RegularCard.Create(CardId.NineOfHearts).Value));

        var aiTurn = AI.Turn(game);

        aiTurn.Should().BeOfType(typeof(LickTurn));
        var lickTurn = aiTurn as LickTurn;
        lickTurn.LickCount.Should().Be(1);
    }
}
