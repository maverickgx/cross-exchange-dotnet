using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrossExchange
{
    public interface ITradeRepository : IGenericRepository<Trade>
    {
        Task<List<Trade>> GetTradingsByPortfolioId(int portFolioId);
    }
}