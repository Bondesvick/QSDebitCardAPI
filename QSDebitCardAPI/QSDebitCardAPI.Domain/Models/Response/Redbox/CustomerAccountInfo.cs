using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace QSDataUpdateAPI.Domain.Models.Response.Redbox
{
    [XmlType(TypeName = "otherResponseDetails")]
    public class CustomerAccountInfo
    {
        [XmlElement("bvn")]
        public string BVN { get; set; }

        [XmlElement("firstName")]
        public string FirstName { get; set; }

        [XmlElement("lastName")]
        public string LastName { get; set; }

        [XmlElement("middleName")]
        public string MiddleName { get; set; }

        [XmlElement("1990-02-18")]
        public string DateOfBirth { get; set; }

        [XmlElement("phoneNumber")]
        public string PhoneNumber { get; set; }

        [XmlElement("address")]
        public string Address { get; set; }

        [XmlElement("cifId")]
        public string CifId { get; set; }

        [XmlElement("gender")]
        public string Gender { get; set; }

        [XmlElement("enrollmentBank")]
        public string EnrollmentBank { get; set; }

        [XmlElement("enrollmentBranch")]
        public string EnrollmentBranch { get; set; }

        [XmlElement("accountOpened")]
        public bool AccountOpened { get; set; }

        [XmlElement("pinSet")]
        public string PinSet { get; set; }

        [XmlElement("pinActive")]
        public string PinActive { get; set; }

        [XmlElement("maskedPhoneNumber")]
        public string MaskedPhoneNumber { get; set; }

        [XmlIgnore]
        public string AccountSegment { get; set; }

        public string EmailAddress { get; set; }
        public string AccountName { get; set; }
        public string AccountSchemeType { get; set; }
        public string AccountSchemeCode { get; set; }
        public string CustomerCreationDate { get; set; }
        public string AccountType { get; set; }
        public string AccountStatus { get; set; }

        public List<string> cardTypes { get; set; }
        public List<string> cardfees { get; set; }
    }

    public class AccountEnquiryInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneNumber1 { get; set; }
        public string EmailAddress { get; set; }
        public string AccountName { get; set; }
        public string AccountSchemeType { get; set; }
        public string AccountSchemeCode { get; set; }
        public string CustomerId { get; set; }
        public string AvailableBalance { get; set; }
        public string Bvn { get; set; }
        public string CustomerCreationDate { get; set; }
        public string AccountStatus { get; set; }
        public string PhoneEmailIdEmail { get; set; }
        public string PhoneEmailIdEmailType { get; set; }
        public string PhoneEmailIdPhone { get; set; }
        public string PhoneEmailIdPhoneType { get; set; }
        public string AccountCurrencyCode { get; set; }
        public string CardType { get; set; }
        public string Gender { get; set; }
        public string Title { get; set; }
        public string MaritalStatus { get; set; }
        public string City { get; set; }
    }

    public class ActiveCardEnquiryInfo
    {
        public string maskedPAN { get; set; }
        public string expiryDate { get; set; }
        public string hotlistCode { get; set; }
    }

    public class TinEnquiryInfo
    {
        public string AccountName { get; set; }
    }

    public class CardTypes
    {
        public List<string> types { get; set; }
        public List<string> fees { get; set; }
    }
}