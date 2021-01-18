using System;
using Npgsql;

namespace mtcg.repositories
{
    public static class CardRepository
    {
        private const string Credentials =
            "Server=127.0.0.1;Port=5432;Database=mtcg-db;User Id=mtcg-user;Password=mtcg-pw";
        
        public static bool InsertCard(Card card)
        {
            var success = false;
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
                    if (query.ExecuteNonQuery() > 0) success = true;
                }
            }
            catch (PostgresException)
            {
                success = false;
            }

            return success;
        }
    }
}