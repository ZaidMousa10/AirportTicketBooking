using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AirportTicketBooking.Models;
using AirportTicketBooking.Repositories;
using AirportTicketBooking.Services;

namespace AirportTicketBooking
{
    class Program
    {
        static FlightService flightService;
        static BookingService bookingService;
        static JsonFileRepo<Passenger> passengerRepo;
        static void Main()
        {
            flightService = new FlightService("Data/flights.csv");
            bookingService = new BookingService("Data/bookings.json", flightService);
            passengerRepo = new JsonFileRepo<Passenger>("Data/passengers.json");

            Console.WriteLine("=== Airport Ticket Booking System ===");

            while (true)
            {
                Console.WriteLine("\nSelect user type:");
                Console.WriteLine("1. Passenger");
                Console.WriteLine("2. Manager");
                Console.WriteLine("0. Exit");
                string choice = Console.ReadLine()!;

                if (choice == "1") PassengerMenu();
                else if (choice == "2") ManagerMenu();
                else if (choice == "0") break;
            }
        }

        #region Passenger
        static void PassengerMenu()
        {
            Console.WriteLine("\n--- Passenger Menu ---");
            string passengerId = GetPassengerIdOrCreateNew(passengerRepo);

            var passenger = passengerRepo.LoadAll().First(p => p.Id == passengerId);

            while (true)
            {
                Console.WriteLine("\n1. Search Flights");
                Console.WriteLine("2. Book Flight");
                Console.WriteLine("3. View My Bookings");
                Console.WriteLine("4. Cancel Booking");
                Console.WriteLine("5. Modify Booking");
                Console.WriteLine("0. Back");
                string choice = Console.ReadLine()!;

                if (choice == "1") SearchFlights();
                else if (choice == "2") BookFlight(passenger.Id);
                else if (choice == "3") ViewBookings(passenger.Id);
                else if (choice == "4") CancelBooking(passenger.Id);
                else if (choice == "5") ModifyBooking(passenger.Id);
                else if (choice == "0") break;
            }
        }

        static void SearchFlights()
        {
            Console.WriteLine("\nSearch Flights:");
            Console.WriteLine("1. Filter Flights");
            Console.WriteLine("2. Show All Flights");
            int choice = ReadIntInRange(1, 2);

            IEnumerable<Flight> flights;

            if (choice == 1)
            {
                Console.WriteLine("\n--- Filter Flights (leave empty to skip filter) ---");
                string departureCountry = ReadOptionalString("Departure Country: ");
                string destinationCountry = ReadOptionalString("Destination Country: ");
                string departureAirport = ReadOptionalString("Departure Airport: ");
                string arrivalAirport = ReadOptionalString("Arrival Airport: ");
                DateTime? departureDate = ReadOptionalDate("Departure Date (yyyy-MM-dd) or leave empty: ");
                decimal? maxPrice = ReadOptionalDecimal("Max Price (or leave empty): ");
                SeatClass? seatClass = ReadOptionalSeatClass("Seat Class (1-Economy, 2-Business, 3-First, empty=all): ");

                flights = flightService.Search(
                    maxPrice: maxPrice,
                    departureCountry: string.IsNullOrWhiteSpace(departureCountry) ? null : departureCountry,
                    destinationCountry: string.IsNullOrWhiteSpace(destinationCountry) ? null : destinationCountry,
                    departureDate: departureDate,
                    departureAirport: string.IsNullOrWhiteSpace(departureAirport) ? null : departureAirport,
                    arrivalAirport: string.IsNullOrWhiteSpace(arrivalAirport) ? null : arrivalAirport,
                    seatClass: seatClass
                );
            }
            else
            {
                flights = flightService.GetAll();
            }

            Console.WriteLine("\nAvailable Flights:");
            foreach (var f in flights)
                Console.WriteLine($"{f.FlightNumber} | {f.DepartureCountry}->{f.DestinationCountry} | {f.DepartureDate:g} | BasePrice: {f.BasePrice:C}");
        }

