using System.Text.Json.Serialization;

namespace Cinema.Models.Responses
{
    public class CreateOrderResponse
    {
        public int ScreeningId { get; set; }
        
        public List<int> PlaceIds { get; set; }
    }   
}
