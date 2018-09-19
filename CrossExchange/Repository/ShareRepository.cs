using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace CrossExchange
{
    public class ShareRepository : GenericRepository<HourlyShareRate>, IShareRepository
    {
        public ShareRepository(ExchangeContext dbContext)
        {
            _dbContext = dbContext;
        }

        //Add method to get shares
        public Task<HourlyShareRate> GetLastSharesBySymbol(string symbol)
        {
            return Query()
                    .Where(x => x.Symbol.Equals(symbol))
                    .OrderByDescending(x => x.TimeStamp).FirstOrDefaultAsync();
        }
    }
}