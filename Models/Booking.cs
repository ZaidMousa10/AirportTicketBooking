using System;

namespace AirportTicketBooking.Models
{
    public class Booking
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FlightId { get; set; } = "";
        public string PassengerId { get; set; } = "";
        public decimal Price { get; set; }
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
        public SeatClass Class { get; set; }
    }
}
