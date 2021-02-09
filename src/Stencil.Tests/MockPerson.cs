using System;
using System.Collections.Generic;
using System.Text;

namespace Stencil.Tests
{
    public class Person
    {
        public Person(
            string firstname = null, 
            string lastname = null,
            string middlename = null,
            string business = null,
            string suffix = null,
            string storedTemplate = null,
            Guid? id = null)
        {
            Id = (id.HasValue) 
                ? id.Value 
                : Guid.NewGuid();
            FirstName = firstname;
            LastName = lastname;
            MiddleName = middlename;
            Business = business;
            Suffix = suffix;
            StoredTemplate = storedTemplate;
        }

        public Guid Id { get; set; }

        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }

        public string StoredTemplate { get; set; }

        public string Business { get; set; }

        public string PhoneNumber1 { get; set; }
        public string PhoneNumber2 { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }
}
