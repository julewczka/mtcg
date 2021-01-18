using System.Collections.Generic;
using System.Linq;

namespace mtcg.repositories
{
    public static class PackageRepository
    {
        
        public static bool CreatePackage(IEnumerable<Card> cards)
        {
            var queryState = cards.Select(CardRepository.InsertCard).ToList();
            return !queryState.Contains(false);
        }
        
    }
}