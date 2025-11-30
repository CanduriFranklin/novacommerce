using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OutboxWorker.Infrastructure
{
    public class OutboxMessage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateTime OccurredOn { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        public string Data { get; set; } = string.Empty;

        public DateTime? ProcessedDate { get; set; }

        public int RetryCount { get; set; }

        public string? Error { get; set; }
    }
}
