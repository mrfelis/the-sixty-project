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
    public class Down
    {
        public Down(int numberOfSets, int minimumInSet, bool allowJoker = true)
        {
            NumberOfSets = numberOfSets;
            MinimumInSet = minimumInSet;
            AllowJoker = allowJoker;
        }

        public int NumberOfSets { get; private set; }
        public int MinimumInSet { get; private set; }
        public bool AllowJoker { get; private set; }

        public static List<Down> Downs = new List<Down>{
            new Down(2,3,false),
            new Down(1,4),
            new Down(2,4),
            new Down(1,5),
            new Down(2,5),
            new Down(1,6)};
    }

    public static class Game
    {
        private class WildSelection
        {
            private readonly List<ICard> _Twos;
            private readonly List<ICard> _jokes;

            public WildSelection(IEnumerable<ICard> hand, bool allowJokers)
            {
                // get the widls cads
                var wilds = hand.Where(c => c.IsWild).ToList();
                _Twos = wilds.Where(c => c.Face == Face.Two).ToList();
                _jokes = allowJokers ? wilds.Where(c => c.Face == Face.Joker).ToList() : new List<ICard>();
            }

            public IEnumerable<List<ICard>> Enumerate()
            {
                var r=  _jokes.Concat(_Twos).Select(c => new List<ICard>() { c }).ToList();

                return r;
            }

            public bool Any()
            {
                return _Twos.Any() || _jokes.Any();
            }

            private bool HasJokeAndTwo()
            {
                return _jokes.Any() && _Twos.Any();
            }

            public bool AnyRemainingPairs()
            {
                return HasJokeAndTwo() ||
                    _Twos.Count >= 2;
            }

            public List<ICard> GetPair()
            {
                List<ICard> pair;
                if (HasJokeAndTwo())
                {
                    pair = new List<ICard>() { _jokes[0], _Twos[0] };
                    _jokes.RemoveAt(0);
                    _Twos.RemoveAt(0);
                }
                else
                {
                    pair = new List<ICard>() { _Twos[0], _Twos[1] };
                    _Twos.RemoveAt(0);
                    _Twos.RemoveAt(0);
                }

                return pair;
            }


            internal IEnumerable<List<ICard>> RemaingWilds()
            {
                return _Twos.Select(d => new List<ICard>() { d });
            }
        }

        public static IEnumerable<IEnumerable<List<ICard>>> GroupWilds(params ICard[] hand)
        {
            return GroupWilds(true, hand);
        }

        public static IEnumerable<IEnumerable<List<ICard>>> GroupWilds(bool allowJokers, params ICard[] hand)
        {
            var wilds = new WildSelection(hand, allowJokers);
            
            if (wilds.Any())
            {
                //throw new NotImplementedException();
                // return simplist list first - all the cards
                yield return wilds.Enumerate();


                var cache = new List<List<ICard>>();
                while (wilds.AnyRemainingPairs())
                {
                    var pair = wilds.GetPair();
                    cache.Add(pair);

                    var l = cache.Concat(wilds.RemaingWilds()).ToList();
                    yield return l;
                }
            }
        }

        public static IEnumerable<IEnumerable<List<ICard>>> GroupWilds(this List<ICard> hand, bool allowJoker = true)
        {
            return GroupWilds(allowJoker, hand.ToArray());
        }

        public static List<ICard> Laydown(this Table table, List<ICard> hand, bool keepWilds)
        {
            var ret = hand.Where(c => table.IsPlayable(c))
                .Where (c => !c.IsWild)
                .ToList();

            if (!keepWilds)
            {
                ret.AddRange(hand.Where(c => c.IsWild));
            }
            //foreach (var card in hand)
            //{
            //    if (table.IsPlayable(card))
            //    {
            //        ret.Add(card);
            //        hand.Remove(card);
            //    }
            //}
            foreach (var card in ret)
            {
                hand.Remove(card);
            }

            return ret;
        }

        public static Player Buy(List<ICard> deck, ICard card, Down down, List<Player> players, Player firstOption, out ICard card1, out ICard card2)
        {
            card1 = card2 = null;

            if (card == null) return null;

            var first = players.IndexOf(firstOption);

            foreach (var player in GetPlayOrder(players, first))
            {
                if (player.Buys(deck, card, down, out card1, out card2)) return player;
            }

            return null;
        }

        private static IEnumerable<Player> GetPlayOrder(List<Player> players, int first)
        {
            return players.Skip(first).Concat(players.Take(first));
        }

        public static ICard SelectDiscard(this List<ICard> hand)
        {
            var sets = hand.Where(c => !c.IsWild)
                .CountSets();
            if (!sets.Any()) return null;

            var singles = sets.Where( s => s.Key != Face.Two || s.Key != Face.Joker)
                .OrderBy(s => s.Value)
                .Select(s => s.Key)
                .First();

            var discard = hand.First(c => c.Face == singles);

            hand.Remove(discard);
            return discard;
        }

        public static List<List<ICard>> MakeSets(this List<ICard> hand)
        {
            //cards = new List<List<ICard>>();
            List<List<ICard>> mixed = new List<List<ICard>>();

            // count the sets
            var sets = hand.Where(c => !c.IsWild)
                .CountSets();

            var wilds = hand.Where(c => c.IsWild)
                .ToList();

            // find natural matches
            var naturals = sets.OrderByDescending(p => p.Value) // largest number of matches first
                .Where(p => p.Value >= 3) // just the sets the meet the required # of cards
                //.Take(down.NumberOfSets) // stop once we have the number we need
                .ToList();

            var faces = naturals.Select(c => c.Key).ToList();

            var pairFaces = sets.Where(p => p.Value == 2)
                .Take(wilds.Count)
                .WithIndex(0)
                .ToDictionary(c => c.Value.Key, c => c.Key);

            mixed = hand.GroupBy(c => c.Face)
            .Where(g => pairFaces.ContainsKey(g.Key))
            .WithIndex(0)
                //.SelectMany(g => g)
            .Select(g =>
            {
                var y = g.Value;
                var z = wilds[g.Key];
                var x = y.Concat(new List<ICard> { z })
                   .ToList();
                return x;
            })
            .ToList();


            // create play for down
            var cards = hand.GroupBy(c => c.Face)
                .Where(g => faces.Contains(g.Key))
                //.SelectMany(g => g)
                .Select(g => new List<ICard>(g))
                .Concat(mixed)
                .ToList();

            // remove down from hand
            foreach (var card in cards.SelectMany(lol => lol))
                hand.Remove(card);


            // do we have our down?
            return cards;
        }

        public static bool MakeDown(this Down down, List<ICard> hand, out List<List<ICard>> cards)
        {
            cards = new List<List<ICard>>();
            List<List<ICard>> mixed = new List<List<ICard>>();

            // count the sets
            var sets = hand.Where(c => !c.IsWild)
                .CountSets();

            // find natural matches
            var naturals = sets.OrderByDescending(p => p.Value) // largest number of matches first
                .Where(p => p.Value >= down.MinimumInSet) // just the sets the meet the required # of cards
                .Take(down.NumberOfSets) // stop once we have the number we need
                .ToList();
            
            // do we have our down?
            var result = naturals.Count == down.NumberOfSets;

            if (!result)
            {
                // attempt to make down with wilds
                var exclude = naturals.Select(n => n.Key).ToList();

                // mininumum # in set must be 2 or 2 less minumum number for down
                var minCount = down.MinimumInSet == 3 ? 2 : down.MinimumInSet - 2; 
                var minSets = down.NumberOfSets - naturals.Count;

                var candidates = sets.Where(s => !exclude.Contains(s.Key)) //remove the natruals
                    .Where(p => p.Value >= minCount) // must have minimum # in set
                    .OrderByDescending(p => p.Value) // minumum # of wilds first
                    .Take(minSets) // number of sets we need to make
                    .Reverse() // same order that GroupWilds will return
                    .ToList();

                // do we have the enough pairs or better to make down?
                result = candidates.Count == minSets;

                // see if there are matching wilds
                if (result)
                {
                    result = false;
                    var wilds = hand.GroupWilds(down.AllowJoker).Select( g => g.ToList()).ToList();
                    var setCounts = wilds.Select(l => l.Select( l1 => l1.Count()).ToList()).ToList();
                    var require = candidates.Select(p => down.MinimumInSet - p.Value).ToList();

                    foreach (var counts in setCounts.WithIndex(0))
                    {
                        if (counts.Value.Count >= require.Count && require.WithIndex(0).All(p => counts.Value[p.Key] == p.Value))
                        {
                            var faces = candidates.WithIndex(0).ToDictionary(c => c.Value.Key, c => c.Key);

                            mixed = hand.GroupBy(c => c.Face)
                                        .Where(g => faces.ContainsKey(g.Key))
                                        //.SelectMany(g => g)
                                        .Select(g => g.Concat(wilds[counts.Key][faces[g.Key]])
                                            .ToList())
                                        .ToList();

                            result = true;
                            break;
                        }

                        //var test = candidates.WithIndex(0)
                        //    .Select( p => new { Needed = down.MinimumInSet - p.Value.Value, Have = set[p.Key]
                    }
                }

            }

            
            if (result)
            {
                var faces = naturals.Select(c => c.Key).ToList();

                // create play for down
                cards = hand.GroupBy(c => c.Face)
                    .Where(g => faces.Contains(g.Key))
                    //.SelectMany(g => g)
                    .Select( g => new List<ICard>(g))
                    .Concat(mixed)
                    .ToList();

                // remove down from hand
                foreach (var card in cards.SelectMany(lol => lol))
                    hand.Remove(card);
            }
            return result;
        }
    }
}