        static void BookFlight(string passengerId)
        {
            string fn = ReadNonEmptyString("Enter Flight Number to book: ");
            var flight = flightService.GetAll().FirstOrDefault(f => f.FlightNumber.Equals(fn, StringComparison.OrdinalIgnoreCase));

            if (flight == null) { Console.WriteLine("Flight not found!"); return; }

            Console.WriteLine("Select Class: 1-Economy, 2-Business, 3-First");
            int clsInt = ReadIntInRange(1, 3);
            var seatClass = (SeatClass)clsInt;

            var booking = new Booking
            {
                FlightId = flight.Id,
                PassengerId = passengerId,
                Class = seatClass,
                Price = flight.GetPrice(seatClass)
            };

            bookingService.AddBooking(booking);
            Console.WriteLine($"Booked {flight.FlightNumber} for {seatClass}. Price: {booking.Price:C}");
        }

        static void ViewBookings(string passengerId)
        {
            var bookings = bookingService.GetByPassenger(passengerId);
            if (!bookings.Any()) { Console.WriteLine("You have no bookings."); return; }

            Console.WriteLine("\nMy Bookings:");
            foreach (var b in bookings)
            {
                var flight = flightService.GetById(b.FlightId);
                if (flight != null)
                    Console.WriteLine($"{b.Id} | {flight.FlightNumber} | {flight.DepartureCountry}->{flight.DestinationCountry} | {b.Class} | {b.Price:C}");
            }
        }

        static void CancelBooking(string passengerId)
        {
            string bookingId = ReadNonEmptyString("Enter Booking Id to cancel: ");
            var bookings = bookingService.GetByPassenger(passengerId);

            if (!bookings.Any(b => b.Id == bookingId)) { Console.WriteLine("Booking not found."); return; }

            bookingService.CancelBooking(bookingId, passengerId);
            Console.WriteLine("Booking canceled successfully.");
        }

        static void ModifyBooking(string passengerId)
        {
            var bookings = bookingService.GetByPassenger(passengerId);
            if (!bookings.Any())
            {
                Console.WriteLine("You have no bookings to modify.");
                return;
            }

            Console.WriteLine("\nYour Bookings:");
            foreach (var b in bookings)
            {
                var flight = flightService.GetById(b.FlightId);
                if (flight != null)
                    Console.WriteLine($"{b.Id} | {flight.FlightNumber} | {flight.DepartureCountry}->{flight.DestinationCountry} | {b.Class} | {b.Price:C}");
            }

            string bookingId = ReadNonEmptyString("Enter Booking ID to modify: ");
            var booking = bookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking == null)
            {
                Console.WriteLine("Booking not found.");
                return;
            }

            var currentFlight = flightService.GetById(booking.FlightId);
            Console.WriteLine($"Current Flight: {currentFlight?.FlightNumber} | {booking.Class}");

            Console.WriteLine("\nSelect modification type:");
            Console.WriteLine("1. Change Flight");
            Console.WriteLine("2. Change Class");
            int choice = ReadIntInRange(1, 2);

            if (choice == 1)
            {
                string newFlightNumber = ReadNonEmptyString("Enter new Flight Number: ");
                var newFlight = flightService.GetAll()
                    .FirstOrDefault(f => f.FlightNumber.Equals(newFlightNumber, StringComparison.OrdinalIgnoreCase));

                if (newFlight == null)
                {
                    Console.WriteLine("Flight not found. Modification cancelled.");
                    return;
                }

                booking.FlightId = newFlight.Id;
                booking.Price = newFlight.GetPrice(booking.Class);
            }
            else if (choice == 2)
            {
                Console.WriteLine("Select new Class: 1-Economy, 2-Business, 3-First");
                int clsInt = ReadIntInRange(1, 3);
                booking.Class = (SeatClass)clsInt;

                var flight = flightService.GetById(booking.FlightId);
                if (flight != null)
                    booking.Price = flight.GetPrice(booking.Class);
            }

            bookingService.UpdateBooking(booking);
            Console.WriteLine("Booking modified successfully!");
        }
        #endregion

        #region Manager
        static void ManagerMenu()
        {
            Console.WriteLine("\n--- Manager Menu ---");

            while (true)
            {
                Console.WriteLine("\n1. View Flights");
                Console.WriteLine("2. Add Flight");
                Console.WriteLine("3. View Bookings");
                Console.WriteLine("0. Back");
                string choice = Console.ReadLine()!;

                if (choice == "1") ViewAllFlights();
                else if (choice == "2") AddFlight();
                else if (choice == "3") ViewAllBookings();
                else if (choice == "0") break;
            }
        }

