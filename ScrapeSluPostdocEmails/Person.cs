using System;

namespace SluEmailScraper
{
    public sealed class Person : IEquatable<Person>
    {
        public Person(string name, string job, string email)
        {
            Name=(name ?? string.Empty).Trim();
            Job=(job ?? string.Empty).Trim();
            Email=(email ?? string.Empty).Trim();
        }

        public string Name { get; }
        
        public string Job { get; }

        public string Email { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Person);
        }

        public bool Equals(Person other)
        {
            return other is not null && string.Equals(Email, other.Email, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Email.GetHashCode();
        }

        public override string ToString()
        {
            return string.Join(", ", Name, Job, Email);
        }

        public static bool operator ==(Person left, Person right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Person left, Person right)
        {
            return !(left==right);
        }
    }
}
