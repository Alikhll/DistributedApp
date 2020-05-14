using System;

namespace Contract.Booking
{
    public class BookingModel
    {
        public Guid BookingId { get; set; }
        public string Hotel { get; set; }
        public string Flight { get; set; }
        public string Car { get; set; }
    }

    public class BookingRequestResponseModel
    {
        public Guid BookingId { get; set; }
        public string Hotel { get; set; }
        public string Flight { get; set; }
        public string Car { get; set; }
    }
}
