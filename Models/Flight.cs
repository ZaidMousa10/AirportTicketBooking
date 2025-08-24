using System;
using System.Globalization;

namespace AirportTicketBooking.Models
{
    public enum SeatClass { Economy = 1, Business = 2, First = 3 }

    public class Flight
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        private string flightNumber = "";
        public string FlightNumber
        {
            get => flightNumber;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Flight Number cannot be empty.");
                flightNumber = value.Trim();
            }
        }

        private string departureCountry = "";
        public string DepartureCountry
        {
            get => departureCountry;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Departure Country cannot be empty.");
                departureCountry = value.Trim();
            }
        }

        private string destinationCountry = "";
        public string DestinationCountry
        {
            get => destinationCountry;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Destination Country cannot be empty.");
                destinationCountry = value.Trim();
            }
        }

        private string departureAirport = "";
        public string DepartureAirport
        {
            get => departureAirport;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Departure Airport cannot be empty.");
                departureAirport = value.Trim();
            }
        }

        private string arrivalAirport = "";
        public string ArrivalAirport
        {
            get => arrivalAirport;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Arrival Airport cannot be empty.");
                arrivalAirport = value.Trim();
            }
        }

        private DateTime departureDate;
        public string DepartureDate
        {
            get => departureDate.ToString("yyyy-MM-dd HH:mm");
            set
            {
                if (!DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                    throw new ArgumentException("Invalid Departure Date. Use format yyyy-MM-dd HH:mm");
                departureDate = dt;
            }
        }

        private DateTime arrivalDate;
        public string ArrivalDate
        {
            get => arrivalDate.ToString("yyyy-MM-dd HH:mm");
            set
            {
                if (!DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
                    throw new ArgumentException("Invalid Arrival Date. Use format yyyy-MM-dd HH:mm");
                arrivalDate = dt;
            }
        }

        private decimal basePrice;
        public decimal BasePrice
        {
            get => basePrice;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Base Price must be a positive value.");
                basePrice = value;
            }
        }

        public decimal GetPrice(SeatClass seatClass) => BasePrice * (seatClass switch
        {
            SeatClass.Economy => 1.0m,
            SeatClass.Business => 1.5m,
            SeatClass.First => 2.0m,
            _ => 1.0m
        });
    }
}
