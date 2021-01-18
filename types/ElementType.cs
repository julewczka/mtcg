using NpgsqlTypes;

namespace mtcg
{
    public enum ElementType
    {
        [PgName("normal")]
        Normal = 0,
        [PgName("water")]
        Water = 1,
        [PgName("fire")]
        Fire = 2
    }
}