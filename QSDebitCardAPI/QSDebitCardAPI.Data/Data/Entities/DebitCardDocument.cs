using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace QSDataUpdateAPI.Core.Domain.Entities
{
    public partial class DebitCardDocument
    {
        public int Id { get; set; }
        public int AccOpeningReqId { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string ContentOrPath { get; set; }
        public string ContentType { get; set; }


        public virtual DebitCardDetails AccountOpeningRequest { get; set; }
    }
}
