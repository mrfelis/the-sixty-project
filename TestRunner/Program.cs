// The Sixty Project
//
// Copyright 2013 Richard Morrison
// All Rights Reserved
//
// See COPYRIGHT.TXT
//

using Cards;
using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var scoreBoard = new ScoreBoard();

            foreach (var down in Down.Downs)
            {
                var deck = Deck.Create(2, Deck.DeckStyle.TwoIsWild)
                    .Shuffle()
                    .ToList();

                var table = new Table();

                var players = //new Player[5];
                    new List<Player>();
                for (int i = 0; i < 5; i++)
                    players.Add(new Player());
                //players[i] = new List<ICard>();

                var singlePlayerHand = players.Select(p =>
                    {
                        var l = new List<ICard>[1];
                        l[0] = p.Hand;
                        return l;
                    })
                    .ToArray();

                Deck.Deal(deck, 13, () => Deck.Create(1, Deck.DeckStyle.NoJokers | Deck.DeckStyle.TwoIsWild)
                    .ToList(), players.Select(p => p.Hand));
                var round = 1;
                var complete = false;

                var discard = Deck.FlipTop(deck);
                table.Discard(discard);
                Console.WriteLine("Player X deals hand and places {0} on top of discard pile.", NameCard(discard));
                Console.WriteLine();

                while (!complete)
                {
                    Console.WriteLine(string.Format("{0} sets of {1}",
                        down.NumberOfSets, down.MinimumInSet));
                    Console.WriteLine("                                *** Round {0} ***", round++);
                    Console.WriteLine();



                    for (int i = 0; i < players.Count; i++)
                    {
                        //Display(string.Format("Player {0}", i + 1), players[i].Hand.OrderByDescending(c => c.Face).ToList());
                        ICard card1, card2;
                        var buyer = Game.Game.Buy(deck, discard, down, players, players[i], out card1, out card2);
                        if (buyer != null)
                        {
                            var buyerIndex = players.IndexOf(buyer) + 1;
                            Console.WriteLine("Player {0} buys {1} and picks up: {2} and {3}", buyerIndex, NameCard(discard),
                                NameCard(card1), NameCard(card2));
                            Console.WriteLine("Palyer {0} has {1} dime{2} left", buyerIndex,
                                buyer.Coins > 0 ? buyer.Coins.ToString() : "no",
                                buyer.Coins == 1 ? string.Empty : "s");
                        }

                        Display("Hand", Deck.CountSets(players[i].Hand)/*.Where(p => p.Value > 1)*/.OrderByDescending(p => p.Value));
                        Deck.Deal<ICard>(deck, 1, null /* null shouldn't be needed but C# can't find the proper overload */,
                            singlePlayerHand[i]);
                        if (buyer == players[i])
                        {
                            Console.WriteLine("Player {0} buys on own turn and doesn't draw", i + 1);
                        }
                        else
                        {
                            Console.WriteLine("Player {1} picks up {0}.", NameCard(players[i].Hand.Last()), i + 1);
                        }

                        //Display("Wilds", players[i].Hand.Where(p => p.IsWild).ToList());

                        List<List<ICard>> sets;
                        var hand = players[i].Hand;

                        if (!players[i].IsDown)
                        {
                            players[i].IsDown = down.MakeDown(hand, out sets);
                            Console.WriteLine("Player {0} down.", players[i].IsDown ? "made" : "didn't make");

                            //Console.WriteLine(string.Format("{0} sets of {1}: {2}",
                            //    down.NumberOfSets, down.MinimumInSet,
                            //    made ? "yes" : "no"));
                            table.Laydown(players[i], sets);

                            foreach (var set in sets.WithIndex())
                            {
                                Display(string.Format("Set {0}", set.Key), set.Value);
                                players[i].Pointers.AddRange(set.Value);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Player is on board.");
                        }

                        if (players[i].IsDown)
                        {
                            var laydown = table.Laydown(hand, true);

                            var plays = hand.MakeSets();
                            table.Laydown(players[i], plays);

                            if (plays.Any())
                            {
                                Console.WriteLine("Playing sets of 3");
                            }
                            foreach (var set in plays.WithIndex())
                            {
                                Display(string.Format("Set {0}", set.Key), set.Value);
                                players[i].Pointers.AddRange(set.Value);
                            }

                            laydown.AddRange(table.Laydown(hand, false));
                            if (laydown.Any())
                            {
                                Display("Player lays down", laydown);
                                players[i].Pointers.AddRange(laydown);
                            }

                        }

                        //table.Laydown(hand);
                        discard = hand.SelectDiscard();
                        table.Discard(discard);

                        if (discard != null)
                        {
                            Console.WriteLine("Player discards {0}.", NameCard(discard));
                        }
                        else
                            Console.WriteLine("Player floats");


                        //Display("hand", hand);
                        Display("Hand", Deck.CountSets(players[i].Hand)/*.Where(p => p.Value > 1)*/.OrderByDescending(p => p.Value));
                        Console.WriteLine("---");

                        complete = !hand.Any() && discard != null;

                        if (complete)
                        {
                            Console.WriteLine("Player {0} discards final card, ending the hand.", i + 1);
                            break;
                        }
                    }

                    Console.WriteLine("===");
                    Pause();
                }

                Console.WriteLine();
                Console.WriteLine("Scoring Hand:");

                foreach (var player in players)
                {
                    int hand = player.ScoreHand();
                    int plays = player.ScorePlays();
                    player.Score += plays;
                    player.Score -= hand;
                    int indexPlayer = players.IndexOf(player) + 1;
                    if (hand == 0)
                    {
                        Console.WriteLine("Player {0} scores {1} points", indexPlayer, plays);
                    }
                    else
                    {
                        Console.WriteLine("Player {0} scores {1} points but throws {2} points away for a total of {3} points played",
                            indexPlayer, plays, hand, plays - hand);
                    }
                }

                scoreBoard.SetScores(down, players);

                Console.WriteLine();
                Console.WriteLine("Score:");
                Console.Write("Down \t");
                for (int i = 0; i < players.Count; i++)
                {
                    Console.Write("\tPlayer {0}", i + 1);
                }
                Console.WriteLine();

                var downs = new List<Down>(Down.Downs);

                foreach (var list in scoreBoard.GetScores())
                {
                    var down1 = downs[0];
                    Console.Write("{0} Set{1} of {2}", down1.NumberOfSets, down1.NumberOfSets == 1 ? string.Empty : "s", down1.MinimumInSet);

                    downs.RemoveAt(0);
                    foreach (int i in list)
                    {
                        Console.Write("\t{0,8}", i);
                    }
                    Console.WriteLine();
                }

                Console.Write("\t");
                for (int i = 0; i < players.Count; i++)
                {
                    Console.Write("\t--------", i + 1);
                }
                Console.WriteLine();

                Console.Write("Total\t");
                foreach (int i in scoreBoard.GetTotals())
                {
                    Console.Write("\t{0,8}", i);
                }
                Console.WriteLine();


                Console.WriteLine();
                Console.WriteLine("===");
                Console.WriteLine();

                Pause();
            }

            Console.WriteLine("Player {0} wins!", scoreBoard.GetTotals()
                .WithIndex(1)
                .OrderByDescending(p => p.Key)
                .Select(p => p.Value)
                .First());

            Pause();

        }

        private static void Display(string title,IEnumerable< KeyValuePair<Face, int>> set)
        {
            Display(title, set.Select(p => string.Format("{0}: {1}", p.Key, p.Value )).ToList());
        }

        static string NameCard(ICard c)
        {
            return c.Face == Face.Joker
                ? string.Format("{1} {0}", c.Face, c.Suit) :
           string.Format("{0} of {1}", c.Face, c.Suit);
        }

        static void Display(string p, List<ICard> list)
        {
            Display(p, list.Select(c => NameCard(c)).ToList());
        }

        static void Test(string[] args)
        {
            var l = new List<string> { "Ace", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Jack", "Queen", "King" };

            Display("Original", l);
            for (int i = 0; i < 6; i++)
            {
                Display((i + 1).ToString(), Deck.Shuffle(l));
            }
            Display("Original", l);



            var p1 = new List<string>();
            var p2 = new List<string>();

            var deck = new List<string>(l);

            deck.Deal(4, p1, p2);
            Console.WriteLine();
            Console.WriteLine("Dealing cards");
            Display("Player 1", p1);
            Display("Player 2", p2);
            Display("Reamining Deck", deck);

            deck = new List<string>(l);
            p1.Clear();
            p2.Clear();

            deck.Deal(13, p1, p2);
            Console.WriteLine();
            Console.WriteLine("Dealing too many cards");
            Display("Player 1", p1);
            Display("Player 2", p2);
            Display("Remaining Deck", deck);


            deck = new List<string>(l);
            p1.Clear();
            p2.Clear();

            int count = 0;
            Func<List<string>> newDeck =  () => 
            { 
                count++;
                return Enumerable.Range(1, 5)
                .Select(i =>string.Format("Extra{0}:{1}", count, i))
                     .ToList();
            };


            Deck.Deal(deck, 13, newDeck, p1, p2);
            Console.WriteLine();
            Console.WriteLine("Adding Additional Decks (5 cards at a time)");
            Display("Player 1", p1);
            Display("Player 2", p2);
            Display("Remaining Deck", deck);


        }

        static void Pause()
        {
            Console.WriteLine();
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
        }

        static void Display(string title, IEnumerable<string> items)
        {
            Console.WriteLine("{0}:", title);

            if (!items.Any())
            {
                Console.WriteLine("<none>");
                return;
            }

            var last = string.Empty;
            foreach (var s in items)
            {
                if (last.Length > 0)
                    Console.Write("{0}, ", last);
                last = s;
            }
            Console.WriteLine(last);
            Console.WriteLine();
        }
    }
}
