using System;
using Npgsql;
using NpgsqlTypes;

namespace mtcg.repositories
{
    public class PackageRepository
    {
        private const string Credentials =
            "Server=127.0.0.1;Port=5432;Database=mtcg-db;User Id=mtcg-user;Password=mtcg-pw";

        public static void CreatePackage(Card[] cards)
        {
            try
            {
                foreach (var card in cards)
                {
                    InsertCard(card);
                }
            }
            catch (PostgresException pe)
            {
                Console.WriteLine(pe.Message);
                Console.WriteLine(pe.StackTrace);
            }
        }
        
        private static void InsertCard(Card card)
        {
            try
            {
                using (var connection = new NpgsqlConnection(Credentials))
                {
                    connection.Open();
                    connection.TypeMapper.MapEnum<ElementType>("element_type");
                    using var query =
                        new NpgsqlCommand(
                            "insert into card(uuid, name, card_type, element_type, damage) values (@uuid,@name,@card_type, @element_type, @damage)",
                            connection);
                    query.Parameters.AddWithValue("uuid", Guid.Parse(card.Uuid));
                    query.Parameters.AddWithValue("name", card.Name);
                    query.Parameters.AddWithValue("card_type", card.CardType);
                    query.Parameters.AddWithValue("element_type", card.ElementType);
                    query.Parameters.AddWithValue("damage", card.Damage);
                    query.ExecuteReader();
                }
            }
            catch (PostgresException pe)
            {
                Console.WriteLine(pe.Message);
                Console.WriteLine(pe.StackTrace);
            }
        }
    }
}