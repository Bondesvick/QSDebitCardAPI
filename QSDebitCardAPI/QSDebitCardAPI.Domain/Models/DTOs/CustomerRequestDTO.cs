using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace QSDataUpdateAPI.Domain.Models.DTOs
{
    public class CustomerRequestDTO
    {
        [JsonProperty("Id")]
        public long RequestId { get; set; }
        [JsonProperty("TranId")]
        public string TranId { get; set; }
        [JsonProperty("AccountNumber")]
        public string AccountNumber { get; set; }
        [JsonProperty("AccountName")]
        public string AccountName { get; set; }
        [JsonProperty("CustomerAuthType")]
        public string CustomerAuthType { get; set; }
        [JsonProperty("Status")]
        public string Status { get; set; }
        [JsonProperty("CreatedDate")]
        public DateTime CreatedDate { get; set; }
        [JsonProperty("TreatedBy")]
        public string TreatedBy { get; set; }
        [JsonProperty("TreatedDate")]
        public DateTime? TreatedDate { get; set; }
        [JsonProperty("RequestType")]
        public string RequestType { get; set; }
        [JsonProperty("TreatedByUnit")]
        public string TreatedByUnit { get; set; }
        [JsonProperty("RejectionReason")]
        public string RejectionReason { get; set; }
        [JsonProperty("Remarks")]
        public string Remark { get; set; }
        //[JsonProperty("CorporateAccountOpeningDetails")]
        //public AdditionalAccOpeningDetailDto AdditionalAccountOpeningDetails { get; set; }
        [JsonProperty("CustomerRequestDocuments")]
        public List<DocumentDTO> CustomerRequestDocuments { get; set; }

    }

    public class AdditionalAccOpeningDetailDto
    {
        public int Id { get; set; }
        public long CustomerReqId { get; set; }
        public string RequestedAccType { get; set; }
        public string AccountSegment { get; set; }
        public string ExistingAccType { get; set; }
        public string Currency { get; set; }
    }

    public class DocumentDTO
    {
        public string DocExtension { get; set; }
        public string DocName { get; set; }
        public string DocContent { get; set; }
        public int DocId { get; set; }
    }

    public class RequestTypeStatusSummaryDTO
    {
        public int AssignedToSapId { get; set; }
        public int AssignedToOthers { get; set; }
        public int Pending { get; set; }
        public int Total { get; set; }
    }

    public class CustomerRequestListDto
    {
        [JsonProperty("requestDetails")]
        public AdditionalAccOpeningDetailDto RequestDetails { get; set; }
        [JsonProperty("request")]
        public CustomerRequestDTO Request { get; set; }
        [JsonProperty("documents")]
        public IEnumerable<DocumentDTO> Documents { get; set; }
    }
}
