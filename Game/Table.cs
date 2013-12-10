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
    public class Table
    {
        private Dictionary<Face, bool> _playables = new Dictionary<Face, bool>();
        public Table()
        {
        }

        public bool IsPlayable(ICard card)
        {
            return card.IsWild || _playables.ContainsKey(card.Face);
        }

        public void Laydown(Player player, List<List<ICard>> sets)
        {
            foreach(var list in sets)
            {
                var card = list.First(c => !c.IsWild);
                if (!IsPlayable(card))
                {
                    MakePlayable(card);
                }
            }
        }

        private void MakePlayable(ICard card)
        {
            if (!card.IsWild) _playables[card.Face] = true;
        }

        public ICard TopCard { get; private set; }

        public void Discard(ICard card)
        {
            TopCard = card;
        }

    }
}
