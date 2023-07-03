using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewDepot.ViewModels
{
    public class MemoViewModel
    {
        [Required]
        public int ApplicationId { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Comment { get; set; }
        public List<string> Params { get; set; }
        public DateTime Date { get; set; }
        public string Address { get; set; }
        public string PMB { get; set; }
        public string TelePhoneNumber { get; set; }
        public string CompanyName { get; set; }
        public string LocationAddress { get; set; }
        public string LGA { get; set; }
        public string State { get; set; }
        public string Signature { get; set; }
    }
}