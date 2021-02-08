using mtcg.classes.entities;
using mtcg.repositories;

namespace mtcg.controller
{
    public static class TransactionController
    {
        private static readonly object TransactionLock = new object();

        public static Response StartTransaction(string resource, User user)
        {
            return resource switch
            {
                "packages" => BuyPackage(user),
                _ => RTypes.NotFoundRequest
            };
        }

        private static Response BuyPackage(User user)
        {
            var stackRepo = new StackRepository();
            
            lock (TransactionLock)
            {
                return (user?.Id != null && stackRepo.BuyPackage(user.Id))
                    ? RTypes.Created
                    : RTypes.CError("Couldn't aquire package!", 400);
            }
        }
    }
}