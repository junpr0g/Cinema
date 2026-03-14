namespace Cinema.Models.DTOs
{
    public record TicketDto(string TicketId,int ReservedId,string Movie,int Place,bool IsVip,string PdfBase64);
}
