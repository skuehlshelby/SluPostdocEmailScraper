using System;
using System.Diagnostics;
using System.Net;

namespace SluEmailScraper
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public sealed class Person : IEquatable<Person>
    {
        public Person(string name, string job, string email, string campus, string department)
        {
            Name = WebUtility.HtmlDecode((name ?? string.Empty).Trim());
            Job = WebUtility.HtmlDecode((job ?? string.Empty).Trim());
            Email = (email ?? string.Empty).Trim();
            Department = department;
            Campus = campus;
        }

        public string Name { get; }

        public string Department { get; }
        
        public string Job { get; }

        public string Campus { get; }

        public string Email { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Person);
        }

        public bool Equals(Person other)
        {
            return other is not null && string.Equals(Email, other.Email);
        }

        public override int GetHashCode()
        {
            return Email.GetHashCode();
        }

        public override string ToString()
        {
            return string.Join(", ", Name, Job, Email, Campus, Department);
        }

        public static bool operator ==(Person left, Person right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Person left, Person right)
        {
            return !(left == right);
        }

        private string GetDebuggerDisplay()
        {
            return $"{Name}, {Job}";
        }
    }
}
