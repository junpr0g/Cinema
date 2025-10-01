namespace Cinema.Models.DatabaseClasses
{
    public class SeatType
    {
        public int Id { get; set; }
        public string Name { get; set; } //Обычный, балкон, ВИП
        public double BasePriceModifier { get; set; } //Множитель или надбавка
    }
}