        static void ViewAllFlights()
        {
            Console.WriteLine("\nView Flights:");
            Console.WriteLine("1. Filter Flights");
            Console.WriteLine("2. Show All Flights");
            int choice = ReadIntInRange(1, 2);

            IEnumerable<Flight> flights;

            if (choice == 1)
            {
                Console.WriteLine("\n--- Filter Flights (leave empty to skip filter) ---");
                string departureCountry = ReadOptionalString("Departure Country: ");
                string destinationCountry = ReadOptionalString("Destination Country: ");
                string departureAirport = ReadOptionalString("Departure Airport: ");
                string arrivalAirport = ReadOptionalString("Arrival Airport: ");
                DateTime? departureDate = ReadOptionalDate("Departure Date (yyyy-MM-dd) or leave empty: ");
                decimal? maxPrice = ReadOptionalDecimal("Max Price (or leave empty): ");
                SeatClass? seatClass = ReadOptionalSeatClass("Seat Class (1-Economy, 2-Business, 3-First, empty=all): ");

                flights = flightService.Search(
                    maxPrice: maxPrice,
                    departureCountry: string.IsNullOrWhiteSpace(departureCountry) ? null : departureCountry,
                    destinationCountry: string.IsNullOrWhiteSpace(destinationCountry) ? null : destinationCountry,
                    departureDate: departureDate,
                    departureAirport: string.IsNullOrWhiteSpace(departureAirport) ? null : departureAirport,
                    arrivalAirport: string.IsNullOrWhiteSpace(arrivalAirport) ? null : arrivalAirport,
                    seatClass: seatClass
                );
            }
            else
            {
                flights = flightService.GetAll();
            }

            Console.WriteLine("\nFlights:");
            foreach (var f in flights)
                Console.WriteLine($"{f.FlightNumber} | {f.DepartureCountry}->{f.DestinationCountry} | {f.DepartureDate:g} | BasePrice: {f.BasePrice:C}");
        }


        static void ViewAllBookings()
        {
            Console.WriteLine("\nView Bookings:");
            Console.WriteLine("1. Filter Bookings");
            Console.WriteLine("2. Show All Bookings");
            int choice = ReadIntInRange(1, 2);

            IEnumerable<Booking> bookings;

            if (choice == 1)
            {
                Console.WriteLine("\n--- Filter Bookings (leave empty to skip filter) ---");
                string passengerId = ReadOptionalString("Passenger ID: ");
                string flightNumber = ReadOptionalString("Flight Number: ");
                string departureCountry = ReadOptionalString("Departure Country: ");
                string destinationCountry = ReadOptionalString("Destination Country: ");
                SeatClass? seatClass = ReadOptionalSeatClass("Seat Class (1-Economy, 2-Business, 3-First, empty=all): ");
                decimal? maxPrice = ReadOptionalDecimal("Max Price: ");

                bookings = bookingService.Search(
                    passengerId: string.IsNullOrWhiteSpace(passengerId) ? null : passengerId,
                    flightNumber: string.IsNullOrWhiteSpace(flightNumber) ? null : flightNumber,
                    departureCountry: string.IsNullOrWhiteSpace(departureCountry) ? null : departureCountry,
                    destinationCountry: string.IsNullOrWhiteSpace(destinationCountry) ? null : destinationCountry,
                    seatClass: seatClass,
                    maxPrice: maxPrice
                );
            }
            else
            {
                bookings = bookingService.GetAll();
            }

            Console.WriteLine("\nBookings:");
            foreach (var b in bookings)
            {
                var flight = flightService.GetById(b.FlightId);
                Console.WriteLine($"{b.Id} | {b.PassengerId} | {flight?.FlightNumber} | {b.Class} | {b.Price:C}");
            }
        }

