// The Sixty Project
//
// Copyright 2013 Richard Morrison
// All Rights Reserved
//
// See COPYRIGHT.TXT
//

using Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class Player
    {
        public Player()
        {
            Hand = new List<ICard>();
            Pointers = new List<ICard>();
            Coins = 6;
        }

        public int Coins { get; private set; }

        public List<ICard> Hand { get; private set; }

        public bool IsDown { get; set; }

        public List<ICard> Pointers { get; private set; }

        public int ScoreHand()
        {
            return (int)Hand.Sum(c => c.Value);
        }

        public int ScorePlays()
        {
            return (int)Pointers.Sum(c => c.Value);
        }

        public int Score { get; set; }

        public bool Buys(List<ICard> deck, ICard card, Down down, out ICard card1, out ICard card2)
        {
            card1 = null;
            card2 = null;

            if (Coins == 0) return false;

            if (new Random().NextDouble() > ((Down.Downs.Count - Down.Downs.IndexOf(down)) / Down.Downs.Count * 3 +
                Coins / 6.0 + (IsDown ? -1 : 1) *3 ) / 7) return false;

            Coins--;
            Hand.Add(card);
            card1 = Deck.FlipTop(deck);
            card2 = Deck.FlipTop(deck);
            Hand.Add(card1);
            Hand.Add(card2);
            return true;
        }
    }
}
