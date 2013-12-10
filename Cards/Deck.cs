// The Sixty Project
//
// Copyright 2013 Richard Morrison
// All Rights Reserved
//
// See COPYRIGHT.TXT
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cards
{
    public enum Suit { Spade, Club, Heart, Diamond, Large, Small }
    public enum Face { Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Joker }

    public interface ICard
    {
        bool IsWild { get; }
        double Value { get; }
        bool IsFaceUp { get; set; }
        Suit Suit { get; }
        Face Face { get; }

    }

    [DebuggerDisplay("{DD}")]
    public class Card : ICard
    {
        public Card(Suit suit, Face face, double value, bool isWild)
        {
            Suit = suit;
            Face = face;
            Value = value;
            IsWild = isWild;
            IsFaceUp = false;
        }

        private string DD { get { return string.Format("Face: {0} Suit: {1}", Face, Suit); } }

        public bool IsWild
        {
            get;
            private set;
        }

        public double Value
        {
            get;
            private set;
        }

        public bool IsFaceUp
        {
            get;
            set;
        }

        public Suit Suit
        {
            get;
            private set;
        }

        public Face Face
        {
            get;
            private set;
        }
    }

    public class ShuffleAdaptor<T>
    {
        private static Random DefaultRandomNumberGenerator = new Random();

        public double Position { get; private set; }
        public T Inner { get; private set; }

        public ShuffleAdaptor(T inner)
        {
            Position = DefaultRandomNumberGenerator.NextDouble();
            Inner = inner;
        }
    }

    public static class EnumUtil
    {
        // http://stackoverflow.com/questions/972307/can-you-loop-through-all-enum-values
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static IEnumerable<T> Less<T>(this IEnumerable<T> list, params T[] exclude)
        {
            return list.Where(v => !exclude.Contains(v));
        }

        //public static bool HasFlag<T>(T value, T flag)
        //    where T:enum
        //{
        //    return (value | flag) != flag;
        //}

        public static IEnumerable<KeyValuePair<int, T>> WithIndex<T>(this IEnumerable<T> items, int index = 1)
        {
            return items.Select(i => new KeyValuePair<int,T>(index++, i));
        }
    }

    public static class Deck
    {
        [Flags]
        public enum DeckStyle
        {
            Standard = 1, TwoIsWild = 2, NoJokers = 4
        }

        public static Dictionary<Face, int> CountSets(this IEnumerable<ICard> cards)
        {
            var result = new Dictionary<Face, int>();

            foreach (var card in cards)
            {
                if (!result.ContainsKey(card.Face))
                    result[card.Face] = 0;

                result[card.Face]++;
            }
            return result;
        }


        public static IEnumerable<ICard> Create(int totalDecks = 1, DeckStyle style = DeckStyle.Standard)
        {
            for (int i = 0; i < totalDecks; i++)
                foreach (var face in EnumUtil.GetValues<Face>())
                {
                    if (face == Face.Joker)
                    {
                        if ((style | DeckStyle.NoJokers) != DeckStyle.NoJokers)
                        {
                            yield return new Card(Suit.Large, Face.Joker, 50, true);
                            yield return new Card(Suit.Small, Face.Joker, 50, true);
                        }
                        continue;
                    }

                    foreach (var suit in EnumUtil.GetValues<Suit>().Less(Suit.Small, Suit.Large))
                    {
                        var value = 5;
                        var isWild = false;

                        if (face == Face.Ace)
                            value = 15;

                        if (face == Face.Two)
                        {
                            value = 20;
                            isWild = (style | DeckStyle.TwoIsWild) == DeckStyle.TwoIsWild;
                        }

                        if ((int)face >= (int)Face.Ten)
                            value = 10;

                        yield return new Card(suit, face, value, isWild);
                    }
                }
        }

        public static T FlipTop<T>(List<T> items)
        {
            
            var top = items[0];
            items.RemoveAt(0);
            return top;
        }

        public static void Deal<T>(List<T> items, int count, Func<List<T>> nextDeck, IEnumerable<List<T>> players)
        {
            Deal(items, count, nextDeck, players.ToArray());
        }

        public static void Deal<T>(List<T> items, int count, Func<List<T>> nextDeck, params List<T>[] players)
        {
            for (int i = 0; i < count; i++)
            {
                foreach (var player in players)
                {
                    player.Add(FlipTop(items));

                    if (items.Count == 0)
                    {
                        items = nextDeck();

                        if (items == null)
                            return;
                    }
                }
            }
        }

        public static void Deal<T>(this List<T> items, int count, IEnumerable< List<T>> players)
        {
            Deal(items, count, () => null, players.ToArray());
        }

        public static void Deal<T>(this List<T> items, int count, params List<T>[] players)
        {
            Deal(items, count, () => null, players);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> items)
        {
            return items.Select(i => new ShuffleAdaptor<T>(i))
                .OrderBy(sa => sa.Position)
                .Select(sa => sa.Inner);
        }

    }
}
