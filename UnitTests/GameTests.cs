// The Sixty Project
//
// Copyright 2013 Richard Morrison
// All Rights Reserved
//
// See COPYRIGHT.TXT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Cards;

namespace UnitTests
{
    [TestFixture]
    public class GameTests
    {
        static Card QueenOfHeart = new Card(Suit.Heart, Face.Queen, 10, false);
        static Card Joker = new Card(Suit.Large, Face.Joker, 10, true);
        static Card Two = new Card(Suit.Diamond, Face.Two, 10, true);

        public static object[] WildCards = { Joker, Two };

        #region Wild Cards

        [Test]
        public void ThatNoWildsIsEmpty()
        {
            var items = Game.Game.GroupWilds(QueenOfHeart);

            Assert.IsFalse(items.Any(), "there should not be any wilds.");
        }

        [TestCaseSource("WildCards")]
        public void ThatWildsAreIdentified(Card wildCard)
        {
            var items = Game.Game.GroupWilds(wildCard, QueenOfHeart)
                .ToList();


            Assert.AreEqual(1, items.Count, "Should have been a single combination of wilds");
            var inner = items[0]
                .ToList();

            Assert.AreEqual(1, inner.Count, "Should have been a single list of cards in inner");
            Assert.AreEqual(new List<Card>() {wildCard}, inner[0], "list doesn't contain expected wild card");
        }

        [Test]
        public void ThatFirstListWildsSeparately()
        {
            var items = Game.Game.GroupWilds(Two, Two, Two, Two)
                .ToList();

            var inner = items[0].ToList();

            var expected = new List<Card>() { Two};

            Assert.AreEqual(new List<List<Card>> { expected, expected, expected, expected },
                inner, "first list doesn't contain each wild card in its own list");
            //Assert.AreEqual(new List<Card>() { wildCard }, items[0], "list doesn't contain expected wild card");
        }

        [Test]
        public void ThatReminingListsGroupWilds()
        {
            var items = Game.Game.GroupWilds(Two, Two, Two, Two)
                .ToList();

            Assert.AreEqual(3, items.Count, "Should have been 3 lists of lists");
            var inner = items[1].ToList();

            var expectedSingle = new List<Card>() { Two };
            var expectedDouble = new List<Card>() { Two, Two };

            Assert.AreEqual(new List<List<Card>> { expectedDouble, expectedSingle, expectedSingle },
                inner, "2nd list doesn't contain a set of Twos");
            
            inner = items[2].ToList();

            Assert.AreEqual(new List<List<Card>> { expectedDouble, expectedDouble },
                inner, "3rd list doesn't contain 2 sets of Twos");

            //Assert.AreEqual(new List<Card>() { wildCard }, items[0], "list doesn't contain expected wild card");
        }

        [Test]
        public void ThatJokerCombinesWithTwo()
        {
            var items = Game.Game.GroupWilds(Joker, Two)
                .ToList();

            Assert.AreEqual(2, items.Count, "Should have been 2 lists of lists");

            var inner = items[1].ToList();
            Assert.AreEqual(new List<List<ICard>> { new List<ICard> { Joker, Two } }, inner);

        }

        [Test]
        public void ThatJokerDonotCombine()
        {
            var items = Game.Game.GroupWilds(Joker, Joker)
                .ToList();

            Assert.AreEqual(1, items.Count, "Should have been 2 lists of lists");

            var inner = items[0].ToList();
            Assert.AreEqual(new List<List<ICard>> { new List<ICard> { Joker }, new List<ICard> {Joker} }, inner);
        }

        #endregion


    }
}
