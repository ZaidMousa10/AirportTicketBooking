using System;
using System.Collections.Generic;
using System.Linq;

namespace AirportTicketBooking.Models
{
    public class Passenger
    {
        public string Id { get; set; } 
        public string FullName { get; set; } = "";

        public Passenger() { }

        // Constructor to assign next available ID dynamically
        public Passenger(List<Passenger> existingPassengers, string fullName)
        {
            FullName = fullName;

            int nextId = 1;
            if (existingPassengers != null && existingPassengers.Count > 0)
            {
                nextId = existingPassengers
                    .Select(p => int.TryParse(p.Id, out int n) ? n : 0)
                    .Max() + 1;
            }


            Id = nextId.ToString(); 
        }
    }
}
