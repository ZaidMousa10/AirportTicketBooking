using System;
using System.Collections.Generic;
using System.Linq;
using AirportTicketBooking.Models;
using AirportTicketBooking.Repositories;

namespace AirportTicketBooking.Services
{
    public class FlightService
    {
        private readonly JsonFileRepo<Flight> _repo;
        private List<Flight> _cache;

        public FlightService(string path)
        {
            _repo = new JsonFileRepo<Flight>(path);
            _cache = _repo.LoadAll();
        }

        // Get all flights in cache(memory).
        public IReadOnlyList<Flight> GetAll() => _cache;

        // Get flight by ID.
        public Flight? GetById(string id) => _cache.FirstOrDefault(f => f.Id == id);

        // Add a new flight. Ensures unique flight ID.
        public void AddFlight(Flight flight)
        {
            if (_cache.Any(f => f.Id == flight.Id))
                throw new Exception("Flight ID already exists.");
            _cache.Add(flight);
            _repo.SaveAll(_cache);
        }

        // Search flights using optional parameters. Can filter by none, one, or multiple criteria.
        public IEnumerable<Flight> Search(
            decimal? maxPrice = null,
            string? departureCountry = null,
            string? destinationCountry = null,
            DateTime? departureDate = null,
            string? departureAirport = null,
            string? arrivalAirport = null,
            SeatClass? seatClass = null)
        {
            var query = _cache.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(departureCountry))
                query = query.Where(f => f.DepartureCountry.Contains(departureCountry, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(destinationCountry))
                query = query.Where(f => f.DestinationCountry.Contains(destinationCountry, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(departureAirport))
                query = query.Where(f => f.DepartureAirport.Contains(departureAirport, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(arrivalAirport))
                query = query.Where(f => f.ArrivalAirport.Contains(arrivalAirport, StringComparison.OrdinalIgnoreCase));

            if (departureDate.HasValue)
                query = query.Where(f => DateTime.ParseExact(f.DepartureDate, "yyyy-MM-dd HH:mm", null).Date == departureDate.Value.Date);

            if (maxPrice.HasValue)
                query = query.Where(f => (seatClass.HasValue ? f.GetPrice(seatClass.Value) : f.BasePrice) <= maxPrice.Value);

            return query.OrderBy(f => f.DepartureDate);
        }
    }
}
