namespace MessagingTests.Runtime
{
    public class Address
    {
        public string City { get; set; }
        public string State { get; set; }

        protected bool Equals(Address other)
        {
            return string.Equals(City, other.City) && string.Equals(State, other.State);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Address) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((City != null ? City.GetHashCode() : 0) * 397) ^ (State != null ? State.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("City: {0}, State: {1}", City, State);
        }
    }
}
