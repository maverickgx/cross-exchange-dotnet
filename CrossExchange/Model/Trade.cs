using System;
using System.Collections.Generic;

namespace CrossExchange
{
    public class Trade
    {
        public int Id { get; set; }
        
        public string Symbol { get; set; }

        public int NoOfShares { get; set; }

        public decimal Price { get; set; }       

        public int PortfolioId { get; set; }
        
        public string Action { get; set; }

        public static implicit operator List<object>(Trade v)
        {
            throw new NotImplementedException();
        }
    }
}