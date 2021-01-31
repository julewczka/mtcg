using mtcg.repositories;

namespace mtcg.controller
{
    public static class TransactionController
    {
        private static readonly object TransactionLock = new object();

        public static Response StartTransaction(string resource, string token)
        {
            return resource switch
            {
                "packages" => BuyPackage(token),
                _ => RTypes.NotFoundRequest
            };
        }

        private static Response BuyPackage(string token)
        {
            lock (TransactionLock)
            {
                var user = UserRepository.SelectUserByToken(token);
                return user?.Id == null ? RTypes.Unauthorized : StackRepository.BuyPackage(user.Id);
            }
        }
    }
}