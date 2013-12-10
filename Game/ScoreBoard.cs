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

namespace Game
{
    public class ScoreBoard
    {
        private Dictionary<Down, List<int>> _scores = new Dictionary<Down, List<int>>();

        public void SetScores(Down down, List<Player> players)
        {
            _scores[down] = players.Select(p => p.Score).ToList();
        }

        public IEnumerable<IEnumerable<int>> GetScores()
        {
            foreach (var down in Down.Downs)
            {
                if (_scores.ContainsKey(down))
                    yield return _scores[down];
            }
        }

        public IEnumerable<int> GetTotals()
        {
            var totals = new List<int>(_scores.Values.First());
            foreach (var score in _scores.Values.Skip(1))
            {
                for (int i = 0; i < totals.Count; i++)
                {
                    totals[i] += score[i];
                }
            }

            return totals;
        }

    }
}
