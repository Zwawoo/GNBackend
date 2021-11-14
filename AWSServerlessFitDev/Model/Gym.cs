using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerlessFitDev.Model
{
    public class Gym : Group
    {
        public Gym()
        {

        }
        public Gym(string chain, string city, string postalCode, string street, int groupId, string groupName, string description, int? creator, GroupPrivacyTypes groupPrivacyType, DateTime? createdAt, DateTime? lastModified)
            : base(groupId, groupName, description, creator, groupPrivacyType, createdAt, lastModified)
        {
            Chain = chain;
            City = city;
            PostalCode = postalCode;
            Street = street;
        }
        public string Chain { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Street { get; set; }
    }
}
