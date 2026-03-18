using Cinema.Models.DatabaseClasses;

namespace Cinema.Models.DTOs
{
    public record TicketDto(string PdfBase64, Ticket ticket, int RowNumber, int PlaceNumber);
}