        static void AddFlight()
        {
            string flightId;
            while (true)
            {
                flightId = ReadNonEmptyString("Enter Flight ID: ");
                if (flightService.GetById(flightId) == null) break;
                Console.WriteLine("Flight ID already exists! Enter a unique ID.");
            }

            string fn = ReadNonEmptyString("Flight Number: ");
            string dc = ReadNonEmptyString("Departure Country: ");
            string dest = ReadNonEmptyString("Destination Country: ");
            string da = ReadNonEmptyString("Departure Airport: ");
            string aa = ReadNonEmptyString("Arrival Airport: ");
            DateTime depDate = ReadDate("Departure Date (yyyy-MM-dd HH:mm): ");
            DateTime arrDate = ReadDate("Arrival Date (yyyy-MM-dd HH:mm): ", depDate);
            decimal price = ReadDecimal("Base Price: ");

            var flight = new Flight
            {
                Id = flightId,
                FlightNumber = fn,
                DepartureCountry = dc,
                DestinationCountry = dest,
                DepartureAirport = da,
                ArrivalAirport = aa,
                DepartureDate = depDate.ToString("yyyy-MM-dd HH:mm"),
                ArrivalDate = arrDate.ToString("yyyy-MM-dd HH:mm"),
                BasePrice = price
            };

            flightService.AddFlight(flight);
            Console.WriteLine("Flight added successfully!");
        }
        #endregion

        #region Input Helpers
        static string GetPassengerIdOrCreateNew(JsonFileRepo<Passenger> passengerRepo)
        {
            Console.Write("Enter your Passenger ID (leave empty to create new): ");
            string input = Console.ReadLine()!.Trim();

            if (string.IsNullOrEmpty(input))
            {
                var existingPassengers = passengerRepo.LoadAll();
                string name = ReadNonEmptyString("Enter your full name: ");

                var newPassenger = new Passenger(existingPassengers, name);
                passengerRepo.Add(newPassenger);
                Console.WriteLine($"New Passenger created! Your ID is: {newPassenger.Id}");
                return newPassenger.Id;
            }
            else
            {
                var passenger = passengerRepo.LoadAll().FirstOrDefault(p => p.Id == input);
                if (passenger == null)
                {
                    Console.WriteLine("Passenger ID not found.");
                    return GetPassengerIdOrCreateNew(passengerRepo);
                }
                else
                {
                    Console.WriteLine($"Welcome back, {passenger.FullName}!");
                    return passenger.Id;
                }
            }
        }

        static string ReadNonEmptyString(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string input = Console.ReadLine()!;
                if (!string.IsNullOrWhiteSpace(input)) return input.Trim();
                Console.WriteLine("Input cannot be empty.");
            }
        }

        static string ReadOptionalString(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine()!.Trim();
        }

        private static decimal? ReadOptionalDecimal(string prompt)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()!.Trim();
            if (decimal.TryParse(input, out decimal val) && val >= 0) return val;
            return null;
        }

        private static SeatClass? ReadOptionalSeatClass(string prompt)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()!.Trim();
            if (int.TryParse(input, out int clsInt) && clsInt >= 1 && clsInt <= 3)
                return (SeatClass)clsInt;
            return null;
        }

        private static DateTime? ReadOptionalDate(string prompt)
        {
            Console.Write(prompt);
            string input = Console.ReadLine()!.Trim();
            if (DateTime.TryParseExact(input, "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime dt))
                return dt;
            return null;
        }

        static int ReadIntInRange(int min, int max)
        {
            while (true)
            {
                if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max)
                    return value;
                Console.WriteLine($"Enter a number between {min} and {max}.");
            }
        }

        static DateTime ReadDate(string prompt, DateTime? minDate = null)
        {
            while (true)
            {
                Console.Write(prompt);
                if (DateTime.TryParseExact(Console.ReadLine()!, "yyyy-MM-dd HH:mm", null, DateTimeStyles.None, out DateTime dt))
                {
                    if (minDate.HasValue && dt <= minDate.Value)
                    {
                        Console.WriteLine($"Date must be after {minDate.Value:yyyy-MM-dd HH:mm}");
                        continue;
                    }
                    return dt;
                }
                Console.WriteLine("Invalid date format. Please use yyyy-MM-dd HH:mm");
            }
        }

        static decimal ReadDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                if (decimal.TryParse(Console.ReadLine()!, out decimal value) && value >= 0)
                    return value;
                Console.WriteLine("Invalid number. Enter a positive value.");
            }
        }
        #endregion
    }
}
