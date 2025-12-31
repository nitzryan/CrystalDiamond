namespace Db
{
    public static class DbTextColumns
    {
        // GamePlayByPlay_GameFielders:SubList
        public record FielderSub
        {
            public int HalfInningEventNum { get; set; }
            public int Inning { get; set; }
            public bool IsHome { get; set; }
            public int Position { get; set; }
            public int MlbId { get; set; }
        }
    }
}
