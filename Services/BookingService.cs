using System;
using System.Collections.Generic;
using System.Linq;
using AirportTicketBooking.Models;
using AirportTicketBooking.Repositories;

namespace AirportTicketBooking.Services
{
    public class BookingService
    {
        private readonly JsonFileRepo<Booking> _repo;
        private readonly FlightService _flightService;
        private List<Booking> _cache;

        public BookingService(string path, FlightService flightService)
        {
            _repo = new JsonFileRepo<Booking>(path);
            _flightService = flightService;
            _cache = _repo.LoadAll();
        }

        // Get all bookings in cache(memory).
        public IReadOnlyList<Booking> GetAll() => _cache;

        // Get booking by ID.
        public void AddBooking(Booking booking)
        {
            _cache.Add(booking);
            _repo.SaveAll(_cache);
        }

        // Update an existing booking.
        public void UpdateBooking(Booking updated)
        {
            var index = _cache.FindIndex(b => b.Id == updated.Id);
            if (index >= 0)
            {
                _cache[index] = updated;
                _repo.SaveAll(_cache);
            }
        }

        // Cancel a booking (passenger side).
        public void CancelBooking(string bookingId, string passengerId)
        {
            var booking = _cache.FirstOrDefault(b => b.Id == bookingId && b.PassengerId == passengerId);
            if (booking != null)
            {
                _cache.Remove(booking);
                _repo.SaveAll(_cache);
            }
        }

        // Get bookings for a passenger (view personal bookings).
        public IEnumerable<Booking> GetByPassenger(string passengerId)
        {
            return _cache.Where(b => b.PassengerId == passengerId);
        }

        // Search / Filter bookings by optional parameters (manager side)
        public IEnumerable<Booking> Search(
            string? passengerId = null,
            string? flightNumber = null,
            string? departureCountry = null,
            string? destinationCountry = null,
            SeatClass? seatClass = null,
            decimal? maxPrice = null)
        {
            var query = _cache.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(passengerId))
                query = query.Where(b => b.PassengerId == passengerId);

            if (!string.IsNullOrWhiteSpace(flightNumber))
                query = query.Where(b => _flightService.GetById(b.FlightId)?.FlightNumber.Contains(flightNumber, StringComparison.OrdinalIgnoreCase) == true);

            if (!string.IsNullOrWhiteSpace(departureCountry))
                query = query.Where(b => _flightService.GetById(b.FlightId)?.DepartureCountry.Contains(departureCountry, StringComparison.OrdinalIgnoreCase) == true);

            if (!string.IsNullOrWhiteSpace(destinationCountry))
                query = query.Where(b => _flightService.GetById(b.FlightId)?.DestinationCountry.Contains(destinationCountry, StringComparison.OrdinalIgnoreCase) == true);

            if (seatClass.HasValue)
                query = query.Where(b => b.Class == seatClass.Value);

            if (maxPrice.HasValue)
                query = query.Where(b => b.Price <= maxPrice.Value);

            return query;
        }
    }
}
