using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace QSDebitCardAPI.Domain.Models.Response.Redbox
{
    public class CardTypeResponse
    {
        [XmlRoot(ElementName = "cardProgram")]
        public class CardProgram
        {
            [XmlElement(ElementName = "cardType")]
            public string CardType { get; set; }

            [XmlElement(ElementName = "fee")]
            public double Fee { get; set; }
        }

        [XmlRoot(ElementName = "cardPrograms")]
        public class CardPrograms
        {
            [XmlElement(ElementName = "cardProgram")]
            public List<CardProgram> CardProgram { get; set; }
        }

        [XmlRoot(ElementName = "detail")]
        public class Detail
        {
            [XmlElement(ElementName = "cardPrograms")]
            public CardPrograms CardPrograms { get; set; }
        }
    }
}