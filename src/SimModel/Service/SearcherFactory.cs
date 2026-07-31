using SimModel.Domain;
using SimModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimModel.Service
{
    public class SearcherFactory
    {
        public Searcher Create(SearchCondition condition, SearchRange range)
            => new Searcher(condition, range);
    }

}
